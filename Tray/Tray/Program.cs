using System;
using System.Threading;
using System.Windows.Forms;

namespace SmartTaskbar
{
    static class Program
    {
        [STAThread]
        private static void Main()
        {
            using (new Mutex(true, "{86cf91d9-05f3-44d6-844d-08de9f6dce39}", out bool createNew))
            {
                if (!createNew)
                    return;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new SystemTray());
            }
        }
    }
}
