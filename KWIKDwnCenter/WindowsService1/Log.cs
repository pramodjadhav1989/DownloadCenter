using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService1
{
    sealed class Log
    {
        private static Log _log;

        private Log()
        { }

        public static Log getLog()
        {
            if (_log == null)
            {
                _log = new Log();
            }
            return _log;
        }
        public  bool WriteLog(string strMessage)
        {
            try
            {
                if (!Directory.Exists(@"E:\DownloadCenter\"))
                {
                    Directory.CreateDirectory(@"E:\DownloadCenter\");
                }
                var path = System.Reflection.Assembly.GetEntryAssembly().Location;
                Path.GetDirectoryName(path);
                FileStream objFilestream = new FileStream(string.Format("{0}\\{1}", @"E:\DownloadCenter\", DateTime.Now.ToString("ddMMyyyy") + ".txt"), FileMode.Append, FileAccess.Write);
                StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
                objStreamWriter.WriteLine(strMessage);
                objStreamWriter.Close();
                objFilestream.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
