;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;General

  ;Name and file
  Name "ADB Net Installer"
  OutFile "install.exe"
  Unicode True

  ;Default installation folder
  InstallDir "$LOCALAPPDATA\adb.net"
  
  ; Request application privileges for Windows Vista and higher
  RequestExecutionLevel user

  ;Get installation folder from registry if available
  InstallDirRegKey HKLM "Software\adb.net" "Install_Dir"

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "LICENSE"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH

  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH

;--------------------------------
;Languages

  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "adb.net (required)" adb.net

  SectionIn RO

  SetOutPath "$INSTDIR"

  ;ADD YOUR OWN FILES HERE...
  File "bin\Release\net8.0-windows\ADB Net Install.deps.json"
  File "bin\Release\net8.0-windows\ADB Net Install.dll"
  File "bin\Release\net8.0-windows\ADB Net Install.exe"
  File "bin\Release\net8.0-windows\ADB Net Install.runtimeconfig.json"
  File "bin\Release\net8.0-windows\AdvancedSharpAdbClient.dll"
  SetOutPath "$INSTDIR\adb"
  File /r "bin\Release\net8.0-windows\adb\*"
  SetOutPath "$INSTDIR"

  ;Store installation folder
  WriteRegStr HKLM "SOFTWARE\adb.net" "Install_Dir" "$INSTDIR"

  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\adb.net" "DisplayName" "adb.net"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\adb.net" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\adb.net" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\adb.net" "NoRepair" 1
  WriteUninstaller "$INSTDIR\uninstall.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\adb.net" "DisplayIcon" '"$INSTDIR\icon.ico"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\adb.net" "Publisher" "imaperson1060"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\adb.net" "HelpLink" "https://imaperson.dev/adb.net"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\adb.net" "DisplayVersion" "1.0.0"

SectionEnd

; Optional section (can be disabled by the user)
Section "Start Menu Shortcuts" sm

  CreateDirectory "$SMPROGRAMS\adb.net"
  CreateShortcut "$SMPROGRAMS\adb.net\Uninstall.lnk" "$INSTDIR\uninstall.exe"
  CreateShortcut "$SMPROGRAMS\adb.net\adb.net.lnk" "$INSTDIR\ADB Net Install.exe"

SectionEnd

Section "Desktop Shortcuts" d

  CreateShortcut "$DESKTOP\adb.net.lnk" "$INSTDIR\ADB Net Install.exe"
  
SectionEnd

;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_adb.net ${LANG_ENGLISH} "adb.net itself."
  LangString DESC_sm ${LANG_ENGLISH} "Put a shortcut on the desktop."
  LangString DESC_d ${LANG_ENGLISH} "Put shortcuts in the start menu."

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${adb.net} $(DESC_adb.net)
    !insertmacro MUI_DESCRIPTION_TEXT ${sm} $(DESC_sm)
    !insertmacro MUI_DESCRIPTION_TEXT ${d} $(DESC_d)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\adb.net"
  DeleteRegKey HKLM "SOFTWARE\adb.net"

  Delete "$INSTDIR\Uninstall.exe"

  ; Remove shortcuts, if any
  Delete "$DESKTOP\adb.net.lnk"
  Delete "$SMPROGRAMS\adb.net\adb.net.lnk"
  Delete "$SMPROGRAMS\adb.net\Uninstall.lnk"

  RMDir /r "$INSTDIR"
  RMDir "$SMPROGRAMS\adb.net"

SectionEnd
