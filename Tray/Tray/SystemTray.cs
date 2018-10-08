using SmartTaskbar.Properties;
using System;
using System.Drawing;
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
            SHAppBarMessage(5, ref msgData);
            timer.Interval = 375;
            timer.Tick += (s, e) =>
            {
                if (maxWindow == IntPtr.Zero)
                {
                    switch (msgData.uEdge)
                    {
                        case 3:
                            if (Control.MousePosition.Y >= msgData.rc.top)
                            {
                                return;
                            }

                            break;
                        case 0:
                            if (Control.MousePosition.X <= msgData.rc.right)
                            {
                                return;
                            }

                            break;
                        case 1:
                            if (Control.MousePosition.Y <= msgData.rc.bottom)
                            {
                                return;
                            }

                            break;
                        default:
                            if (Control.MousePosition.X >= msgData.rc.left)
                            {
                                return;
                            }

                            break;
                    }

                    EnumWindows((h, l) =>
                    {
                        if (IsWindowVisible(h) == false)
                        {
                            return true;
                        }

                        GetWindowPlacement(h, ref placement);
                        if (placement.showCmd != 3)
                        {
                            return true;
                        }

                        DwmGetWindowAttribute(h, 14, out cloakedval, sizeof(int));
                        if (cloakedval)
                        {
                            return true;
                        }

                        maxWindow = h;
                        return false;
                    }, IntPtr.Zero);

                    if (maxWindow == IntPtr.Zero)
                    {
                        if (tryshowbar == false)
                        {
                            return;
                        }

                        tryshowbar = false;
                        Show();
                        return;
                    }
                    Hide();
                    timer.Interval = 500;
                }

                if (IsWindowVisible(maxWindow) == false)
                {
                    goto END;
                }

                DwmGetWindowAttribute(maxWindow, 14, out cloakedval, sizeof(int));
                if (cloakedval)
                {
                    goto END;
                }

                GetWindowPlacement(maxWindow, ref placement);
                if (placement.showCmd == 3)
                {
                    return;
                }

                END:
                ResetTimer();
            };
            if (Settings.Default.Auto)
            {
                timer.Start();
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
                    timer.Start();
                }
                else
                {
                    auto.Checked = false;
                    timer.Stop();
                }
            };

            animation.Click += (s, e) => animation.Checked = ChangeTaskbarAnimation();

            auto.Click += (s, e) =>
            {
                if (Settings.Default.Auto)
                {
                    Settings.Default.Auto = false;
                    Show();
                }
                else
                {
                    Settings.Default.Auto = true;
                }
            };

            exit.Click += (s, e) =>
            {
                timer.Stop();
                Show();
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

                if (Settings.Default.Auto == true && timer.Enabled == false)
                {
                    ResetTimer();
                    timer.Start();
                }
            };

            notifyIcon.MouseDoubleClick += (s, e) =>
            {
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
            tryshowbar = true;
            maxWindow = IntPtr.Zero;
            SHAppBarMessage(5, ref msgData);
            timer.Interval = 375;
        }
    }
}