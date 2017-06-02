using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;
using 搜索引擎自动营销助手V1._0.Model;

namespace 搜索引擎自动营销助手V1._0.Classs
{
    public class Utils
    {
        public static string GetGUID()
        {
            return Guid.NewGuid().ToString();
        }

        public static string GetImgString(string html)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(html);
            }
            catch
            {
                return null;
            }
            XmlNode idNode = xmlDoc.SelectSingleNode("Root/Id");
            XmlNode resultNode = xmlDoc.SelectSingleNode("Root/Result");
            XmlNode errorNode = xmlDoc.SelectSingleNode("Root/Error");
            string result = string.Empty;
            string topidid = string.Empty;
            if (resultNode != null && idNode != null)
            {
                topidid = idNode.InnerText;
                result = resultNode.InnerText;
                return result;
            }
            return null;
        }

        public static IWebElement GetElementFormWebDriver(IWebDriver driver, By by, int seconds)
        {
            DateTime now = DateTime.Now;
            while (true)
            {
                try
                {
                    IWebElement ret = driver.FindElement(by);
                    return ret;
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    Thread.Sleep(2000);
                }
                
                if ((DateTime.Now - now).TotalSeconds > seconds)
                {
                    return null;
                }
            }
        }

        public static IWebElement GetElementFormElement(IWebElement driver, By by, int seconds)
        {
            DateTime now = DateTime.Now;
            while (true)
            {
                try
                {
                    IWebElement ret = driver.FindElement(by);
                    return ret;
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    Thread.Sleep(2000);
                }
                
                if ((DateTime.Now - now).TotalSeconds > seconds)
                {
                    return null;
                }
            }
        }

        public static bool ExistsElementFormWebDriver(IWebDriver driver, By by, int seconds)
        {
            DateTime now = DateTime.Now;
            while (true)
            {
                if ((DateTime.Now - now).TotalSeconds > seconds)
                {
                    return false;
                }
                try
                {
                    IWebElement ret = driver.FindElement(by);
                    return true;
                }
                catch (Exception ex) {}
                Thread.Sleep(200);
            }
        }

        public static void CaputureImage(IWebDriver js, string filename)
        {
            ITakesScreenshot screen = (ITakesScreenshot)js;
            screen.GetScreenshot().SaveAsFile(filename, ScreenshotImageFormat.Jpeg);
        }

        #region 检查一个账号是否已提交过某个问题
        /// <summary>
        /// 检查一个账号是否已提交过某个问题
        /// </summary>
        /// <param name="user"></param>
        /// <param name="question"></param>
        /// <returns></returns>
        public static bool ExistsQuestionData(UserModel user, QuestionInfoModel question)
        {
            string sql = String.Format("select count(*) as ret from QuestionData where uid='{0}' and qid='{1}' and category={2}", user.Id, question.Id,user.Category);
            int ret = int.Parse(SQLiteHelper.ExecuteScalar(sql, null).ToString());
            if (ret <= 0)
            {
                return false;
            }
            return true;
        }
        #endregion

        /// <summary>
        /// 访问一个地址
        /// </summary>
        /// <param name="js"></param>
        /// <param name="url"></param>
        /// <param name="sleep">毫秒</param>
        public static void GoUrl(IWebDriver js,string url,int sleep)
        {
            js.Navigate().GoToUrl("http://about:blank");
            Thread.Sleep(100);
            js.Navigate().GoToUrl(url);
            Thread.Sleep(sleep);
            return;
        }


        public static void KillPhantomjs()
        {
            Process[] processes = System.Diagnostics.Process.GetProcesses();
            Process process;
            for (int i = 0; i < processes.Length - 1; i++)
            {
                process = processes[i];
                if (process.ProcessName.Contains("phantomjs") == true)
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
        }

        public static List<AskModel> GetAsks(int category)
        {
            string sql = String.Format("select * from QuestionData where category={0}",category);
            DataSet ds= SQLiteHelper.ExecuteDataSet(sql, null);
            List<AskModel> asks = new List<AskModel>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow row = ds.Tables[0].Rows[i];
                asks.Add(new AskModel() {
                    category = int.Parse(row["category"].ToString()),
                    goodaid = row["goodaid"] == null ? "" : row["goodaid"].ToString(),
                    Id = row["Id"].ToString(),
                    qid= row["qid"].ToString(),
                    qstate= row["qstate"].ToString(),
                    qurl= row["qurl"].ToString(),
                    qurlId= row["qurlId"].ToString(),
                    uid= row["uid"].ToString()
                });
            }
            return asks;
        }

        public static QuestionInfoModel GetQuesionInfo(string id)
        {
            string sql = String.Format("select * from Question where Id='{0}'", id);
            DataSet ds = SQLiteHelper.ExecuteDataSet(sql, null);
            if (ds.Tables[0].Rows.Count <= 0)
            {
                return null;
            }
            DataRow row = ds.Tables[0].Rows[0];
            return new QuestionInfoModel()
            {
                Answer_one = row["Answer_one"].ToString(),
                Answer_three= row["Answer_three"].ToString(),
                Answer_two= row["Answer_two"].ToString(),
                batchName= row["batchName"].ToString(),
                content= row["content"].ToString(),
                Id= row["Id"].ToString(),
                qId= row["qId"].ToString(),
                title= row["title"].ToString()
            };
        }

        public static AskModel GetAskByNotUser(List<AskModel> asks,UserModel user)
        {
            if (asks == null || asks.Count <= 0)
            {
                return null;
            }

            for (int i = 0; i < asks.Count; i++)
            {
                if (asks[i].uid != user.Id)
                {
                    AskModel ret = asks[i];
                    asks.RemoveAt(i);
                    return ret;
                }
            }
            return null;
        }

        #region 获取回答问题的账号集合
        /// <summary>
        /// 获取回答问题的账号
        /// </summary>
        /// <param name="qinfo"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public static List<UserModel> GetAnswerUser(AskModel ask,int category)
        {
            string sql = String.Format(@"Select * from Users where category={0} and Id not in(
Select * from(
select uid from AnswerData where qid='{1}' and category={2}
union ALL
select uid from QuestionData where Id='{3}' and category={4}))", category,ask.Id, category,ask.Id,category);

            DataSet ds= SQLiteHelper.ExecuteDataSet(sql, null);

            if (ds.Tables[0].Rows.Count <= 0)
            {
                return null;
            }

            List<UserModel> list = new List<UserModel>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow row = ds.Tables[0].Rows[i];
                list.Add(new UserModel()
                {
                    Category = int.Parse(row["category"].ToString()),
                    Id = row["Id"].ToString(),
                    LoginName = row["loginname"].ToString(),
                    LoginPwd = row["loginpwd"].ToString(),
                });
            }
            return list;
        }
        #endregion

        #region 获取 回答集合
        /// <summary>
        /// 获取 回答集合
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static List<AnswerModel> GetAllAnswers(int category)
        {
            DataSet ds = SQLiteHelper.ExecuteDataSet(String.Format("select a.Id,a.aid,b.qurl,a.uid from AnswerData a left join QuestionData b on a.qid=b.Id where a.ismark is null and a.category={0}",category), null);

            if (ds.Tables[0].Rows.Count <= 0)
            {
                return null;
            }


            List<AnswerModel> list = new List<AnswerModel>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow row = ds.Tables[0].Rows[i];
                list.Add(new AnswerModel()
                {
                    Id=row["Id"].ToString(),
                    AnswerID = row["aid"].ToString(),
                    QuesionUrl = row["qurl"].ToString(),
                    UserID = row["uid"].ToString()
                });
            }
            return list;
        }
        #endregion

        #region 获取点赞的帐号集合
        /// <summary>
        /// 获取点赞的帐号集合
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public static List<UserModel> GetMarkAllUsers(AnswerModel answer, int category)
        {

           
            DataSet ds = SQLiteHelper.ExecuteDataSet(String.Format(@"select * from Users where Id not in('{0}') and category={2}",answer.UserID, category), null);

            if (ds.Tables[0].Rows.Count <= 0)
            {
                return null;
            }

            List<UserModel> list = new List<UserModel>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow row = ds.Tables[0].Rows[i];
                list.Add(new UserModel()
                {
                    Category = int.Parse(row["category"].ToString()),
                    Id = row["Id"].ToString(),
                    LoginName = row["loginname"].ToString(),
                    LoginPwd = row["loginpwd"].ToString(),
                });
            }
            return list;
        }
        #endregion

        #region 更新点赞
        /// <summary>
        /// 更新点赞
        /// </summary>
        /// <param name="answer"></param>
        public static void UpdateMark(AnswerModel answer,int category)
        {
            SQLiteHelper.ExecuteNonQuery(String.Format("update AnswerData set ismark=1 where Id='{0}' and category={1}", answer.Id, category), null);
            return;
        }
        #endregion

        #region 更新最佳答案
        /// <summary>
        /// 更新最佳答案
        /// </summary>
        /// <param name="answer"></param>
        public static void UpdateGoodAnswer(GoodAnswerModel answer, int category)
        {
            SQLiteHelper.ExecuteNonQuery(String.Format("update QuestionData set goodaid='{0}' where Id='{1}' and category={2}", answer.AnswerId,answer.Id, category), null);
            return;
        }
        #endregion

        #region 获取 最佳答案数据
        /// <summary>
        /// 获取 回答集合
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static List<GoodAnswerModel> GetGoodAnswer(int category)
        {
            DataSet ds = SQLiteHelper.ExecuteDataSet(String.Format(@"Select * from(
select a.Id,b.loginname,b.loginpwd,a.qurl,a.goodaid,(select aid from AnswerData where qid=a.Id and category={0} LIMIT 0,1) as aid,a.qurlId from QuestionData a
left join Users b on a.uid=b.Id
where a.goodaid='' and a.category={1}) where aid is not NULL", category,category), null);

            if (ds.Tables[0].Rows.Count <= 0)
            {
                return null;
            }

            List<GoodAnswerModel> list = new List<GoodAnswerModel>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow row = ds.Tables[0].Rows[i];
                list.Add(new GoodAnswerModel()
                {
                    Id = row["Id"].ToString(),
                    QuesionUrl = row["qurl"].ToString(),
                    AnswerId = row["aid"].ToString(),
                    QusionUrlId = row["qurlId"].ToString(),
                    User = new UserModel() { LoginName = row["loginname"].ToString(), LoginPwd = row["loginpwd"].ToString() }
                });
            }
            return list;
        }
        #endregion


        [DllImport("util.dll",CallingConvention = CallingConvention.Cdecl)]
        public static extern bool HidePhantomjs(long pid);


        #region 获取一个新的Phantomjs实例
        /// <summary>
        /// 获取一个新的Phantomjs实例
        /// </summary>
        /// <param name="proxy">代理地址</param>
        /// <returns></returns>
        public static PhantomJSDriver NewPhantomjs(string proxy)
        {
            Utils.KillPhantomjs();
            Thread.Sleep(300);
            PhantomJSDriverService jsService = PhantomJSDriverService.CreateDefaultService();
            if (proxy != null && proxy != "")
            {
                jsService.Proxy = proxy;
            }
            PhantomJSDriver js = new OpenQA.Selenium.PhantomJS.PhantomJSDriver(jsService);
            Utils.HidePhantomjs(Utils.GetPhantomjsPid());
            js.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
            return js;
        }
        #endregion

        #region 获取账号集合
        /// <summary>
        /// 获取账号集合
        /// </summary>
        /// <param name="category">账号类型</param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool GetUserList(int category,List<UserModel> list)
        {
            string sql = String.Format("select *  from Users where category={0}", category);
            list.Clear();
            try
            {
                DataTable dt = SQLiteHelper.ExecuteDataSet(sql, null).Tables[0];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    list.Add(new UserModel() { Id = dt.Rows[i]["Id"].ToString(), LoginName = dt.Rows[i]["loginname"].ToString(), LoginPwd = dt.Rows[i]["loginpwd"].ToString(), Category = Int32.Parse(dt.Rows[i]["category"].ToString()), service = new SogouService() });
                }
                return true;
            }
            catch (Exception ex) { return false; }
        }
        #endregion

        #region 获取IP代理接口地址
        /// <summary>
        /// 获取IP代理接口地址
        /// </summary>
        /// <returns></returns>
        public static string GetProxyRemote()
        {
            try
            {
                return SQLiteHelper.ExecuteScalar("select url from Proxy", null).ToString().Trim();
            }
            catch (Exception ex) { return null; }
        }
        #endregion

        #region 获取若快打码平台参数
        /// <summary>
        /// 获取若快打码平台参数
        /// </summary>
        /// <param name="dama"></param>
        /// <returns></returns>
        public static bool GetDama(Dictionary<string, string> dama)
        {
            try
            {
                DataSet ds = SQLiteHelper.ExecuteDataSet("select * from DaMa", null);
                if (ds.Tables[0].Rows.Count == 1)
                {
                    dama["username"] = ds.Tables[0].Rows[0]["loginname"].ToString();
                    dama["password"] = ds.Tables[0].Rows[0]["loginpwd"].ToString();
                    dama["typeid"] = "";
                    dama["timeout"] = ds.Tables[0].Rows[0]["timeout"].ToString();
                    dama["softid"] = ds.Tables[0].Rows[0]["softid"].ToString();
                    dama["softkey"] = ds.Tables[0].Rows[0]["softkey"].ToString();
                    dama["url"] = ds.Tables[0].Rows[0]["url"].ToString();
                    return true;
                }
                return false;
            }
            catch (Exception ex) { return false; }
        }
        #endregion


        #region 检测控件是否可用
        /// <summary>
        /// 检测控件是否可用
        /// </summary>
        /// <param name="eles"></param>
        /// <returns></returns>
        public static bool IsValid(params IWebElement[] eles)
        {
            foreach (IWebElement item in eles)
            {
                if (item == null || item.Enabled == false || item.Displayed == false)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region 检测IP代理是否可用
        /// <summary>
        /// 检测IP代理是否可用
        /// </summary>
        /// <param name="proxyIP"></param>
        /// <returns></returns>
        public static bool IsProxyValid(string proxyIP)
        {
            try
            {
                //设置超时
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.baidu.com/img/bd_logo1.png");
                request.Proxy = new WebProxy(proxyIP);
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.110 Safari/537.36";
                request.Timeout = 5000;
                WebResponse response= request.GetResponse();
                request.Abort();
                response.Close();
                request = null;
                response = null;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        #endregion

        #region 获取一个Phantomjs实例
        /// <summary>
        /// 获取一个Phantomjs实例
        /// </summary>
        /// <param name="localip">是否使用本地IP</param>
        /// <param name="proxy">代理服务对象</param>
        /// <returns></returns>
        public static PhantomJSDriver GetPhantomJsDriver(bool localip,ProxyService proxy)
        {
            try
            {
                //获取IP代理
                if (localip == false)
                {
                    string proxyStr = proxy.GetProxyIP();
                    if (proxyStr == null && proxyStr == "")
                    {
                        return null;
                    }
                    else
                    {
                        return Utils.NewPhantomjs(proxyStr);
                    }
                }
                else
                {
                    return Utils.NewPhantomjs(null);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        public static List<AskModel> GetAskList(int category)
        {
            string sql = String.Format(@"select a.Id,a.qid,a.uid,a.goodaid,a.qurl,a.qurlId,a.category,a.qstate from QuestionData a where a.category=2 and a.Id not in(select qid from AnswerData where category={0})", category);

            DataSet ds = SQLiteHelper.ExecuteDataSet(sql, null);

            if (ds.Tables[0].Rows.Count <= 0)
            {
                return null;
            }

            List<AskModel> list = new List<AskModel>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow row = ds.Tables[0].Rows[i];
                list.Add(new AskModel()
                {
                    Id = row["Id"].ToString(),
                    qid = row["qid"].ToString(),
                    uid = row["uid"].ToString(),
                    goodaid = row["goodaid"].ToString(),
                    qurl = row["qurl"].ToString(),
                    qurlId = row["qurlId"].ToString(),
                    category = int.Parse(row["category"].ToString()),
                    qstate = row["qstate"].ToString()
                });
            }
            return list;
        }

        public static int GetPhantomjsPid()
        {
            Process[] processes = System.Diagnostics.Process.GetProcesses();
            Process process;
            for (int i = 0; i < processes.Length - 1; i++)
            {
                process = processes[i];
                if (process.ProcessName.Contains("phantomjs") == true)
                {
                    return process.Id;
                }
            }
            return 0;
        }

        public static string GetLocalCode()
        {
            WmiHelper hardwareInfo = new WmiHelper();
            string hardDiskID = hardwareInfo.GetHardDiskID();
            string cpuID = hardwareInfo.GetCpuID();
            string mac = hardwareInfo.GetMacAddress();
            return hardDiskID + cpuID + mac;
        }
    }
}
