using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Win32;
using WindowsService1;

namespace DownloadManager
{
    sealed class DBOperation
    {
        public static DBOperation _DBOperation;
        public static SqlConnection _SqlConnection;
        public static String _Connection;//"Password=cl053_dO0r;Persist Security Info=False;User ID=appUser;Initial Catalog=Report;Data Source=192.168.99.174,24115;trusted_connection=False;Connection Timeout=0";
        Log log = Log.getLog();
        
        private DBOperation()
        {
            
           
        }

        

        public static DBOperation getDBOperation()
        {
            if (_DBOperation == null)
            {
                _DBOperation = new DBOperation();
            }
            
                return _DBOperation;
            
        }

        public  SqlConnection getConnection()
        {
            if (_SqlConnection == null)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\KWIKDownloadCenter");
                log.WriteLog(DateTime.Now.ToString() + " :DBOperation :Access Reg keys");

                //if it does exist, retrieve the stored values  
                if (key != null)
                {
                    log.WriteLog(DateTime.Now.ToString() + " :DBOperation :getConnection" + key.GetValue("Connection"));
                    _Connection = Convert.ToString(key.GetValue("Connection"));
                    key.Close();
                }
                _SqlConnection = new SqlConnection("Password=cl053_dO0r;Persist Security Info=False;User ID=appUser;Initial Catalog=Report;Data Source=192.168.99.174,24115;trusted_connection=False;Connection Timeout=0");
            }
            return _SqlConnection;
        }


        public DataSet DynamicSp(String ReportXML)
        {
            SqlConnection con = getConnection();
            DataSet ds = new DataSet();
            try
            {
                SqlCommand sql_cmnd = new SqlCommand("GetDynamicReport", con);
                sql_cmnd.CommandTimeout = 120;
                sql_cmnd.CommandType = CommandType.StoredProcedure;
                sql_cmnd.Parameters.AddWithValue("@XmlParameters", SqlDbType.NVarChar).Value = ReportXML;
                SqlDataAdapter da = new SqlDataAdapter(sql_cmnd);
                da.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                log.WriteLog(DateTime.Now.ToString() + " :DynamicSp "+ex.Message);
                return null;
            }
            finally {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
        public  DataSet DBOps(String process,Dictionary<String,Object> param)
        {
            SqlConnection con = getConnection();
            try
            {

                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand("PROC_DOWNLODCENTER", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PROCESS", process);
                foreach (var p in param.ToArray())
                {
                    var cmdparam = cmd.CreateParameter();
                    cmdparam.ParameterName = p.Key;
                    cmdparam.Value = p.Value;
                    cmd.Parameters.Add(cmdparam);
                }
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                log.WriteLog(DateTime.Now.ToString() + " :DBOps " + ex.Message);
                return null;
            }
            finally {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }

        

    }
}
