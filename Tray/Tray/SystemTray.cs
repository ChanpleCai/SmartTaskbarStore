using SmartTaskbar.Properties;
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static SmartTaskbar.SafeNativeMethods;

namespace SmartTaskbar
{
    internal class SystemTray : ApplicationContext
    {

        private readonly NotifyIcon notifyIcon;
        private readonly ContextMenuStrip contextMenuStrip;
        private readonly ToolStripMenuItem animation;
        private readonly ToolStripMenuItem auto;
        private readonly ToolStripMenuItem exit;
        private readonly Timer timer = new Timer();

        public SystemTray()
        {
            timer.Interval = 375;
            timer.Tick += (s, e) =>
            {
                switch (MsgData.uEdge)
                {
                    case 3:
                        if (Control.MousePosition.Y >= MsgData.rc.top)
                        {
                            return;
                        }

                        break;
                    case 0:
                        if (Control.MousePosition.X <= MsgData.rc.right)
                        {
                            return;
                        }

                        break;
                    case 1:
                        if (Control.MousePosition.Y <= MsgData.rc.bottom)
                        {
                            return;
                        }

                        break;
                    default:
                        if (Control.MousePosition.X >= MsgData.rc.left)
                        {
                            return;
                        }

                        break;
                }

                Forewindow = GetForegroundWindow();

                if (IsWindowVisible(Forewindow) == false)
                {
                    PostMessageW(FindWindow("Shell_TrayWnd", null), 0x05CB, (IntPtr)1, (IntPtr)0x10001);
                    return;
                }


                DwmGetWindowAttribute(Forewindow, 14, out Cloakedval, sizeof(int));
                if (Cloakedval)
                {
                    PostMessageW(FindWindow("Shell_TrayWnd", null), 0x05CB, (IntPtr)1, (IntPtr)0x10001);
                    return;
                }


                StringBuilder sb = new StringBuilder(255);
                GetClassNameW(Forewindow, sb, 255);
                string name = sb.ToString();
                if (name == "WorkerW" || name == "Progman")
                {
                    PostMessageW(FindWindow("Shell_TrayWnd", null), 0x05CB, (IntPtr)1, (IntPtr)0x10001);
                    return;
                }

                GetWindowRect(Forewindow, out TagRect lpRect);
                SHAppBarMessage(5, ref MsgData);

                if (MsgData.rc.top > lpRect.bottom || lpRect.left > MsgData.rc.right || MsgData.rc.left > lpRect.right || lpRect.top > MsgData.rc.bottom)
                {
                    PostMessageW(FindWindow("Shell_TrayWnd", null), 0x05CB, (IntPtr)1, (IntPtr)0x10001);
                    return;
                }
                PostMessageW(FindWindow("Shell_TrayWnd", null), 0x05CB, (IntPtr)0, (IntPtr)0);
            };

            if (Settings.Default.Auto)
            {
                ResetTimer();
            }

            #region Initialization

            var resource = new ResourceCulture();
            var font = new Font("Segoe UI", 9F);
            animation = new ToolStripMenuItem
            {
                Text = resource.GetString(nameof(animation)),
                Font = font
            };
            auto = new ToolStripMenuItem
            {
                Text = resource.GetString(nameof(auto)),
                Font = font
            };
            exit = new ToolStripMenuItem
            {
                Text = resource.GetString(nameof(exit)),
                Font = font
            };
            contextMenuStrip = new ContextMenuStrip
            {
                Renderer = new Win10Renderer()
            };

            contextMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                animation,
                auto,
                new ToolStripSeparator(),
                exit
            });

            notifyIcon = new NotifyIcon
            {
                ContextMenuStrip = contextMenuStrip,
                Text = Application.ProductName,
                Icon = Resources.logo_32,
                Visible = true
            };

            #endregion

            #region Load Event

            Settings.Default.PropertyChanged += (s, e) =>
            {
                Settings.Default.Save();

                if (Settings.Default.Auto)
                {
                    auto.Checked = true;
                    ResetTimer();
                }
                else
                {
                    auto.Checked = false;
                    timer.Stop();
                }
            };

            animation.Click += (s, e) => animation.Checked = ChangeTaskbarAnimation();

            auto.Click += (s, e) => Settings.Default.Auto = !Settings.Default.Auto;

            exit.Click += (s, e) =>
            {
                timer.Stop();
                if (Settings.Default.Auto)
                {
                    Show();
                }

                notifyIcon.Dispose();
                Application.Exit();
            };

            notifyIcon.MouseClick += (s, e) =>
            {
                if (e.Button != MouseButtons.Right)
                {
                    return;
                }

                animation.Checked = GetTaskbarAnimation();

                if (Settings.Default.Auto == false)
                {
                    return;
                }

                ResetTimer();
            };

            notifyIcon.MouseDoubleClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    return;
                }

                Settings.Default.Auto = false;
                if (IsHide())
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            };

            #endregion

            #region Load Settings

            auto.Checked = Settings.Default.Auto;

            #endregion
        }

        private void ResetTimer()
        {
            Hide();
            timer.Start();
        }
    }
}