using OpenQA.Selenium.PhantomJS;
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
    public partial class Form_百度问答 : Form
    {

        List<UserModel> users = new List<UserModel>(); //360账号
        ProxyService proxy = new ProxyService();//代理
        Dictionary<string, string> dama = new Dictionary<string, string>();//打码
        List<QuestionInfoModel> questionData = new List<QuestionInfoModel>(); //问题队列    
        bool stopflag = true;//停止标志    
        BaiduProvider bdProvider;


        public Form_百度问答()
        {
            InitializeComponent();
        }

        private void Form_百度问答_Load(object sender, EventArgs e)
        {
            //初始化
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.initAutoQuestion), null);
            this.bdProvider = new BaiduProvider(new BaiduProvider.Log(this.log), new BaiduProvider.ShowImg(this.showCodeImg));
        }


        #region 百度 提问初始化
        /// <summary>
        /// 百度 提问初始化
        /// </summary>
        /// <param name="obj"></param>
        private void initAutoQuestion(Object obj)
        {
            //获取百度账号
            if (Utils.GetUserList(0, users) == false)
            {
                MessageBox.Show(String.Format("百度账号初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //获取代理
            string ret = Utils.GetProxyRemote();
            if (ret != null)
            {
                proxy.RemoteUrl = ret;
            }
            else
            {
                MessageBox.Show(String.Format("IP代理接口初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //获取打码信息
            if (Utils.GetDama(dama) == false)
            {
                MessageBox.Show(String.Format("若快打码接口初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //问答库
            try
            {
                DataSet ds = SQLiteHelper.ExecuteDataSet("select qId,batchName from Question group by qId,batchName", new Object[] { });
                this.comboBox1_自动问答_问答库选择.Invoke(new Action(() =>
                {
                    this.comboBox1_自动问答_问答库选择.Items.Clear();
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        this.comboBox1_自动问答_问答库选择.Items.Add(new QuestionBatchModel() { BatchID = ds.Tables[0].Rows[i]["qId"].ToString(), BatchName = ds.Tables[0].Rows[i]["batchName"].ToString() });
                    }
                }));
            }
            catch (Exception ex) { MessageBox.Show(String.Format("问答库初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); }

            //可操作账号数量
            this.textBox2_自动问答_可操作账号数量.Invoke(new Action(() =>
            {
                this.textBox2_自动问答_可操作账号数量.Text = this.users.Count.ToString();
            }));
        }
        #endregion

        #region 百度 回答初始化
        /// <summary>
        /// 百度 回答初始化
        /// </summary>
        /// <param name="obj"></param>
        private void initAutoAnswer(Object obj)
        {
            //获取360账号
            if (Utils.GetUserList(0, users) == false)
            {
                MessageBox.Show(String.Format("百度账号初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //获取代理
            string ret = Utils.GetProxyRemote();
            if (ret != null)
            {
                proxy.RemoteUrl = ret;
            }
            else
            {
                MessageBox.Show(String.Format("IP代理接口初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //获取打码信息
            if (Utils.GetDama(dama) == false)
            {
                MessageBox.Show(String.Format("若快打码接口初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //可操作账号数量
            this.textBox2_自动问答_可操作账号数量.Invoke(new Action(() =>
            {
                this.textBox2_自动问答_可操作账号数量.Text = this.users.Count.ToString();
            }));

            //提问详情
            //回答数据
            DataSet markds = SQLiteHelper.ExecuteDataSet(@"select b.loginname as '提问帐号',c.title as '问题标题',c.batchName as '所属批次',qurl as '提问地址' from QuestionData a 
left join Users b on a.uid=b.Id
left join Question c on a.qid=c.Id
where a.category=0 and a.Id not in(select qid from AnswerData where category=0)", null);
            this.dataGridView1_回答_提问详情.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1_回答_提问详情.Invoke(new Action(() =>
            {
                this.dataGridView1_回答_提问详情.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                this.dataGridView1_回答_提问详情.DataSource = markds.Tables[0];
            }));

        }
        #endregion

        #region 百度 自动点赞初始化
        /// <summary>
        /// 百度 自动点赞
        /// </summary>
        /// <param name="obj"></param>
        private void initAutoMark(Object obj)
        {
            //获取百度账号
            if (Utils.GetUserList(0, users) == false)
            {
                MessageBox.Show(String.Format("百度账号初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //获取代理
            string ret = Utils.GetProxyRemote();
            if (ret != null)
            {
                proxy.RemoteUrl = ret;
            }
            else
            {
                MessageBox.Show(String.Format("IP代理接口初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //获取打码信息
            if (Utils.GetDama(dama) == false)
            {
                MessageBox.Show(String.Format("若快打码接口初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //可操作账号数量
            this.textBox7_点赞_可操作帐号数量.Invoke(new Action(() =>
            {
                this.textBox7_点赞_可操作帐号数量.Text = this.users.Count.ToString();
            }));

            //回答数据
            DataSet markds = SQLiteHelper.ExecuteDataSet(@"select 
b.loginname as '回答帐号',
a.aid as '回答ID',
c.qurl as '提问地址',
d.title as '提问标题',
d.batchName as '所属问题库批次'
from AnswerData a 
left join Users b on a.uid=b.Id
left join QuestionData c on a.qid=c.Id
left join Question d on c.qid=d.Id
where a.ismark is null and a.category=0", null);
            this.dataGridView1_点赞_回答问题详情.Invoke(new Action(() =>
            {
                this.dataGridView1_点赞_回答问题详情.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                this.dataGridView1_点赞_回答问题详情.DataSource = markds.Tables[0];
            }));
        }
        #endregion

        #region 百度 选择答案初始化
        /// <summary>
        /// 百度 选择答案初始化
        /// </summary>
        /// <param name="obj"></param>
        private void initAutoGoodAnswer(Object obj)
        {
            //获取搜狗账号
            if (Utils.GetUserList(0, users) == false)
            {
                MessageBox.Show(String.Format("360问答账号初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //获取代理
            string ret = Utils.GetProxyRemote();
            if (ret != null)
            {
                proxy.RemoteUrl = ret;
            }
            else
            {
                MessageBox.Show(String.Format("IP代理接口初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //获取打码信息
            if (Utils.GetDama(dama) == false)
            {
                MessageBox.Show(String.Format("若快打码接口初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //可操作账号数量
            this.textBox1_最佳答案_帐号数量.Invoke(new Action(() =>
            {
                this.textBox1_最佳答案_帐号数量.Text = this.users.Count.ToString();
            }));

            //提问详情
            DataSet markds = SQLiteHelper.ExecuteDataSet(@"select c.loginname as 提问帐号,b.title as 标题,b.content as 内容,count(d.aid) as 回答数量,a.qurl as 提问地址 from QuestionData a 
left join Question b on a.qid=b.Id
left join Users c on a.uid=c.Id
left join AnswerData d on a.Id=d.qid
where goodaid='' and d.aid is not null and a.category=0
GROUP BY loginname,title,content,qurlId", null);
            this.dataGridView2_最佳答案_提问详情.Invoke(new Action(() =>
            {
                this.dataGridView2_最佳答案_提问详情.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                this.dataGridView2_最佳答案_提问详情.DataSource = markds.Tables[0];
            }));
        }
        #endregion

        private void comboBox1_自动问答_问答库选择_SelectedIndexChanged(object sender, EventArgs e)
        {
            QuestionBatchModel qb = (QuestionBatchModel)this.comboBox1_自动问答_问答库选择.SelectedItem;
            ThreadPool.QueueUserWorkItem(new WaitCallback((Object obj) =>
            {
                DataSet ds = SQLiteHelper.ExecuteDataSet(String.Format("select Id as 编码, qId as 批次号,batchName as 批次名,title as 标题,content as 内容,Answer_one as 回答1,Answer_two as 回答2,Answer_three as 回答3 from Question where qId='{0}'", qb.BatchID)
                    , null);
                if (this.dataGridView3_自动问答_问答库详情.InvokeRequired == true)
                {
                    this.dataGridView3_自动问答_问答库详情.Invoke(new Action(() =>
                    {
                        this.dataGridView3_自动问答_问答库详情.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        this.dataGridView3_自动问答_问答库详情.DataSource = ds.Tables[0];
                        this.questionData.Clear();
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            DataRow row = ds.Tables[0].Rows[i];
                            this.questionData.Add(new QuestionInfoModel()
                            {
                                Id = row["编码"].ToString(),
                                title = row["标题"].ToString(),
                                Answer_one = row["回答1"].ToString(),
                                Answer_three = row["回答3"].ToString(),
                                Answer_two = row["回答2"].ToString(),
                                batchName = row["批次名"].ToString(),
                                content = row["内容"].ToString(),
                                qId = row["批次号"].ToString()
                            });
                        }
                    }));
                }
            }), null);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl1.SelectedTab.Text.Contains("提问"))
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.initAutoQuestion), null);
            }

            if (this.tabControl1.SelectedTab.Text.Contains("回答"))
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.initAutoAnswer), null);
                return;
            }

            if (this.tabControl1.SelectedTab.Text.Contains("点赞"))
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.initAutoMark), null);
                return;
            }

            if (this.tabControl1.SelectedTab.Text.Contains("选择最佳答案"))
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.initAutoGoodAnswer), null);
                return;
            }
        }

        private void log(string msg)
        {
            this.textBox1_自动问答_日志窗口.Invoke(new Action(() =>
            {
                this.textBox1_自动问答_日志窗口.AppendText(msg + "\r\n");
            }));
        }

        private void showCodeImg(byte[] buff)
        {
            this.pictureBox1_自动问答验证码.Invoke(new Action(() =>
            {
                if (buff == null)
                {
                    return;
                }

                MemoryStream ms = new MemoryStream(buff);
                this.pictureBox1_自动问答验证码.Image = Image.FromStream(ms);
                ms.Dispose();
                ms = null;

            }));
        }

        private void button1_自动问答_启动_Click(object sender, EventArgs e)
        {
            if (users.Count == 0)
            {
                MessageBox.Show(String.Format("没有可操作的账号"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (this.comboBox1_自动问答_问答库选择.SelectedItem == null)
            {
                MessageBox.Show(String.Format("没有可操作的问答库"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.stopflag = false;

            ThreadPool.QueueUserWorkItem(new WaitCallback((Object obj) =>
            {
                this.log("自动提问已启动...");

                bool localip = false;//使用本机IP

                bool isrepeat = false;//可重复提交
                bool allAnswer = true; //答案全部回答
                PhantomJSDriver js = null;

                //使用本机IP
                this.checkBox1_使用本机IP.Invoke(new Action(() => { localip = this.checkBox1_使用本机IP.Checked; }));

                //可重复提交
                this.checkBox1_可重复提交.Invoke(new Action(() => { isrepeat = this.checkBox1_可重复提交.Checked; }));

                //答案全部回答
                this.checkBox1_自动问答_全部回答.Invoke(new Action(() => { allAnswer = this.checkBox1_自动问答_全部回答.Checked; }));

                #region 360 提问/回答
                int qindex = 0;
                for (int i = 0; i < users.Count; i++)
                {
                    try
                    {
                        //结束线程
                        if (stopflag == true)
                        {
                            Utils.KillPhantomjs();
                            this.log("操作结束");
                            return;
                        }

                        //问题库是否使用完毕
                        /*if (qindex >= questionData.Count)
                        {
                            Utils.KillPhantomjs();
                            this.log("操作结束，原因: 问题库已经操作完毕");
                            return;
                        }


                        //该用户是否重复提交问题
                        if (Utils.ExistsQuestionData(users[i], questionData[qindex]) == true)
                        {
                            if (isrepeat == false)
                            {
                                this.log(String.Format("账号: {0} 提交问题失败，原因: 该账号已提交过这个提问", users[i].LoginName));
                                continue;
                            }
                        }*/

                        js = Utils.GetPhantomJsDriver(localip, proxy);
                        if (js == null)
                        {
                            this.log(String.Format("系统提示: 实例化Web失败,重试中..."));
                            Thread.Sleep(1000);
                            continue;
                        }


                        this.bdProvider.Login(js, users[i], dama);
                        continue;
                        //登陆失败
                        if (this.users[i].isLogin == false)
                        {
                            continue;
                        }

                        //提问
                        bool outerr = false;
                        AskModel ask = this.bdProvider.SendQuestion(js, users[i], questionData[qindex], dama, out outerr);

                        //提问成功，进入下一个问题
                        if (ask != null)
                        {
                            qindex++;
                            continue;
                        }

                        //提问失败，但根据outerr逻辑值，跳过该问题
                        if (outerr == false)
                        {
                            qindex++;
                            continue;
                        }
                    }
                    catch (Exception ex) { this.log(String.Format("账号: {0} 操作失败 网络超时", users[i].LoginName)); }

                }
                #endregion

                Utils.KillPhantomjs();
                this.log("操作结束");
                return;
            }), null);
        }

        private void button1_自动问答_停止问答_Click(object sender, EventArgs e)
        {
            this.stopflag = true;
        }

    }
}
