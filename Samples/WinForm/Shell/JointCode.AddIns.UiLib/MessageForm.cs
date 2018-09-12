using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace JointCode.AddIns.UiLib
{
    public partial class MessageForm : Form
    {
        public MessageForm(string message, string title)
        {
            InitializeComponent();
            this.Text = title;
            this.label1.Text = message;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
