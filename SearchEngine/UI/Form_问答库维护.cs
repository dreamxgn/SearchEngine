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
using 搜索引擎自动营销助手V1._0.Model;

namespace 搜索引擎自动营销助手V1._0.UI
{
    public partial class Form_问答库维护 : Form
    {
        public Form_问答库维护()
        {
            InitializeComponent();
        }

        private void Form_问答库维护_Load(object sender, EventArgs e)
        {
            //载入问答批次
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.loadbatch));
        }

        /// <summary>
        /// 载入问答批次
        /// </summary>
        private void loadbatch(Object obj)
        {
            DataSet ds= SQLiteHelper.ExecuteDataSet("select qId,batchName from Question group by qId,batchName", new Object[] { });
            if (ds.Tables[0].Rows.Count <= 0)
            {
                return;
            }

            if (this.comboBox1_所属批次.InvokeRequired == true)
            {
                this.comboBox1_所属批次.Invoke(new Action(()=> {
                    this.comboBox1_所属批次.Items.Clear();
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        DataRow row = ds.Tables[0].Rows[i];
                        this.comboBox1_所属批次.Items.Add(new QuestionBatchModel() { BatchID = row["qId"].ToString(), BatchName = row["batchName"].ToString() });
                    }
                }),null);
            }

            if (this.comboBox2_查询批次.InvokeRequired == true)
            {
                this.comboBox2_查询批次.Invoke(new Action(() => {
                    this.comboBox2_查询批次.Items.Clear();
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        DataRow row = ds.Tables[0].Rows[i];
                        this.comboBox2_查询批次.Items.Add(new QuestionBatchModel() { BatchID = row["qId"].ToString(), BatchName = row["batchName"].ToString() });
                    }
                }), null);
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBox1_导入文件路径.Text = this.openFileDialog1.FileName.Trim();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.textBox1_导入文件路径.Text == "")
            {
                MessageBox.Show("选择导入问答库文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (this.comboBox1_所属批次.SelectedItem == null && this.comboBox1_所属批次.Text=="")
            {
                MessageBox.Show("选择或输入所属批次", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var param = new { BatchName = this.comboBox1_所属批次.Text.Trim(), BatchID = this.comboBox1_所属批次.SelectedItem != null ? ((QuestionBatchModel)this.comboBox1_所属批次.SelectedItem).BatchID : "", FilePath = this.openFileDialog1.FileName };

            ThreadPool.QueueUserWorkItem(new WaitCallback((object obj) => {
                var p = (dynamic)obj;
                string[] data = File.ReadAllLines(p.FilePath);

                string batchid = p.BatchID == "" ? Utils.GetGUID() : p.BatchID;
                string batchName = p.BatchName;
                int count = 0;//成功导入数据数量
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] != null && data[i] != "")
                    {
                        try
                        {
                            string[] row = data[i].Split(new String[] { "----" }, StringSplitOptions.RemoveEmptyEntries);
                            SQLiteHelper.ExecuteNonQuery(String.Format(
                                "insert into Question(Id,qId,batchName,title,content,Answer_one,Answer_two,Answer_three)values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')",
                                Utils.GetGUID(),batchid,batchName,row[0],row[1], row[2], "", ""
                                ), null);
                            count++;
                        }
                        catch (Exception ex) { }
                    }
                }
                MessageBox.Show(String.Format("成功导入: {0} 条数据", count), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }), param);
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.comboBox2_查询批次.SelectedItem == null)
            {
                return;
            }

            ThreadPool.QueueUserWorkItem(new WaitCallback((Object obj) => {
                QuestionBatchModel qm = (QuestionBatchModel)obj;
                DataSet ds= SQLiteHelper.ExecuteDataSet(String.Format("select qId as 批次号,batchName as 批次名,title as 标题,content as 内容,Answer_one as 回答1,Answer_two as 回答2,Answer_three as 回答3 from Question where qId='{0}'", qm.BatchID)
                    , null);
                if (this.dataGridView1.InvokeRequired == true)
                {
                    this.dataGridView1.Invoke(new Action(()=> {
                        this.dataGridView1.DataSource = ds.Tables[0];
                    }));
                }

            }),this.comboBox2_查询批次.SelectedItem);

        }
    }
}
