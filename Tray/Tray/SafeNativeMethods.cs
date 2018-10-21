using System;
using System.Runtime.InteropServices;
using System.Security;

namespace SmartTaskbar
{
    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        public static IntPtr maxWindow;
        public static bool cloakedval = true;
        public static bool tryshowbar = true;

        #region Taskbar Display State

        public static APPBARDATA msgData = new APPBARDATA { cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA)) };

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
            public TagRECT rc;

            /// LPARAM->LONG_PTR->int
            public int lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TagRECT
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
            msgData.lParam = 1;
            SHAppBarMessage(10, ref msgData);
            //see https://github.com/ChanpleCai/SmartTaskbar/issues/27
            PostMessageW(FindWindow("Shell_TrayWnd", null), 0x05CB, (IntPtr)0, (IntPtr)0);
        }
        /// <summary>
        /// Set AlwaysOnTop Mode
        /// </summary>
        public static void Show()
        {
            msgData.lParam = 0;
            SHAppBarMessage(10, ref msgData);
        }
        /// <summary>
        /// Indicate if the Taskbar is Auto-Hide
        /// </summary>
        /// <returns>Return true when Auto-Hide</returns>
        public static bool IsHide()
        {
            return SHAppBarMessage(4, ref msgData) == (IntPtr)1;
        }

        /// <summary>
        /// Change the display status of the Taskbar
        /// </summary>
        public static void ChangeDisplayState()
        {
            if (IsHide())
            {
                Show();
            }
            else
            {
                Hide();
            }
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

        #region GetWindowPlacement

        public static TagWINDOWPLACEMENT placement = new TagWINDOWPLACEMENT { length = (uint)Marshal.SizeOf(typeof(TagWINDOWPLACEMENT)) };

        [StructLayout(LayoutKind.Sequential)]
        public struct TagWINDOWPLACEMENT
        {

            /// UINT->unsigned int
            public uint length;

            /// UINT->unsigned int
            public uint flags;

            /// UINT->unsigned int
            public uint showCmd;

            /// POINT->Point
            public Point ptMinPosition;

            /// POINT->Point
            public Point ptMaxPosition;

            /// RECT->TagRECT
            public TagRECT rcNormalPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {

            /// LONG->int
            public int x;

            /// LONG->int
            public int y;
        }

        /// Return Type: BOOL->int
        ///hWnd: HWND->HWND__*
        ///lpwndpl: WINDOWPLACEMENT*
        [DllImport("user32.dll", EntryPoint = "GetWindowPlacement")]
        public static extern int GetWindowPlacement([In()] IntPtr hWnd, ref TagWINDOWPLACEMENT lpwndpl);

        #endregion

        #region EnumWindows

        /// Return Type: BOOL->int
        ///param0: HWND->HWND__*
        ///param1: LPARAM->LONG_PTR->int
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public delegate bool WNDENUMPROC(IntPtr param0, IntPtr param1);

        /// Return Type: BOOL->int
        ///lpEnumFunc: WNDENUMPROC
        ///lParam: LPARAM->LONG_PTR->int
        [DllImport("user32.dll", EntryPoint = "EnumWindows")]
        public static extern int EnumWindows(WNDENUMPROC lpEnumFunc, IntPtr lParam);
        #endregion

        #region IsWindowVisible

        /// Return Type: BOOL->int
        ///hWnd: HWND->HWND__*
        [DllImport("user32.dll", EntryPoint = "IsWindowVisible")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible([In()] IntPtr hWnd);

        #endregion


        #region DwmGetWindowAttribute

        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, [MarshalAs(UnmanagedType.Bool)] out bool pvAttribute, int cbAttribute);

        #endregion

        #region PostMessage

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
        private static extern IntPtr FindWindow(string strClassName, string strWindowName);

        #endregion

    }
}
