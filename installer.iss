; GovBDRadar Inno Setup Script
; Creates a Windows installer for GovBDRadar

#define MyAppName "GovBDRadar"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Your Company"
#define MyAppURL "https://github.com/timinator87/GovBDRadar"
#define MyAppExeName "GovBDRadar.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
AppId={{A1B2C3D4-E5F6-G7H8-I9J0-K1L2M3N4O5P6}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=LICENSE
OutputDir=installer_output
OutputBaseFilename=GovBDRadar-Setup-v{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Main executable and all dependencies
Source: "dist\GovBDRadar\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Documentation
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "WEB_INTERFACE_GUIDE.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "EXAMPLES.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Start Menu shortcuts
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\User Guide"; Filename: "{app}\WEB_INTERFACE_GUIDE.md"
Name: "{group}\Documentation"; Filename: "{app}\README.md"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
; Desktop shortcut (if selected)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Option to launch the application after installation
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
procedure InitializeWizard();
begin
  WizardForm.LicenseAcceptedRadio.Checked := True;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;

  // Check if .NET Framework or other prerequisites are needed
  // Add any prerequisite checks here
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Create data directories
    ForceDirectories(ExpandConstant('{app}\data\pending'));
    ForceDirectories(ExpandConstant('{app}\data\approved'));
    ForceDirectories(ExpandConstant('{app}\data\training'));
  end;
end;
