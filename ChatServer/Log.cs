using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    class Log
    {
        private static string logFile = "ChatLog.txt";
        static ReaderWriterLock logFileLock = new ReaderWriterLock();

        //public static Log getInstance()
        //{
        //    if (instance == null)
        //    {
        //        instance = new Log();
        //    }
        //    return instance;
        //}

        //public static void SetLogFile(string fileName)
        //{
        //    using (StreamWriter sw = File.CreateText(fileName))
        //    {
                
        //    }
        //}

        public static void append(string content)
        {
            try
            {
                logFileLock.AcquireWriterLock(1000);//wait at most 1 second.

                DateTime dateTime = DateTime.Now;
                content = dateTime.ToShortDateString() + " "
                    + dateTime.ToLongTimeString() + " " + content;
                if (!File.Exists(logFile))
                {
                    using (StreamWriter sw = File.CreateText(logFile))
                    {
                        sw.WriteLine(content);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(logFile))
                    {
                        sw.WriteLine(content);
                    }
                }
                Console.WriteLine(content);

                logFileLock.ReleaseWriterLock();
            }
            catch (Exception e)
            {
                Console.WriteLine("Log Writing Exception: "+e.ToString());
            }
        }
    }
}
