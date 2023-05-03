using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System.Runtime.CompilerServices;

namespace Bending
{
    public class L4Logger
    {
        //싱글톤임. 생성자를 꼭 감춰야함.. 선언해서 써도되고 그냥써도되고..
        private static volatile L4Logger instance = null;
        private static object syncRoot = new Object();

        public ILog log;
        public RollingFileAppender rollingAppender;
        public PatternLayout layout;

        public static L4Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new L4Logger();
                    }
                }
                return instance;
            }
        }


        private L4Logger()
        {
            string FilePath = AppDomain.CurrentDomain.BaseDirectory + "\\Log\\App.log"; //실행폴더 아래에 Log폴더

            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Configured = true;

            RollingFileAppender rollingAppender = new RollingFileAppender();
            rollingAppender.Name = "logger";
            rollingAppender.File = FilePath; // 로그 파일 이름
            rollingAppender.AppendToFile = true;

            rollingAppender.StaticLogFileName = true;
            rollingAppender.CountDirection = 1;
            rollingAppender.RollingStyle = RollingFileAppender.RollingMode.Date;
            rollingAppender.LockingModel = new FileAppender.MinimalLock();
            rollingAppender.DatePattern = "_yyyyMMdd\".log\""; // 날짜가 변경되면 이전 로그에 붙은 이름

            //PatternLayout layout = new PatternLayout("%date [%-5level] %message%newline");//로그 출력 포맷
            PatternLayout layout = new PatternLayout("%date [%thread] %-5level %logger [%property{NDC}] - %message%newline");//로그 출력 포맷

            rollingAppender.Layout = layout;

            hierarchy.Root.AddAppender(rollingAppender);
            rollingAppender.ActivateOptions(); ;
            hierarchy.Root.Level = log4net.Core.Level.All;

            log = LogManager.GetLogger("logger");
            Logger l = (Logger)log.Logger;
        }

        public void Add(string LogMsg)
        {
            log.Debug(LogMsg);
        }
        //CallerMemberName 함수명
        //CallerFilePath 경로
        //CallerLineNumber 라인위치
        public void Add2(string LogMsg, [CallerMemberName] string member = null, [CallerFilePath] string path = null, [CallerLineNumber] int line = 0)
        {
            log.Debug(LogMsg + member + path + line.ToString());
        }

        public void Close()
        {
            LogManager.Shutdown();
        }
    }
}
