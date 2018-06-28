using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BSky.Lifetime.Interfaces;
using BSky.Lifetime;
using BSky.ConfService.Intf.Interfaces;

namespace BSky.Controls
{
    partial class dialogHelp : Form
    {
        IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();
        public dialogHelp()
        {
            InitializeComponent();
        }

      

     

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            if (checkBox1.Checked == true)
            {
                confService.ModifyConfig("helppopup", "false");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

       
    }
}
