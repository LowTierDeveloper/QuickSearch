#define MyAppName "QuickSearch"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "QuickSearch"
#define MyAppExeName "QuickSearch.exe"
#define PublishDir "C:\Users\ASUS\source\QuickSearch\QuickSearch\bin\Release\net10.0-windows\win-x64\publish"

[Setup]
AppId={{A81E61B7-2D42-4C38-9A51-6B9C43A11D72}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\QuickSearch
DefaultGroupName=QuickSearch
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64
WizardStyle=modern
OutputDir=installer-output
OutputBaseFilename=QuickSearch-Setup
UninstallDisplayIcon={app}\QuickSearch.exe

[Tasks]
Name: "autostart"; Description: "Start QuickSearch automatically when I sign in to Windows"; Flags: checkedonce

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\QuickSearch"; Filename: "{app}\QuickSearch.exe"
Name: "{group}\Uninstall QuickSearch"; Filename: "{uninstallexe}"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "QuickSearch"; ValueData: """{app}\QuickSearch.exe"" --background"; Flags: uninsdeletevalue; Tasks: autostart

[Run]
Filename: "{app}\QuickSearch.exe"; Description: "Launch QuickSearch"; Flags: postinstall nowait skipifsilent