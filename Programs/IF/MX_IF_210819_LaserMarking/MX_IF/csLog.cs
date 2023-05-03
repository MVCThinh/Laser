using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MX_IF
{
    class csLog
    {
        //20160827
        public enum LogKind { System };

        //public static string cLogPath;
        //public static string cExceptionLogPath;

        public void ExceptionLogSave(string lcString)
        {
            try
            {
                lcString = string.Format("{0}: {1}", DateTime.Now.ToString("yy/MM/dd HH:mm:ss:fff"), lcString);

                string lcdate = DateTime.Now.ToString("yyyy-MM-dd");
                DateTime lcDT = DateTime.Now;
                string lcHour = lcDT.Hour.ToString();

                if (!Directory.Exists(cConfig.cExceptionLogPath + "/" + lcdate))
                    Directory.CreateDirectory(cConfig.cExceptionLogPath + "/" + lcdate);
                StreamWriter FileInfo = new StreamWriter(cConfig.cExceptionLogPath + "/" + lcdate + "/" + lcdate + "-" + lcHour + ".log", true);
                FileInfo.WriteLine(lcString);
                FileInfo.Close();

                int lcLogDeleteCount = DateTime.Now.Year * 365 + DateTime.Now.Month * 30 + DateTime.Now.Day;
                string[] dirs = Directory.GetDirectories(cConfig.cExceptionLogPath);
                foreach (string lcDString in dirs)
                {
                    try
                    {
                        string lcTempstring = lcDString.Substring(cConfig.cExceptionLogPath.Length + 1, lcDString.Length - cConfig.cExceptionLogPath.Length - 1);
                        string lcYYYYstring = lcTempstring.Substring(0, 4);
                        string lcMMstring = lcTempstring.Substring(5, 2);
                        string lcDDstring = lcTempstring.Substring(8, 2);
                        int lcFileCount = int.Parse(lcYYYYstring) * 365 + int.Parse(lcMMstring) * 30 + int.Parse(lcDDstring);

                        if (lcLogDeleteCount > (lcFileCount + cConfig.LogKeepDay))
                        {
                            Directory.Delete(lcDString, true);
                        }
                    }
                    catch { };
                }
            }
            catch
            { }
        }

        public void LogSave(string lcString)
        {
            try
            {
                lcString = string.Format("{0}: {1}", DateTime.Now.ToString("yy/MM/dd HH:mm:ss:fff"), lcString);
                string lcdate = DateTime.Now.ToString("yyyy-MM-dd");
                DateTime lcDT = DateTime.Now;
                string lcHour = lcDT.Hour.ToString();

                if (!Directory.Exists(cConfig.cSystemLogPath + "/" + lcdate))
                    Directory.CreateDirectory(cConfig.cSystemLogPath + "/" + lcdate);
                StreamWriter FileInfo = new StreamWriter(cConfig.cSystemLogPath + "/" + lcdate + "/" + lcdate + "-" + lcHour + ".log", true);
                FileInfo.WriteLine(lcString);
                FileInfo.Close();

                int lcLogDeleteCount = DateTime.Now.Year * 365 + DateTime.Now.Month * 30 + DateTime.Now.Day;
                string[] dirs = Directory.GetDirectories(cConfig.cSystemLogPath);
                foreach (string lcDString in dirs)
                {
                    try
                    {
                        string lcTempstring = lcDString.Substring(cConfig.cSystemLogPath.Length + 1, lcDString.Length - cConfig.cSystemLogPath.Length - 1);
                        string lcYYYYstring = lcTempstring.Substring(0, 4);
                        string lcMMstring = lcTempstring.Substring(5, 2);
                        string lcDDstring = lcTempstring.Substring(8, 2);
                        int lcFileCount = int.Parse(lcYYYYstring) * 365 + int.Parse(lcMMstring) * 30 + int.Parse(lcDDstring);

                        if (lcLogDeleteCount > (lcFileCount + cConfig.LogKeepDay))
                        {
                            Directory.Delete(lcDString, true);
                        }
                    }
                    catch { };
                }
            }
            catch
            { }
        }
        
    }
}
