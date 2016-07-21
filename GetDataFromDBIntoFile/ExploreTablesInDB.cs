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

        private static string strExploreTables = "SELECT s.Name as SchemaName,t.Name as TableName FROM SYS.TABLES t JOIN sys.Schemas s on t.Schema_id = s.Schema_ID WHERE T.TYPE = 'U' AND T.Name LIKE '[A-Z]%'";
        private static string strSelectTop100 = "SELECT TOP 100 * FROM ";
        private static string strNOLOCK = " WITH (NOLOCK)";

        public ExploreTablesInDB()
        {
            InitializeComponent();
        }

        private void ExploreTablesInDB_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'sampleConnectionDataSet.Connections' table. You can move, or remove it, as needed.
            //this.connectionsTableAdapter.Fill(this.sampleConnectionDataSet.Connections);

            try
            {
                SQLiteConnection oSqliteConn = new SQLiteConnection(Properties.Settings.Default.sqliteconn);
                SQLiteCommand oSqliteComm = new SQLiteCommand("Select name, connection from connections", oSqliteConn);
                SQLiteDataAdapter oSqliteAdapter = new SQLiteDataAdapter(oSqliteComm);
                oSqliteAdapter.Fill(this.sampleConnectionDataSet.Connections);
            }
            catch (Exception )
            {
                throw;

            }


        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Items.Count == 0) return;

            try
            {
                this.strConnection = comboBox1.SelectedValue.ToString().Trim();
                this.strCommand = strExploreTables;
                QueryData qd = new QueryData(strConnection, strCommand);
                DataTable dt = qd.ExecuteDataSet();

                dataGridView1.Rows.Clear();
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;


                dataGridView1.DataSource = dt;
            }
            catch (Exception)
            {

                throw;
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

        private void button3_Click(object sender, EventArgs e)
        {
            this.strConnection = comboBox1.SelectedValue.ToString().Trim();

            strCommand = this.tabControl1.SelectedTab.Controls[0].Text.ToString();
            QueryData qd = new QueryData(strConnection, strCommand);
            DataTable dt = qd.ExecuteDataSet();
            //this.dataGridView2.DataSource = dt;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tabControl1.Controls.Clear();

            //foreach (Control c in tabControl1.Controls)
            //{
            //    this.tabControl1.Controls.Remove(c);
            //}
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            DataConnectionDialog odialog = new DataConnectionDialog();
            odialog.DataSources.Add(DataSource.SqlDataSource);
            odialog.SelectedDataProvider = DataProvider.SqlDataProvider;

            if (DataConnectionDialog.Show(odialog, this) == DialogResult.OK)
            {
                string cString = odialog.ConnectionString;
                SqlConnectionStringBuilder oBuilder = new SqlConnectionStringBuilder(cString);

                string cDataSource = oBuilder.DataSource;
                string cDBName = oBuilder.InitialCatalog;

                string cName = cDataSource.Replace(' ', '_') + '_' + cDBName.Replace(' ', '_');

                string cResult = Microsoft.VisualBasic.Interaction.InputBox("Enter a name to save the connectionstring", "Save", cName, 0, 0);


                if (string.Empty != cResult)
                {
                    try
                    {
                        using (SQLiteConnection oSqliteConn = new SQLiteConnection(Properties.Settings.Default.sqliteconn))
                        {
                            SQLiteCommand oSqliteComm = new SQLiteCommand(
                                string.Format("insert into connections (name, connection) values('{0}', '{1}')", cResult, cString),
                                oSqliteConn);
                            oSqliteConn.Open();
                            oSqliteComm.ExecuteNonQuery();

                            this.sampleConnectionDataSet.Clear();

                            oSqliteComm.CommandText = "Select name, connection from connections";
                            SQLiteDataAdapter oSqliteAdapter = new SQLiteDataAdapter(oSqliteComm);
                            oSqliteAdapter.Fill(this.sampleConnectionDataSet.Connections);
                        }
                    }
                    catch (Exception)
                    {
                        throw;

                    }
                }

            }

        }

        private void UpdateSource(int ntype)
        {
            string cCommand = string.Empty;


            DataConnectionDialog odialog = new DataConnectionDialog();
            odialog.DataSources.Add(DataSource.SqlDataSource);
            odialog.SelectedDataProvider = DataProvider.SqlDataProvider;

            switch (ntype)
            {
                case 0: //add
                    cCommand = "insert into connections (name, connection) values('{0}', '{1}')";
                    break;
                case 1: //update
                    cCommand = "Update connections set name = '{0}, connection = '{1}' where name = '{2}'";
                    odialog.ConnectionString = comboBox1.SelectedValue.ToString().Trim();
                    break;
                default:
                    return;
            }

            if (DataConnectionDialog.Show(odialog, this) == DialogResult.OK)
            {
                // Save data


                // update UI
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
                return;

            strTable = (dataGridView1.SelectedRows[0]).Cells[1].Value.ToString();
            strCommand = strSelectTop100 + strTable + strNOLOCK;

            TabPage tp = new TabPage();
            //TextBox tb = new TextBox();

            //tp.Text = strTable;
            //tp.Size = new System.Drawing.Size(1000, 400);
            //tp.TabIndex = 1;
            //tp.Controls.Add(tb);

            //tb.Text = strCommand;
            //tb.Size = new System.Drawing.Size(1000, 400);

            myTabs oTab = new myTabs(strTable, strCommand, comboBox1.SelectedValue.ToString().Trim());

            tp.Controls.Add(oTab);
            tp.Text = strTable;

            tabControl1.Controls.Add(tp);
            tabControl1.SelectTab(tabControl1.TabCount - 1);
            //tabControl1.Dock = DockStyle.Fill;
            tp.Size = new System.Drawing.Size(tabControl1.Width - 5, tabControl1.Height - 5);
            oTab.Dock = DockStyle.Fill;

            oTab.ReleaseMe +=  closetab;
            
        }

        public void closetab(myTabs oTab)
        {
            TabPage oPage = (TabPage)oTab.Parent;

            tabControl1.Controls.Remove(oPage);

        }
    }
}
