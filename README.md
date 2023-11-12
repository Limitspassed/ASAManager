# ASAManager
Ark Survival Ascended Dedicated Manager

## Overview
ASAManager is a simple utility to assist in managing and updating ARK: Survival Ascended servers. It provides a user-friendly interface for tasks such as installing SteamCMD, updating the ARK server, and running RCON commands.

## Prerequisites
- **Operating System:** Windows
- **Internet Connection:** Required for downloading SteamCMD and server files.
- **DirectX:** [Download DirectX](https://www.microsoft.com/en-us/download/details.aspx?id=35)
- **Microsoft Redistributables:** [Latest Supported VC++ Redistributables](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170)
- **.NET 7.0 Runtime:** Will prompt for installation if not found.

## How to Use ASA
1. **Installation:**
   - Download the latest release from the [Releases](https://github.com/Limitspassed/ASAManager) page.
   - Run the executable file (`ASA.exe`).

2. **Tabs:**
   - **Install Ark Server:**
     - Enter the paths for SteamCMD and ARK server installation.
     - Click "Install ARK Server."
   - **Update Ark Server:**
     - Enter the paths for SteamCMD and ARK server installation.
     - Click "Update ARK Server."
   - **Rcon:**
     - Configure RCON settings (Server IP, Port, Admin Password).
     - Enter RCON commands and click "Execute."

3. **Settings Persistence:**
   - ASA stores the last known good values for paths and settings.
   - Settings are loaded automatically on application start.

4. **Join Our Community:**
   - Join our [Discord](https://discord.gg/ce4VPBxxAC) for discussions, support, and updates.

5. **Feedback and Issues:**
   - For feedback or bug reports, please open an [issue](https://github.com/Limitspassed/ASAManager/issues).

6. **Additional Configuration** - Editing start.bat
   - After installing or updating your ARK server using ASAManager, it's crucial to fine-tune server-specific configurations. This involves editing the start.bat file located in your ARK server directory.

   - Navigate to Your ARK Server Directory:

   - Open the folder where you installed your ARK server using ASAManager.
   - Locate start.bat:

   - Find the start.bat file in the following path: YourARKServer/ShooterGame/Binaries/Win64/start.bat.
   - Edit start.bat:

   - Right-click on start.bat and open it with a text editor (e.g., Notepad).
   - Adjust server settings such as server name, admin password, and other configurations.

   - start ArkAscendedServer.exe TheIsland?listen?SessionName=YourServerName?ServerAdminPassword=YourAdminPassword?Port=7777?QueryPort=27015?MaxPlayers=26 -UseBattlEye

   - Save the changes.
   - Restart Your Server:

   - After editing start.bat, restart your ARK server for the changes to take effect.
   - This step ensures that your server reflects the desired settings, providing a personalized and immersive experience for players on your ARK server.

Enjoy managing your ARK server with ASAManager!
