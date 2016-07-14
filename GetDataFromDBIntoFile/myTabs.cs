using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace GetDataFromDBIntoFile
{
    public partial class myTabs : UserControl
    {
        public string Tablename {
            get { return table; }
            set { table = value; }
        }

        public string SelectCmd
        {
            get { return selectcmd; }
            set { selectcmd = value; }
        }

       

        private string table;
        private string selectcmd;
        private SqlConnection oConn;


        public myTabs(string ctable, string cSelect, SqlConnection Conn)
        {
            InitializeComponent();
            this.selectcmd = cSelect;
            this.table = ctable;
            this.oConn = Conn;
        }

        private void myTabs_Load(object sender, EventArgs e)
        {
            textBox1.Text = selectcmd;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.selectcmd = textBox1.Text;
        }
    }
}
