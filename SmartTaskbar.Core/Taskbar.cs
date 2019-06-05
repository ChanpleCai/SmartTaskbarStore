﻿using System;
using System.Drawing;

namespace SmartTaskbar.Core
{
    internal class Taskbar
    {
        public Taskbar(IntPtr handle)
        {
            Handle = handle;
        }

        public IntPtr Handle { get; set; }

        public IntPtr Monitor { get; set; }
    }
}