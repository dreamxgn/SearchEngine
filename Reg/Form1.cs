using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utility;

namespace 注册机
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.textBox1_硬件码.Text.Length <= 10)
            {
                return;
            }

            EncryptHelper md5 = new EncryptHelper();

            this.textBox2_注册码.Text= md5.MD5Encrypt(this.textBox1_硬件码.Text + "y987b789");

        }
    }
}
