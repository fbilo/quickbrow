using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetDataFromDBIntoFile
{
    public partial class msgwindow : Form
    {
        string title;
        string message;

        public msgwindow(string ctitle, string cMessage)
        {
            title = ctitle;
            message = cMessage;
            InitializeComponent();
        }

        private void msgwindow_Load(object sender, EventArgs e)
        {
            this.Text = title;
            label1.Text = message;
            label1.Visible = true;
            label1.Refresh();

            Application.DoEvents();
        }
    }
}
