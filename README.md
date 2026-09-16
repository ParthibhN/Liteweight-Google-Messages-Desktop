# Google Messages Desktop

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform: Windows](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011-0078D6.svg)](https://microsoft.com/windows)

A lightweight, standalone Windows desktop application for [Google Messages Web](https://messages.google.com/web/) built with **C# .NET 8** and **Microsoft Edge WebView2**.

Designed to be ultra-fast and consume minimal system resources (~50–100MB RAM) without the heavy overhead of Electron or full Chromium instances.

---

## Features

- **⚡ Lightweight & Fast**: Uses the native Windows Edge WebView2 runtime already built into Windows 10/11.
- **🔒 Persistent Login & Pairing**: Your pairing tokens, cookies, and local session are securely stored in `%AppData%\GoogleMessagesDesktop\Profile`. Pair once and stay signed in.
- **🔔 Native Desktop Notifications**: Incoming SMS/RCS messages trigger standard Windows toast notifications.
- **💬 Click-to-Chat Focus**: Clicking an incoming notification brings the app out of the tray and jumps directly into the active conversation thread.
- **📥 System Tray & Background Operation**: Closing the window minimizes the app to the system tray so you never miss a message.
- **🎯 Single-Instance Protection**: Opening the app shortcut while it is already running seamlessly focuses your existing window without opening conflicting duplicate sessions.
- **🌐 Smart External Link Handling**: Web links inside text messages open automatically in your default system browser (Chrome, Firefox, Edge, etc.).
- **🖥️ High-DPI & Multi-Monitor Support**: Crisp typography and vector icon scaling on 1080p, 1440p, and 4K monitors.

---

## 📥 Pre-Built Downloads

You do not need to compile the project yourself to use it.

1. Go to the [**Releases**](https://github.com/ParthibhN/Liteweight-Google-Messages-Desktop/releases) page.
2. Download `GoogleMessagesDesktop.exe`.
3. Move the file anywhere you like (e.g. `C:\Users\<YourUser>\AppData\Local\Programs` or your Desktop) and run it!

> **Note:** Requires Windows 10 (1809+) or Windows 11 with the Microsoft Edge WebView2 Runtime (pre-installed by default on modern Windows).

---

## 🚀 How to Set Up & Pair

1. Launch `GoogleMessagesDesktop.exe`.
2. On the welcome screen, click **"Pair with QR code"** (or use Google Account pairing).
3. Ensure **"Remember this computer"** is checked.
4. On your Android phone, open **Google Messages** $\rightarrow$ tap your profile icon $\rightarrow$ **Device pairing** $\rightarrow$ **QR code scanner** and scan the code.

---

## 🛠️ App Controls & Behavior

| Action | Result |
| :--- | :--- |
| **`X` (Close Button)** | Minimizes app to System Tray (keeps notifications active in background) |
| **Tray Icon (Double Click)** | Restores and brings the window to the front |
| **Tray Icon (Right Click)** | Context menu: *Open*, *Reload Page*, *Exit* |
| **`F5` or `Ctrl + R`** | Refresh the Google Messages web page |
| **Notification Click** | Un-hides app and focuses the conversation |

---

## 💻 Building from Source

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or newer
- Windows 10/11

### 1. Clone the repository
```powershell
git clone https://github.com/ParthibhN/Liteweight-Google-Messages-Desktop.git
cd Liteweight-Google-Messages-Desktop
```

### 2. Run in Development Mode
```powershell
dotnet run
```

### 3. Compile to a Single-File Executable

* **Framework-Dependent (~2 MB, requires .NET 8 Runtime installed):**
  ```powershell
  dotnet publish -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -o ./publish
  ```

* **Self-Contained (~60 MB, runs out-of-the-box on any 64-bit Windows PC):**
  ```powershell
  dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o ./publish
  ```

The compiled standalone executable will be located at `./publish/GoogleMessagesDesktop.exe`.

---

## ❓ Frequently Asked Questions (FAQ)

<details>
<summary><b>Where is my login data and message cache stored?</b></summary>
All local session data, cookies, and cache are stored in your user profile folder at:
<code>%AppData%\GoogleMessagesDesktop\Profile</code>.
Deleting this folder will reset the app to its initial state.
</details>

<details>
<summary><b>How do I make the app start automatically with Windows?</b></summary>
1. Press <code>Win + R</code>, type <code>shell:startup</code>, and press Enter.<br>
2. Right-click inside the folder $\rightarrow$ <b>New</b> $\rightarrow$ <b>Shortcut</b>.<br>
3. Browse and select your <code>GoogleMessagesDesktop.exe</code>.
</details>

<details>
<summary><b>How do I completely close the app?</b></summary>
Right-click the Google Messages icon in the System Tray (bottom-right near the clock) and select <b>Exit</b>.
</details>

---

## 📜 Disclaimer

This is an independent, open-source project and is not affiliated with, endorsed by, or sponsored by Google LLC. Google Messages is a trademark of Google LLC.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
