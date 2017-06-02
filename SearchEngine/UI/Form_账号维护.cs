using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using 搜索引擎自动营销助手V1._0.Classs;

namespace 搜索引擎自动营销助手V1._0.UI
{

    /*
    账号类型(category): 0=百度 1=搜狗 2=360问答
    */

    public partial class Form_账号维护 : Form
    {
        public Form_账号维护()
        {
            InitializeComponent();
        }

        private void Form_百度账号维护_Load(object sender, EventArgs e)
        {
            loadGridView();
        }


        private void loadGridView()
        {
            string sql = "select loginname as 账号,loginpwd as 密码,case when category=0 then '百度' when category=1 then '搜狗' when category=2 then '360问答' else 'Unknow' end as 账号分类  from Users";

            if (this.textBox1_百度账号名模糊查询.Text.Trim() != "")
            {
                sql = sql + " where loginname like'%" + this.textBox1_百度账号名模糊查询.Text.Trim() + "%'";
            }

            if (this.comboBox1_账号分类.Text!=null && this.comboBox1_账号分类.Text.Trim() != "" && this.comboBox1_账号分类.Text.Trim()!="所有")
            {
                if (sql.Contains("where"))
                {
                    sql = sql + " and category=" + this.GetCategoryByName(this.comboBox1_账号分类.Text.Trim());
                }
                else
                {
                    sql = sql + " where category=" + this.GetCategoryByName(this.comboBox1_账号分类.Text.Trim());
                }
            }

            ThreadPool.QueueUserWorkItem(new WaitCallback((Object obj) => {
                string sqlStr = obj.ToString();
                if (this.dataGridView1.InvokeRequired == true)
                {
                    this.dataGridView1.Invoke(new Action(() => {
                        DataSet ds = SQLiteHelper.ExecuteDataSet(sqlStr, null);
                        this.dataGridView1.DataSource = ds.Tables[0];
                    }));
                }
               
            }), sql);

        }

        private string GetCategoryByName(string name)
        {
            /*百度
            搜狗
            360问答*/
            if (name == "百度")
            {
                return "0";
            }
            if (name == "搜狗")
            {
                return "1";
            }
            if (name == "360问答")
            {
                return "2";
            }
            return "unkonw";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            loadGridView();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback((Object obj) => {
                    string[] data = File.ReadAllLines(obj.ToString());
                    int count = 0;
                    for (int i = 0; i < data.Length; i++)
                    {
                        try
                        {
                            string[] row = data[i].Split(new String[] { "----"}, StringSplitOptions.RemoveEmptyEntries);
                            SQLiteHelper.ExecuteNonQuery(String.Format("insert into Users(Id,loginname,loginpwd,category)values('{0}','{1}','{2}','{3}')",Utils.GetGUID(),row[0],row[1],row[2]), null);
                            count++;
                        }
                        catch (Exception ex) { }
                    }
                    MessageBox.Show(String.Format("成功导入: {0} 条数据", count), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }), openFileDialog1.FileName);
            }
        }
    }
}
