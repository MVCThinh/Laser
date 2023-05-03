using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bending
{
    public partial class frmDL : Form
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, ref Win32API.COPYDATASTRUCT lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);

        public class Win32API
        {
            public const Int32 WM_COPYDATA = 0x004A;

            public struct COPYDATASTRUCT
            {
                public IntPtr dwData;
                public int cbData;

                [MarshalAs(UnmanagedType.LPStr)]
                public string lpData;
            }
        }
        public frmDL()
        {
            //InitializeComponent();
            //Visible = true;
            //Connection(true);
        }
        public void KillDL()
        {
            try
            {
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.ProcessName.StartsWith("DLFTP"))
                    {
                        process.Kill();
                    }
                }
            }
            catch
            {
            }
        }

        private void OpenDL()
        {
            ProcessStartInfo StartInfo = new ProcessStartInfo();
            StartInfo.WorkingDirectory = @"C:\EQData\DLFTP";
            StartInfo.FileName = "DLFTP";
            StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            System.Diagnostics.Process.Start(StartInfo);
        }

        public void SendData(string sMsg)
        {
            if (hwnd == IntPtr.Zero) //pcy210115 없으면 찾기
            {
                hwnd = FindWindow(null, "DLFTP");
            }
            if (hwnd != IntPtr.Zero)
            {
                Win32API.COPYDATASTRUCT cds = new Win32API.COPYDATASTRUCT();
                cds.dwData = IntPtr.Zero; //임의값
                cds.cbData = Encoding.Default.GetBytes(sMsg).Length + 1;
                cds.lpData = sMsg;
                SendMessage(hwnd, Win32API.WM_COPYDATA, IntPtr.Zero, ref cds);
            }
        }

        private IntPtr hwnd;

        public int Connection(bool first)
        {
            //if (first)
            //{
            //    KillDL();
            //    OpenDL();
            //}
            //else
            //{
            //    bool bFind = false;
            //    foreach (Process process in Process.GetProcesses())
            //    {
            //        if (process.ProcessName.StartsWith("DLFTP"))
            //        {
            //            hwnd = FindWindow(null, "DLFTP");
            //            bFind = true;
            //            break;
            //        }
            //    }

            //    if (!bFind)
            //    {
            //        OpenDL();
            //        foreach (Process process in Process.GetProcesses())
            //        {
            //            if (process.ProcessName.StartsWith("DLFTP"))
            //            {
            //                hwnd = FindWindow(null, "DLFTP");
            //                return 0;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        return 0;
            //    }
            //}
            return -1;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Win32API.WM_COPYDATA:
                    Win32API.COPYDATASTRUCT cds1 = (Win32API.COPYDATASTRUCT)m.GetLParam(typeof(Win32API.COPYDATASTRUCT));
                    string[] sData = cds1.lpData.Split(new char[] { ',' });
                    //pcy210115
                    if (sData[0] == "RELOAD")
                    {
                        Bending.Menu.DLStart();
                        SendData("RELOAD,OK");
                    }
                    else if (sData[0] == "RECONNECT")
                    {
                        hwnd = IntPtr.Zero;
                        Visible = false;
                        SendData("RECONNECT");
                    }
                    else if (sData[0] == "CONNECT")
                    {
                        Visible = false;
                    }
                    else if (sData[0] == "NP")
                    {
                        //아무것도 안함.
                    }
                    else //정해진 작업이 없을때 (no protocol) echo
                    {
                        SendData("NP," + cds1.lpData);
                    }
                    break;
            }
            base.WndProc(ref m);
        }
    }
}
