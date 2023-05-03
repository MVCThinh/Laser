using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MX_IF
{
    public class cConfig
    {       
        #region DLL
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
        string cINIPath = "C:/EQData/INI/MXIFConfig.ini";
        #endregion
        
        public static string FFU_SP;     //Comport No
        public static bool FFU_CNCT;     //통신 232 연결 확인
        public static int FFU_CNT;       //ffu갯수 추가

        public static int[] FFU_RPM = new int[32];
        public static int[] FFU_PRESSURE = new int[32];
        public static int[] FFU_Alarm = new int[32];
        public static int[] FFU_Power = new int[32];
       
        public const byte STX = 0x02;
        public const byte ETX = 0x03;
        public const byte MODE1 = 0x8A;
        public const byte DPU_ID = 0x9F;
        public const byte LCU_ID = 0x81;


        public static bool Vision_Init = false;
        public static int rclet = 0;
        public static bool[] bPCrep = new bool[32]; //1 ~ 6 (MCR)

        public static int[] iPCRep = new int[2];

        public static int LogKeepDay = 3;
        public static string cExceptionLogPath;
        public static string cSystemLogPath;
        public static string cInspectionLogPath;
        public static bool GMSUSE = false;
        public static string LGMSAddress;
        public static string RGMSAddress;
        public static string LGMSPort;
        public static string RGMSPort;
        public static string GMSGetTimeDelay;
        public static string HeightIPAddress;    //IP Address
        public static string HeightPort;
        public static string PEPort;
        public static string PEGetTimeDelay;
        public static string GPSIP;
        public static string UPSIP;
        public string INIFileRead(string Section, string Key)
        {
            try
            {
                StringBuilder sb = new StringBuilder(500);
                int Flag = GetPrivateProfileString(Section, Key, "", sb, 500, cINIPath);
                return sb.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        public bool LoadConfig()
        {
            try
            {
                cConfig.LogKeepDay = int.Parse(INIFileRead("MXIFConfig", "LOGKEEPDAY"));
                cConfig.cSystemLogPath = INIFileRead("MXIFConfig", "SystemLogPath");
                cConfig.cExceptionLogPath = INIFileRead("MXIFConfig", "ExceptionLogPath");
                cConfig.cInspectionLogPath = INIFileRead("MXIFConfig", "InspectionLogPath");                
                cConfig.FFU_SP = INIFileRead("MXIFConfig", "FFUCOMPORT");
                cConfig.LGMSPort = INIFileRead("MXIFConfig", "LeftGMSPORT");
                cConfig.RGMSPort = INIFileRead("MXIFConfig", "RightGMSPORT");
                cConfig.GMSGetTimeDelay = INIFileRead("MXIFConfig", "GMSDataGetTimeSet");
                cConfig.HeightIPAddress = INIFileRead("MXIFConfig", "HEIGHTIP");
                cConfig.HeightPort = INIFileRead("MXIFConfig", "HEIGHTPort");
                cConfig.PEPort = INIFileRead("MXIFConfig", "PEPort");
                cConfig.PEGetTimeDelay = INIFileRead("MXIFConfig", "PEGetTimeDelay");

                try
                {
                    cConfig.GPSIP = INIFileRead("MXIFConfig", "GPSIP");
                    cConfig.UPSIP = INIFileRead("MXIFConfig", "UPSIP");
                }
                catch
                {
                    cConfig.GPSIP = "192.168.3.50";
                    cConfig.GPSIP = "192.168.3.51";
                }
                if (!int.TryParse(INIFileRead("MXIFConfig", "FFUCOUNT"), out cConfig.FFU_CNT)) //pcy200618 ffu갯수 추가
                {
                    cConfig.FFU_CNT = 10;
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }


    }
}
