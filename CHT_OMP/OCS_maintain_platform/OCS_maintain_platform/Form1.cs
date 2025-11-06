using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OCS_Parser_Minoss_GUI;
using PuppeteerSharp;
using System.Threading;
using System.Diagnostics;

namespace OCS_maintain_platform
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void parseMinossLog_Click(object sender, EventArgs e)
        {
            ParseResult frm = new ParseResult();
            frm.Show(this);
        }

    }

}