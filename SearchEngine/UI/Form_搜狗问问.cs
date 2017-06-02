using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using 搜索引擎自动营销助手V1._0.Classs;
using 搜索引擎自动营销助手V1._0.Model;

namespace 搜索引擎自动营销助手V1._0.UI
{
    public partial class Form_搜狗问问 : Form
    {

        List<UserModel> users = new List<UserModel>(); //搜狗问问用户
        ProxyService proxy = new ProxyService();//代理
        Dictionary<string, string> dama = new Dictionary<string, string>();//打码
        List<QuestionInfoModel> questionData = new List<QuestionInfoModel>(); //问题队列    
        bool stopflag = true;//停止标志    
        SogoProvider sogoProvider;

        public Form_搜狗问问()
        {
            InitializeComponent();
            sogoProvider = new SogoProvider(new SogoProvider.Log(this.log),new SogoProvider.ShowImg(this.showValidCode));
            sogoProvider.EnableDebug();
        }

        private void Form_搜狗问问_Load(object sender, EventArgs e)
        {
            //初始化
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.initAutoQuestion), null);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl1.SelectedTab.Text.Contains("提问"))
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.initAutoQuestion), null);
                return;
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

        private void tabPage1_自动问答_Click(object sender, EventArgs e)
        {

        }

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
                int currIPCount = 0;//当前IP数量
                bool isrepeat = false;//可重复提交
                bool allAnswer = true; //答案全部回答
                string proxyStr = null;//IP代理
                PhantomJSDriver js;

                //使用本机IP
                this.checkBox1_使用本机IP.Invoke(new Action(() => { localip = this.checkBox1_使用本机IP.Checked; }));

                //可重复提交
                this.checkBox1_可重复提交.Invoke(new Action(() => { isrepeat = this.checkBox1_可重复提交.Checked; }));

                //答案全部回答
                this.checkBox1_自动问答_全部回答.Invoke(new Action(() => { allAnswer = this.checkBox1_自动问答_全部回答.Checked; }));

                #region 搜狗问问 提问/回答
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

                        //帐号是否使用完毕
                        if (i >= users.Count)
                        {
                            Utils.KillPhantomjs();
                            this.log("操作结束，原因: 帐号已经使用完毕");
                            return;
                        }


                        //该用户是否重复提交问题
                        if (Utils.ExistsQuestionData(users[i], questionData[i]) == true)
                        {
                            if (isrepeat == false)
                            {
                                this.log(String.Format("账号: {0} 提交问题失败，原因: 该账号已提交过这个提问", users[i].LoginName));
                                continue;
                            }
                        }


                        //登陆
                        js = Utils.GetPhantomJsDriver(localip, proxy);
                        try
                        {
                            this.sogoProvider.Login(js, users[i], dama);
                        }
                        catch (Exception ex)
                        {
                            this.log(String.Format("系统提示: 帐号: {0} 网络超时",users[i].LoginName));
                            continue;
                        }

                        if (users[i].isLogin == false)
                        {
                            continue;
                        }

                        //提问
                        AskModel ask = this.sogoProvider.SendQuestion(js, users[i], questionData[i], dama);

                        

                    }
                    catch (Exception ex) { this.log(String.Format("账号: {0} 操作失败 异常堆栈: {1}", users[i].LoginName, ex.StackTrace)); }

                }
                #endregion

                Utils.KillPhantomjs();
                this.log("操作结束");
                return;
            }), null);
        }


        private void log(string msg)
        {
            this.textBox1_自动问答_日志窗口.Invoke(new Action(() =>
            {
                this.textBox1_自动问答_日志窗口.AppendText(msg + "\r\n");
            }));
        }

        private void showValidCode(byte[] img)
        {
            this.pictureBox1_自动问答_验证码.Invoke(new Action(() =>
            {
                MemoryStream ms = new MemoryStream(img);
                this.pictureBox1_自动问答_验证码.Image = Image.FromStream(ms);
                ms.Close();
                ms.Dispose();
                ms = null;
            }));
        }

        private void button1_自动问答_停止问答_Click(object sender, EventArgs e)
        {
            this.log("正在停止操作线程，请稍后");
            this.stopflag = true;
        }


        #region 搜狗 提问初始化
        /// <summary>
        /// 搜狗 提问初始化
        /// </summary>
        /// <param name="obj"></param>
        private void initAutoQuestion(Object obj)
        {
            //获取搜狗账号
            if (Utils.GetUserList(1, users) == false)
            {
                MessageBox.Show(String.Format("搜狗账号初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        #region 搜狗 自动点赞初始化
        /// <summary>
        /// 搜狗 自动点赞
        /// </summary>
        /// <param name="obj"></param>
        private void initAutoMark(Object obj)
        {
            //获取搜狗账号
            if (Utils.GetUserList(1, users) == false)
            {
                MessageBox.Show(String.Format("搜狗账号初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
where a.ismark=0 and a.category=1", null);
            this.dataGridView1_点赞_回答问题详情.Invoke(new Action(() =>
            {
                this.dataGridView1_点赞_回答问题详情.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                this.dataGridView1_点赞_回答问题详情.DataSource = markds.Tables[0];
            }));
        }
        #endregion

        #region 搜狗 选择答案初始化
        /// <summary>
        /// 搜狗 选择答案初始化
        /// </summary>
        /// <param name="obj"></param>
        private void initAutoGoodAnswer(Object obj)
        {
            //获取搜狗账号
            if (Utils.GetUserList(1, users) == false)
            {
                MessageBox.Show(String.Format("搜狗账号初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
where goodaid='' and d.aid is not null
GROUP BY loginname,title,content,qurlId", null);
            this.dataGridView2_最佳答案_提问详情.Invoke(new Action(() =>
            {
                this.dataGridView2_最佳答案_提问详情.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                this.dataGridView2_最佳答案_提问详情.DataSource = markds.Tables[0];
            }));
        }
        #endregion

        #region 搜狗 回答初始化
        /// <summary>
        /// 搜狗 回答初始化
        /// </summary>
        /// <param name="obj"></param>
        private void initAutoAnswer(Object obj)
        {
            //获取搜狗账号
            if (Utils.GetUserList(1, users) == false)
            {
                MessageBox.Show(String.Format("搜狗账号初始化失败"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            this.textBox1_回答_可操作帐号数量.Invoke(new Action(() =>
            {
                this.textBox1_回答_可操作帐号数量.Text = this.users.Count.ToString();
            }));

            //提问详情
            //回答数据
            DataSet markds = SQLiteHelper.ExecuteDataSet(@"select b.loginname as '提问帐号',c.title as '问题标题',c.batchName as '所属批次',qurl as '提问地址' from QuestionData a 
left join Users b on a.uid=b.Id
left join Question c on a.qid=c.Id
where a.category=1 and a.Id not in(select qid from AnswerData where category=1)", null);
            this.dataGridView1_回答_提问详情.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1_回答_提问详情.Invoke(new Action(() =>
            {
                this.dataGridView1_回答_提问详情.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                this.dataGridView1_回答_提问详情.DataSource = markds.Tables[0];
            }));

        }
        #endregion



        private void button2_点赞_启动_Click(object sender, EventArgs e)
        {
            this.stopflag = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback((Object obj) =>
            {
                this.log("自动点赞已启动");

                bool localip = false;//使用本机IP
                const int ipCount = 3; //每IP数量
                int currIPCount = 0;//当前IP数量
                string proxyStr = null;//IP代理
                PhantomJSDriver js;

                //使用本机IP
                this.checkBox1_点赞_本机IP.Invoke(new Action(() => { localip = this.checkBox1_点赞_本机IP.Checked; }));

                //获取所有的回答
                List<AnswerModel> answerList = Utils.GetAllAnswers(1);
                if (answerList == null && answerList.Count <= 0)
                {
                    this.log(String.Format("没有找到回答数据"));
                    return;
                }

                //初始化IP代理
                if (localip == false)
                {
                    proxyStr = proxy.GetProxyIP();
                    if (proxyStr == null && proxyStr == "")
                    {
                        this.log(String.Format("系统提示: {0} 代理IP获取失败"));
                        Utils.KillPhantomjs();
                        this.log("操作结束");
                        return;
                    }
                    else
                    {
                        js = Utils.NewPhantomjs(proxyStr);
                        this.log(String.Format("系统提示: 初始化IP代理成功"));
                        currIPCount = 0;
                    }
                }
                else
                {
                    js = Utils.NewPhantomjs(null);
                }

                try
                {
                    for (int i = 0; i < answerList.Count; i++)
                    {

                        if (stopflag == true)
                        {
                            Utils.KillPhantomjs();
                            this.log("操作结束");
                        }

                        List<UserModel> users = Utils.GetMarkAllUsers(answerList[i], 1);

                        if (users == null || users.Count <= 0)
                        {
                            this.log(String.Format("没有用于点赞的帐号数据"));
                            Utils.KillPhantomjs();
                            return;
                        }

                        for (int i1 = 0; i1 < users.Count; i1++)
                        {

                            //本地IP
                            if (localip == false)
                            {
                                if (currIPCount > ipCount)
                                {

                                    this.log(String.Format("账号: {0} 更换IP代理", users[i].LoginName));
                                    proxyStr = proxy.GetProxyIP();
                                    if (proxyStr == null && proxyStr == "")
                                    {
                                        this.log(String.Format("账号: {0} 点赞失败，原因: 代理IP获取失败", users[i].LoginName));
                                        continue;
                                    }
                                    else
                                    {
                                        Utils.NewPhantomjs(proxyStr);
                                        this.log(String.Format("账号: {0} 成功更换IP代理", users[i].LoginName));
                                        currIPCount = 0;
                                    }
                                }
                                currIPCount++;
                            }



                            this.sogoProvider.Login(js, users[i1], dama);
                            if (users[i1].isLogin == true)
                            {
                                bool ret = this.sogoProvider.Mark(js, users[i1], answerList[i]);
                                if (ret == true)
                                {
                                    //点赞成功刷新回答数据
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
where a.ismark=0 and a.category=1", null);
                                    this.dataGridView1_点赞_回答问题详情.Invoke(new Action(() =>
                                    {
                                        this.dataGridView1_点赞_回答问题详情.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                                        this.dataGridView1_点赞_回答问题详情.DataSource = markds.Tables[0];
                                    }));
                                    break;
                                }
                            }
                        }
                        users = null;
                    }
                }
                catch (Exception ex) 
                {
                    this.log(String.Format("搜狗自动点赞操作出现异常，原因：{0}", ex.Message + "----" + ex.StackTrace));
                    Utils.KillPhantomjs();
                    this.log("操作结束");
                    return;
                }

                Utils.KillPhantomjs();
                this.log("操作结束");

            }), null);
        }

        private void button1_点赞_停止_Click(object sender, EventArgs e)
        {
            this.log("正在停止操作线程，请稍后");
            this.stopflag = true;
        }

        private void button3_最佳答案_启动_Click(object sender, EventArgs e)
        {
            this.stopflag = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback((Object obj) =>
            {
                this.log("自动选择最佳答案已启动");

                bool localip = false;//使用本机IP
                const int ipCount = 3; //每IP数量
                int currIPCount = 0;//当前IP数量
                string proxyStr = null;//IP代理
                PhantomJSDriver js;


               

                //使用本机IP
                this.checkBox1_最佳答案_使用本机IP.Invoke(new Action(() => { localip = this.checkBox1_最佳答案_使用本机IP.Checked; }));


                //获取最佳答案操作数据
                List<GoodAnswerModel> goodList = Utils.GetGoodAnswer(1);
                if (goodList == null && goodList.Count <= 0)
                {
                    this.log(String.Format("没有找到选择最佳答案操作数据"));
                    return;
                }

                //初始化IP代理
                if (localip == false)
                {
                    proxyStr = proxy.GetProxyIP();
                    if (proxyStr == null && proxyStr == "")
                    {
                        this.log(String.Format("系统提示: {0} 代理IP获取失败"));
                        Utils.KillPhantomjs();
                        this.log("操作结束");
                        return;
                    }
                    else
                    {
                        js = Utils.NewPhantomjs(proxyStr);
                        this.log(String.Format("系统提示: 初始化IP代理成功"));
                        currIPCount = 0;
                    }
                }
                else
                {
                    js = Utils.NewPhantomjs(null);
                }

                try
                {
                    for (int i = 0; i < goodList.Count; i++)
                    {

                        if (stopflag == true)
                        {
                            Utils.KillPhantomjs();
                            this.log("操作结束");
                        }

                        //本地IP
                        if (localip == false)
                        {
                            if (currIPCount > ipCount)
                            {

                                this.log(String.Format("账号: {0} 更换IP代理", users[i].LoginName));
                                proxyStr = proxy.GetProxyIP();
                                if (proxyStr == null && proxyStr == "")
                                {
                                    this.log(String.Format("账号: {0} 点赞失败，原因: 代理IP获取失败", users[i].LoginName));
                                    continue;
                                }
                                else
                                {
                                    Utils.NewPhantomjs(proxyStr);
                                    this.log(String.Format("账号: {0} 成功更换IP代理", users[i].LoginName));
                                    currIPCount = 0;
                                }
                            }
                            currIPCount++;
                        }

                        this.sogoProvider.Login(js,goodList[i].User, dama);

                        if (goodList[i].User.isLogin == true)
                        {
                            bool ret= this.sogoProvider.GoodAnswer(js,goodList[i].User, goodList[i]);
                            if (ret == true)
                            {
                                //选择最佳答案成功刷新显示
                                DataSet markds = SQLiteHelper.ExecuteDataSet(@"select c.loginname as 提问帐号,b.title as 标题,b.content as 内容,count(d.aid) as 回答数量,a.qurl as 提问地址 from QuestionData a 
left join Question b on a.qid=b.Id
left join Users c on a.uid=c.Id
left join AnswerData d on a.Id=d.qid
where goodaid='' and d.aid is not null
GROUP BY loginname,title,content,qurlId", null);
                                this.dataGridView2_最佳答案_提问详情.Invoke(new Action(() =>
                                {
                                    this.dataGridView2_最佳答案_提问详情.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                                    this.dataGridView2_最佳答案_提问详情.DataSource = markds.Tables[0];
                                }));
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    this.log(String.Format("搜狗自动选择最佳答案操作出现异常，原因：{0}", ex.Message + "----" + ex.StackTrace));
                    Utils.KillPhantomjs();
                    this.log("操作结束");
                    return;
                }

                Utils.KillPhantomjs();
                this.log("操作结束");

            }), null);
        }

        private void button1_最佳答案_停止_Click(object sender, EventArgs e)
        {
            this.log("正在停止操作线程，请稍后");
            this.stopflag = true;
        }

        private void button2_回答_启用回答_Click(object sender, EventArgs e)
        {
            if (users.Count == 0)
            {
                MessageBox.Show(String.Format("没有可操作的账号"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.stopflag = false;

            ThreadPool.QueueUserWorkItem(new WaitCallback((Object obj) =>
            {
                this.log("自动回答已启动...");

                bool localip = false;//使用本机IP

                bool isrepeat = false;//可重复提交
                bool allAnswer = true; //答案全部回答
                PhantomJSDriver js = null;

                //使用本机IP
                this.checkBox3_回答_使用本机IP.Invoke(new Action(() => { localip = this.checkBox3_回答_使用本机IP.Checked; }));

                //答案全部回答
                this.checkBox1_回答_答案全部回答.Invoke(new Action(() => { allAnswer = this.checkBox1_回答_答案全部回答.Checked; }));


                //获取本次提问详情
                List<AskModel> asks = Utils.GetAskList(1);
                if (asks == null || asks.Count <= 0)
                {
                    this.log(String.Format("系统提示: 没有可操作的提问"));
                    Utils.KillPhantomjs();
                    this.log("操作结束");
                    return;
                }

                #region 搜狗 回答
                for (int i = 0; i < asks.Count; i++)
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

                        List<UserModel> list = null;
                        bool err = false;



                        QuestionInfoModel qinfo = Utils.GetQuesionInfo(asks[i].qid);
                        if (qinfo == null)
                        {
                            this.log(String.Format("系统提示: 提问ID: {0} 没有找到提问信息", asks[i].qid));
                            continue;
                        }

                        if (allAnswer == true)
                        {
                            //全部回答3个问题

                            //回答1
                            list = Utils.GetAnswerUser(asks[i], 2);
                            if (list != null && list.Count > 0)
                            {
                                for (int i1 = 0; i1 < list.Count; i1++)
                                {
                                    try
                                    {
                                        js = Utils.GetPhantomJsDriver(localip, proxy);
                                        this.sogoProvider.Login(js, list[i1], dama);
                                        if (list[i1].isLogin == true)
                                        {
                                            this.log(String.Format("系统提示: 账号:{0} 开始回答 提问地址: {1}", list[i1], asks[i].qurl));

                                            bool ret = this.sogoProvider.Answer(js, list[i1], qinfo.Answer_one, asks[i]);
                                            if (ret == true)
                                            {
                                                this.log(String.Format("系统提示: 账号:{0} 回答成功 提问地址: {1}", list[i1], asks[i].qurl));
                                                break;
                                            }
                                            if (err == true)
                                            {
                                                break;
                                            }
                                            this.log(String.Format("系统提示: 账号:{0} 回答失败,更换下一个账号继续 提问地址: {1}", list[i1], asks[i].qurl));
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }

                            //回答2
                            list = Utils.GetAnswerUser(asks[i], 2);
                            if (list != null && list.Count > 0)
                            {
                                for (int i1 = 0; i1 < list.Count; i1++)
                                {
                                    try
                                    {
                                        js = Utils.GetPhantomJsDriver(localip, proxy);
                                        this.sogoProvider.Login(js, list[i1], dama);
                                        if (list[i1].isLogin == true)
                                        {
                                            this.log(String.Format("系统提示: 账号:{0} 开始回答 提问地址: {1}", list[i1], asks[i].qurl));
                                            bool ret = this.sogoProvider.Answer(js, list[i1], qinfo.Answer_two, asks[i]);
                                            if (ret == true)
                                            {
                                                this.log(String.Format("系统提示: 账号:{0} 回答成功 提问地址: {1}", list[i1], asks[i].qurl));
                                                break;
                                            }
                                            if (err == true)
                                            {
                                                break;
                                            }
                                            this.log(String.Format("系统提示: 账号:{0} 回答失败,更换下一个账号继续 提问地址: {1}", list[i1], asks[i].qurl));
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }

                            //回答3
                            list = Utils.GetAnswerUser(asks[i], 2);
                            if (list != null && list.Count > 0)
                            {
                                for (int i1 = 0; i1 < list.Count; i1++)
                                {
                                    try
                                    {
                                        js = Utils.GetPhantomJsDriver(localip, proxy);
                                        this.sogoProvider.Login(js, list[i1], dama);
                                        if (list[i1].isLogin == true)
                                        {
                                            this.log(String.Format("系统提示: 账号:{0} 开始回答 提问地址: {1}", list[i1], asks[i].qurl));
                                            bool ret = this.sogoProvider.Answer(js, list[i1], qinfo.Answer_three, asks[i]);
                                            if (ret == true)
                                            {
                                                this.log(String.Format("系统提示: 账号:{0} 回答成功 提问地址: {1}", list[i1], asks[i].qurl));
                                                break;
                                            }
                                            if (err == true)
                                            {
                                                break;
                                            }
                                            this.log(String.Format("系统提示: 账号:{0} 回答失败,更换下一个账号继续 提问地址: {1}", list[i1], asks[i].qurl));
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }


                        }
                        else
                        {
                            //回答一个问题
                            //回答1
                            list = Utils.GetAnswerUser(asks[i], 2);
                            if (list != null && list.Count > 0)
                            {
                                for (int i1 = 0; i1 < list.Count; i1++)
                                {
                                    try
                                    {
                                        js = Utils.GetPhantomJsDriver(localip, proxy);
                                        this.sogoProvider.Login(js, list[i1], dama);
                                        if (list[i1].isLogin == true)
                                        {
                                            this.log(String.Format("系统提示: 账号:{0} 开始回答 提问地址: {1}", list[i1], asks[i].qurl));
                                            bool ret = this.sogoProvider.Answer(js, list[i1], qinfo.Answer_one, asks[i]);
                                            if (ret == true)
                                            {
                                                this.log(String.Format("系统提示: 账号:{0} 回答成功 提问地址: {1}", list[i1], asks[i].qurl));
                                                break;
                                            }
                                            if (err == true)
                                            {
                                                break;
                                            }
                                            this.log(String.Format("系统提示: 账号:{0} 回答失败,更换下一个账号继续 提问地址: {1}", list[i1], asks[i].qurl));
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }
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
    }
}
