using DownloadManager;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsService1
{
    partial class DownloadManager : ServiceBase
    {
        public DownloadManager()
        {
            InitializeComponent();  
        }

        private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        private Thread _thread;
        private Boolean isRun = true;

        private void WorkerThreadFunc()
        {
            // we're going to wait 5 minutes between calls to Database, so 
            // set the interval to 300000 milliseconds 
            // (1000 milliseconds = 1 second, 5 * 60 * 1000 = 300000)
            int interval = 300000; // 5 minutes    
                                   // this variable tracks how many milliseconds have gone by since 
                                   // the last call to GetEmployees. Set it to zero to indicate we're 
                                   // starting fresh
            int elapsed = 0;
            // because we don't want to use 100% of the CPU, we will be 
            // sleeping for 1 second between checks to see if it's time to 
            // call GetEmployees
            int waitTime = 1000; // 1 second


            Log log = Log.getLog();
            log.WriteLog(DateTime.Now.ToString() + " :OnStart");
            try
            {

                while (true)
                {
                    // if enough time has passed
                    if (interval >= elapsed)
                    {
                        // reset how much time has passed to zero
                        elapsed = 0;
                        RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\KWIKDownloadCenter");
                        DBOperation dBOperation = DBOperation.getDBOperation();
                        FileOperation fileOperation = FileOperation.getFileOperation();

                        Dictionary<string, object> IParam = new Dictionary<string, object>();
                        DataSet ds = dBOperation.DBOps("GET PENDING REQUEST", IParam);
                        if (ds != null)
                        {
                            if (ds.Tables.Count > 0)
                            {
                                fileOperation.fileOp(ds.Tables[0]);
                            }
                        }

                        //Clean Up

                        fileOperation.fileCleanUp();
                    }
                    // Sleep for 1 second
                    Thread.Sleep(waitTime);
                    // indicate that 1 additional second has passed
                    elapsed += waitTime;
                }
                
            }
            catch (Exception ex)
            {
                log.WriteLog(DateTime.Now.ToString() + " :OnStart :" + ex.Message);
            }
        }


        protected override void OnStart(string[] args)
        {

            _thread = new Thread(new ThreadStart(WorkerThreadFunc) );
            _thread.Name = "Download Centar Worker Thread";
            _thread.IsBackground = true;
            _thread.Start();
            // TODO: Add code here to start your service.

        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
    }
}
