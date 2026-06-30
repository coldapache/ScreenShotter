// Screenshotter — a reliable Win+Shift+S snip-to-clipboard tool for Windows.
//
// Press Win+Shift+S, drag a region, and the image lands on your clipboard
// (paste with Alt+V / Ctrl+V) AND is auto-saved as a timestamped PNG in
// Pictures\Screenshots. Runs silently in the system tray.
//
// Built against .NET Framework 4.x using the built-in csc.exe — no installs.
//
// Key technique: a low-level keyboard hook (WH_KEYBOARD_LL) intercepts
// Win+Shift+S BEFORE Windows routes it to the (flaky) built-in snip, fires our
// capture, and swallows the keystroke so the built-in never appears.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Screenshotter
{
    internal static class Program
    {
        // --- Win32 ---
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private const int VK_SHIFT = 0x10;
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;
        private const int VK_S = 0x53;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        // Keep the delegate alive for the life of the process (prevents GC of the hook callback).
        private static LowLevelKeyboardProc _hookProc;
        private static IntPtr _hookHandle = IntPtr.Zero;

        private static TrayContext _context;
        private static volatile bool _overlayOpen = false;

        [STAThread]
        private static void Main()
        {
            // Single-instance guard so login-launch + manual-launch don't double-run.
            bool createdNew;
            using (var mutex = new Mutex(true, "Screenshotter_SingleInstance_Mutex_v1", out createdNew))
            {
                if (!createdNew) return;

                // Make capture pixel-accurate on high-DPI / multi-monitor setups.
                try { SetProcessDPIAware(); } catch { }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                _context = new TrayContext();

                _hookProc = HookCallback;
                using (var module = System.Diagnostics.Process.GetCurrentProcess().MainModule)
                {
                    _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, GetModuleHandle(module.ModuleName), 0);
                }

                Application.Run(_context);

                if (_hookHandle != IntPtr.Zero) UnhookWindowsHookEx(_hookHandle);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                {
                    int vk = Marshal.ReadInt32(lParam); // vkCode is the first DWORD of KBDLLHOOKSTRUCT
                    if (vk == VK_S)
                    {
                        bool win = Down(VK_LWIN) || Down(VK_RWIN);
                        bool shift = Down(VK_SHIFT);
                        if (win && shift)
                        {
                            // Swallow Win+Shift+S and fire our capture on the UI thread.
                            _context.TriggerCapture();
                            return (IntPtr)1;
                        }
                    }
                }
            }
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        private static bool Down(int vKey)
        {
            return (GetAsyncKeyState(vKey) & 0x8000) != 0;
        }

        public static bool OverlayOpen
        {
            get { return _overlayOpen; }
            set { _overlayOpen = value; }
        }
    }

    // Owns the tray icon, menu, and marshals work onto the UI thread.
    internal sealed class TrayContext : ApplicationContext
    {
        private readonly NotifyIcon _tray;
        private readonly Form _sync;       // hidden window used only for BeginInvoke
        private readonly string _shotsDir;

        public TrayContext()
        {
            _shotsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots");

            // Hidden form gives us a reliable handle to marshal from the keyboard hook.
            _sync = new Form
            {
                ShowInTaskbar = false,
                WindowState = FormWindowState.Minimized,
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                Opacity = 0
            };
            _sync.Load += (s, e) => { _sync.Visible = false; };
            var force = _sync.Handle; // force handle creation without showing

            var menu = new ContextMenuStrip();
            menu.Items.Add("Capture now", null, (s, e) => TriggerCapture());
            menu.Items.Add("Open screenshots folder", null, (s, e) => OpenShotsFolder());
            var startup = new ToolStripMenuItem("Start on login")
            {
                Checked = StartupShortcut.Exists(),
                CheckOnClick = true
            };
            startup.Click += (s, e) => StartupShortcut.Set(startup.Checked);
            menu.Items.Add(startup);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit", null, (s, e) => ExitApp());

            _tray = new NotifyIcon
            {
                Icon = LoadAppIcon(),
                Text = "Screenshotter — Win+Shift+S to capture",
                Visible = true,
                ContextMenuStrip = menu
            };
            _tray.DoubleClick += (s, e) => TriggerCapture();

            _tray.BalloonTipTitle = "Screenshotter is running";
            _tray.BalloonTipText = "Press Win+Shift+S to capture a region.";
            _tray.ShowBalloonTip(2500);
        }

        // Prefer the custom app.ico (next to the exe), then the exe's embedded icon,
        // then the generic stock icon as a last resort.
        private static Icon LoadAppIcon()
        {
            try
            {
                string dir = Path.GetDirectoryName(Application.ExecutablePath);
                string ico = Path.Combine(dir, "app.ico");
                if (File.Exists(ico)) return new Icon(ico, SystemInformation.SmallIconSize);
            }
            catch { }
            try { return Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch { }
            return SystemIcons.Application;
        }

        // Called from the keyboard hook thread (which is this UI thread, pumping messages),
        // but we BeginInvoke to stay out of the hook callback's reentrant context.
        public void TriggerCapture()
        {
            if (_sync.IsDisposed) return;
            _sync.BeginInvoke((MethodInvoker)delegate { ShowOverlay(); });
        }

        private void ShowOverlay()
        {
            if (Program.OverlayOpen) return;
            Program.OverlayOpen = true;
            try
            {
                using (var overlay = new OverlayForm())
                {
                    var result = overlay.ShowDialog();
                    if (result == DialogResult.OK && overlay.Result != null)
                    {
                        HandleCapture(overlay.Result);
                    }
                }
            }
            finally
            {
                Program.OverlayOpen = false;
            }
        }

        private void HandleCapture(Bitmap bmp)
        {
            string savedPath = null;
            try { savedPath = SavePng(bmp); } catch { /* saving is best-effort */ }

            bool copied = ClipboardImage.TrySet(bmp);

            _tray.BalloonTipTitle = copied ? "Copied to clipboard" : "Saved (clipboard busy)";
            _tray.BalloonTipText = copied
                ? "Paste with Alt+V or Ctrl+V."
                : (savedPath != null ? "Saved PNG to Pictures\\Screenshots." : "Capture failed to copy.");
            _tray.ShowBalloonTip(1500);

            bmp.Dispose();
        }

        private string SavePng(Bitmap bmp)
        {
            Directory.CreateDirectory(_shotsDir);
            string name = "Screenshot " + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".png";
            string path = Path.Combine(_shotsDir, name);
            // Avoid clobbering if two captures land in the same second.
            int n = 1;
            while (File.Exists(path))
            {
                name = "Screenshot " + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + " (" + n++ + ").png";
                path = Path.Combine(_shotsDir, name);
            }
            bmp.Save(path, ImageFormat.Png);
            return path;
        }

        private void OpenShotsFolder()
        {
            try
            {
                Directory.CreateDirectory(_shotsDir);
                System.Diagnostics.Process.Start("explorer.exe", "\"" + _shotsDir + "\"");
            }
            catch { }
        }

        private void ExitApp()
        {
            _tray.Visible = false;
            _tray.Dispose();
            ExitThread();
        }
    }

    // Fullscreen, all-monitors selection overlay. Captures the desktop up front and
    // crops the user's selection — no flicker, pixel-accurate, no dimming in the shot.
    internal sealed class OverlayForm : Form
    {
        private readonly Bitmap _full;        // snapshot of the whole virtual desktop
        private readonly Rectangle _virtual;  // virtual screen bounds (may have negative origin)
        private Point _start;
        private Point _current;
        private bool _dragging;

        public Bitmap Result { get; private set; }

        public OverlayForm()
        {
            _virtual = SystemInformation.VirtualScreen;

            _full = new Bitmap(_virtual.Width, _virtual.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(_full))
            {
                g.CopyFromScreen(_virtual.Left, _virtual.Top, 0, 0, _virtual.Size, CopyPixelOperation.SourceCopy);
            }

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Bounds = _virtual;
            TopMost = true;
            ShowInTaskbar = false;
            Cursor = Cursors.Cross;
            DoubleBuffered = true;
            BackColor = Color.Black;
            KeyPreview = true;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Activate();
            BringToFront();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            base.OnKeyDown(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _dragging = true;
                _start = e.Location;
                _current = e.Location;
                Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging)
            {
                _current = e.Location;
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_dragging && e.Button == MouseButtons.Left)
            {
                _dragging = false;
                Rectangle sel = Normalize(_start, _current);
                if (sel.Width >= 3 && sel.Height >= 3)
                {
                    // Hide before grabbing pixels so nothing of the overlay can bleed in,
                    // though we actually crop from the pre-captured snapshot.
                    Result = _full.Clone(sel, PixelFormat.Format32bppArgb);
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    DialogResult = DialogResult.Cancel;
                }
                Close();
            }
            base.OnMouseUp(e);
        }

        private static Rectangle Normalize(Point a, Point b)
        {
            int x = Math.Min(a.X, b.X);
            int y = Math.Min(a.Y, b.Y);
            int w = Math.Abs(a.X - b.X);
            int h = Math.Abs(a.Y - b.Y);
            return new Rectangle(x, y, w, h);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            // Live desktop snapshot as the backdrop.
            g.DrawImageUnscaled(_full, 0, 0);

            // Dim everything.
            using (var dim = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
            {
                g.FillRectangle(dim, ClientRectangle);
            }

            if (_dragging)
            {
                Rectangle sel = Normalize(_start, _current);
                if (sel.Width > 0 && sel.Height > 0)
                {
                    // "Spotlight" the selection by redrawing the un-dimmed snapshot there.
                    g.DrawImage(_full, sel, sel, GraphicsUnit.Pixel);
                    using (var pen = new Pen(Color.FromArgb(255, 0, 174, 255), 1.5f))
                    {
                        g.DrawRectangle(pen, sel.X, sel.Y, sel.Width, sel.Height);
                    }

                    // Size readout near the cursor.
                    string label = sel.Width + " x " + sel.Height;
                    using (var font = new Font("Segoe UI", 9f))
                    using (var back = new SolidBrush(Color.FromArgb(200, 0, 0, 0)))
                    using (var fore = new SolidBrush(Color.White))
                    {
                        SizeF ts = g.MeasureString(label, font);
                        float lx = sel.X;
                        float ly = sel.Y - ts.Height - 4;
                        if (ly < 0) ly = sel.Y + 4;
                        g.FillRectangle(back, lx, ly, ts.Width + 8, ts.Height + 2);
                        g.DrawString(label, font, fore, lx + 4, ly + 1);
                    }
                }
            }

            base.OnPaint(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _full != null) _full.Dispose();
            base.Dispose(disposing);
        }
    }

    // Puts an image on the clipboard in multiple formats so it pastes broadly
    // (CF_BITMAP for Office/most apps, PNG stream for Chromium-based apps).
    internal static class ClipboardImage
    {
        public static bool TrySet(Bitmap bmp)
        {
            var data = new DataObject();
            data.SetData(DataFormats.Bitmap, true, new Bitmap(bmp));

            try
            {
                var ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Png);
                data.SetData("PNG", false, ms);
            }
            catch { /* PNG format is a bonus; ignore if it fails */ }

            // The clipboard can be transiently locked by another process; retry briefly.
            for (int i = 0; i < 8; i++)
            {
                try
                {
                    Clipboard.SetDataObject(data, true);
                    return true;
                }
                catch
                {
                    Thread.Sleep(60);
                }
            }
            return false;
        }
    }

    // Manages the "run on login" shortcut in the user's Startup folder via WScript.Shell COM
    // (no extra assembly references needed).
    internal static class StartupShortcut
    {
        private static string ShortcutPath
        {
            get
            {
                string startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                return Path.Combine(startup, "Screenshotter.lnk");
            }
        }

        public static bool Exists()
        {
            return File.Exists(ShortcutPath);
        }

        public static void Set(bool enabled)
        {
            try
            {
                if (enabled) Create();
                else if (File.Exists(ShortcutPath)) File.Delete(ShortcutPath);
            }
            catch { }
        }

        private static void Create()
        {
            string exe = Application.ExecutablePath;
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return;
            dynamic shell = Activator.CreateInstance(shellType);
            try
            {
                dynamic sc = shell.CreateShortcut(ShortcutPath);
                sc.TargetPath = exe;
                sc.WorkingDirectory = Path.GetDirectoryName(exe);
                sc.Description = "Screenshotter — Win+Shift+S snip to clipboard";
                sc.WindowStyle = 7; // minimized
                sc.Save();
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }
    }
}
