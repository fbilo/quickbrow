using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;
using System.Data;

namespace GetDataFromDBIntoFile
{
    class QueryData
    {
        private string strConnection;
        private string strCommand;
        private string strFile;
        private SqlConnection sc;
        private StringBuilder sb = new StringBuilder();
        private static double FileLimit = Math.Pow(1024, 2) * 100;
        private static double StringLimit = Math.Pow(1024, 2);
        //private StreamWriter sw;

        public QueryData(string strConnection, string strCommand, string strFile)
        {
            // TODO: Complete member initialization
            this.strConnection = strConnection;
            this.strCommand = strCommand;
            this.strFile = strFile;
        }

        public QueryData(string strConnection, string strCommand)
        {
            // TODO: Complete member initialization
            this.strConnection = strConnection;
            this.strCommand = strCommand;
        }

        internal void GenerateFile()
        {
            GetConnection();
            RunQuery();
            ReleaseConnection();
        }

        internal DataSet GetData()
        {
            GetConnection();
            DataSet ds = GenerateData();
            ReleaseConnection();
            return ds;
        }
        private void ReleaseConnection()
        {
            this.sc.Close();
        }

        private DataSet GenerateData()
        {
            DataSet ds = new DataSet("TablesOverall");
            SqlDataAdapter sa = new SqlDataAdapter(strCommand, strConnection);
            sa.TableMappings.Add("Table", "TablesOverall");
            sa.Fill(ds);
            return ds;
        }
        private void RunQuery()
        {
            SqlCommand scom = new SqlCommand(strCommand);
            scom.Connection = this.sc;
            SqlDataReader dr = scom.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    if (this.sb.Length >= StringLimit)
                    {
                        FileUpdate();
                    }
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        if (i != (dr.FieldCount - 1))
                        {
                            this.sb.Append(dr.GetValue(i)).Append(",");
                        }
                        else
                        {
                            this.sb.Append(dr.GetValue(i)).AppendLine();
                        }
                    }
                }
                FileUpdate();
                scom.Dispose();
            }
        }

        private void FileUpdate()
        {
            FileInfo fi = new FileInfo(strFile);
            if (fi.Length >= FileLimit)
            {
                int index;
                int.TryParse(this.strFile.Substring(strFile.IndexOf('.') - 1, 1), out index);
                index++;
                this.strFile = this.strFile.Substring(0, strFile.IndexOf('.') - 1) + index.ToString() + ".txt";
            }

            FileStream fs = new FileStream(strFile, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            try
            {
                sw.Write(this.sb.ToString());
            }
            finally
            {
                sw.Flush();
                sw.Close();
                fs.Close();
                this.sb.Clear();
            }
        }

        private void GetConnection()
        {
            this.sc = new SqlConnection(strConnection);
            sc.Open();
        }

        internal DataTable ExecuteDataSet()
        {
            GetConnection();
            DataTable dt = new DataTable();
            using (SqlConnection scon = new SqlConnection(strConnection))
            {
                using (SqlCommand scom = new SqlCommand(strCommand, scon))
                {
                    scom.CommandType = CommandType.Text;
                    using (SqlDataAdapter sa = new SqlDataAdapter(scom))
                    {
                        sa.Fill(dt);
                    }
                }
                ReleaseConnection();
                return dt;
            }
        }
    }
}
