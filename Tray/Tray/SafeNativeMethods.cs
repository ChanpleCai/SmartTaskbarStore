using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using SmartTaskbar.Properties;

namespace SmartTaskbar
{
    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        public static IntPtr Forewindow;
        public static bool Cloakedval = true;

        #region Taskbar Display State

        public static APPBARDATA MsgData = new APPBARDATA { cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA)) };

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {

            /// DWORD->unsigned int
            public uint cbSize;

            /// HWND->HWND__*
            public IntPtr hWnd;

            /// UINT->unsigned int
            public uint uCallbackMessage;

            /// UINT->unsigned int
            public uint uEdge;

            /// RECT->TagRECT
            public TagRect rc;

            /// LPARAM->LONG_PTR->int
            public int lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TagRect
        {

            /// LONG->int
            public int left;

            /// LONG->int
            public int top;

            /// LONG->int
            public int right;

            /// LONG->int
            public int bottom;
        }

        /// Return Type: UINT_PTR->unsigned int
        ///dwMessage: DWORD->unsigned int
        ///pData: PAPPBARDATA->_AppBarData*
        [DllImport("shell32.dll", EntryPoint = "SHAppBarMessage", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);
        /// <summary>
        /// Set Auto-Hide Mode
        /// </summary>
        public static void Hide()
        {
            MsgData.lParam = 1;
            SHAppBarMessage(10, ref MsgData);
            //see https://github.com/ChanpleCai/SmartTaskbar/issues/27
            PostMessageW(FindWindow("Shell_TrayWnd", null), 0x05CB, (IntPtr)0, (IntPtr)0);
        }
        /// <summary>
        /// Set AlwaysOnTop Mode
        /// </summary>
        public static void Show()
        {
            MsgData.lParam = 0;
            SHAppBarMessage(10, ref MsgData);
        }
        /// <summary>
        /// Indicate if the Taskbar is Auto-Hide
        /// </summary>
        /// <returns>Return true when Auto-Hide</returns>
        public static bool IsHide()
        {
            return SHAppBarMessage(4, ref MsgData) == (IntPtr)1;
        }

        #endregion

        #region Taskbar Animation

        /// Return Type: BOOL->bool
        ///uiAction: UINT->uint
        ///uiParam: UINT->uint
        ///pvParam: PVOID->bool
        ///fWinIni: UINT->uint
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetSystemParameters(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        /// Return Type: BOOL->bool
        ///uiAction: UINT->uint
        ///uiParam: UINT->uint
        ///pvParam: PVOID->out bool
        ///fWinIni: UINT->uint
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetSystemParameters(uint uiAction, uint uiParam, out bool pvParam, uint fWinIni);

        private static bool animation;
        /// <summary>
        /// Get the taskbar animation state
        /// </summary>
        /// <returns>Taskbar animation state</returns>
        public static bool GetTaskbarAnimation()
        {
            GetSystemParameters(0x1002, 0, out animation, 0);
            return animation;
        }
        /// <summary>
        /// Change the taskbar animation state
        /// </summary>
        /// <returns>Taskbar animation state</returns>
        public static bool ChangeTaskbarAnimation()
        {
            animation = !animation;
            SetSystemParameters(0x1003, 0, animation? (IntPtr)1 : IntPtr.Zero, 0x01 | 0x02);
            return animation;
        }
        #endregion

        #region PostMessageW

        /// Return Type: BOOL->int
        ///hWnd: HWND->HWND__*
        ///Msg: UINT->unsigned int
        ///wParam: WPARAM->UINT_PTR->unsigned int
        ///lParam: LPARAM->LONG_PTR->int
        [DllImport("user32.dll", EntryPoint = "PostMessageW")]
        public static extern int PostMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        #endregion

        #region FindWindow

        /// Return Type: HWND->HWND__*
        ///lpClassName: LPCWSTR->WCHAR*
        ///lpWindowName: LPCWSTR->WCHAR*
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        #endregion

        #region GetForegroundWindow

        /// Return Type: HWND->HWND__*
        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        public static extern IntPtr GetForegroundWindow();

        #endregion

        #region GetClassNameW

        /// Return Type: int
        ///hWnd: HWND->HWND__*
        ///lpClassName: LPWSTR->WCHAR*
        ///nMaxCount: int
        [DllImport("user32.dll")]
        public static extern int GetClassNameW([In()] IntPtr hWnd, [Out()] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpClassName, int nMaxCount);

        #endregion

        #region GetWindowRect

        /// Return Type: BOOL->int
        ///hWnd: HWND->HWND__*
        ///lpRect: LPRECT->tagRECT*
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect([In()] IntPtr hWnd, [Out()] out TagRect lpRect);

        #endregion

        #region DwmGetWindowAttribute

        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, [MarshalAs(UnmanagedType.Bool)] out bool pvAttribute, int cbAttribute);

        #endregion

        #region IsWindowVisible

        /// Return Type: BOOL->int
        ///hWnd: HWND->HWND__*
        [DllImport("user32.dll", EntryPoint = "IsWindowVisible")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible([In()] IntPtr hWnd);

        #endregion
    }
}
