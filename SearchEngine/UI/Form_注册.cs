using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utility;
using 搜索引擎自动营销助手V1._0.Classs;

namespace 搜索引擎自动营销助手V1._0.UI
{
    public partial class Form_注册 : Form
    {
        public Form_注册()
        {
            InitializeComponent();
        }

        private void Form_注册_Load(object sender, EventArgs e)
        {
            string devCode = Utils.GetLocalCode();
            EncryptHelper md5 = new EncryptHelper();

            this.textBox1_硬件码.Text = md5.MD5Encrypt(devCode);
        }

        private void button1_注册_Click(object sender, EventArgs e)
        {
            string devCode = Utils.GetLocalCode();
            EncryptHelper md5 = new EncryptHelper();

            string comStr = md5.MD5Encrypt(md5.MD5Encrypt(devCode) + "y987b789");
            if (comStr == this.textBox2_注册码.Text)
            {
                File.WriteAllText("reg.dll", comStr);
                MessageBox.Show("恭喜，产品注册成功", "提示", MessageBoxButtons.OK,MessageBoxIcon.Information);
                this.Close();
                this.Dispose();
            }
            else
            {
                MessageBox.Show("输入的注册错误", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
    }
}
