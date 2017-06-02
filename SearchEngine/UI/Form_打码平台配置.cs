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
    public partial class Form_打码平台配置 : Form
    {
        public Form_打码平台配置()
        {
            InitializeComponent();
        }

        private void Form_打码平台配置_Load(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((Object obj) => {
                DataSet ds= SQLiteHelper.ExecuteDataSet("select * from DaMa", null);
                if (ds.Tables[0].Rows.Count != 1)
                {
                    return;
                }
                this.textBox1_接口地址.Invoke(new Action(() => { this.textBox1_接口地址.Text = ds.Tables[0].Rows[0]["url"].ToString(); }));
                this.textBox2_账号.Invoke(new Action(() => { this.textBox2_账号.Text = ds.Tables[0].Rows[0]["loginname"].ToString(); }));
                this.textBox3_密码.Invoke(new Action(() => { this.textBox3_密码.Text = ds.Tables[0].Rows[0]["loginpwd"].ToString(); }));
                this.textBox4_超时时间.Invoke(new Action(() => { this.textBox4_超时时间.Text = ds.Tables[0].Rows[0]["timeout"].ToString(); }));
            }),null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.textBox1_接口地址.Text.Trim() == "" || this.textBox2_账号.Text.Trim()=="" || this.textBox3_密码.Text.Trim()=="" || this.textBox4_超时时间.Text.Trim()=="")
            {
                return;
            }

            dynamic param = new { Url = this.textBox1_接口地址.Text.Trim(), LoginName = this.textBox2_账号.Text.Trim(),
                LoginPwd = this.textBox3_密码.Text.Trim(), TimeOut = this.textBox4_超时时间.Text.Trim() };

            string sql= String.Format("update DaMa set url='{0}',loginname='{1}',loginpwd='{2}',timeout='{3}'", param.Url, param.LoginName, param.LoginPwd, param.TimeOut);
            ThreadPool.QueueUserWorkItem(new WaitCallback((Object obj) => {
                try
                {
                    SQLiteHelper.ExecuteNonQuery(obj.ToString(), null);
                    MessageBox.Show(String.Format("打码平台配置保存成功"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("打码平台配置保存失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }), sql);

        }
    }
}
