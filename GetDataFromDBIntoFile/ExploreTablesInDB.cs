using System;
using System.Collections.Generic;
using System.ComponentModel;


using System.Diagnostics;
using System.Windows.Forms;

using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using Microsoft.Data.ConnectionUI;
using Microsoft.VisualBasic;


namespace GetDataFromDBIntoFile
{
    public partial class ExploreTablesInDB : Form
    {
        private string strConnection = "";
        private string strTable = "";
        private string strCommand = "";
        private string strschema = "";
        private static string unSupportTypes = "'blob', 'binary', 'varbinary', 'geography', 'text', 'ntext', 'image'";

        private static string strExploreTables = @"SELECT s.Name as SchemaName,t.Name as TableName 
FROM SYS.TABLES t
JOIN sys.Schemas s
    on t.Schema_id = s.Schema_ID
WHERE T.TYPE = 'U' AND T.Name LIKE '[A-Z]%'
UNION ALL
select s.name as SchemaName, views.name as TableName
from sys.views
inner join sys.schemas s
    on Views.schema_id = s.schema_id";

        private static string strGetTableFields =
            @"SELECT '[' + LTRIM(RTRIM(Column_name)) + '],' FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = '{0}' AND TABLE_SCHEMA = '{1}' AND DATA_TYPE NOT IN ({2}) FOR XML PATH('') ";
               //"SELECT Name + ',' FROM sys.columns where Object_ID = Object_ID('{0}') FOR XML Path('')";

        private static string strSelectTop100 = "SELECT TOP 100 {0} FROM {1}";
        private static string strNOLOCK = " WITH (NOLOCK)";

        private bool inReloadMode = false;

        public ExploreTablesInDB()
        {
            InitializeComponent();

            //dataGridView1.Rows.Clear();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        }

        private void ExploreTablesInDB_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'sampleConnectionDataSet.Connections' table. You can move, or remove it, as needed.
            //this.connectionsTableAdapter.Fill(this.sampleConnectionDataSet.Connections);

            this.LoadDataFromLocalDB();
            resetCombobox();
        }

        private void LoadDataFromLocalDB()
        {
            try
            {
                using (SQLiteConnection oSqliteConn = new SQLiteConnection(Properties.Settings.Default.sqliteconn))
                {
                    SQLiteCommand oSqliteComm = new SQLiteCommand("Select name, connection from connections", oSqliteConn);
                    SQLiteDataAdapter oSqliteAdapter = new SQLiteDataAdapter(oSqliteComm);
                    oSqliteAdapter.Fill(this.sampleConnectionDataSet.Connections);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "falied load local Database!");
            }
        }

        private void SetupDataGridView(DataTable oData)
        {
            dataGridView1.DataSource = "";
            dataGridView1.DataSource = oData;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Items.Count == 0 || comboBox1.Text == string.Empty || inReloadMode == true)
                return;

            msgwindow oMsg;
            oMsg = new msgwindow("Database operating", "program is retriving table/views for the connection...");

            try
            {
                this.strConnection = comboBox1.SelectedValue.ToString().Trim();
                this.strCommand = strExploreTables;
               
                oMsg.Show();
                oMsg.Refresh();

                QueryData qd = new QueryData(strConnection, strCommand);
                DataTable dt = qd.ExecuteDataSet();

                SetupDataGridView(dt);
            }
            catch (Exception ex)
            {
                if (oMsg != null) oMsg.Close();
                MessageBox.Show(ex.Message, "failed to Connect to the Data Source!");
            }
            finally
            {
                if (oMsg != null) oMsg.Close();
            }

        }



        private void button1_Click(object sender, EventArgs e)
        {
            SelectForm sf = new SelectForm();
            sf.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tabControl1.Controls.Clear();

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            UpdateSource(0);
        }

        private void UpdateSource(int ntype)
        {
            string cCommand = string.Empty;


            DataConnectionDialog odialog = new DataConnectionDialog();
            odialog.DataSources.Add(DataSource.SqlDataSource);
            odialog.SelectedDataProvider = DataProvider.SqlDataProvider;

            if (ntype == 1 && comboBox1.SelectedValue.ToString().Trim().IndexOf("Data Source=")>=0)
                odialog.ConnectionString = comboBox1.SelectedValue.ToString().Trim();

            if (DataConnectionDialog.Show(odialog, this) == DialogResult.OK)
            {
                string cString = odialog.ConnectionString;
                SqlConnectionStringBuilder oBuilder = new SqlConnectionStringBuilder(cString);

                string cDataSource = oBuilder.DataSource;
                string cDBName = oBuilder.InitialCatalog;
                string cName;

                // get connection string
                switch (ntype)
                {
                    case 0: //add
                        cName = cDataSource.Replace(' ', '_') + '_' + cDBName.Replace(' ', '_');
                        string cResult = Microsoft.VisualBasic.Interaction.InputBox("Enter a name to save the connectionstring", "Save", cName, 0, 0);

                        if (string.Empty != cResult)
                        {
                            cCommand = string.Format("insert into connections (name, connection) values('{0}', '{1}')",
                                cResult,
                                cString);
                        }
                        else return;
                        break;
                    case 1: //update
                        cName = comboBox1.Text.Trim();
                        cCommand = string.Format("Update connections set connection = '{0}' where name = '{1}'", cString,cName);
                        break;
                    default:
                        return;
                }

                bool lSucc = true;

                // Save data
                try
                {
                    using (SQLiteConnection oSqliteConn = new SQLiteConnection(Properties.Settings.Default.sqliteconn))
                    {
                        SQLiteCommand oSqliteComm = new SQLiteCommand(cCommand, oSqliteConn);
                        oSqliteConn.Open();
                        oSqliteComm.ExecuteNonQuery();

                     }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "failed in upgrade to the local database conn.db!");
                    lSucc = false;
                }

                if (!lSucc) return;

                // update UI
                try
                {
                    //CleanUpUI();
                    //sampleConnectionDataSet.Clear();

                    // set this flag to avoid combobox's interactivechange event happen
                    inReloadMode = true;

                    sampleConnectionDataSet.Connections.Rows.Clear();

                    LoadDataFromLocalDB();
                    inReloadMode = false;

                    resetCombobox();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "failed in reload data or refresh UI"); ;
                }

            }
            else return;

        }

        private void resetCombobox()
        {
            comboBox1.SelectedIndex = 0;
            comboBox1.Text = "";
        }

        private void CleanUpUI()
        {
            //Combobox
            //comboBox1.DataSource = "";
            //comboBox1.Items.Clear();

            //DataGridView
            dataGridView1.DataSource = "";
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
        }
        
        private string GetTableFields(string tablename, string schema)
        {
            string cResult = string.Empty;
            string cSql = string.Format(strGetTableFields, tablename, schema, unSupportTypes);
            string cConn = comboBox1.SelectedValue.ToString().Trim();

            try
            {
                using (SqlConnection oConn = new SqlConnection(cConn))
                {
                    oConn.Open();
                    SqlCommand oComm = new SqlCommand(cSql, oConn);

                    // if result string longer than 2033 characters, the XML result will be return as multiply lines
                    //cResult = (string)oComm.ExecuteScalar();
                    System.Xml.XmlReader reader = oComm.ExecuteXmlReader();
                    reader.MoveToContent();
                    //cResult = reader.ReadOuterXml();
                    cResult = reader.Value;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "failed get table columns...", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            return cResult;

        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
                return;

            string cColumns;
            strTable = (dataGridView1.SelectedRows[0]).Cells[1].Value.ToString();
            strschema = (dataGridView1.SelectedRows[0]).Cells[0].Value.ToString() == string.Empty
                ?"dbo": (dataGridView1.SelectedRows[0]).Cells[0].Value.ToString();
            cColumns = GetTableFields(strTable, strschema);
            cColumns = cColumns == string.Empty ? "*" : cColumns.Remove(cColumns.Length - 1);
            
            strCommand = string.Format(strSelectTop100, cColumns, strschema + "." +  strTable) + strNOLOCK;

            TabPage tp = new TabPage();
 
            myTabs oTab = new myTabs(strTable, strCommand, comboBox1.SelectedValue.ToString().Trim());

            tp.Controls.Add(oTab);
            tp.Text = strTable;

            tabControl1.Controls.Add(tp);
            tabControl1.SelectTab(tabControl1.TabCount - 1);
 
            tp.Size = new System.Drawing.Size(tabControl1.Width - 5, tabControl1.Height - 5);
            oTab.Dock = DockStyle.Fill;

            oTab.ReleaseMe +=  closetab;
            
        }

        public void closetab(myTabs oTab)
        {
            TabPage oPage = (TabPage)oTab.Parent;
            tabControl1.Controls.Remove(oPage);

        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            UpdateSource(1);
        }

        private void ExploreTablesInDB_FormClosed(object sender, FormClosedEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}
