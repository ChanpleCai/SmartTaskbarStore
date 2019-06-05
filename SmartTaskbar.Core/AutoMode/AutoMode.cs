﻿using System;
using System.Runtime.CompilerServices;
using SmartTaskbar.Core.Helpers;
using SmartTaskbar.Core.UserConfig;
using static SmartTaskbar.Core.SafeNativeMethods;
using static SmartTaskbar.Core.InvokeMethods;

namespace SmartTaskbar.Core.AutoMode
{
    public class AutoMode
    {
        private static IntPtr _maxWindow;
        private static bool _tryShowBar;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AutoMode()
        {
            Ready();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Ready()
        {
            _maxWindow = IntPtr.Zero;
            _tryShowBar = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run()
        {
            if (_maxWindow != IntPtr.Zero)
            {
                if (_maxWindow.IsWindowInvisible() || _maxWindow.IsNotMaximizeWindow()) Ready();
                return;
            }

            if (Variable.taskbars.IsMouseOverTaskbar()) return;

            EnumWindows((handle, lp) =>
            {
                if (handle.IsWindowInvisible()) return true;

                if (handle.IsNotMaximizeWindow()) return true;

                switch (lp)
                {
                    case AutoModeType.BlacklistMode when handle.InBlacklist():
                        return true;
                    case AutoModeType.WhitelistMode when handle.NotInWhitelist():
                        return true;
                    default:
                        _maxWindow = handle;
                        return false;
                }
            }, Settings.ModeType);

            if (_maxWindow == IntPtr.Zero)
            {
                if (_tryShowBar == false) return;
                _tryShowBar = false;
                AutoHide.CancelAutoHide();
            }
            AutoHide.SetAutoHide();
        }
    }
}