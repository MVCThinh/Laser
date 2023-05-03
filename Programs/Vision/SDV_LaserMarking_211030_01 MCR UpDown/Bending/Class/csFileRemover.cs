using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace Bending
{
    // Log File 삭제하기. lyw. 2017.01.09.
    public class csDirRemover
    {
#if(TEST)
        public frmMain mainForm;
#endif

        public enum eRemoveType
        {
            Directory,
            File,
        }

        public bool externalDeleteUse = false; // 외부 삭제 사용 여부.

        BackgroundWorker bw = new BackgroundWorker();

        private string baseDirectory = "";
        private string deleteDirectory = "";
        private bool subDirectory = false;
        private int expirePeriodHour = -1; // default 한달 지난 것 삭제하기.
        private bool baseRemove = false;
        private string fileExtend;

        private DateTime checkDateTime;
        private int checkPeriodHour;

        private eRemoveType removeType;

        private bool removeStart = false;

        private System.Windows.Forms.Timer timer1 = null;

        bool timerCheck = false;

        public csDirRemover()
        {
            ;
        }

        /// <summary>
        /// checkPeriodTime 은 24 시간제.
        /// </summary>
        /// <param name="checkPeriodTime"></param>
        public csDirRemover(string dirPath, bool subDirectory, int expirePeriodHour, bool baseRemove, int checkPeriodHour)
        {
            RemoverSet(dirPath, subDirectory, expirePeriodHour, baseRemove, checkPeriodHour, "");
        }

        public csDirRemover(string dirPath, bool subDirectory, string fileExt, int expirePeriodHour, bool baseRemove, int checkPeriodHour)
        {
            RemoverSet(dirPath, subDirectory, expirePeriodHour, baseRemove, checkPeriodHour, fileExt);
        }

        private void RemoverSet(string dirPath, bool subDirectory, int expirePeriodHour, bool baseRemove, int checkPeriodHour, string fileExt)
        {
            this.baseDirectory = dirPath;
            this.subDirectory = subDirectory;
            this.expirePeriodHour = expirePeriodHour;
            this.baseRemove = baseRemove;
            this.fileExtend = fileExt;

            if (fileExt == "")
                this.removeType = eRemoveType.Directory;
            else
                this.removeType = eRemoveType.File;


            this.checkPeriodHour = checkPeriodHour;

            checkDateTime = DateTime.Now;

            removeStart = true;

            if (checkPeriodHour > -1)
            {
                this.timer1 = new System.Windows.Forms.Timer();

#if(TEST)
                timer1.Interval = 1000; // 여기.
#else

                timer1.Interval = 6 * 1000 * 10; // 10분에 한번.
#endif

                timer1.Tick += new EventHandler(timer1_Tick);
                timer1.Enabled = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timerCheck)
                return;

            if (bw != null)
            {
                if (bw.IsBusy)
                {
                    return;
                }
            }

            timerCheck = true;

            TimeSpan diffTime = DateTime.Now - checkDateTime;

#if(TEST)
            if (diffTime.TotalSeconds >= 5 || removeStart) // 여기.
#else
            if (diffTime.TotalHours >= this.checkPeriodHour || removeStart)
#endif
            {
                removeStart = false;

                this.checkDateTime = DateTime.Now;

                if (this.removeType == eRemoveType.Directory)
                    this.DirectoryRemove(this.baseDirectory, this.expirePeriodHour, this.subDirectory, this.baseRemove);
                else
                    this.FileRemove(this.baseDirectory, this.fileExtend, this.expirePeriodHour, this.subDirectory);
            }

            timerCheck = false;
        }

        // directory list 가져오기.
        private DirectoryInfo[] GetDirectoryList(string dirPath, bool subDirectory, bool embBaseDir)
        {
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            SearchOption searchOption;

            if (!subDirectory)
                searchOption = SearchOption.TopDirectoryOnly;
            else
                searchOption = SearchOption.AllDirectories;

            DirectoryInfo[] infos = dir.GetDirectories("*.*", searchOption);

            if (embBaseDir)
            {
                Array.Resize(ref infos, infos.Length + 1);
                Array.Copy(new DirectoryInfo[] { dir }, infos, 1);
            }

            return infos;
        }

        // file list 가져오기.
        private FileInfo[] GetFileList(string dirPath, string searchPattern, bool subDirectory)
        {
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            SearchOption searchOption;

            if (!subDirectory)
                searchOption = SearchOption.TopDirectoryOnly;
            else
                searchOption = SearchOption.AllDirectories;

            FileInfo[] infos = dir.GetFiles(searchPattern, searchOption);

            return infos;
        }

        private void OpenFileDelete(string dirPath, bool subDirectory, int expirePeriodHour, bool baseRemove, string fileExt, eRemoveType removeType)
        {
            System.Diagnostics.ProcessStartInfo StartInfo = new System.Diagnostics.ProcessStartInfo();

            StartInfo.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            StartInfo.FileName = "FileRemover";
            StartInfo.Arguments = dirPath + "@" + expirePeriodHour.ToString() + "@" + subDirectory.ToString() + "@" + baseRemove.ToString() + "@" + fileExt + " " + eRemoveType.Directory.ToString();
            StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            System.Diagnostics.Process.Start(StartInfo);
        }

        // hour 으로 삭제.
        public void DirectoryRemove(string dirPath, int expirePeriodHour, bool subDirectory, bool baseRemove, string deleteDirectoryBase = "")
        {
            if (externalDeleteUse)
            {
                //dirPath = dirPath.Replace(' ', '$');
                OpenFileDelete(dirPath, subDirectory, expirePeriodHour, false, "", eRemoveType.Directory);
            }
            else
            {
                if (!bw.IsBusy)
                {
                    //if (expirePeriodHour > -1)
                    //{
                    //bw = new BackgroundWorker();

                    this.baseDirectory = dirPath;
                    this.deleteDirectory = deleteDirectoryBase;
                    this.subDirectory = subDirectory;
                    this.expirePeriodHour = expirePeriodHour;
                    this.baseRemove = baseRemove;

                    bw.DoWork += new DoWorkEventHandler(bw_DirectoryRemove);

                    //handler = new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

                    bw.RunWorkerAsync();
                    //}
                }
            } // if.
        }


        private void bw_RunWorkerCompleted(object sender, EventArgs e)
        {
            bw.DoWork -= bw_DirectoryRemove;
            bw.RunWorkerCompleted -= bw_RunWorkerCompleted;
            //bw.Dispose();
            //this.timerCheck = false;
        }

        private void bw_DirectoryRemove(object sender, DoWorkEventArgs e)
        {
            try
            {
                DirectoryInfo[] dirInfo = GetDirectoryList(this.baseDirectory, this.subDirectory, this.baseRemove);

                DateTime baseDateTime = DateTime.Now;

                foreach (System.IO.DirectoryInfo directory in dirInfo)
                {
                    //string[] Split;
                    
                    //Split = directory.ToString().Split('-');
                    //pcy190528 월-일-시로 되어있는 폴더때문에 폴더삭제가 안되어 추가.
                    //if (Split[0].Length < 3)
                    //{
                    //    Split[2] = Split[1];
                    //    Split[1] = Split[0];
                    //    Split[0] = DateTime.Now.ToString("yyyy");
                    //}
                    //if (Split.Length < 3)
                    //{
                    //    directory.Delete(this.subDirectory);
                    //}
                    //else
                    //{
                    //DateTime FileTime = new DateTime(int.Parse(Split[0]), int.Parse(Split[1]), int.Parse(Split[2]));
                    TimeSpan diffTime = baseDateTime - directory.CreationTime;
                    //TimeSpan diffTime = baseDateTime - FileTime;

#if (TEST)
                    if (diffTime.TotalSeconds >= 10)
#else
                    if (diffTime.TotalHours >= this.expirePeriodHour) // 여기.
#endif
                    {
                        try
                        {
#if (TEST)
                            mainForm.listBox1.Items.Add(directory.FullName);
                            if (mainForm.listBox1.Items.Count > 20)
                                mainForm.listBox1.Items.Clear();
#endif

                            directory.Delete(true);
                        }
                        catch (Exception exp)
                        {
                            //MessageBox.Show(exp.Message);

                            System.Diagnostics.Debug.WriteLine(exp.Message);
                        }
                    }
                    //}
                } // foreach.
                if (CONST.DdriveSpace > 80) //용량이 커지면 가장 오래된 2일치 삭제 //테스트필요
                {
                    DirectoryInfo[] lastdi = dirInfo.OrderBy(di => di.LastWriteTime).Take(1).ToArray();
                    foreach (DirectoryInfo s in lastdi)
                    {
                        try
                        {
                            s.Delete(true);
                        }
                        catch (Exception EX)
                        {
                            System.Diagnostics.Debug.WriteLine(EX.Message);
                        }
                    }
                }
            }
            catch 
            {
                //MessageBox.Show(EX.Message);
                //csLog cLog = new csLog();

                //cLog.ExceptionLogSave("DirectoryDelete" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }
        public void FileRemove(string dirPath, string fileExt, int expirePeriodHour, bool subDirectory)
        {
            if (externalDeleteUse)
            {
                dirPath = dirPath.Replace(' ', '$');
                //StartInfo.Arguments = dirPath + "@" + expirePeriodDay.ToString() + "@" + subDirectory.ToString() + "@" + baseRemove.ToString() + "@" + fileExt + " " + eRemoveType.Directory.ToString();
                OpenFileDelete(dirPath, subDirectory, expirePeriodHour, false, fileExt, eRemoveType.File);
            }
            else
            {
                if (this.expirePeriodHour > -1)
                {
                    bw = new BackgroundWorker();

                    this.baseDirectory = dirPath;
                    this.subDirectory = subDirectory;
                    this.fileExtend = fileExt;
                    this.expirePeriodHour = expirePeriodHour;

                    bw.DoWork += new DoWorkEventHandler(bw_FileRemove);
                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                    bw.RunWorkerAsync();
                }
            }
        }

        private void bw_FileRemove(object sender, DoWorkEventArgs e)
        {
            try
            {
                FileInfo[] fileInfo = GetFileList(this.baseDirectory, "*." + fileExtend, this.subDirectory);

                DateTime baseDateTime = DateTime.Now;

                foreach (System.IO.FileInfo file in fileInfo)
                {
                    TimeSpan diffTime = baseDateTime - file.CreationTime;

#if(TEST)
                    if (diffTime.TotalSeconds >= 10) // 여기.
#else
                    if (diffTime.TotalHours >= expirePeriodHour)
#endif
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception exp)
                        {
                            //MessageBox.Show(exp.Message);

                            System.Diagnostics.Debug.WriteLine(exp.Message);
                        }
                    }
                } // foreach.
            }
            catch 
            {
                //MessageBox.Show(EX.Message);
                //csLog cLog = new csLog();

                //cLog.ExceptionLogSave("LogDirectoryDelete" + "," + EX.GetType().Name + "," + EX.Message);
            }
        }

    }
}
