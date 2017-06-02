using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using 搜索引擎自动营销助手V1._0.Classs;

namespace 搜索引擎自动营销助手V1._0.UI
{
    public partial class Form_代理配置 : Form
    {
        public Form_代理配置()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.textBox1_代理接口地址.Text.Trim() == "")
            {
                return;
            }

            string sql = "update Proxy set url='" + this.textBox1_代理接口地址.Text.Trim() + "'";


            ThreadPool.QueueUserWorkItem(new WaitCallback((Object obj) => {
                Object ret = SQLiteHelper.ExecuteNonQuery(obj.ToString(), null);
                if (ret == null)
                {
                    MessageBox.Show(String.Format("代理接口配置保存失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (this.textBox1_代理接口地址.InvokeRequired == true)
                {
                    this.textBox1_代理接口地址.Invoke(new Action(() => {
                        MessageBox.Show(String.Format("代理接口配置保存成功"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                }
            }), sql);

        }

        private void Form_代理配置_Load(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((Object obj) => {
                Object ret= SQLiteHelper.ExecuteScalar("select url from Proxy", null);
                if (ret == null)
                {
                    return;
                }

                if (this.textBox1_代理接口地址.InvokeRequired == true)
                {
                    this.textBox1_代理接口地址.Invoke(new Action(() => {
                        this.textBox1_代理接口地址.Text = ret.ToString();
                    }));
                }
            }), null);
        }
    }
}
