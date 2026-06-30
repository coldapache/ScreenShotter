<div align="center">

<img src="icon.png" width="110" alt="Screenshotter icon" />

<pre>
┌─                                                                  ─┐
   ███████╗ ██████╗██████╗ ███████╗███████╗███╗   ██╗
   ██╔════╝██╔════╝██╔══██╗██╔════╝██╔════╝████╗  ██║
   ███████╗██║     ██████╔╝█████╗  █████╗  ██╔██╗ ██║
   ╚════██║██║     ██╔══██╗██╔══╝  ██╔══╝  ██║╚██╗██║
   ███████║╚██████╗██║  ██║███████╗███████╗██║ ╚████║
   ╚══════╝ ╚═════╝╚═╝  ╚═╝╚══════╝╚══════╝╚═╝  ╚═══╝
   ███████╗██╗  ██╗ ██████╗ ████████╗████████╗███████╗██████╗
   ██╔════╝██║  ██║██╔═══██╗╚══██╔══╝╚══██╔══╝██╔════╝██╔══██╗
   ███████╗███████║██║   ██║   ██║      ██║   █████╗  ██████╔╝
   ╚════██║██╔══██║██║   ██║   ██║      ██║   ██╔══╝  ██╔══██╗
   ███████║██║  ██║╚██████╔╝   ██║      ██║   ███████╗██║  ██║
   ╚══════╝╚═╝  ╚═╝ ╚═════╝    ╚═╝      ╚═╝   ╚══════╝╚═╝  ╚═╝
└─                       snip  →  clipboard                       ─┘
</pre>

### A dead-simple, _reliable_ replacement for the Windows Snipping Tool

Press **`Win`** + **`Shift`** + **`S`** → drag a box → it's on your clipboard. **Every time.**

<br />

![Platform](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D7?style=for-the-badge&logo=windows&logoColor=white)
![Built with](https://img.shields.io/badge/C%23-.NET%20Framework%204.x-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Dependencies](https://img.shields.io/badge/dependencies-none-2ea44f?style=for-the-badge)
![Privacy](https://img.shields.io/badge/offline-100%25-2ea44f?style=for-the-badge)

</div>

---

## 📑 Contents

- [Why this exists](#-why-this-exists)
- [Features](#-features)
- [What it looks like](#-what-it-looks-like)
- [Requirements](#-requirements)
- [Quick start](#-quick-start)
- [Usage](#-usage)
- [Configuration](#-configuration)
- [How it works](#-how-it-works)
- [Troubleshooting & FAQ](#-troubleshooting--faq)
- [Security & privacy](#-security--privacy)
- [Build from source](#-build-from-source)
- [Project structure](#-project-structure)
- [Uninstall](#-uninstall)
- [Roadmap](#-roadmap)
- [Contributing](#-contributing)
- [License](#-license)

---

## 💡 Why this exists

The built-in Windows snip (`Win+Shift+S`) is unreliable — sometimes it just **doesn't fire**,
or the notification eats your capture, or it pops the wrong tool.

**Screenshotter quietly takes over that exact shortcut** and does one job perfectly: grab a
region, drop it on your clipboard, and keep a backup. No editor to wade through, no clutter,
no surprises — just the muscle-memory shortcut you already use, made dependable.

| | 🪟 Windows Snipping Tool | 📸 Screenshotter |
|---|:---:|:---:|
| Fires reliably on the hotkey | ⚠️ sometimes | ✅ always |
| Straight to clipboard | ✅ | ✅ |
| Auto-saves a backup file | ⚠️ extra clicks | ✅ automatic |
| Background / instant | ⚠️ app launch | ✅ instant |
| Extra dependencies | — | ✅ none |
| Phones home / accounts | varies | ✅ never |

---

## ✨ Features

- 🎯 **Reliable hotkey** — `Win+Shift+S` is intercepted by a low-level keyboard hook, so it
  fires *before* (and instead of) the flaky built-in snip.
- 📋 **Instant clipboard** — paste anywhere with **`Alt+V`** or **`Ctrl+V`**. The image is put
  on the clipboard in **multiple formats** (bitmap + PNG) so it pastes cleanly into Office,
  browsers, Slack, Discord, email — everywhere.
- 💾 **Automatic backup** — every capture is also saved as a timestamped PNG in
  `Pictures\Screenshots`, so nothing is ever lost.
- 🖥️ **Multi-monitor & high-DPI** — select across any screen; pixel-accurate on scaled displays.
- 🌑 **Clean selection UI** — the screen dims, your selection stays bright, with a **live size
  readout** (`640 x 320`) as you drag.
- 🔕 **Silent tray app** — no taskbar window; a single tray icon with a small menu.
- 🚀 **Starts on login** — always ready, nothing to launch.
- 📦 **Zero dependencies** — one C# file, compiled by the C# compiler already in Windows. No
  installer, no .NET SDK, no downloads.
- 🔒 **Fully offline** — no network, no telemetry, no account.

---

## 👀 What it looks like

When you press the hotkey, the screen dims and you drag a selection:

```
  ┌────────────────────────────────────────────────────────┐
  │░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░│  ← whole screen dims
  │░░░░░░░░┌───────────────────────────┐░░░░░░░░░░░░░░░░░░░░░│
  │░░░░░░░░│                           │░░░░░░░░░░░░░░░░░░░░░│
  │░░░░░░░░│      your selection       │░░░░░░  640 x 320  ░░│  ← live size readout
  │░░░░░░░░│      (stays bright)       │░░░░░░░░░░░░░░░░░░░░░│
  │░░░░░░░░│                           │░░░░░░░░░░░░░░░░░░░░░│
  │░░░░░░░░└───────────────────────────┘░░░░░░░░░░░░░░░░░░░░░│
  │░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░│
  └────────────────────────────────────────────────────────┘
        release the mouse  →  📋 copied   +   💾 saved
```

The tray icon (bottom-right of the taskbar) marks it running:

```
        ┌─        ─┐
        │     ●     │     ← blue tile, white selection brackets
        └─        ─┘
   right-click  →  menu
```

---

## ✅ Requirements

- **Windows 10 or 11**
- **.NET Framework 4.x** — preinstalled on every modern Windows (nothing to install)

> No admin rights, no internet, no package manager needed.

---

## 🚀 Quick start

**1.** Download / clone this folder anywhere (e.g. your Desktop).

**2.** Right-click **`install.ps1`** → **Run with PowerShell**.

<sub>…or from any PowerShell window inside the folder:</sub>

```powershell
powershell -ExecutionPolicy Bypass -File ".\install.ps1"
```

That single step:

```
  install.ps1
      │
      ├─▶  builds  Screenshotter.exe   (via the built-in C# compiler)
      ├─▶  generates the app icon       (app.ico)
      ├─▶  adds a Desktop shortcut       📌
      ├─▶  adds a Startup shortcut        🚀  (runs on every login)
      └─▶  launches it now                ✅
```

A tray balloon confirms it's running. **Done.** 🎉

> **First-run note:** Windows may show a SmartScreen prompt because the `.exe` is freshly
> built and unsigned. Click **More info → Run anyway**. (It's your own local build — you can
> read every line of `Screenshotter.cs`.)

---

## 🖱️ Usage

| Step | Action |
|:----:|--------|
| **1** | Press **`Win`** + **`Shift`** + **`S`** anywhere |
| **2** | The screen dims — **drag** a rectangle over what you want |
| **3** | **Release** → it's copied. Paste with **`Alt+V`** or **`Ctrl+V`** |
| **4** | A copy is also saved to **`Pictures\Screenshots`** |

**Cancel** a capture anytime with **`Esc`** or a **right-click**.

**Tray menu** — right-click the blue icon:

```
  📸  Capture now              start a capture without the hotkey
  📁  Open screenshots folder  jump to your saved PNGs
  ☑️  Start on login           toggle auto-start
  ─────────────────────────
  ✖️  Exit                     quit (restores Windows' built-in snip)
```

---

## 🔧 Configuration

<details>
<summary><b>🎹 Change the hotkey</b></summary>

<br />

Open `Screenshotter.cs` and edit the virtual-key constants near the top and the condition
inside `HookCallback`:

```csharp
private const int VK_SHIFT = 0x10;
private const int VK_LWIN  = 0x5B;
private const int VK_RWIN  = 0x5C;
private const int VK_S     = 0x53;   // ← the trigger key
```

Then rebuild:

```powershell
powershell -ExecutionPolicy Bypass -File ".\build.ps1"
```

[Virtual-key code reference →](https://learn.microsoft.com/windows/win32/inputdev/virtual-key-codes)

</details>

<details>
<summary><b>📁 Change where screenshots are saved</b></summary>

<br />

In `Screenshotter.cs`, the save folder is set in the `TrayContext` constructor:

```csharp
_shotsDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots");
```

Point it anywhere you like, then rebuild.

</details>

<details>
<summary><b>🚫 Turn off the auto-save backup (clipboard only)</b></summary>

<br />

In `HandleCapture`, remove (or comment out) the `SavePng(bmp)` call. Rebuild. Captures will
then go to the clipboard only.

</details>

<details>
<summary><b>🎨 Restyle the icon</b></summary>

<br />

Edit `make-icon.ps1` (colors, corner brackets, center dot), delete `app.ico`, then run
`build.ps1` — the icon regenerates automatically and is re-embedded in the `.exe`.

</details>

---

## ⚙️ How it works

```
            Win + Shift + S
                  │
                  ▼
   ┌─────────────────────────────────┐    ┌──────────────────────────────┐
   │  Low-level keyboard hook         │ ─▶ │  swallows the keystroke so    │
   │  SetWindowsHookEx(WH_KEYBOARD_LL)│    │  Windows' built-in snip never │
   │                                  │    │  even sees it                 │
   └────────────────┬─────────────────┘    └──────────────────────────────┘
                    ▼
   ┌─────────────────────────────────┐
   │  Snapshot every monitor up front │   ← Graphics.CopyFromScreen
   │  (pixel-accurate, no flicker)    │      over the full virtual desktop
   └────────────────┬─────────────────┘
                    ▼
        dim overlay  →  you drag a region  →  crop from the snapshot
                    │
          ┌─────────┴──────────┐
          ▼                    ▼
   📋  Clipboard          💾  PNG backup
   bitmap + PNG           Pictures\Screenshots\
   (Alt+V / Ctrl+V)       Screenshot YYYY-MM-DD HHmmss.png
```

**Why a keyboard hook instead of a normal hotkey?**
`Win+Shift+S` is a *system-reserved* shortcut already owned by Windows, so the usual
`RegisterHotKey` API can't claim it. A low-level keyboard hook sees the keys **before** the OS
routes them, lets Screenshotter act, and suppresses the event — which is what lets it cleanly
*replace* the built-in snip on the same key.

---

## 🩹 Troubleshooting & FAQ

<details>
<summary><b>The hotkey doesn't do anything</b></summary>

<br />

- Make sure Screenshotter is running — look for the blue tray icon (it may be hidden under the
  **`^`** "show hidden icons" arrow). Double-click the **Desktop** shortcut to start it.
- Another tool already grabbing `Win+Shift+S` (ShareX, Greenshot, etc.) can conflict — exit
  the other one, or [change Screenshotter's hotkey](#-configuration).
</details>

<details>
<summary><b>"Windows protected your PC" / SmartScreen appears</b></summary>

<br />

That's expected for a freshly built, unsigned `.exe`. Click **More info → Run anyway**. The
entire source is in `Screenshotter.cs` for you to review.
</details>

<details>
<summary><b>Paste shows nothing / pastes the old clipboard</b></summary>

<br />

Another app briefly locked the clipboard. Screenshotter retries automatically, and the tray
balloon tells you whether the copy succeeded. Either way, your capture is safe in
`Pictures\Screenshots` — re-copy it from there if needed.
</details>

<details>
<summary><b>I want the normal Windows snip back</b></summary>

<br />

Right-click the tray icon → **Exit** (instant), or run `uninstall.ps1` to also remove it from
startup. `Win+Shift+S` returns to the Windows built-in immediately.
</details>

<details>
<summary><b>Does it work on multiple monitors?</b></summary>

<br />

Yes — the overlay spans the entire virtual desktop, including secondary monitors and displays
with different DPI scaling.
</details>

<details>
<summary><b>Where are my screenshots?</b></summary>

<br />

`This PC → Pictures → Screenshots`, or right-click the tray icon → **Open screenshots folder**.
Files are named `Screenshot YYYY-MM-DD HHmmss.png` so they sort chronologically.
</details>

---

## 🔒 Security & privacy

- **Runs entirely on your machine.** No network calls, no cloud, no telemetry, no account.
- **No admin rights** required to build, install, or run.
- **Auditable.** The whole app is a single readable C# file (`Screenshotter.cs`) plus small
  PowerShell scripts — nothing is hidden or minified.
- The only system-level behavior is a **keyboard hook** (to capture the hotkey) and writing PNG
  files to your own `Pictures` folder.

---

## 🛠️ Build from source

You don't need Visual Studio or the .NET SDK — just Windows.

```powershell
# Compile Screenshotter.exe (auto-generates app.ico if missing)
powershell -ExecutionPolicy Bypass -File ".\build.ps1"

# Run it
.\Screenshotter.exe
```

`build.ps1` invokes the C# compiler bundled with .NET Framework:

```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
```

with `/target:winexe` (no console window) and `/win32icon:app.ico`.

---

## 📂 Project structure

```
screenshotter/
├── Screenshotter.cs     ← the entire app (one C# source file)
├── build.ps1            ← compile the .exe with the built-in C# compiler
├── make-icon.ps1        ← generate app.ico (the blue snip icon)
├── install.ps1          ← build + Desktop/Startup shortcuts + launch
├── uninstall.ps1        ← stop the app + remove shortcuts
├── app.ico              ← app icon  (generated)
├── icon.png             ← icon for this README
├── README.md            ← you are here
└── Screenshotter.exe    ← the built app  (generated)
```

---

## 🧹 Uninstall

| Goal | Do this |
|------|---------|
| **Pause it** | Right-click tray → **Exit** _(built-in snip returns instantly)_ |
| **Stop auto-start** | Run `uninstall.ps1` _(stops it + removes Desktop/Startup shortcuts)_ |
| **Remove completely** | Run `uninstall.ps1`, then delete this folder |

```powershell
powershell -ExecutionPolicy Bypass -File ".\uninstall.ps1"
```

---

## 🗺️ Roadmap

Deliberately minimal today. Possible future additions (PRs welcome):

- [ ] Optional quick markup (arrow / box / highlight) before copying
- [ ] Configurable hotkey via a small settings file (no recompile)
- [ ] Capture-window and capture-fullscreen modes
- [ ] Optional sound / flash on capture
- [ ] Code signing to skip the SmartScreen prompt

**Intentionally out of scope:** cloud upload, accounts, scrolling capture, video. This stays a
fast, local, single-purpose tool.

---

## 🤝 Contributing

Issues and pull requests are welcome. Because the whole app is one C# file plus a few scripts,
it's easy to read and hack on:

1. Edit `Screenshotter.cs` (or the `.ps1` scripts).
2. Run `build.ps1` to compile.
3. Test the hotkey, capture, clipboard, and save paths.
4. Open a PR describing the change.

Please keep the project **dependency-free** and **offline** — that's the whole point.

---

## 📄 License

Released under the **MIT License** — free to use, modify, and share.

```
MIT License — do anything you like; no warranty. Keep the notice.
```

<sub>Add a `LICENSE` file with the full MIT text before publishing if you want it formally recognized by hosting sites.</sub>

---

<div align="center">

**Built to be the snipping tool that just works.**

<sub>Everything runs locally · No network · No cloud · No telemetry</sub>

<br />

⭐ If this saved you from Snipping Tool rage, give it a star.

</div>
