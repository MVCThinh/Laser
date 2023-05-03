using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace Bending
{
    public class FileRW
    {
        // INI 파일에 쓰기
        [DllImport("kernel32")]
        static extern long WritePrivateProfileString(string strSection, string strKey, string strValue, string strFilePath);

        // INI 파일을 읽기
        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string strSection, string strKey, string strDefault, StringBuilder retVal, int iSize, string strFilePath);

        public string Path { get; set; }        
        public string ModelPath { get; set; }   
        public string FileName { get; set; }    
        const int defaultSize = 255;            

        StringBuilder sbBuffer;                 

        public FileRW(string _path, string _fileName)
        {
            this.Path = _path;
            this.FileName = _fileName;
            sbBuffer = new StringBuilder(defaultSize);

            if (!File.Exists(Path))
            {
                if (!Directory.Exists(Path))
                    Directory.CreateDirectory(Path);
            }
        }

        public void WriteValue(string _model, string _section, string _key, string _value)
        {

            string dirPath = Path +"\\" + _model;
            string filePath = dirPath +"\\" + FileName  + ".ini";

            if (!File.Exists(filePath))
            {
                if (!File.Exists(dirPath))
                {
                    if (!Directory.Exists(Path))
                        Directory.CreateDirectory(Path);

                    Directory.CreateDirectory(dirPath);
                }
            }

            //using (File.Create(filePath)) { };
            WritePrivateProfileString(_section, _key, _value, filePath);
        }


        public void WriteValue(string _model, string _section, string _key, object _iValue)
        {
            string dirPath = Path + "\\" + _model;
            string filePath = dirPath + "\\" + FileName  + ".ini";

            if (!File.Exists(filePath))
            {
                if (!File.Exists(dirPath))
                {
                    if (!Directory.Exists(Path))
                        Directory.CreateDirectory(Path);

                    Directory.CreateDirectory(dirPath);
                }

                File.Create(filePath);
            }

            WritePrivateProfileString(_section, _key, _iValue.ToString(), filePath);
        }


        public string ReadValue(string _model, string _section, string _key, string _defaultValue)
        {
            string dirPath;
            if(_model == "")
                dirPath  = Path;
            else
                dirPath = Path + "\\" + _model;

            string filePath = dirPath + "\\" + FileName  + ".ini";

            sbBuffer.Clear();
            GetPrivateProfileString(_section, _key, _defaultValue, sbBuffer, defaultSize, filePath);

            return sbBuffer.ToString();
        }

        public double ReadValue(string _model, string _section, string _key, double _defaultValue)
        {
            string dirPath = Path + "\\" + _model;
            string filePath = dirPath + "\\" + FileName  + ".ini";

            sbBuffer.Clear();
            double value;

            GetPrivateProfileString(_section, _key, _defaultValue.ToString(), sbBuffer, defaultSize, filePath);

            if (double.TryParse(sbBuffer.ToString(), out value))
                return value;
            else
                return -1;
        }

        public double Readvalue_FIX(string Path, string file, string _section, string _key, double _defaultValue)
        {
            string filePath = Path + file;
            sbBuffer.Clear();
            double value;

            GetPrivateProfileString(_section, _key, _defaultValue.ToString(), sbBuffer, defaultSize, filePath);

            if (double.TryParse(sbBuffer.ToString(), out value))
                return value;
            else
                return -1;
        }

        public void Writevalue_FIX(string Path, string file, string _section, string _key, string _value)
        {
            string dirPath = Path + file;
            if (!File.Exists(dirPath))
            {
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
                File.Create(dirPath);
            }
            WritePrivateProfileString(_section, _key, _value, dirPath);
        }

        public double ReadValue_Resolution(string _model, string _section, string _key, double _defaultValue)
        {
            string dirPath = _model;
            //string filePath = dirPath + ".dat";

            sbBuffer.Clear();
            double value;

            GetPrivateProfileString(_section, _key, _defaultValue.ToString(), sbBuffer, defaultSize, dirPath);

            if (double.TryParse(sbBuffer.ToString(), out value))
                return value;
            else
                return -1;
        }
    }
}

