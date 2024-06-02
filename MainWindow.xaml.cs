using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Models;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace ADB_Net_Install {
	public partial class MainWindow : Window {
		public MainWindow() => InitializeComponent();

		private void URLInput_TextChanged(object sender, TextChangedEventArgs e) => DownloadBtn.IsEnabled = Uri.TryCreate(URLInput.Text, UriKind.Absolute, out _);

		private void DownloadBtn_Click(object sender, RoutedEventArgs e) => Download(URLInput.Text, URLInput.Text.Split('/')[^1]);

		private void TmpFilesInput_TextChanged(object sender, TextChangedEventArgs e) {
			TmpFilesPasteEvent();
			if (TmpFilesIDInput.Text.Length == 7 && int.TryParse(TmpFilesIDInput.Text, out _) && TmpFilesNameInput.Text.Length != 0) {
				string url = $"https://tmpfiles.org/{TmpFilesIDInput.Text}/{TmpFilesNameInput.Text}";
				Cursor = Cursors.Wait;
				HttpResponseMessage response = new HttpClient().SendAsync(new HttpRequestMessage(HttpMethod.Head, url)).Result;
				Cursor = Cursors.Arrow;
				TmpFilesDownloadBtn.IsEnabled = response.IsSuccessStatusCode;
			} else TmpFilesDownloadBtn.IsEnabled = false;
		}

		private void TmpFilesPasteBtn_Click(object sender, RoutedEventArgs e) => TmpFilesPasteEvent(true);

		private void TmpFilesPasteEvent(bool fromButton = false) {
			string data = (string)Clipboard.GetData(DataFormats.Text);
			if (data == null) {
				if (fromButton) MessageBox.Show("The data in the clipboard is not text.");
				return;
			}
			data = data.Trim();
			if (data.StartsWith("http://")) data = data.Remove(0, 7);
			if (data.StartsWith("https://")) data = data.Remove(0, 8);
			string[] parts = data.Split('/');
			if ((parts.Length != 3 || !parts[0].Equals("tmpfiles.org", StringComparison.OrdinalIgnoreCase) || parts[1].Length != 7 || !int.TryParse(parts[1], out _) || parts[2].Length == 0) && (parts.Length != 4 || !parts[0].Equals("tmpfiles.org", StringComparison.OrdinalIgnoreCase) || !parts[1].Equals("dl", StringComparison.OrdinalIgnoreCase) || parts[1].Length != 7 || !int.TryParse(parts[2], out _) || parts[3].Length == 0)) {
				if (fromButton) MessageBox.Show("The data in the clipboard is not a valid tmpfiles.org URL.");
				return;
			}
			TmpFilesIDInput.Text = parts[^2];
			TmpFilesNameInput.Text = parts[^1];
		}

		private void TmpFilesDownloadBtn_Click(object sender, RoutedEventArgs e) => Download($"https://tmpfiles.org/dl/{TmpFilesIDInput.Text}/{TmpFilesNameInput.Text}", TmpFilesNameInput.Text);

		public void Download(string url, string name) {
			Cursor = Cursors.Wait;
			HttpResponseMessage response = new HttpClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, url)).Result;
			Cursor = Cursors.Arrow;
			if (!response.IsSuccessStatusCode) MessageBox.Show("The file could not be downloaded.");
			else File.WriteAllBytes(Path.Combine(Path.GetTempPath(), name), response.Content.ReadAsByteArrayAsync().Result);
			APKName_TextBlock.Text = name;
			APKName_TextBlock.ToolTip = Path.Combine(Path.GetTempPath(), name);
			APKWarning_TextBlock.Visibility = name.EndsWith(".apk") ? Visibility.Hidden : Visibility.Visible;
			FlashBtn.IsEnabled = true;
		}

		private void FlashBtn_Click(object sender, RoutedEventArgs e) {
			Cursor = Cursors.Wait;
			if (!AdbServer.Instance.GetStatus().IsRunning)
				if (new AdbServer().StartServer(@"adb\adb.exe", false) != StartServerResult.Started) MessageBox.Show("Can't start ADB server!");
			AdbClient client = new();
			client.Connect("127.0.0.1:62001");
			DeviceData device = client.GetDevices().FirstOrDefault();
			if (device.IsEmpty) {
				MessageBox.Show("No device found.");
				return;
			}
			Cursor = Cursors.Arrow;

			if (!APKName_TextBlock.ToolTip.ToString()!.EndsWith(".apk")) {
				File.Move(APKName_TextBlock.ToolTip.ToString()!, APKName_TextBlock.ToolTip.ToString() + ".apk", true);
				APKName_TextBlock.ToolTip = APKName_TextBlock.ToolTip.ToString() + ".apk";
				APKName_TextBlock.Text += ".apk";
				APKWarning_TextBlock.Visibility = Visibility.Hidden;
			}
			if (MessageBox.Show($"The following file will be installed to your device ({device.Name}): {APKName_TextBlock.Text}", "Confirm installation", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

			PackageManager pm = new(client, device);
			try { pm.InstallPackage((string)APKName_TextBlock.ToolTip, (InstallProgressEventArgs e) => { if (e.PackageFinished == 1 && e.PackageRequired == 1 && e.State == PackageInstallProgressState.PostInstall) MessageBox.Show("Installation successful."); }); }
			catch (PackageInstallationException) { MessageBox.Show("Installation failed due to an error. Try making sure the file you downloaded is an APK. Hover the file name to view the full temporary path."); }
		}


		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) => Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
	}
}