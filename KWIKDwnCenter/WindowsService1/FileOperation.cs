using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Win32;
using WindowsService1;

namespace DownloadManager
{
    sealed class FileOperation
    {
        private static FileOperation _FileOperation;
        private static String Folder;
        Log log = Log.getLog();
        private FileOperation()
        {

           
        }

        public static FileOperation getFileOperation()
        {
            if (_FileOperation == null)
            {
                _FileOperation = new FileOperation();
            }

            return _FileOperation;
        }

        public static void DeleteIfFileExists(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        public  void fileCleanUp()
        {
            try {
                string drive = Path.GetPathRoot(@"E://");   // e.g. K:\
                String Folder = @"E://reports/";
                if (!Directory.Exists(drive))
                {

                    Folder = @"C://reports/" ;
                }
                DirectoryInfo info = new DirectoryInfo(Folder);
                FileInfo[] files = info.GetFiles().Where(p => p.CreationTime<=DateTime.Today.AddDays(-5)).ToArray();
                foreach (FileInfo f in files)
                {
                    if (f.FullName.Contains("reports"))
                    {
                        f.Delete();
                    }

                }

            }
            catch (Exception ex)
            { 
            }

        }
        public static string GenerateFileName(string userId, string requestId, string ReportFormat)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\KWIKDownloadCenter");

            //if it does exist, retrieve the stored values  
            if (key != null)
            {
                Folder = Convert.ToString(key.GetValue("Folder"));
                key.Close();
            }
            string extention = ReportFormat.ToLower().Equals("csv") ? "csv" : "xlsx";
            DateTime datetime = DateTime.Now;
            string fileName = $"{userId}_{datetime.Day}{datetime.Month}{datetime.Year}{datetime.Hour}{datetime.Minute}{datetime.Second}_{requestId}_report.{extention}";


            string drive = Path.GetPathRoot(@"E://");   // e.g. K:\

            if (Directory.Exists(drive))
            {

                 return @"E://reports/" + fileName; 
            }
            else
            {
                return @"C://reports/" + fileName;
            }

            
        }


        public bool ExportToExcel(DataTable data, string fileName)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(data);
                    workbook.SaveAs(fileName);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static Boolean ToCsv( DataTable table, string fileLocation)
        {
            try
            {
                string colSep = "";
                string rowSep = "\r\n";
                String print = "";
                var format = string.Join(colSep, Enumerable.Range(0, table.Columns.Count)
                                                        .Select(i => string.Format("{{{0}}}", i)));

                print = string.Join(rowSep, table.Rows.OfType<DataRow>()
                                                    .Select(i => string.Format(format, i.ItemArray)));

                File.WriteAllText(fileLocation, print);
            }
            catch (Exception ex) {
                return false;
            }

            return true;
        }
        public void fileOp(DataTable ds)
        {
            try
            {

                for (int i = 0; i < ds.Rows.Count; i++)
                {
                    String RptSPwithParameter = ds.Rows[i]["xmlparameter"].ToString();
                    int srnum = Int32.Parse(ds.Rows[i]["id"].ToString());
                    int reportId = Int32.Parse(ds.Rows[i]["Rpt_Srno"].ToString());
                    String ReportFormat = ds.Rows[i]["ReportFormat"].ToString();

                    String Username = ds.Rows[i]["Username"].ToString();


                    //Update stastus  --flag =1

                    Dictionary<string, object> IParam = new Dictionary<string, object>();
                    IParam.Add("ID", srnum);
                    IParam.Add("STATUS", 1);
                    DBOperation dBOperation = DBOperation.getDBOperation();
                    dBOperation.DBOps("UPDATE REQUEST", IParam);


                    string Report_JSon_Str = "";
                    string Report_Header_Str = "";
                    string Report_Info_Str = "";

                    String fileLocation = GenerateFileName(Username, srnum.ToString(), ReportFormat);

                    
                    DataSet outDs = dBOperation.DynamicSp(RptSPwithParameter);
                    Boolean isFileGenerated = false;
                    if (outDs != null)
                    {
                        if (outDs.Tables.Count > 0)
                        {
                            if (outDs.Tables[0].Rows.Count > 0)
                            {
                                Report_Info_Str = outDs.Tables[0].Select("ID=1")[0]["OutPut"].ToString();
                                Report_Header_Str = outDs.Tables[0].Select("ID=2")[0]["OutPut"].ToString();
                                Report_JSon_Str = outDs.Tables[0].Select("ID=3")[0]["OutPut"].ToString();

                                DataTable data= JsonConvert.DeserializeObject<DataTable>(Report_JSon_Str);

                                var reportHeaders = Report_Header_Str.Split(',');
                                for (int k = 0; k < data.Columns.Count; k++)
                                {
                                    data.Columns[k].ColumnName = reportHeaders[k];
                                }
                                data.TableName = "MOFSL";
                                data.AcceptChanges();

                                if(data.Rows.Count>0)
                                {
                                    if (ReportFormat.ToLower().Equals("csv"))
                                    {
                                       isFileGenerated= ToCsv(data, fileLocation);
                                    }
                                    else
                                    {

                                       isFileGenerated= ExportToExcel(data, fileLocation);
                                    }

                                }
                                
                            }
                        }
                    }


                    IParam.Clear();
                    IParam.Add("ID", srnum);
                    IParam.Add("STATUS", isFileGenerated?2:3);
                    IParam.Add("FILENAME", isFileGenerated ? fileLocation : "");

                    dBOperation.DBOps("UPDATE REQUEST", IParam);


                }
            }
            catch (Exception ex) {
                log.WriteLog(DateTime.Now.ToString() + " :DBOps " + ex.Message);
            }
        }
    }
}
