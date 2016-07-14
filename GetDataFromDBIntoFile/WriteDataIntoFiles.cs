using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace GetDataFromDBIntoFile
{
    public partial class WriteDataIntoFiles : Form
    {
        private string strConnection;
        private string strSqlCommand;
        private string strFile;
        public const string FilePath = "d:/Analysis/";
        public const string conDefaultExtension = "_0.txt";
        public WriteDataIntoFiles()
        {
            InitializeComponent();
        }
        private void GetData()
        {
            this.strConnection = comboBox1.SelectedValue.ToString().Trim();
            this.strSqlCommand = textBox1.Text.ToUpper().Trim();
            this.strFile = textBox2.Text.ToUpper().Trim();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            GetData();
            ValidateData();
        }

        public void ValidateData()
        {
            if (String.IsNullOrWhiteSpace(this.strConnection))
            {
                MessageBox.Show("Please Select The Connection!");
                return;
            }
            if (String.IsNullOrWhiteSpace(this.strSqlCommand))
            {
                MessageBox.Show("Please Type In The SQL Command!");
                return;
            }
            if (String.IsNullOrWhiteSpace(this.strFile))
            {
                MessageBox.Show("Please Type In The File Name!");
                return;
            }

            CheckSQL();
            ValidateFile();
            MessageBox.Show("Input Is Fine");
        }

        private void ValidateFile()
        {
            DirectoryInfo p = new DirectoryInfo(FilePath);
            FileInfo[] fs = p.GetFiles();
            foreach (FileInfo f in fs)
            {
                if (f.FullName.Contains(strFile + "_0.txt"))
                {
                    MessageBox.Show("The file already exists, please change it or will DELETE it during run！");
                    break;
                }
            }
        }

        private void CheckFile()
        {
            DirectoryInfo p = new DirectoryInfo(FilePath);
            FileInfo[] fs = p.GetFiles();
            foreach (FileInfo f in fs)
            {
                if (f.FullName.Contains(strFile))
                {
                    f.Delete();
                }
            }

            if (strFile.Contains("."))
            {
                this.strFile = FilePath + this.strFile;
            }
            else
            {
                this.strFile = FilePath + this.strFile + conDefaultExtension;
            }           
            File.Create(strFile).Dispose();
        }

        private void CheckSQL()
        {
            if (!this.strSqlCommand.StartsWith("SELECT"))
            {
                MessageBox.Show("Please Type In Valid SQL Command!");
                return;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetData();
            CheckFile();
            this.button1.IsAccessible = false;
            this.button2.IsAccessible = false;
            this.button3.IsAccessible = false;
            QueryData qd = new QueryData(this.strConnection, this.strSqlCommand, this.strFile);
            qd.GenerateFile();
            MessageBox.Show("File Updated With Query Result! Please Check.");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'sampleConnectionDataSet.Connections' table. You can move, or remove it, as needed.
            this.connectionsTableAdapter.Fill(this.sampleConnectionDataSet.Connections);

        }
    }
}
