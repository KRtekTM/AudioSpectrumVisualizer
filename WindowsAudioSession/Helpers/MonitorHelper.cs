using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAudioSession.Helpers
{
    internal static class MonitorHelper
    {
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MONITORPOWER = 0xF170;
        internal static MonitorStates LastMonitorState = MonitorStates.MONITOR_ON;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, int dwData);

        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        internal static System.Windows.Forms.Screen GetRequiredDisplay(int requiredWidth, int requiredHeight)
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            return screens.FirstOrDefault(screen => screen.Bounds.Width == requiredWidth && screen.Bounds.Height == requiredHeight);
        }

        internal enum MonitorStates {
            MONITOR_OFF = 2,
            MONITOR_ON = 1
        }

        internal static void TurnMonitorOnOff(System.Windows.Forms.Screen screen, MonitorStates monitorState)
        {
            var monitorIndexStr = screen.DeviceName.Replace(@"\\.\DISPLAY", "");
            int monitorIndex = Convert.ToInt32(monitorIndexStr);

            if (monitorIndex >= 0)
            {
                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
                {
                    if (monitorIndex == 0)
                    {
                        SendMessage(hMonitor, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (int)monitorState);
                        return false;
                    }
                    monitorIndex--;
                    return true;
                }, 0);
            }
        }

        internal static void ToggleMonitorOnOff(System.Windows.Forms.Screen screen)
        {
            TurnMonitorOnOff(screen, LastMonitorState == MonitorStates.MONITOR_ON ? MonitorStates.MONITOR_OFF : MonitorStates.MONITOR_ON);
        }
    }
}
