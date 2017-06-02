using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using 搜索引擎自动营销助手V1._0.Model;

namespace 搜索引擎自动营销助手V1._0.Classs
{
    /// <summary>
    /// 搜狗接口
    /// </summary>
    public class SogoProvider
    {
        public delegate void Log(string msg);
        public delegate void ShowImg(byte[] buff);

        private Log log;
        private ShowImg showimg;
        private bool debug;

        public SogoProvider(Log _log,ShowImg _showimg)
        {
            this.log = _log;
            this.showimg = _showimg;
            this.debug = false;
        }

        #region 登录搜狗问问
        /// <summary>
        /// 登录搜狗问问
        /// </summary>
        /// <param name="js">PhantomJSDriver对象</param>
        /// <param name="jsService">PhantomJSDriverService 对象</param>
        /// <param name="sleep">操作延时</param>
        /// <param name="user">登录用户</param>
        /// <param name="dama">若快打码</param>
        public void Login(PhantomJSDriver js, UserModel user,Dictionary<string,string> dama) 
        {
            try
            {
                this.log(String.Format("帐号：{0} 正在登录", user.LoginName));
                js.Manage().Cookies.DeleteAllCookies();
                Utils.GoUrl(js, "http://wenwen.sogou.com/", 1000);
                user.isLogin = false;

                //显示登录界面
                if (this.ShowLoginWindow(js, user) == false)
                {
                    return;
                }

                //切换登录Ifrime
                if (this.SwitchLoginIfrime(js, user) == false)
                {
                    return;
                }

                //输入帐号密码并登录
                if (this.InputAccountAndpwd(js, user) == false)
                {
                    return;
                }
                Thread.Sleep(200);

                //是否密码错误
                if (this.IsPasswordError(js, user) == true)
                {
                    return;
                }

                //是否登录成功
                if (this.isLoginSucess(js) == true)
                {
                    this.LoginSucess(js, user, dama);
                    return;
                }
                this.DebugLog(String.Format("正在获取账号昵称控件失败"));

                //是否需要验证码
                if (this.IsNeedValidCode(js, user) == true)
                {
                    this.DebugLog(String.Format("账号: {0} 需要验证码，正在处理",user.LoginName));
                    //验证码
                    this.DebugLog(String.Format("账号: {0} 正在获取登录验证码图片控件", user.LoginName));
                    IWebElement ele_Codeimg = Utils.GetElementFormWebDriver(js, By.Id("capImg"),1);
                    this.DebugLog(String.Format("账号: {0} 获取登录验证码图片控件成功 id=capImg", user.LoginName));
                    this.DebugLog(String.Format("账号: {0} 正在识别登录验证码", user.LoginName));
                    string yzmCode = this.GetLoginValidCode(js,ele_Codeimg, user, dama);
                    if (yzmCode == null || yzmCode == "")
                    {
                        this.DebugLog(String.Format("账号: {0} 识别登录验证码失败", user.LoginName));
                        return;
                    }
                    this.DebugLog(String.Format("账号: {0} 识别登录验证码成功 {1}", user.LoginName,yzmCode));

                    this.DebugLog(String.Format("账号: {0} 正在获取登录验证码录入控件", user.LoginName));
                    IWebElement codeInput = Utils.GetElementFormWebDriver(js, By.Id("capAns"),30);
                    if (Utils.IsValid(codeInput) == false)
                    {
                        this.DebugLog(String.Format("账号: {0} 获取登录验证码录入控件失败", user.LoginName));
                        return;
                    }
                    codeInput.Clear();
                    codeInput.SendKeys(yzmCode);
                    this.DebugLog(String.Format("账号: {0} 录入登录验证码成功", user.LoginName));

                    this.DebugLog(String.Format("账号: {0} 正在获取登录点击控件", user.LoginName));
                    IWebElement codebtn = Utils.GetElementFormWebDriver(js, By.Id("submit"),30);
                    if (Utils.IsValid(codebtn) == false)
                    {
                        this.DebugLog(String.Format("账号: {0} 获取登录点击控件失败", user.LoginName));
                        return;
                    }
                    
                    codebtn.Click();
                    Thread.Sleep(2000);
                    this.DebugLog(String.Format("账号: {0} 获取登录点击控件成功", user.LoginName));

                    string loginerr = this.GetLoginMessage(js, user);
                    if (loginerr != null)
                    {
                        this.log(String.Format("账号: {0} 登陆失败，原因：{1}", user.LoginName, loginerr));
                        return;
                    }

                    Object obj= ((IJavaScriptExecutor)js).ExecuteScript("return document.getElementById('tip_word');");
                    if (obj != null)
                    {
                        obj = ((IJavaScriptExecutor)js).ExecuteScript("return document.getElementById('tip_word').innerText;");
                        this.DebugLog(String.Format("账号: {0} 验证码处理服务器返回错误消息 {1}", user.LoginName,obj.ToString()));

                        if (obj.ToString().Contains("输入有误") == true)
                        { 
                            //验证码错误重试次数为3次
                            int codeCount = 0;
                            while (true)
                            {
                                Thread.Sleep(2000);
                                codeCount++;
                                if (codeCount > 3)
                                {
                                    this.log(String.Format("系统提示: 帐号: {0} 验证码输入错误次数超过3次，已跳过", user.LoginName));
                                    return;
                                }
                                yzmCode = this.GetLoginValidCode(js, ele_Codeimg, user, dama);
                                if (yzmCode == null || yzmCode == "")
                                {
                                    return;
                                }
                                this.log(String.Format("系统提示: 帐号: {0} 识别验证码为: {1}", user.LoginName, yzmCode));

                                //((IJavaScriptExecutor)js).ExecuteScript("document.getElementById('capAns').value='" + yzmCode.Trim() + "';");
                                codeInput= Utils.GetElementFormWebDriver(js, By.Id("capAns"), 1);
                                if (Utils.IsValid(codeInput) == false)
                                {
                                    this.log(String.Format("账号: {0} 登陆失败，原因：验证码输入控件没有找到", user.LoginName));
                                    return;
                                }
                                codeInput.Clear();
                                codeInput.SendKeys(yzmCode);
                                codebtn.Click();
                                Thread.Sleep(1000);

                                //判断是否有关于登录的错误消息
                                loginerr = this.GetLoginMessage(js, user);
                                if (loginerr != null)
                                {
                                    this.log(String.Format("账号: {0} 登陆失败，原因：{1}", user.LoginName, loginerr));
                                    return;
                                }

                                //this.SwitchValidCodeIfrime(js, user);
                                obj = ((IJavaScriptExecutor)js).ExecuteScript("return document.getElementById('tip_word');");
                                //判断是否有关于验证码的错误消息
                                if (obj != null)
                                { 
                                    obj = ((IJavaScriptExecutor)js).ExecuteScript("return document.getElementById('tip_word').innerText;");
                                    if (obj.ToString().Contains("输入有误") == true)
                                    {
                                        continue;
                                    }
                                }

                                //是否登录成功
                                if (this.isLoginSucess(js) == true)
                                {
                                    this.LoginSucess(js, user, dama);
                                    return;
                                }


                            }
                        }
                        this.log(String.Format("账号: {0} 登陆失败，原因：{1}", user.LoginName, obj.ToString()));
                        return;
                    }

                    //是否登录成功
                    if (this.isLoginSucess(js) == true)
                    {
                        this.LoginSucess(js, user, dama);
                        return;
                    }

                    this.log(String.Format("账号: {0} 登陆失败，原因：{1}", user.LoginName, "无法判断登录状态"));
                    return;
                }

                this.log(String.Format("帐号 {0} 登陆失败", user.LoginName));
                return;
               
            }
            catch (Exception ex) { this.log(String.Format("账号: {0} 操作失败 异常堆栈: {1}", user.LoginName, ex.Message + "---" + ex.StackTrace)); }
        }
        #endregion

        #region 搜狗提问
        /// <summary>
        /// 搜狗提问
        /// </summary>
        /// <param name="js">PhantomJSDriver</param>
        /// <param name="jsService">PhantomJSDriverService</param>
        /// <param name="sleep">操作延时</param>
        /// <param name="user">提问用户</param>
        /// <param name="quesion">问题</param>
        /// <param name="isrepeat">是否允许用户重复提问</param>
        /// <param name="dama">若快打码</param>
        /// <returns></returns>
        public AskModel SendQuestion(PhantomJSDriver js, UserModel user, QuestionInfoModel quesion,Dictionary<string,string> dama)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 开始提交问题", user.LoginName));
                Utils.GoUrl(js, "http://wenwen.sogou.com/question/ask", 2000);

                IWebElement ele = null;

                this.DebugLog(String.Format("账号: {0} 正在获取提问标题输入控件", user.LoginName));
                IWebElement ele_title = Utils.GetElementFormWebDriver(js, By.Id("ask_title"),30);
                if (Utils.IsValid(ele_title)==false)
                {
                    this.log(String.Format("账号: {0} 提问失败，原因: 没有找到问题标题控件", user.LoginName));
                    return null;
                }
                this.DebugLog(String.Format("账号: {0} 获取提问标题输入控件成功", user.LoginName));

                //初始化分类
                try
                {
                    ele_title.Clear();
                    ele_title.SendKeys(quesion.title);
                }
                catch (Exception ex) { }

                this.DebugLog(String.Format("账号: {0} 输入提问标题成功", user.LoginName));

                this.DebugLog(String.Format("账号: {0} 正在获取提问内容输入控件", user.LoginName));
                ele = Utils.GetElementFormWebDriver(js, By.Id("ask_content"),30);
                if (Utils.IsValid(ele)==false)
                {
                    this.log(String.Format("账号: {0} 操作失败，原因: 没有找到问题内容控件", user.LoginName));
                    return null;
                }
                this.DebugLog(String.Format("账号: {0} 获取提问内容输入控件成功", user.LoginName));

                try
                {
                    ele.Clear();
                    ele.SendKeys(quesion.content);
                }
                catch (Exception ex) { }
                this.DebugLog(String.Format("账号: {0} 输入提问内容成功", user.LoginName));

                //验证码
                ele = Utils.GetElementFormWebDriver(js, By.Id("validCode_img"),3);
                if (Utils.IsValid(ele) == true)
                {

                }



                if (ele == null)
                {
                    this.log(String.Format("账号: {0} 操作失败，原因: 没有找到验证码控件", user.LoginName));
                    return null;
                }
                if (ele.Enabled == false || ele.Displayed == false)
                {
                    this.log(String.Format("账号: {0} 提问失败，原因: 验证码控件未启用或隐藏", user.LoginName));
                    return null;
                }

                string yzmCode= this.GetValidCode(js, ele, user, quesion, dama);
                if (yzmCode == null || yzmCode == "")
                {
                    return null;
                }
                

                ele = Utils.GetElementFormWebDriver(js, By.Id("validCode"), 3);
                if (ele.Enabled == false || ele.Displayed == false)
                {
                    this.log(String.Format("账号: {0} 提问失败，原因: 验证码输入控件未启用或隐藏", user.LoginName));
                    return null;
                }
                ele.Clear();
                ele.SendKeys(yzmCode);
                //this.log(String.Format("调试信息:  账号：{0} 已输入验证码", user.LoginName));

                ele = Utils.GetElementFormWebDriver(js, By.Id("submit_question"), 3);
                if (ele == null)
                {
                    this.log(String.Format("账号: {0} 操作失败，原因: 没有找到问题提交控件", user.LoginName));
                    return null;
                }
                if (ele.Enabled == false || ele.Displayed == false)
                {
                    this.log(String.Format("账号: {0} 提问失败，原因: 提交控件未启用或隐藏", user.LoginName));
                    return null;
                }
                ele.Click();
                Thread.Sleep(500);

                ele = Utils.GetElementFormWebDriver(js, By.XPath("//div[@id='wrap']//div[@class='prompt-tips']"), 2);
                if (ele != null)
                {
                    string label = ele.Text;
                    if (label.Contains("验证码错误"))
                    {
                        this.log(String.Format("账号: {0} 操作失败，原因: 验证码错误", user.LoginName));
                        return null;
                    }
                    if (label.Contains("审核未通过"))
                    {
                        this.log(String.Format("账号: {0} 操作失败，原因: 提交内容审核未通过", user.LoginName));
                        return null;
                    }
                }


                Regex re = new Regex(@"<link rel=""canonical"" href=""(.+)"">");
                if (re.IsMatch(js.PageSource) == false)
                {
                    this.log(String.Format("账号: {0} 提交问题失败", user.LoginName));
                    return null;
                }

                string qurl = re.Match(js.PageSource).Groups[1].Value;
                re = new Regex(@"qid=(\d+)");
                string qurlid = re.Match(qurl).Groups[1].Value;

                //写入数据库
                AskModel ask = new AskModel();
                ask.Id = Utils.GetGUID();
                ask.qid = quesion.Id;
                ask.uid = user.Id;
                ask.goodaid = "";
                ask.qurl = qurl;
                ask.qurlId = qurlid;
                ask.category = 1;
                ask.qstate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string sql = String.Format(@"insert into QuestionData(Id,qid,uid,goodaid,qurl,qurlId,category,qstate)
                values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')",
                    ask.Id,
                   ask.qid,
                    ask.uid,
                    "",
                    ask.qurl,
                    ask.qurlId,
                    ask.category,
                    ask.qstate
                    );
                SQLiteHelper.ExecuteNonQuery(sql, null);
                this.log(String.Format("账号: {0} 自动提问成功,地址: {1}", user.LoginName, qurl));
                return ask;
            }
            catch (Exception ex) { this.log(String.Format("账号: {0} 操作失败 异常堆栈: {1}", user.LoginName, ex.Message + "---" + ex.StackTrace)); return null; }
        }
        #endregion

        #region 搜狗回答
        /// <summary>
        /// 搜狗回答
        /// </summary>
        /// <param name="js">PhantomJSDriver</param>
        /// <param name="jsService">PhantomJSDriverService</param>
        /// <param name="sleep">操作延时</param>
        /// <param name="user">提问用户</param>
        /// <param name="answerContent">回答内容</param>
        /// <param name="ask">提问对象</param>
        public bool Answer(PhantomJSDriver js, UserModel user, string answerContent, AskModel ask)
        {
            try
            {
                js.Navigate().GoToUrl("http://about:blank");
                js.Navigate().GoToUrl(ask.qurl);

                IWebElement ele = Utils.GetElementFormWebDriver(js, By.Id("ueditor_0"),3);
                if (ele == null)
                {
                    this.log(String.Format("问题ID: {0} 回答失败，地址: {1}，原因: 回答内容框架未找到", ask.Id, ask.qurl));
                    return false;
                }

                js.SwitchTo().Frame(ele);

                ele = Utils.GetElementFormWebDriver(js, By.XPath("//body[@class='view']/p[1]"),3);
                if (ele == null)
                {
                    this.log(String.Format("问题ID: {0} 回答失败，地址: {1}，原因: 回答内容控件未找到", ask.Id, ask.qurl));
                    return false;
                }

                ((IJavaScriptExecutor)js).ExecuteScript("arguments[0].innerText='" + answerContent + "'", ele);
                js.SwitchTo().DefaultContent();
                ele = Utils.GetElementFormWebDriver(js, By.XPath("//*[@id=\"submit_answer\"]"),3);
                if (ele == null)
                {
                    this.log(String.Format("问题ID: {0} 回答失败，地址: {1}，原因: 提交回答内容控件未找到", ask.Id, ask.qurl));
                    return false;
                }

                ((IJavaScriptExecutor)js).ExecuteScript("arguments[0].click();", ele);
                Thread.Sleep(200);
                js.Navigate().GoToUrl("http://about:blank");
                js.Navigate().GoToUrl(ask.qurl);
                Thread.Sleep(500);
                Regex re = new Regex("<li class=\"delete_item\" data-type=\"answer\" data-id=\"(\\d+)\">");
                if (re.IsMatch(js.PageSource) == false)
                {
                    this.log(String.Format("问题ID: {0} 回答失败，地址: {1}，原因: 页面未找到提交信息", ask.Id, ask.qurl));
                    return false;
                }

                string aid = re.Match(js.PageSource).Groups[1].Value;
                SQLiteHelper.ExecuteNonQuery(String.Format(@"insert into AnswerData(Id,qid,uid,aid,category,savedate)values('{0}','{1}','{2}','{3}',{4},'{5}')",
                    Utils.GetGUID(),
                    ask.Id,
                    user.Id,
                    aid,
                    1,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    ), null);
                this.log(String.Format("问题ID: {0} 操作成功，地址: {1}，回答ID: {2} 回答账号: {3}", ask.Id, ask.qurl, aid, user.LoginName));
                return true;
            }
            catch (Exception ex) { this.log(String.Format("账号: {0} 操作失败 异常堆栈: {1}", user.LoginName, ex.Message + "---" + ex.StackTrace)); return false; }
        }
        #endregion

        #region 搜狗点赞
        /// <summary>
        /// 搜狗点赞
        /// </summary>
        /// <param name="js"></param>
        /// <param name="jsService"></param>
        /// <param name="sleep"></param>
        /// <param name="user"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        public bool Mark(PhantomJSDriver js, UserModel user,AnswerModel answer)
        {
            try
            {
                js.SwitchTo().DefaultContent();
                js.Navigate().GoToUrl("http://about:blank");
                js.Navigate().GoToUrl(answer.QuesionUrl);

                Thread.Sleep(300);

                IWebElement mark = Utils.GetElementFormWebDriver(js, By.XPath(String.Format("//div[@data-id='{0}']//a[@title='点赞']", answer.AnswerID)), 5);
                if (mark == null)
                {
                    this.log(String.Format("回答ID：{0} 点赞失败,原因：没有找到点赞控件", answer.AnswerID));
                    return false;
                }

                mark.Click();

                Utils.UpdateMark(answer, 1);
                this.log(String.Format("回答ID：{0} 提问地址：{1}  点赞成功", answer.AnswerID, answer.QuesionUrl));
                return true;
            }
            catch (Exception ex) 
            {
                this.log(String.Format("回答ID：{0} 点赞失败,原因：{1}", answer.AnswerID,ex.Message+"---"+ex.StackTrace));
                return false;
            }
        }
        #endregion

        #region 选择最佳答案
        /// <summary>
        /// 选择最佳答案
        /// </summary>
        /// <param name="js"></param>
        /// <param name="jsService"></param>
        /// <param name="sleep"></param>
        /// <param name="user"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        public bool GoodAnswer(PhantomJSDriver js, UserModel user, GoodAnswerModel answer)
        {
            try
            {
                js.SwitchTo().DefaultContent();
                js.Navigate().GoToUrl("http://about:blank");
                js.Navigate().GoToUrl(answer.QuesionUrl);

                Thread.Sleep(500);

                IWebElement good = Utils.GetElementFormWebDriver(js, By.XPath(String.Format("//div[@data-id='{0}']//a[@class='btn-down btn-adoption adopt_answer']",answer.AnswerId)),3);
                if (good == null)
                {
                    this.log(String.Format("回答ID：{0} 提问地址：{1} 采纳最佳答案失败,原因：没有找到采纳答案控件", answer.AnswerId,answer.QuesionUrl));
                    return false;
                }

                good.Click();

                Utils.UpdateGoodAnswer(answer, 1);
                this.log(String.Format("回答ID：{0} 提问地址：{1}  采纳最佳答案成功", answer.AnswerId, answer.QuesionUrl));
                return true;
            }
            catch (Exception ex)
            {
                this.log(String.Format("回答ID：{0} 提问地址：{1} 采纳最佳答案失败,原因：{2}", answer.AnswerId,answer.QuesionUrl, ex.Message + "---" + ex.StackTrace));
                return false;
            }
        }
        #endregion

        #region 返回是否登录成功
        /// <summary>
        /// 返回是否登录成功
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        private bool isLoginSucess(IWebDriver driver)
        {
            driver.SwitchTo().DefaultContent();
            this.DebugLog(String.Format("正在获取账号昵称控件"));
            IWebElement ele = Utils.GetElementFormWebDriver(driver, By.XPath("//div[@id='header']/div[@class='login']/div[@class='user-option']/a/div[@class='user-name-box']/div"),2);
            return Utils.IsValid(ele);
        }
        #endregion

        public void EnableDebug()
        {
            this.debug = true;
        }

        public void DisableDebug()
        {
            this.debug = false;
        }

        private string GetValidCode(PhantomJSDriver js,IWebElement ele_code , UserModel user, QuestionInfoModel quesion, Dictionary<string, string> dama)
        {
            IWebElement ele = ele_code;

            string pageImg = Utils.GetGUID() + ".jpeg";
            Utils.CaputureImage(js, pageImg);

            Bitmap bitmap = new Bitmap(pageImg);
            Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format16bppRgb555);
            Graphics draw = Graphics.FromImage(bitmap2);
            draw.DrawImage(bitmap, 0, 0);
            draw.Dispose();
            bitmap.Dispose();
            bitmap = null;

            Size size = new Size(Math.Min(ele.Size.Width, ele.Size.Width), Math.Min(ele.Size.Height, ele.Size.Height));
            size.Width = size.Width;
            size.Height = size.Height + 5;
            Rectangle crop = new Rectangle(ele.Location, size);
            MemoryStream ms = new MemoryStream();
            Bitmap validCode = bitmap2.Clone(crop, bitmap2.PixelFormat);
            validCode.Save(ms, ImageFormat.Jpeg);

            //删除图片
            validCode.Dispose();
            validCode = null;
            bitmap2.Dispose();
            bitmap2 = null;


            File.Delete(pageImg);

            //提交参数
            var param = new Dictionary<object, object>
                        {
                            {"username",dama["username"]},
                            {"password",dama["password"]},
                            {"typeid","4040"},
                            {"timeout",dama["timeout"]},
                            {"softid",dama["softid"]},
                            {"softkey",dama["softkey"]}
                        };

            string httpResult = RuoKuaiHttp.Post("http://api.ruokuai.com/create.xml", param, ms.ToArray());
            string yzmCode = Utils.GetImgString(httpResult);
            if (yzmCode == null || yzmCode == "")
            {
                this.log(String.Format("账号: {0} 操作失败，原因: 打码平台返回验证码为空", user.LoginName));
                return null;
            }

            ms.Close();
            ms.Dispose();
            return yzmCode;
        }

        private void LoginSucess(PhantomJSDriver js, UserModel user, Dictionary<string, string> dama)
        {
            js.SwitchTo().DefaultContent();
            IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//div[@id='header']/div[@class='login']/div[@class='user-option']/a/div[@class='user-name-box']/div"),3);

            string niceName = ele.Text;
            this.log(String.Format("账号: {0} 昵称: {1} 登陆成功", user.LoginName, niceName));
            js.SwitchTo().DefaultContent();
            user.isLogin = true;

            //签到
            ele = Utils.GetElementFormWebDriver(js, By.Id("signin_btn"), 3);
            if (ele != null)
            {
                ele.Click();
                this.log(String.Format("账号: {0} 已经成功签到", user.LoginName));
                return;
            }
            else
            {
                this.log(String.Format("账号: {0} 今天已经签到，不能重复签到", user.LoginName));
                return;
            }
        }

        private bool ShowLoginWindow(PhantomJSDriver js,UserModel user)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在获取显示登录界面控件 id=s_login",user.LoginName));
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.Id("s_login"),30);
                if (Utils.IsValid(ele) == false)
                {
                    this.log(String.Format("账号: {0} 登陆失败，原因: 没有找到登陆控件", user.LoginName));
                    return false;
                }
                ele.Click();
                Thread.Sleep(1000);
                this.DebugLog(String.Format("账号: {0} 获取显示登录界面控件成功 id=s_login",user.LoginName));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void DebugLog(string msg)
        {
            if (this.debug == true)
            {
                this.log(msg);
            }
        }

        private bool SwitchLoginIfrime(PhantomJSDriver js, UserModel user)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在切换登录Ifrime",user.LoginName));
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.Id("login_iframe"),30);
                if (Utils.IsValid(ele) == true)
                {
                    js.SwitchTo().Frame("login_iframe");
                    ele = Utils.GetElementFormWebDriver(js, By.Id("switcher_plogin"),30);
                    if (Utils.IsValid(ele) == true)
                    {
                        ele.Click();
                        this.DebugLog(String.Format("账号: {0} 切换登录Ifrime成功",user.LoginName));
                        return true;
                    }
                    return false;
                }
                this.log(String.Format("账号: {0} 登陆失败，原因: 没有找到QQ三方登陆Iframe", user.LoginName));
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool InputAccountAndpwd(PhantomJSDriver js,UserModel user)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在获取账号录入控件", user.LoginName));
                IWebElement loginname = Utils.GetElementFormWebDriver(js, By.Id("u"),30);
                this.DebugLog(String.Format("账号: {0} 获取账号录入控件成功", user.LoginName));
                this.DebugLog(String.Format("账号: {0} 正在获取密码录入控件", user.LoginName));
                IWebElement loginpwd = Utils.GetElementFormWebDriver(js, By.Id("p"),30);
                this.DebugLog(String.Format("账号: {0} 获取密码录入控件成功", user.LoginName));

                if (Utils.IsValid(loginname, loginpwd) == false)
                {
                    this.log(String.Format("账号: {0} 登陆失败，原因: 没有找到账号和密码控件", user.LoginName));
                    return false;
                }

                this.DebugLog(String.Format("账号: {0} 正在录入账号和密码", user.LoginName));
                loginname.Clear();
                loginpwd.Clear();
                loginname.SendKeys(user.LoginName);
                loginpwd.SendKeys(user.LoginPwd);
                this.DebugLog(String.Format("账号: {0} 录入账号和密码成功", user.LoginName));
                this.DebugLog(String.Format("账号: {0} 正在获取登录点击控件", user.LoginName));
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.Id("login_button"), 1);
                if (Utils.IsValid(ele) == true)
                {
                    ele.Click();
                    this.DebugLog(String.Format("账号: {0} 获取登录点击控件成功,触发登录过程", user.LoginName));
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool IsPasswordError(PhantomJSDriver js, UserModel user)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在获取错误提示控件", user.LoginName));
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.Id("err_m"),30);
                this.DebugLog(String.Format("账号: {0} 获取错误提示控件成功", user.LoginName));
                if (ele.Displayed == true)
                {
                    this.DebugLog(String.Format("账号: {0} 错误提示控件已显示 Displayed=True", user.LoginName));
                }
                Object obj = ((IJavaScriptExecutor)js).ExecuteScript("return document.getElementById('err_m').innerText;");
                if (obj == null || obj.ToString() == "")
                {
                    return false;
                }
                string errno = obj.ToString();
                this.log(String.Format("系统提示: 帐号: {0} 服务器返回消息: {1}", user.LoginName, errno));
                if (errno.Contains("密码不正确") == true)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string GetLoginMessage(PhantomJSDriver js, UserModel user)
        {
            try
            {
                js.SwitchTo().DefaultContent();
                js.SwitchTo().Frame("login_iframe");
                Object obj = ((IJavaScriptExecutor)js).ExecuteScript("return document.getElementById('err_m').innerText;");
                if (obj == null || obj.ToString() == "")
                {
                    return null;
                }
                string errno = obj.ToString();
                return errno;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private bool IsNeedValidCode(PhantomJSDriver js, UserModel user)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在切换主页面结构并进入login_iframe", user.LoginName));
                js.SwitchTo().DefaultContent();
                js.SwitchTo().Frame("login_iframe");
                this.DebugLog(String.Format("账号: {0} 切换主页面结构并进入login_iframe成功", user.LoginName));

                this.DebugLog(String.Format("账号: {0} 正在获取验证码Iframe id=newVcodeIframe", user.LoginName));
                IWebElement newVcodeIframe = Utils.GetElementFormWebDriver(js, By.Id("newVcodeIframe"),30);
                if (Utils.IsValid(newVcodeIframe)==false)
                {
                    this.DebugLog(String.Format("账号: {0} 获取验证码Iframe id=newVcodeIframe失败", user.LoginName));
                    return false;
                }
                this.DebugLog(String.Format("账号: {0} 获取验证码Iframe id=newVcodeIframe成功", user.LoginName));

                this.DebugLog(String.Format("账号: {0} 正在获取验证码CodeIframe name=iframe", user.LoginName));
                IWebElement ele = null;
                IWebElement codeFrame = Utils.GetElementFormElement(newVcodeIframe, By.TagName("iframe"), 3);

                if (codeFrame == null)
                {
                    this.DebugLog(String.Format("账号: {0} 获取验证码CodeIframe name=iframe失败", user.LoginName));
                    return false;
                }
                this.DebugLog(String.Format("账号: {0} 获取验证码CodeIframe name=iframe成功", user.LoginName));

                js.SwitchTo().Frame(codeFrame);

                this.DebugLog(String.Format("账号: {0} 获取验证码图片控件", user.LoginName));
                ele = Utils.GetElementFormWebDriver(js, By.Id("capImg"), 3);
                if (Utils.IsValid(ele) == false)
                {
                    this.DebugLog(String.Format("账号: {0} 获取验证码图片控件失败", user.LoginName));
                    return false;
                }
                this.DebugLog(String.Format("账号: {0} 获取验证码图片控件成功", user.LoginName));
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool ClickLoginButton(PhantomJSDriver js, UserModel user)
        {
            try
            {
                js.SwitchTo().Frame("login_iframe");
                this.DebugLog("账号: {0} 正在获取登录点击控件");
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.Id("login_button"),3);
                if (Utils.IsValid(ele) == true)
                {
                    ele.Click();
                    Thread.Sleep(3000);
                    this.DebugLog("账号: {0} 获取登录点击控件成功并触发登录过程");
                    return true;
                }
                this.log(String.Format("系统提示: 帐号: {0} 登录控件没有找到", user.LoginName));
                return false;
                
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 帐号: {0} 登录控件没有找到", user.LoginName));
                return false;
            }
        }

        private string GetLoginValidCode(PhantomJSDriver js,IWebElement codeimg,UserModel user,Dictionary<string,string> dama)
        {
            try
            {
                IWebElement ele = codeimg;
                /*js.SwitchTo().DefaultContent();
                js.SwitchTo().Frame("login_iframe");

                IWebElement newVcodeIframe = Utils.GetElementFormWebDriver(js, By.Id("newVcodeIframe"), 3);
                IWebElement codeFrame = Utils.GetElementFormElement(newVcodeIframe, By.TagName("iframe"), 3);

                js.SwitchTo().Frame(codeFrame);

                ele = Utils.GetElementFormWebDriver(js, By.Id("capImg"), 3);
                if (ele == null)
                {
                    this.log(String.Format("账号: {0} 登陆失败，原因: 没有找到验证码图像", user.LoginName));
                    return null;
                }
                IWebElement ele_reload = Utils.GetElementFormWebDriver(js, By.Id("reload"), 1);
                if (Utils.IsValid(ele_reload) == false)
                {
                    this.log(String.Format("账号: {0} 登陆失败，原因: 没有找到刷新验证码控件", user.LoginName));
                    return null;
                }
                ele_reload.Click();
                Thread.Sleep(2000);*/

                string pageImg = Utils.GetGUID() + ".jpeg";

                //验证码
                Utils.CaputureImage(js, pageImg);


                //从磁盘读取截屏文件并生成副本解除文件占用
                Bitmap bitmap = new Bitmap(pageImg);
                Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format16bppRgb555);
                Graphics draw = Graphics.FromImage(bitmap2);
                draw.DrawImage(bitmap, 0, 0);
                draw.Dispose();
                bitmap.Dispose();
                bitmap = null;


                Size size = new Size(Math.Min(ele.Size.Width, ele.Size.Width), Math.Min(ele.Size.Height, ele.Size.Height));
                size.Width = size.Width + 50;
                size.Height = size.Height + 5;
                Rectangle crop = new Rectangle(ele.Location, size);


                MemoryStream ms = new MemoryStream();
                Bitmap codeImg = bitmap2.Clone(crop, bitmap2.PixelFormat);
                codeImg.Save(ms, ImageFormat.Jpeg);
                codeImg.Save("yzm.jpeg");
                byte[] imgBuff= ms.ToArray();
                this.showimg(imgBuff);

                codeImg.Dispose();
                codeImg = null;
                bitmap2.Dispose();
                bitmap2 = null;
                ms.Close();
                ms.Dispose();
                ms = null;

                //删除图片
                File.Delete(pageImg);

                //提交参数
                var param = new Dictionary<object, object>
                        {
                            {"username",dama["username"]},
                            {"password",dama["password"]},
                            {"typeid","2040"},
                            {"timeout",dama["timeout"]},
                            {"softid",dama["softid"]},
                            {"softkey",dama["softkey"]}
                        };

                string httpResult = RuoKuaiHttp.Post("http://api.ruokuai.com/create.xml", param, imgBuff);
                string yzmCode = Utils.GetImgString(httpResult);
                if (yzmCode == null || yzmCode == "")
                {
                    this.log(String.Format("账号: {0} 操作失败，原因: 打码平台返回验证码为空", user.LoginName));
                    return null;
                }
                
                return yzmCode;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private bool SwitchValidCodeIfrime(PhantomJSDriver js, UserModel user)
        {
            try
            {
                js.SwitchTo().DefaultContent();
                js.SwitchTo().Frame("login_iframe");

                IWebElement newVcodeIframe = Utils.GetElementFormWebDriver(js, By.Id("newVcodeIframe"), 3);
                if (Utils.IsValid(newVcodeIframe) == false)
                {
                    return false;
                }

                IWebElement codeFrame = Utils.GetElementFormElement(newVcodeIframe, By.TagName("iframe"), 3);

                if (codeFrame == null)
                {
                    return false;
                }

                js.SwitchTo().Frame(codeFrame);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
