using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace WeekendOff
{
    /// <summary>
    /// Kill Microsoft Teams and Hansoft or any process written in ProcessToKillList.txt (one process by line) on weekend.
    /// Waits for maximum 5 minutes to find the processes and kill them.
    /// Ends when all are killed or timed out.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class WeekendOff
    {
        public static readonly List<string> ProcessToKillOnWeekend = new List<string> {"Teams", "HPMClient_x64"};

        /// <summary>
        /// Stops trying to kill after 5 minutes.
        /// </summary>
        public static int Timeout = 5 * 60;

        private static void Main()
        {
            DayOfWeek day = DateTime.Now.DayOfWeek;
            if (day != DayOfWeek.Saturday && day != DayOfWeek.Sunday)
            {
                return;
            }

            try
            {
                var lines = File.ReadAllLines("ProcessToKillList.txt");
                ProcessToKillOnWeekend.Clear();
                ProcessToKillOnWeekend.AddRange(lines);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { }

            List<string> toKillList = new List<string> (ProcessToKillOnWeekend);

            Process[] processes;
            int timeSpent = 0;
            while (timeSpent < Timeout)
            {
                Thread.Sleep(5);
                timeSpent += 5;
                processes = Process.GetProcesses();
                foreach (var pToKill in ProcessToKillOnWeekend)
                {
                    foreach (var process in processes)
                    {
                        if (process.ProcessName == pToKill)
                        {
                            try
                            {
                                process.Kill();
                                toKillList.Remove(pToKill);
                            }
                            // ReSharper disable once EmptyGeneralCatchClause
                            catch 
                            {
                                toKillList.Remove(pToKill);
                            }
                        }
                    }
                }
                if (toKillList.Count == 0)
                    break;
            }
            RefreshTrayArea();
        }

        #region "Refresh Notification Area Icons"

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        public static void RefreshTrayArea()
        {
            IntPtr systemTrayContainerHandle = FindWindow("Shell_TrayWnd", null);
            IntPtr systemTrayHandle = FindWindowEx(systemTrayContainerHandle, IntPtr.Zero, "TrayNotifyWnd", null);
            IntPtr sysPagerHandle = FindWindowEx(systemTrayHandle, IntPtr.Zero, "SysPager", null);
            IntPtr notificationAreaHandle = FindWindowEx(sysPagerHandle, IntPtr.Zero, "ToolbarWindow32", "Notification Area");
            if (notificationAreaHandle == IntPtr.Zero)
            {
                notificationAreaHandle = FindWindowEx(sysPagerHandle, IntPtr.Zero, "ToolbarWindow32", "User Promoted Notification Area");
                IntPtr notifyIconOverflowWindowHandle = FindWindow("NotifyIconOverflowWindow", null);
                IntPtr overflowNotificationAreaHandle = FindWindowEx(notifyIconOverflowWindowHandle, IntPtr.Zero, "ToolbarWindow32", "Overflow Notification Area");
                RefreshTrayArea(overflowNotificationAreaHandle);
            }
            RefreshTrayArea(notificationAreaHandle);
        }


        private static void RefreshTrayArea(IntPtr windowHandle)
        {
            const uint wmMousemove = 0x0200;
            GetClientRect(windowHandle, out var rect);
            for (var x = 0; x < rect.right; x += 5)
                for (var y = 0; y < rect.bottom; y += 5)
                    SendMessage(windowHandle, wmMousemove, 0, (y << 16) + x);
        }
        #endregion
    }
}
