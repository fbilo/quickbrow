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
        public msgwindow(string ctitle, string cMessage)
        {
            InitializeComponent();
            this.Text = ctitle;
            label1.Text = cMessage;
            label1.Visible = true;
        }
    }
}
