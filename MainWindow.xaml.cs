/*
 * Ark Server Manager (ASAManager)
 * Copyright (c) 2023 Limitspassed (John)
 * GitHub Repository: https://github.com/Limitspassed/ASAManager
 *
 * This software is licensed under the GNU General Public License v3.0.
 * For the full license text, see: https://opensource.org/licenses/GPL-3.0
 */


using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;
using System.Windows.Threading;
using System.IO.Compression;
using System.Net;
using Path = System.IO.Path;
using System.Net.Http;
using System.ComponentModel;

namespace ASAManager
{
    // Storing persistent settings in a JSON file
    public class ServerSettings
    {
        public string AutoRestartOption { get; set; }
        public string ServerName { get; set; }
        public string ServerPassword { get; set; }
        public string AdminPassword { get; set; }
        public string ModList { get; set; }
        public string MaxPlayers { get; set; }
        public bool EnableBattleEye { get; set; }
        public bool EnableRCON { get; set; }
        public bool StartServerIfNotRunning { get; set; }
        public string RconIp { get; set; }
        public string RconPort { get; set; }
        public string Map { get; set; }
        public string Port { get; set; }
        public string QueryPort { get; set; }
        public string RconExecutablePath { get; set; }
        public string SteamCmdPath { get; set; }
        public string RconCommand { get; set; }
        public bool ScheduledBackup { get; set; }
        public string BackupInterval { get; set; }
        public string BackupLocation { get; set; }
        public string KeepOldestBackup { get; set; }
    }
    public partial class MainWindow : Window
    {
        // Declare a private DispatcherTimer variable
        private DispatcherTimer backupTimer;
        private int intervalMinutes;
        public MainWindow()
        {
            InitializeComponent();

            // Load the saved settings when the window is created

            // Set the txtServerPath text to the saved server path
            txtServerPath.Text = Properties.Settings.Default.ServerPath;
            Closing += MainWindow_Closing;

            // Backup timer for backups to load at start of application
            backupTimer = new DispatcherTimer();
            backupTimer.Tick += BackupTimer_Tick;

        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // Save the ServerPath setting when the application is closing
            Properties.Settings.Default.ServerPath = txtServerPath.Text;
            Properties.Settings.Default.Save();
        }

        // Server Path Group Box Start
        // Start Update Server List Button Logic
        private void btnUpdateServerList_Click(object sender, RoutedEventArgs e)
        {
            // Get the server path from the text box
            string serverPath = txtServerPath.Text;

            // Check if the directory exists
            if (Directory.Exists(serverPath))
            {
                // Get a list of subdirectories (servers) in the server path
                string[] serverDirectories = Directory.GetDirectories(serverPath);

                // Clear existing items in the ComboBox
                cmbServerSelection.Items.Clear();

                // Add each server to the ComboBox
                foreach (string serverDirectory in serverDirectories)
                {
                    // Extract the server name from the path
                    string serverName = System.IO.Path.GetFileName(serverDirectory);

                    // Add the server name to the ComboBox
                    cmbServerSelection.Items.Add(serverName);
                }

                // If there are items in the ComboBox, select the first one
                if (cmbServerSelection.Items.Count > 0)
                {
                    cmbServerSelection.SelectedIndex = 0;
                    txtInstallServer.IsEnabled = false;
                }
            }
            else
            {
                // Handle the case where the server path is not valid
                // Shows message box letting you know server path is not valid could have this logged in the outputtextbox
                MessageBox.Show("Server Path is not valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtInstallServer.IsEnabled = true;
            }
        }
        // Stop Update Server List Button Logic

        // Start Save Settings Button Logic
        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Declaring variables to save
                var serverSettings = new ServerSettings
                {
                    AutoRestartOption = cmbAutoRestartOptions.SelectedItem?.ToString(),
                    ServerName = txtServerName.Text,
                    ServerPassword = txtServerPassword.Text,
                    AdminPassword = txtAdminPassword.Text,
                    ModList = txtModList.Text,
                    MaxPlayers = txtMaxPlayers.Text,
                    EnableBattleEye = chkBattleEye.IsChecked ?? false,
                    EnableRCON = chkEnableRCON.IsChecked ?? false,
                    StartServerIfNotRunning = chkStartServerIfNotRunning.IsChecked ?? false,
                    RconIp = txtRconIP.Text,
                    RconPort = txtRconPort.Text,
                    Map = txtMap.Text,
                    Port = txtPort.Text,
                    QueryPort = txtQueryPort.Text,
                    RconExecutablePath = txtRconExecutablePath.Text,
                    SteamCmdPath = txtSteamCmdPath.Text,
                    RconCommand = txtRconCommand.Text,
                    ScheduledBackup = chkScheduledBackup.IsChecked ?? false,
                    BackupInterval = txtBackupInterval.Text,
                    BackupLocation = txtBackupLocation.Text,
                    KeepOldestBackup = txtKeepOldestBackup.Text
                };

                // Save settings to serversettings
                SaveSettingsToJson(serverSettings);


                // Shows message box letting you know settings have been saved could have this logged in the outputtextbox
                MessageBox.Show("Settings saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Handle the exception, log it, or display an error message.
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Stop Save Settings Button Logic

        // Start Load Settings Button Logic
        private void btnLoadSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load the saved settings
                LoadSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
        }
        private void LoadSettings()
        {
            try
            {
                // Load other settings
                var serverSettings = LoadSettingsFromJson();

                // Load UI elements from serverSettings
                cmbAutoRestartOptions.SelectedItem = serverSettings.AutoRestartOption;
                txtServerName.Text = serverSettings.ServerName;
                txtServerPassword.Text = serverSettings.ServerPassword;
                txtAdminPassword.Text = serverSettings.AdminPassword;
                txtModList.Text = serverSettings.ModList;
                txtMaxPlayers.Text = serverSettings.MaxPlayers; // Assuming MaxPlayers is an int
                chkBattleEye.IsChecked = serverSettings.EnableBattleEye;
                chkEnableRCON.IsChecked = serverSettings.EnableRCON;
                chkStartServerIfNotRunning.IsChecked = serverSettings.StartServerIfNotRunning;

                // Load RCON settings
                txtRconExecutablePath.Text = serverSettings.RconExecutablePath;
                txtRconIP.Text = serverSettings.RconIp;
                txtRconPort.Text = serverSettings.RconPort; // Assuming RconPort is an int
                                                            // Add other RCON settings loading as needed

                // Load other network settings
                txtMap.Text = serverSettings.Map;
                txtPort.Text = serverSettings.Port; // Assuming Port is an int
                txtQueryPort.Text = serverSettings.QueryPort; // Assuming QueryPort is an int

                // Load additional persistent entries
                txtSteamCmdPath.Text = serverSettings.SteamCmdPath;
                txtRconCommand.Text = serverSettings.RconCommand;
                chkScheduledBackup.IsChecked = serverSettings.ScheduledBackup;
                txtBackupInterval.Text = serverSettings.BackupInterval;
                txtBackupLocation.Text = serverSettings.BackupLocation;
                txtKeepOldestBackup.Text = serverSettings.KeepOldestBackup;
            }
            catch (Exception ex)
            {
                // Handle the exception, log it, or display an error message.
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ServerSettings LoadSettingsFromJson()
        {
            // Load settings from JSON file
            string serverName = cmbServerSelection.SelectedItem?.ToString() ?? "DefaultServer";
            string serverFolderPath = System.IO.Path.Combine(txtServerPath.Text, cmbServerSelection.Text, "ShooterGame", "Saved", "Config", "WindowsServer");
            string jsonFilePath = System.IO.Path.Combine(serverFolderPath, $"{serverName}.json");

            // Print out the file path for debugging
            Console.WriteLine($"Loading JSON from: {jsonFilePath}");

            // Deserialize from JSON file
            if (File.Exists(jsonFilePath))
            {
                string json = File.ReadAllText(jsonFilePath);

                // Print out the loaded JSON for debugging
                Console.WriteLine($"Loaded JSON: {json}");

                return Newtonsoft.Json.JsonConvert.DeserializeObject<ServerSettings>(json);
            }
            else
            {
                // If the file doesn't exist, return a new instance
                Console.WriteLine("JSON file does not exist.");

                return new ServerSettings();
            }
        }
        // Stop Load Settings Button Logic

        // Start Clear Server Selection Button Logic
        private void btnClearServerSelection_Click(object sender, RoutedEventArgs e)
        {
            cmbServerSelection.SelectedIndex = -1; // Clear the selection
            txtInstallServer.IsEnabled = true; // Enable the Install Server TextBox
        }
        // Stop Clear Server Selection Button Logic

        // Start GetServerSettings Logic
        private ServerSettings GetServerSettings()
        {
            ServerSettings serverSettings = new ServerSettings
            {
                Map = txtMap.Text,
                ServerName = txtServerName.Text,
                AdminPassword = txtAdminPassword.Text,
                Port = txtPort.Text,
                QueryPort = txtQueryPort.Text,
                MaxPlayers = txtMaxPlayers.Text,
                ModList = txtModList.Text
            };

            return serverSettings;
        }
        // Stop GetServerSettings Logic

        // Start Update Start Bat Button Logic
        private void btnUpdateStartBat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the server path and server selection information from UI elements
                string serverPath = txtServerPath.Text; // Replace with the actual TextBox name
                string serverSelection = cmbServerSelection.SelectedItem?.ToString(); // Replace with the actual ComboBox name

                if (string.IsNullOrEmpty(serverPath) || string.IsNullOrEmpty(serverSelection))
                {
                    MessageBox.Show("Please enter server path and select server.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Construct the full path to start.bat
                string startBatPath = Path.Combine(serverPath, serverSelection, "ShooterGame", "Binaries", "Win64", "start.bat");

                // Get the server settings
                ServerSettings serverSettings = GetServerSettings();

                // Generate the new content for start.bat
                string newStartBatContent = GenerateStartBatContent(serverSettings);

                // Update the start.bat file with the new content
                UpdateStartBatFile(startBatPath, newStartBatContent);
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., directory not found, insufficient permissions)
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void UpdateStartBatFile(string startBatPath, string newContent)
        {
            try
            {
                // Write the new content to the start.bat file, overwriting the existing content
                File.WriteAllText(startBatPath, newContent);

                MessageBox.Show("start.bat updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Handle the exception, log it, or display an error message
                MessageBox.Show($"Error updating start.bat: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Stop Update Start Bat Button Logic

        // Start Update GameUserSettings.ini Button Logic
        private void btnUpdateGameUserSettingsIni_Click(object sender, RoutedEventArgs e)
        {
            // Get the path to the GameUserSettings.ini file
            string serverPath = txtServerPath.Text; // Replace with the actual TextBox name
            string serverSelection = cmbServerSelection.SelectedItem?.ToString(); // Replace with the actual ComboBox name

            if (string.IsNullOrEmpty(serverPath) || string.IsNullOrEmpty(serverSelection))
            {
                MessageBox.Show("Please enter server path and select server.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string gameUserSettingsIniPath = Path.Combine(serverPath, serverSelection, "ShooterGame", "Saved", "Config", "WindowsServer", "GameUserSettings.ini");

            // Insert the ActiveMods line below the [ServerSettings] line
            InsertActiveModsLineBelowServerSettings(gameUserSettingsIniPath);

            // Update the ActiveMods value
            UpdateActiveModsInGameUserSettingsIni(gameUserSettingsIniPath, "ActiveMods=", txtModList.Text);
        }
        private void InsertActiveModsLineBelowServerSettings(string iniPath)
        {
            // Read the ini file
            string iniContents = File.ReadAllText(iniPath);

            // Find the line with the [ServerSettings] key
            int serverSettingsIndex = iniContents.IndexOf("[ServerSettings]");

            // If the [ServerSettings] key is not found, return
            if (serverSettingsIndex == -1)
            {
                return;
            }

            // Get the line that contains the [ServerSettings] key
            string serverSettingsLine = iniContents[serverSettingsIndex..].Split('\n')[0];

            // Insert the ActiveMods line below the [ServerSettings] line
            iniContents = iniContents.Insert(serverSettingsIndex + 1 + serverSettingsLine.Length, "\nActiveMods=");

            // Write the updated ini file
            File.WriteAllText(iniPath, iniContents);
        }

        public static void UpdateActiveModsInGameUserSettingsIni(string iniPath, string key, string value)
        {
            // Read the ini file
            string iniContents = File.ReadAllText(iniPath);

            // Find the line with the specified key
            int keyIndex = iniContents.IndexOf(key);

            // If the key is not found, add it
            if (keyIndex == -1)
            {
                iniContents += $"{Environment.NewLine}{key}={value}";
            }
            else
            {
                // Key found, so update the value
                iniContents = iniContents.Replace($"{key}=", $"{key}={value}");
            }

            // Write the updated ini file
            File.WriteAllText(iniPath, iniContents);
        }
        // Stop Update GameUserSettings.ini Button Logic
        // Stop Server Path Group Box

        // Install Server Group Box Start
        // Start Install Server Button Logic
        private async void btnInstallServer_Click(object sender, RoutedEventArgs e)
        {
            // Get server path from the TextBox
            string serverPath = txtServerPath.Text.Trim();
            string installServerName = txtInstallServer.Text.Trim();
            string steamCmdPath = Path.Combine(serverPath, "steamcmd");

            // Check if Install Server name is specified
            if (string.IsNullOrWhiteSpace(installServerName))
            {
                MessageBox.Show("Please specify the Install Server name.", "Missing Install Server Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Check if SteamCMD path is specified
                if (string.IsNullOrWhiteSpace(txtSteamCmdPath.Text))
                {
                    MessageBox.Show("Please specify the SteamCMD path.", "Missing SteamCMD Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    // Use the specified SteamCMD path
                    steamCmdPath = txtSteamCmdPath.Text.Trim();
                }

                // Check if SteamCMD exists in the specified path
                if (!Directory.Exists(steamCmdPath))
                {
                    // SteamCMD doesn't exist, download and extract it
                    outputTextBox.AppendText("Downloading SteamCMD...\n");

                    await DownloadSteamCmd(steamCmdPath);

                    outputTextBox.AppendText("SteamCMD downloaded and installed.\n");
                }

                // Load server settings
                ServerSettings serverSettings = LoadSettingsFromJson();

                // Run the Ark server installation command asynchronously
                await InstallARKServerAsync(steamCmdPath, serverPath, installServerName, serverSettings);

                outputTextBox.AppendText("Ark server installation completed.\n");
            }
            catch (Exception ex)
            {
                outputTextBox.AppendText($"Error: {ex.Message}\n");
            }
        }

        private async Task InstallARKServerAsync(string steamcmdPath, string serverPath, string installServerName, ServerSettings serverSettings)
        {
            try
            {
                // Construct the ARK server installation path
                string arkServerInstallPath = Path.Combine(serverPath, installServerName);

                // Check if the SteamCMD folder exists, create it if not
                string steamCmdFolder = Path.Combine(steamcmdPath, "steamcmd");
                if (!Directory.Exists(steamCmdFolder))
                {
                    Directory.CreateDirectory(steamCmdFolder);
                    outputTextBox.AppendText($"SteamCMD folder created at {steamCmdFolder}\n");
                }

                // Check if steamcmd.exe exists or download and extract SteamCMD
                string steamCmdExePath = Path.Combine(steamCmdFolder, "steamcmd.exe");
                if (!File.Exists(steamCmdExePath))
                {
                    outputTextBox.AppendText($"SteamCMD executable not found at {steamCmdExePath}, downloading and extracting...\n");
                    await DownloadSteamCmd(steamCmdFolder);
                }

                // Install ARK server asynchronously
                string installCommand = $"{steamCmdExePath} +login anonymous +force_install_dir {arkServerInstallPath} +app_update 2430930 validate +quit";
                outputTextBox.AppendText($"Installation Command: {installCommand}\n");

                Task installationTask = Task.Run(() => RunCommand(installCommand));

                // Wait for the completion of the installation task
                await installationTask;

                // Continue only if the installation is still in progress
                if (!installationTask.IsCompleted)
                {
                    // Create the necessary directory structure
                    string gameBinPath = Path.Combine(arkServerInstallPath, "ShooterGame", "Binaries", "Win64");
                    if (!Directory.Exists(gameBinPath))
                    {
                        Directory.CreateDirectory(gameBinPath);
                        outputTextBox.AppendText($"Directory structure created at {gameBinPath}\n");

                        // Create start.bat in the Win64 folder
                        string startBatPath = Path.Combine(gameBinPath, "start.bat");
                        string startBatContent = GenerateStartBatContent(serverSettings);
                        File.WriteAllText(startBatPath, startBatContent);
                        outputTextBox.AppendText($"start.bat has been created in {startBatPath}\n");
                    }
                }

                // Additional logging
                outputTextBox.AppendText($"Installation completed.\n");
            }
            catch (Exception ex)
            {
                outputTextBox.AppendText($"Error: {ex.Message}\n");
            }
        }

        // Helper method to run a command in a separate process
        private void RunCommand(string command)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (Process process = new Process { StartInfo = processInfo })
            {
                process.OutputDataReceived += (sender, e) => Dispatcher.Invoke(() => outputTextBox.AppendText(e.Data + "\n"));
                process.ErrorDataReceived += (sender, e) => Dispatcher.Invoke(() => outputTextBox.AppendText($"Error: {e.Data}\n"));
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
        }

        private string GenerateStartBatContent(ServerSettings serverSettings)
        {
            StringBuilder content = new StringBuilder();

            // Assuming serverSettings.ModList contains mod IDs separated by commas
            string mods = !string.IsNullOrWhiteSpace(serverSettings.ModList) ? $"-mods=\"{serverSettings.ModList}\"" : "";

            // Determine the BattlEye option based on the checkbox state
            string battleyeOption = chkBattleEye.IsChecked == true ? "-usebattleye" : "-nobattleye";

            content.AppendLine($"start ArkAscendedServer.exe \"{serverSettings.Map}\"?"
                            + $"listen?SessionName=\"{serverSettings.ServerName}\"?"
                            + $"Port=\"{serverSettings.Port}\"?"
                            + $"QueryPort=\"{serverSettings.QueryPort}\"?"
                            + $"RCONEnabled=\"{chkEnableRCON.IsChecked}\"?"
                            + $"RCONPort=\"{txtRconPort.Text}\"?"
                            + $"ServerAdminPassword=\"{txtAdminPassword.Text}\"?"
                            + $" -WinLiveMaxPlayers=\"{serverSettings.MaxPlayers}\" {battleyeOption} {mods}");

            return content.ToString();
        }





        private string ReadStartBatContent(string startBatPath)
        {
            try
            {
                // Read the content of start.bat
                return File.ReadAllText(startBatPath);
            }
            catch (Exception ex)
            {
                // Handle the exception, log it, or display an error message
                MessageBox.Show($"Error reading start.bat: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        private async Task DownloadSteamCmd(string steamCmdPath)
        {
            string steamCmdZipUrl = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";
            string steamCmdZipFile = Path.Combine(steamCmdPath, "steamcmd.zip");

            try
            {
                using (WebClient client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(new Uri(steamCmdZipUrl), steamCmdZipFile);
                }

                // Extract the downloaded ZIP file
                ZipFile.ExtractToDirectory(steamCmdZipFile, steamCmdPath);

                // Clean up: Delete the downloaded ZIP file
                File.Delete(steamCmdZipFile);

                outputTextBox.AppendText("SteamCMD downloaded and installed.\n");
            }
            catch (WebException ex)
            {
                outputTextBox.AppendText($"WebException: {ex.Message}\n");

                if (ex.Response is HttpWebResponse response)
                {
                    outputTextBox.AppendText($"HTTP Status Code: {response.StatusCode}\n");
                }
            }
            catch (Exception ex)
            {
                outputTextBox.AppendText($"Error: {ex.Message}\n");
            }
        }
        // Stop Install Server Button Logic

        // Start Start Server
        private void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get server path and selection
                string serverPath = txtServerPath.Text.Trim();
                string serverSelection = cmbServerSelection.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(serverSelection))
                {
                    MessageBox.Show("Please select a server.", "Missing Server Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Construct the ARK server executable path
                string arkServerExecutablePath = GetStartBatPath();

                // Start the ARK server
                StartARKServerAsync(arkServerExecutablePath);

                // Inform the user
                MessageBox.Show("ARK server started successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately (log or display an error message)
                MessageBox.Show($"Error starting ARK server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetStartBatPath()
        {
            // Get the server path from the text box
            string serverPath = txtServerPath.Text.Trim();

            // Get the selected server name from the combo box
            string selectedServer = cmbServerSelection.SelectedItem?.ToString();

            // Combine the path to start.bat
            return Path.Combine(serverPath, selectedServer, "ShooterGame", "Binaries", "Win64", "start.bat");
        }

        private async Task StartARKServerAsync(string startBatPath)
        {
            try
            {
                // Start the server process
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = startBatPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(startBatPath),
                };

                using (Process process = new Process { StartInfo = processInfo })
                {
                    process.Start();
                    await Task.Delay(5000); // Allow the process to start
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                MessageBox.Show($"Error starting server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Stop Start Server

        // Start Stop Server

        private async void btnStopServer_Click(object sender, RoutedEventArgs e)
        {
            string rconIp = txtRconIP.Text;
            int rconPort;

            if (!int.TryParse(txtRconPort.Text, out rconPort))
            {
                LogAction("Invalid RCON Port. Please enter a valid numeric value.");
                return;
            }

            string rconAdminPassword = txtAdminPassword.Text;
            string mcrconPath = txtRconExecutablePath.Text;

            // Check if mcrcon.exe exists, download and extract if not
            if (!File.Exists(Path.Combine(mcrconPath, "mcrcon.exe")))
            {
                await DownloadMcrcon(mcrconPath);
            }

            string stopCommand = $"servercommand rconIp:{rconIp} rconPort:{rconPort} rconAdminPassword:{rconAdminPassword} DoExit";

            // Execute the RCON command and capture the output
            string output = await ExecuteRconCommandAsync(mcrconPath, rconIp, rconPort, rconAdminPassword, stopCommand);

            // Log the stop action and response
            LogAction($"Stop button clicked. Command: {stopCommand}\nResponse: {output}");
        }

        private async Task DownloadMcrcon(string mcrconPath)
        {
            string mcrconUrl = "https://github.com/Tiiffi/mcrcon/releases/download/v0.7.2/mcrcon-0.7.2-windows-x86-64.zip";
            string mcrconZipFile = Path.Combine(Path.GetTempPath(), "mcrcon.zip");

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    // Download the ZIP file
                    byte[] fileBytes = await httpClient.GetByteArrayAsync(mcrconUrl);
                    File.WriteAllBytes(mcrconZipFile, fileBytes);
                }

                // Extract the downloaded ZIP file to the specified mcrcon path
                ZipFile.ExtractToDirectory(mcrconZipFile, mcrconPath);

                // Clean up: Delete the downloaded ZIP file
                File.Delete(mcrconZipFile);

                MessageBox.Show("mcrcon has been downloaded and extracted to " + mcrconPath);
            }
            catch (Exception ex)
            {
                LogAction($"Error during mcrcon download: {ex.Message}");
            }
        }

        private async Task<string> ExecuteRconCommandAsync(string mcrconPath, string serverIP, int rconPort, string adminPassword, string rconCommand)
        {
            string mcrconCommand = Path.Combine(mcrconPath, "mcrcon.exe") + $" -H {serverIP} -P {rconPort} -p {adminPassword} {rconCommand}";

            try
            {
                // Use Task.Run to execute the command in a separate thread
                return await Task.Run(() => ExecuteCommand(mcrconCommand));
            }
            catch (Exception ex)
            {
                // Log any exceptions
                LogAction($"Error during RCON command execution: {ex.Message}");
                return string.Empty;
            }
        }

        private void LogAction(string message)
        {
            // Use Dispatcher to update UI from a different thread
            Dispatcher.Invoke(() => outputTextBox.AppendText(message + "\n"));
        }

        private string ExecuteCommand(string command)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Environment.CurrentDirectory // Set the working directory appropriately
            };

            using (Process process = new Process { StartInfo = processInfo })
            {
                process.Start();

                // Execute the command
                process.StandardInput.WriteLine(command);
                process.StandardInput.WriteLine("exit");

                // Wait for the command to complete
                process.WaitForExit();

                // Capture only the lines that contain the actual response
                StringBuilder outputBuilder = new StringBuilder();
                using (StreamReader reader = process.StandardOutput)
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Skip lines that contain system or directory information
                        if (!line.Contains("Microsoft Windows") && !line.Contains("C:\\"))
                        {
                            // Skip the line with "RCON Response: (c) Microsoft Corporation. All rights reserved."
                            if (!line.StartsWith("RCON Response: (c) Microsoft Corporation. All rights reserved."))
                            {
                                outputBuilder.AppendLine(line);
                            }
                        }
                    }
                }

                return outputBuilder.ToString();
            }
        }

        private void ExecuteCommand(string command, TextBox outputTextBox)
        {
            try
            {
                // Use ProcessStartInfo to execute the command
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c {command}";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                // Use Dispatcher to update UI on the main thread
                process.OutputDataReceived += (sender, args) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        outputTextBox.AppendText(args.Data + "\n");
                    });
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        outputTextBox.AppendText($"Error: {args.Data}\n");
                    });
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    outputTextBox.AppendText($"Error executing command: {ex.Message}\n");
                });
            }
        }
        // Stop Stop Server

        // Start Update Server
        private async void btnUpdateServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the necessary server information from your application
                string serverPath = txtServerPath.Text.Trim();
                string installServerName = cmbServerSelection.SelectedItem?.ToString(); // Get the selected server name from the ComboBox
                string steamCmdPath = txtSteamCmdPath.Text.Trim();
                ServerSettings serverSettings = GetServerSettings();

                if (string.IsNullOrEmpty(installServerName))
                {
                    MessageBox.Show("Please select a server from the list.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Call the UpdateARKServerAsync method with the appropriate parameters
                await UpdateARKServerAsync(steamCmdPath, serverPath, installServerName, serverSettings);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateARKServerAsync(string steamcmdPath, string serverPath, string installServerName, ServerSettings serverSettings)
        {
            try
            {
                // Construct the ARK server installation path
                string arkServerInstallPath = Path.Combine(serverPath, installServerName);

                // Check if the SteamCMD folder exists, create it if not
                string steamCmdFolder = Path.Combine(steamcmdPath, "steamcmd");
                if (!Directory.Exists(steamCmdFolder))
                {
                    Directory.CreateDirectory(steamCmdFolder);
                    outputTextBox.AppendText($"SteamCMD folder created at {steamCmdFolder}\n");
                }

                // Check if steamcmd.exe exists or download and extract SteamCMD
                string steamCmdExePath = Path.Combine(steamCmdFolder, "steamcmd.exe");
                if (!File.Exists(steamCmdExePath))
                {
                    outputTextBox.AppendText($"SteamCMD executable not found at {steamCmdExePath}, downloading and extracting...\n");
                    await DownloadSteamCmd(steamCmdFolder);
                }

                // Check again if steamcmd.exe exists
                if (File.Exists(steamCmdExePath))
                {
                    // Update ARK server asynchronously
                    string updateCommand = $"{steamCmdExePath} +login anonymous +force_install_dir {arkServerInstallPath} +app_update 2430930 validate +quit";
                    outputTextBox.AppendText($"Update Command: {updateCommand}\n");

                    Task updateTask = Task.Run(() => RunCommand(updateCommand));

                    // Wait for the completion of the update task
                    await updateTask;

                    // Additional logging
                    outputTextBox.AppendText($"Update completed.\n");
                }
                else
                {
                    // Log an error message or display it in the UI
                    outputTextBox.AppendText($"Error: SteamCMD executable not found.\n");
                }
            }
            catch (Exception ex)
            {
                outputTextBox.AppendText($"Error: {ex.Message}\n");
            }
        }


        // End Update Server
        // Stop Install Server Group Box

        // RCON Settings Group Box Start
        // Start Send RCON Command
        private async void btnSendRconCommand_Click(object sender, RoutedEventArgs e)
        {
            string rconIp = txtRconIP.Text;
            int rconPort;

            if (!int.TryParse(txtRconPort.Text, out rconPort))
            {
                LogAction("Invalid RCON Port. Please enter a valid numeric value.");
                return;
            }

            string rconAdminPassword = txtAdminPassword.Text;
            string mcrconPath = txtRconExecutablePath.Text;

            // Check if mcrcon.exe exists, download and extract if not
            if (!File.Exists(Path.Combine(mcrconPath, "mcrcon.exe")))
            {
                await DownloadMcrcon(mcrconPath);
            }

            string rconCommand = $"\"" + txtRconCommand.Text + "\"";

            // Execute the RCON command and capture the output
            string output = await ExecuteRconCommandAsync(mcrconPath, rconIp, rconPort, rconAdminPassword, rconCommand);

            // Log the RCON command and response
            LogAction($"RCON command sent. Command: {rconCommand}\nResponse: {output}");

        }
        // Stop Send RCON Command
        // RCON Settings Group Box Start

        private void LoadRCONSettings()
        {
            // Load RCON settings here
            txtRconIP.Text = Properties.Settings.Default.RconIp;
        }

        private void SaveRCONSettings()
        {
            // Save RCON settings here
            Properties.Settings.Default.RconIp = txtRconIP.Text;
            Properties.Settings.Default.Save();
        }

        private void SaveSettingsToJson(ServerSettings serverSettings)
        {
            // Save settings to JSON file
            string serverName = cmbServerSelection.SelectedItem?.ToString() ?? "DefaultServer";
            string serverFolderPath = System.IO.Path.Combine(txtServerPath.Text, cmbServerSelection.Text, "ShooterGame", "Saved", "Config", "WindowsServer");

            // Ensure the directory exists, create it if not
            Directory.CreateDirectory(serverFolderPath);

            string jsonFilePath = System.IO.Path.Combine(serverFolderPath, $"{serverName}.json");

            // Print out the file path for debugging
            Console.WriteLine($"Saving JSON to: {jsonFilePath}");

            try
            {
                // Serialize and save to JSON file
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(serverSettings);
                File.WriteAllText(jsonFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving JSON: {ex.Message}");
            }
        }
        private void chkEnableRCON_Checked(object sender, RoutedEventArgs e)
        {

        }
        // Stop Send RCON Command
        // RCON Settings Group Box Stop

        // Start Headache Backup Settings
        // Start Backup Settings Group Box
        private void StopBackupTimer()
        {
            backupTimer.Stop();
            LogAction("Backup timer stopped.");
        }

        private void StartBackupTimer()
        {
            // Start backup timer with the specified interval
            if (int.TryParse(txtBackupInterval.Text, out int intervalMinutes))
            {
                backupTimer.Stop(); // Stop the timer before modifying the interval
                backupTimer.Interval = TimeSpan.FromMinutes(intervalMinutes);
                backupTimer.Start(); // Start the timer with the new interval
            }
        }

        private void BackupTimer_Tick(object sender, EventArgs e)
        {
            // Check if scheduled backup is enabled
            if (!chkScheduledBackup.IsChecked == true)
            {
                LogAction("Backup timer ticked. Scheduled backup is not enabled.");
                return;
            }

            LogAction("Backup timer ticked. Initiating backup process...");

            // Iterate over each of the servers
            foreach (string server in cmbServerSelection.Items)
            {
                // Get the selected server name from the combo box
                string selectedServer = cmbServerSelection.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedServer))
                    PerformBackup(selectedServer);
            }
        }

        private void PerformBackup(string selectedServer)
        {
            // Perform the backup operation
            try
            {
                // Convert string to int, if it fails use 0
                int keepOldestDays = int.TryParse(txtKeepOldestBackup.Text, out int oldestDays) ? oldestDays : 0;

                // Backup location string
                string backupLocation = txtBackupLocation.Text;

                // Get the current map name from the Map text box
                string mapName = txtMap.Text.Trim();

                // Create a new folder with the current date and time inside the server's folder
                string serverBackupFolder = Path.Combine(backupLocation, selectedServer, $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}");
                Directory.CreateDirectory(serverBackupFolder);

                // Copy Ark world file to the new folder
                string worldFilePath = Path.Combine(txtServerPath.Text, selectedServer, "ShooterGame", "Saved", "SavedArks", $"{mapName}", $"{mapName}.ark");
                string destinationWorldPath = Path.Combine(serverBackupFolder, $"{mapName}.ark");
                File.Copy(worldFilePath, destinationWorldPath, true);

                // Copy AntiCorruptionBackup file to the new folder
                string antiCorruptionBackupPath = Path.Combine(txtServerPath.Text, selectedServer, "ShooterGame", "Saved", "SavedArks", $"{mapName}", $"{mapName}_AntiCorruptionBackup.bak");
                string destinationAntiCorruptionBackupPath = Path.Combine(serverBackupFolder, $"{mapName}_AntiCorruptionBackup.bak");
                File.Copy(antiCorruptionBackupPath, destinationAntiCorruptionBackupPath, true);

                // Could add logic here for backing up ark profile files, etc.

                // Delete old backups
                DeleteOldBackups(Path.Combine(backupLocation, selectedServer), keepOldestDays);

                LogAction($"Backup completed. Location: {serverBackupFolder}");
            }
            catch (Exception ex)
            {
                LogAction($"Error during backup: {ex.Message}");
            }
        }


        private void DeleteOldBackups(string backupLocation, int keepOldestDays)
        {
            try
            {

                // Get the list of directories in the backup location
                var directories = Directory.GetDirectories(backupLocation)
                                           .Select(dir => new DirectoryInfo(dir))
                                           .OrderByDescending(dir => dir.CreationTime);

                DateTime cutoffDate = DateTime.Now.AddDays(-keepOldestDays);

                // Loop through the directories and delete the old ones
                foreach (var directory in directories)
                {
                    if (directory.CreationTime < cutoffDate)
                    {
                        // Delete the directory or perform your cleanup logic
                        Directory.Delete(directory.FullName, true);
                        LogAction($"Deleted old backup: {directory.FullName}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogAction($"Error deleting old backups: {ex.Message}");
            }
        }

        private void chkScheduledBackup_Checked(object sender, RoutedEventArgs e)
        {
            if (chkScheduledBackup.IsChecked == true)
            {
                // Start timer when checkbox is checked
                StartBackupTimer();
                LogAction("Backup timer started.");
            }
            else
            {
                // Stop timer when checkbox is unchecked
                StopBackupTimer();
                LogAction("Backup timer stopped.");
            }
        }
        // Stop Headache Backup Settings
        // Stop Backup Settings Group Box

        // Start Load Ini File Button Logic
        private void btnLoadIniFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedFileName = ((ComboBoxItem)cmbIniFiles.SelectedItem)?.Content.ToString();

                // Check if an INI file is selected
                if (string.IsNullOrEmpty(selectedFileName))
                {
                    MessageBox.Show("Please select an INI file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Construct the full path to the selected INI file
                string serverPath = txtServerPath.Text; // Replace with the actual TextBox name
                string serverSelection = cmbServerSelection.SelectedItem?.ToString(); // Replace with the actual ComboBox name

                if (string.IsNullOrEmpty(serverPath) || string.IsNullOrEmpty(serverSelection))
                {
                    MessageBox.Show("Please enter server path and select server.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string iniFilePath = Path.Combine(serverPath, serverSelection, "ShooterGame", "Saved", "Config", "WindowsServer", selectedFileName);

                // Check if the INI file exists
                if (!File.Exists(iniFilePath))
                {
                    // If it doesn't exist, create an empty file
                    File.WriteAllText(iniFilePath, string.Empty);
                }

                // Read the content of the INI file and display it in the TextBox
                string iniContent = File.ReadAllText(iniFilePath);
                txtIniContent.Text = iniContent;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading INI file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Stop Load Ini File Button Logic

        // Start Save INI File Button Logic
        private void btnSaveIniFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedFileName = ((ComboBoxItem)cmbIniFiles.SelectedItem)?.Content.ToString();

                // Check if an INI file is selected
                if (string.IsNullOrEmpty(selectedFileName))
                {
                    MessageBox.Show("Please select an INI file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Construct the full path to the selected INI file
                string serverPath = txtServerPath.Text; // Replace with the actual TextBox name
                string serverSelection = cmbServerSelection.SelectedItem?.ToString(); // Replace with the actual ComboBox name

                if (string.IsNullOrEmpty(serverPath) || string.IsNullOrEmpty(serverSelection))
                {
                    MessageBox.Show("Please enter server path and select server.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string iniFilePath = Path.Combine(serverPath, serverSelection, "ShooterGame", "Saved", "Config", "WindowsServer", selectedFileName);

                // Write the content of the TextBox back to the INI file
                File.WriteAllText(iniFilePath, txtIniContent.Text);

                MessageBox.Show("INI file saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving INI file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void chkBattleEye_Checked_1(object sender, RoutedEventArgs e)
        {

        }
        // Stop Save INI File Button Logic
    }
}
