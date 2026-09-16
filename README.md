# Google Messages Desktop

A lightweight, native Windows desktop wrapper for [Google Messages Web](https://messages.google.com/web/) built with C# .NET and Microsoft Edge WebView2.

---

## Features

- **Low Resource Usage**: Built on native Windows WebView2 (~50–100MB RAM) without the bloat of Electron.
- **Persistent Login & Pairing**: Sessions, cookies, and device pairing are preserved across app restarts.
- **Native Notifications**: Desktop toast notifications for incoming SMS/RCS messages.
- **System Tray Support**: Minimizes to the notification area and runs in the background.
- **Single-Instance Protection**: Prevents duplicate sessions and focuses the active window on launch.
- **External Link Routing**: Links clicked inside messages open in your default web browser.

---

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or newer)
- Windows 10/11 with the Microsoft Edge WebView2 Runtime (pre-installed on modern Windows)

---

## Build & Run

### Run in Development
```powershell
dotnet run
```

### Build Standalone Single-File Executable
```powershell
dotnet publish -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -o ./publish
```

The output executable will be created at `./publish/GoogleMessagesDesktop.exe`.

---

## License

This project is licensed under the [MIT License](LICENSE).
