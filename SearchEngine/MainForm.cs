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
using 搜索引擎自动营销助手V1._0.UI;

namespace 搜索引擎自动营销助手V1._0
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void 百度知道ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*Form_百度问答 form_百度问答 = new Form_百度问答();
            form_百度问答.MdiParent = this;
            form_百度问答.Show();*/
        }

        private void 问答库导入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_问答库维护 form_问答库维护 = new Form_问答库维护();
            form_问答库维护.MdiParent = this;
            form_问答库维护.Show();
        }

        private void 百度账号维护ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_账号维护 form_账号维护 = new Form_账号维护();
            form_账号维护.MdiParent = this;
            form_账号维护.Show();
        }

        private void 代理配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_代理配置 form_代理配置 = new Form_代理配置();
            form_代理配置.MdiParent = this;
            form_代理配置.Show();
        }

        private void 若快打码ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Form_打码平台配置 form_打码平台配置 = new Form_打码平台配置();
            form_打码平台配置.MdiParent = this;
            form_打码平台配置.Show();

        }

        private void 搜狗问问ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_搜狗问问 form_搜狗问问 = new Form_搜狗问问();
            form_搜狗问问.MdiParent = this;
            form_搜狗问问.Show();
        }

        private void 搜索ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_360问答 form_360问答 = new Form_360问答();
            form_360问答.MdiParent = this;
            form_360问答.Show();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            /*WmiHelper hardwareInfo = new WmiHelper();
            string hardDiskID = hardwareInfo.GetHardDiskID();
            string cpuID = hardwareInfo.GetCpuID();
            string mac= hardwareInfo.GetMacAddress();*/

            if (File.Exists("reg.dll") == true)
            {
                string reg = File.ReadAllText("reg.dll");
                EncryptHelper md5 = new EncryptHelper();
                string comStr = md5.MD5Encrypt(md5.MD5Encrypt(Utils.GetLocalCode()) + "y987b789");
                if (reg == comStr)
                {
                    return;
                }
            }
            Form_注册 form_注册 = new Form_注册();
            form_注册.MdiParent = this;
            form_注册.Show();
        }
    }
}
