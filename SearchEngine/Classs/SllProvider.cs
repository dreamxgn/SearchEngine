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
    public class SllProvider
    {
        public delegate void Log(string msg);
        public delegate void ShowImg(byte[] buff,int imgIndex);

        private Log log;
        private ShowImg showimg;
        private bool debug;

        public SllProvider(Log _log,ShowImg _showimg)
        {
            this.log = _log;
            this.showimg = _showimg;
            this.debug = false;
        }

        public void EnableDebug()
        {
            this.debug = true;
        }

        public void DisableDebug()
        {
            this.debug = false;
        }

        private void DebugLog(string msg)
        {
            if (this.debug == true)
            {
                this.log(msg);
            }
        }


        #region 登录360问答
        /// <summary>
        /// 登录360问答
        /// </summary>
        /// <param name="js">PhantomJSDriver对象</param>
        /// <param name="jsService">PhantomJSDriverService 对象</param>
        /// <param name="sleep">操作延时</param>
        /// <param name="user">登录用户</param>
        /// <param name="dama">若快打码</param>
        public void Login(PhantomJSDriver js, UserModel user, Dictionary<string, string> dama)
        {
            this.DebugLog(String.Format("账号: {0} 正在登陆", user.LoginName));

            js.Manage().Cookies.DeleteAllCookies();
            Utils.GoUrl(js, "http://wenda.so.com/",2000);
            user.isLogin = false;


            //显示登陆窗口
            if (this.ClickLoginWindowShow(js, user) == false)
            {
                return;
            }

            //输入账号和密码
            if (this.InputAccountandPwd(js, user) == false)
            {
                return;
            }

            //点击登陆
            if (this.ClickLogin(js, user) == false)
            {
                return;
            }

            //登陆失败
            string errno = this.GetErrorMessage(js, user);
            if (errno == null || errno=="")
            {
                Utils.GoUrl(js, "http://wenda.so.com/", 2000);
                //是否登陆成功
                if (this.IsLoginSucess(js,user) == true)
                {
                    this.LoginSucess(js, user);
                    return;
                }
                this.log(String.Format("系统提示: 账号: {0} 系统异常无法确定登录状态...", user.LoginName));
                return;
            }

            //如果是验证码输入错误则重试
            if (errno.Contains("请输入验证码") == true)
            {
                this.DebugLog(String.Format("账号: {0} 登陆需要输入验证码", user.LoginName));

                int codeCount = 0; //验证码输入次数

                while (true)
                {
                    codeCount++;
                    if (codeCount > 3)
                    {
                        this.log(String.Format("系统提示: 账号: {0} 验证码超过最大重试次数,已跳过...", user.LoginName));
                        return;
                    }

                    this.DebugLog(String.Format("账号: {0} 验证码登陆尝试第{0}次", user.LoginName,codeCount));
                    if (this.ProcessValidCode(js, user, dama) == true)
                    {
                        //点击登陆
                        if (this.ClickLogin(js, user) == false)
                        {
                            return;
                        }

                        errno = this.GetErrorMessage(js, user);
                        if (errno == null || errno == "")
                        {
                            Utils.GoUrl(js, "http://wenda.so.com/", 1000);
                            //是否登陆成功
                            if (this.IsLoginSucess(js,user) == true)
                            {
                                this.LoginSucess(js, user);
                                return;
                            }
                            this.log(String.Format("系统提示: 账号: {0} 系统异常无法确定登录状态...", user.LoginName));
                            return;
                        }

                        //验证码错误(重试)
                        if (errno.Contains("验证码") == true)
                        {
                            continue;
                        }
                        //其它错误则一律登陆失败处理
                        break;
                    }
                    else
                    {
                        //登陆失败
                        break;
                    }
                }
            }

            //其它则输出错误日志并退出
            this.log(String.Format("系统提示: 账号 {0} 登陆失败,原因: {1}", user.LoginName, errno));
            user.isLogin = false;
            return;
        }
        #endregion

        #region 360问答 提问
        /// <summary>
        /// 360问答 提问
        /// </summary>
        /// <param name="js">PhantomJSDriver</param>
        /// <param name="jsService">PhantomJSDriverService</param>
        /// <param name="sleep">操作延时</param>
        /// <param name="user">提问用户</param>
        /// <param name="quesion">问题</param>
        /// <param name="isrepeat">是否允许用户重复提问</param>
        /// <param name="dama">若快打码</param>
        /// <returns></returns>
        public AskModel SendQuestion(PhantomJSDriver js, UserModel user, QuestionInfoModel quesion, Dictionary<string, string> dama,out bool outerr)
        {
            this.DebugLog(String.Format("系统提示: 账号: {0} 开始提交问题 {1}", user.LoginName,quesion.title));

            outerr = true;//outer=true 那么如该问题提交失败则更换账号重试

            //进入提问页面
            if (this.ClickAskQuesiton(js, user) == false)
            {
                return null;
            }

            //输入标题和内容
            if (this.InputAskTitleAndContent(js, user, quesion) == false)
            {
                return null;
            }

            //提交
            if (this.ClickSubmitAsk(js, user) == false)
            {
                return null;
            }

            //提问规则检测
            if (this.AskContentValid(js, user) == false)
            {
                outerr = false; //返回false表示这个问题不再重试提交了
                return null;
            }

            //检测验证码处理
            if (this.IsNeedValidCode(js, user) == true)
            {
                int errCount = 0; //3次验证码输入错误则，跳过

                while (true)
                {
                    errCount++;
                    if (errCount > 3)
                    {
                        this.log(String.Format("系统提示: 账号: {0} 提问失败 原因: 连续3次验证码输入错误，已跳过", user.LoginName));
                        outerr = false;
                        return null;
                    }

                    this.DebugLog(String.Format("账号: {0} 提交问题 第{1}次验证码尝试", user.LoginName, errCount));

                    //验证码校验
                    if (this.ProcessAskValidCode(js, user, dama) == false)
                    {
                        outerr = false;
                        return null;
                    }

                    if (this.ClickSubmitAsk(js, user) == false)
                    {
                        return null; 
                    }
                    //提问规则检测
                    if (this.AskContentValid(js, user) == false)
                    {
                        outerr = false; //返回false表示这个问题不再重试提交了
                        return null;
                    }

                    IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//span[@class=\"captcha-error js-captcha-error\"]"),3);
                    if (Utils.IsValid(ele) == true)
                    {
                        string err = ele.Text;
                        if (err.Contains("验证码错误") == true)
                        {
                            this.log(String.Format("系统提示: 账号: {0} 提问验证码输入错误，重试中...", user.LoginName));
                            continue;
                        }
                        this.log(String.Format("系统提示: 账号: {0} 提问失败,原因: {1}", user.LoginName,err));
                        outerr = false;
                        return null;
                    }

                    //验证是否提问成功
                    string ids = this.IsAskSucess(js, user);
                    if (ids != null && ids != "")
                    {
                        //提问成功
                        //写入数据库
                        AskModel ask = new AskModel();
                        ask.Id = Utils.GetGUID();
                        ask.qid = quesion.Id;
                        ask.uid = user.Id;
                        ask.goodaid = "";
                        ask.qurl = "http://wenda.so.com/q/"+ ids;
                        ask.qurlId = ids;
                        ask.category = 2;
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
                        this.log(String.Format("账号: {0} 自动提问成功,地址: {1}", user.LoginName, ask.qurl));
                        return ask;
                    }
                    return null;
                }
            }


            //验证是否提问成功
            string id = this.IsAskSucess(js, user);
            if (id != null && id != "")
            {
                //提问成功
                //写入数据库
                AskModel ask = new AskModel();
                ask.Id = Utils.GetGUID();
                ask.qid = quesion.Id;
                ask.uid = user.Id;
                ask.goodaid = "";
                ask.qurl = "http://wenda.so.com/q/" + id;
                ask.qurlId = id;
                ask.category = 2;
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
                this.log(String.Format("账号: {0} 自动提问成功,地址: {1}", user.LoginName,ask.qurl));
                return ask;
            }
            return null;
        }
        #endregion

        #region 360问答 回答
        /// <summary>
        /// 360问答 回答
        /// </summary>
        /// <param name="js">PhantomJSDriver</param>
        /// <param name="jsService">PhantomJSDriverService</param>
        /// <param name="sleep">操作延时</param>
        /// <param name="user">提问用户</param>
        /// <param name="answerContent">回答内容</param>
        /// <param name="ask">提问对象</param>
        public bool Answer(PhantomJSDriver js, UserModel user, string answerContent, AskModel ask,Dictionary<string,string> dama,out bool err)
        {
            Utils.GoUrl(js,ask.qurl, 2000);

            err = false; //返回True则跳过

            //输入回答内容
            if (this.WriteAnswerContent(js, user, answerContent) == false)
            {
                return false;
            }

            //点击提交
            if (this.ClickSubmitAnswer(js, user) == false)
            {
                return false;
            }

            //检测是否回答内容规则限制
            if (this.AsnwerContentValid(js, user) == false)
            {
                err = true;
                return false;
            }

            //检测是否需要验证码
            if (this.IsNeedAnserValidCode(js, user) == true)
            {
                //验证码处理
                int codeCount = 0; //重试次数
                while (true)
                {
                    codeCount++;
                    if (codeCount > 3)
                    {
                        return false;
                    }

                    this.DebugLog(String.Format("账号: {0} 当前验证码重试次数: {1}", user.LoginName, codeCount));

                    if (this.ProcessAnswerValidCode(js, user, dama) == true)
                    {
                        //点击提交
                        if (this.ClickSubmitAnswer(js, user) == true)
                        {
                            if (this.IsNeedAnserValidCode(js, user) == true)
                            {
                                continue;
                            }
                            //检测是否回答内容规则限制
                            if (this.AsnwerContentValid(js, user) == false)
                            {
                                err = true;
                                return false;
                            }
                            break;
                        }
                        return false;
                    }
                    return false;
                }
            }

            //检测是否回答内容规则限制
            if (this.AsnwerContentValid(js, user) == false)
            {
                err = true;
                return false;
            }
            return this.AnswerProcessSuccess(js, user, ask);
        }
        #endregion

        #region 360问答 点赞
        /// <summary>
        /// 360问答 点赞
        /// </summary>
        /// <param name="js"></param>
        /// <param name="jsService"></param>
        /// <param name="sleep"></param>
        /// <param name="user"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        public bool Mark(PhantomJSDriver js, UserModel user, AnswerModel answer)
        {
            try
            {
                Utils.GoUrl(js, answer.QuesionUrl, 3);

                this.DebugLog(String.Format("账号: {0} 正在获取点赞数据控件", user.LoginName));
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath(String.Format("//li[@ans_id=\"{0}\"]", answer.AnswerID)), 2);
                if (Utils.IsValid(ele) == false)
                {
                    this.log(String.Format("系统提示: 帐号{0} 点赞失败 地址: {1} 原因: 未找到点赞数据",user.LoginName ,answer.QuesionUrl));
                    return false;
                }
                this.DebugLog(String.Format("账号: {0} 获取点赞数据控件成功", user.LoginName));

                this.DebugLog(String.Format("账号: {0} 正在获取点触发控件", user.LoginName));
                IWebElement link = Utils.GetElementFormElement(ele, By.XPath("//a[@class=\"approve js-approve\"]"), 2);
                if (Utils.IsValid(ele) == false)
                {
                    this.log(String.Format("系统提示: 帐号{0} 点赞失败 地址: {1} 原因: 未找到点赞链接link", user.LoginName,answer.QuesionUrl));
                    return false;
                }

                link.Click();
                Thread.Sleep(2000);
                this.DebugLog(String.Format("账号: {0} 获取点触发控件成功，并已触发", user.LoginName));

                Utils.UpdateMark(answer,2);
                this.log(String.Format("系统提示: 帐号{0} 点赞成功 地址: {1}", user.LoginName,answer.QuesionUrl));
                return true;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 帐号{0} 点赞失败 地址: {1} 原因: {1}", user.LoginName,answer.QuesionUrl, ex.Message + "---" + ex.StackTrace));
                return false;
            }
        }
        #endregion

        #region 360问答 选择最佳答案
        /// <summary>
        /// 360问答 选择最佳答案
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
                Utils.GoUrl(js, answer.QuesionUrl, 3);

                string s1= "{\"nodeName\":\"js-set-confirm\",ask_id:"+answer.QusionUrlId+",ans_id:"+answer.AnswerId+",url:\"/submit/chosebest/\"}";

                this.DebugLog(String.Format("账号: 正在获取选择最佳答案触发控件", user.LoginName));
                IWebElement link = Utils.GetElementFormWebDriver(js, By.XPath("//a[@data='"+s1+"']"), 2);
                if (Utils.IsValid(link) == false)
                {
                    this.log(String.Format("系统提示: 帐号{0} 选择答案失败 地址: {1} 原因: 未找到回答数据", user.LoginName, answer.QuesionUrl));
                    return false;
                }

                link.Click();
                Thread.Sleep(2000);

                this.DebugLog(String.Format("账号: 获取选择最佳答案触发控件成功，并已触发", user.LoginName));

                Utils.UpdateGoodAnswer(answer,2);
                this.log(String.Format("回答ID：{0} 提问地址：{1}  采纳最佳答案成功", answer.AnswerId, answer.QuesionUrl));
                return true;

            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 帐号{0} 选择答案失败 地址: {1} 原因: {1}", user.LoginName, answer.QuesionUrl, ex.Message + "---" + ex.StackTrace));
                return false;
            }
        }
        #endregion

        #region 获取登陆是否需要验证码
        /// <summary>
        /// 获取登陆是否需要验证码
        /// </summary>
        /// <param name="js"></param>
        /// <returns></returns>
        private bool IsLoginValidCode(IWebDriver js)
        {
            IWebElement ele1 = Utils.GetElementFormWebDriver(js, By.XPath("//input[@class=\"quc-input quc-input-captcha\"]"),1);
            IWebElement ele2 = Utils.GetElementFormWebDriver(js, By.XPath("//img[@class=\"quc-captcha-img quc-captcha-change\"]"),1);
            if (Utils.IsValid(ele1,ele2) == true)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 获取登陆360问答是否成功
        /// <summary>
        /// 获取登陆360问答是否成功
        /// </summary>
        /// <param name="js"></param>
        /// <returns></returns>
        private bool IsLoginSucess(IWebDriver js,UserModel user)
        {
            this.DebugLog(String.Format("账号: {0} 正在获取账号昵称控件", user.LoginName));
            IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//a[@class=\"user-name\"]"),3);
            if (Utils.IsValid(ele) == false)
            {
                return false;
            }
            this.DebugLog(String.Format("账号: {0} 获取账号昵称控件成功", user.LoginName));
            return true;
        }
        #endregion

        #region 获取登陆验证码
        /// <summary>
        /// 获取登陆验证码
        /// </summary>
        /// <param name="js"></param>
        /// <param name="user"></param>
        /// <param name="dama"></param>
        /// <returns></returns>
        private string GetValidCode(PhantomJSDriver js,UserModel user,Dictionary<string,string> dama)
        {
            this.DebugLog(String.Format("账号: {0} 正在获取验证码图片控件", user.LoginName));
            IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//img[@class=\"quc-captcha-img quc-captcha-change\"]"), 2);
            if (Utils.IsValid(ele) == false)
            {
                this.log(String.Format("系统提示: 账号: {0} 登陆失败,原因: 没有找到验证码控件", user.LoginName));
                return null;
            }
            this.DebugLog(String.Format("账号: {0} 获取验证码图片控件成功", user.LoginName));



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
            size.Height = size.Height;
            Point location = new Point(528, 1010);
            Rectangle crop = new Rectangle(location, size);
            MemoryStream ms = new MemoryStream();
            Bitmap validCode = bitmap2.Clone(crop, bitmap2.PixelFormat);
            validCode.Save(ms, ImageFormat.Jpeg);
            this.showimg(ms.ToArray(),1);

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
                            {"typeid","3000"},
                            {"timeout",dama["timeout"]},
                            {"softid",dama["softid"]},
                            {"softkey",dama["softkey"]}
                        };
            this.DebugLog(String.Format("账号: {0} 正在从打码平台获取验证码", user.LoginName));
            string httpResult = RuoKuaiHttp.Post("http://api.ruokuai.com/create.xml", param, ms.ToArray());
            string yzmCode = Utils.GetImgString(httpResult);
            if (yzmCode == null || yzmCode == "")
            {
                this.log(String.Format("系统提示 账号: {0} 获取验证码失败，原因: 打码平台返回验证码为空", user.LoginName));
                return null;
            }
            this.DebugLog(String.Format("账号: {0} 打码平台识别验证码为: {1}", user.LoginName,yzmCode));
            ms.Close();
            ms.Dispose();
            return yzmCode;
        }
        #endregion

        #region 获取提交问题验证码
        /// <summary>
        /// 获取提交问题验证码
        /// </summary>
        /// <param name="js"></param>
        /// <param name="user"></param>
        /// <param name="dama"></param>
        /// <returns></returns>
        private string GetAskValidCode(PhantomJSDriver js, UserModel user, Dictionary<string, string> dama)
        {
            IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//img[@class=\"js-captcha-btn\"]"), 2);
            if (Utils.IsValid(ele) == false)
            {
                this.log(String.Format("系统提示: 账号: {0} 提交问题失败,原因: 没有找到验证码控件", user.LoginName));
                return null;
            }

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
            size.Height = size.Height;
            Point location = new Point(ele.Location.X,ele.Location.Y);
            Rectangle crop = new Rectangle(location, size);
            MemoryStream ms = new MemoryStream();
            Bitmap validCode = bitmap2.Clone(crop, bitmap2.PixelFormat);
            validCode.Save(ms, ImageFormat.Jpeg);
            validCode.Save("yzm.jpeg");
            this.showimg(ms.ToArray(),2);

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
                            {"typeid","3040"},
                            {"timeout",dama["timeout"]},
                            {"softid",dama["softid"]},
                            {"softkey",dama["softkey"]}
                        };
            this.DebugLog(String.Format("账号: {0} 正在从打码平台获取验证码", user.LoginName));
            string httpResult = RuoKuaiHttp.Post("http://api.ruokuai.com/create.xml", param, ms.ToArray());
            string yzmCode = Utils.GetImgString(httpResult);
            if (yzmCode == null || yzmCode == "")
            {
                this.log(String.Format("系统提示 账号: {0} 获取验证码失败，原因: 打码平台返回验证码为空", user.LoginName));
                return null;
            }

            ms.Close();
            ms.Dispose();
            this.DebugLog(String.Format("账号: {0} 打码平台获取验证码为: {1}", user.LoginName,yzmCode));
            return yzmCode;
        }
        #endregion

        private bool ProcessValidCode(PhantomJSDriver js,UserModel user,Dictionary<string,string> dama)
        {
            try
            {
                IWebElement ele = null;
                string yzmCode = this.GetValidCode(js, user, dama);
                if (yzmCode == null || yzmCode == "")
                {
                    return false;
                }

                this.DebugLog(String.Format("账号: {0} 正在获取验证码输入控件", user.LoginName));
                ele = Utils.GetElementFormWebDriver(js, By.XPath("//input[@class=\"quc-input quc-input-captcha\"]"), 2);
                if (Utils.IsValid(ele) == false)
                {
                    this.log(String.Format("系统提示: 账号: {0} 登陆失败,原因: 验证码控件未获取到", user.LoginName));
                    return false;
                }

                ele.Clear();
                ele.SendKeys(yzmCode);

                this.DebugLog(String.Format("账号: {0} 获取验证码输入控件成功，并已输入验证码", user.LoginName));
                return true;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 登陆失败,原因: {1}", user.LoginName, ex.Message + "---" + ex.StackTrace));
                return false;
            }
        }

        private bool ClickLogin(PhantomJSDriver js,UserModel user)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在获取登陆触发控件", user.LoginName));
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//input[@class=\"quc-submit quc-button quc-button-sign-in\"]"), 3);
                if (Utils.IsValid(ele) == false)
                {
                    this.log(String.Format("系统提示: 账号: {0} 登陆失败,原因: 登陆控件未获取到", user.LoginName));
                    return false;
                }
                ele.Click();
                Thread.Sleep(2000);
                this.DebugLog(String.Format("账号: {0} 获取登陆触发控件成功，并已触发登陆过程", user.LoginName));
                return true;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 登陆失败,原因: {1}", user.LoginName, ex.Message + "---" + ex.StackTrace));
                return false;
            }
        }

        private bool ClickLoginWindowShow(PhantomJSDriver js, UserModel user)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在获取登陆界面触发控件", user.LoginName));
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//a[@class='btn btn-1 js-user-login']"), 3);
                if (Utils.IsValid(ele) == false)
                {
                    this.log(String.Format("系统提示: 账号: {0} 登陆失败,原因: 登陆界面触发控件未获取到", user.LoginName));
                    return false;
                }
                ele.Click();
                Thread.Sleep(2000);
                this.DebugLog(String.Format("账号: {0} 获取登陆界面触发控件成功,并已触发", user.LoginName));
                return true;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 登陆失败,原因: {1}", user.LoginName, ex.Message));
                return false;
            }
        }

        private bool InputAccountandPwd(PhantomJSDriver js, UserModel user)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在获取账号输入控件", user.LoginName));
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//input[@class=\"quc-input quc-input-account\"]"), 3);
                if (Utils.IsValid(ele) == false)
                {
                    this.log(String.Format("系统提示: 账号: {0} 登陆失败,原因: 账号控件未获取到", user.LoginName));
                    return false;
                }

                try
                {
                    ele.Clear();
                    ele.SendKeys(user.LoginName);
                }
                catch (Exception ex) { }

                this.DebugLog(String.Format("账号: {0} 获取账号输入控件成功，并已输入账号", user.LoginName));

                this.DebugLog(String.Format("账号: {0} 正在获取密码输入控件", user.LoginName));
                ele = Utils.GetElementFormWebDriver(js, By.XPath("//input[@class=\"quc-input quc-input-password\"]"), 3);
                if (Utils.IsValid(ele) == false)
                {
                    this.log(String.Format("系统提示: 账号: {0} 登陆失败,原因: 密码控件未获取到", user.LoginName));
                    return false;
                }

                try
                {
                    ele.Clear();
                    ele.SendKeys(user.LoginPwd);
                }
                catch (Exception ex) { }


                this.DebugLog(String.Format("账号: {0} 获取密码输入控件成功，并已输入密码", user.LoginName));
                Thread.Sleep(200);
                return true;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 登陆失败,原因: {1}", user.LoginName,ex.Message+"---"+ex.StackTrace));
                return false;
            }
        }

        private void LoginSucess(PhantomJSDriver js,UserModel user)
        {
            try
            {
                //登陆成功
                string label = Utils.GetElementFormWebDriver(js, By.XPath("//a[@class=\"user-name\"]"),3).Text;
                this.log(String.Format("系统提示: 账号: {0} 昵称: {1}  登陆成功", user.LoginName, label));
                user.isLogin = true;
                return;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 登陆失败", user.LoginName));
                return;
            }
        }

        private string GetErrorMessage(PhantomJSDriver js,UserModel user)
        {
            try
            {
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//p[@class=\"quc-tip quc-tip-error\"]"), 2);
                string error = "";
                if (Utils.IsValid(ele) == true)
                {
                    error = ele.Text;
                }
                return error;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 登陆失败,原因: {1}", user.LoginName, ex.Message));
                return null;
            }
        }

        private bool ClickAskQuesiton(PhantomJSDriver js,UserModel user)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在获取提问界面导航控件", user.LoginName));
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//a[@class=\"ask-btn js-com-ask-btn\"]"), 2);
                if (Utils.IsValid(ele) == true)
                {
                    ele.Click();
                    Thread.Sleep(2000);
                    this.DebugLog(String.Format("账号: {0} 获取提问界面导航控件成功，并已触发", user.LoginName));
                    return true;
                }
                this.log(String.Format("系统提示: 账号: {0} 提问失败,原因: 访问提问界面失败", user.LoginName));
                return false;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 提问失败,原因: {1}", user.LoginName, ex.Message));
                return false;
            }
        }


        private bool InputAskTitleAndContent(PhantomJSDriver js, UserModel user, QuestionInfoModel qinfo)
        {
            try
            {
                //标题
                this.DebugLog(String.Format("账号: {0} 正在获取提问标题输入控件", user.LoginName));
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//input[@class=\"ask-title js-ask-title\"]"),30);
                if (Utils.IsValid(ele) == false)
                {
                    this.log(String.Format("系统提示: 账号: {0} 提问失败,原因: 未找到标题控件", user.LoginName));
                    return false;
                }

                try
                {
                    ele.Clear();
                    ele.SendKeys(qinfo.title);
                }
                catch (Exception ex) { }

                this.DebugLog(String.Format("账号: {0} 获取提问标题输入控件成功，并已输入提问标题", user.LoginName));

                //内容
                this.DebugLog(String.Format("账号: {0} 正在获取提问内容输入控件", user.LoginName));
                ele = Utils.GetElementFormWebDriver(js, By.Id("askUmeditor"),10);
                if (Utils.IsValid(ele) == false)
                {
                    this.log(String.Format("系统提示: 账号: {0} 提问失败,原因: 未找到内容控件", user.LoginName));
                    return false;
                }

                try
                {
                    ele.Clear();
                    ele.SendKeys(qinfo.content);
                }
                catch (Exception ex) { }
                this.DebugLog(String.Format("账号: {0} 获取提问内容输入控件成功，并已输入提问内容", user.LoginName));

                return true;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 提问失败,原因: {1}", user.LoginName, ex.Message + "---" + ex.StackTrace));
                return false;
            }
        }

        private bool ClickSubmitAsk(PhantomJSDriver js,UserModel user)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在获取问题提交控件", user.LoginName));
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//input[@class=\"btn btn-2 fr js-ask-submit\"]"),30);
                if (Utils.IsValid(ele) == true)
                {
                    ele.Click();
                    Thread.Sleep(2000);
                    this.DebugLog(String.Format("账号: {0} 获取问题提交控件成功,并已触发", user.LoginName));
                    return true;
                }
                this.log(String.Format("系统提示: 账号: {0} 提问失败,原因: 未找到问题提交控件", user.LoginName));
                return false;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 提问失败,原因: {1}", user.LoginName, ex.Message));
                return false;
            }
        }

        private bool IsNeedValidCode(PhantomJSDriver js, UserModel user)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在获取验证码输入控件", user.LoginName));
                IWebElement ele_input = Utils.GetElementFormWebDriver(js, By.XPath("//input[@class=\"js-captcha-input\"]"),2);

                this.DebugLog(String.Format("账号: {0} 正在获取验证码图片控件", user.LoginName));
                IWebElement ele_img = Utils.GetElementFormWebDriver(js, By.XPath("//img[@class=\"js-captcha-btn\"]"),2);

                if (Utils.IsValid(ele_input,ele_img) == true)
                {
                    this.DebugLog(String.Format("账号: {0} 获取验证码输入控件成功", user.LoginName));
                    this.DebugLog(String.Format("账号: {0} 获取验证码图片控件成功", user.LoginName));
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 提问失败,原因: {1}", user.LoginName, ex.Message + "---" + ex.StackTrace));
                return false;
            }
        }

        private bool ProcessAskValidCode(PhantomJSDriver js, UserModel user, Dictionary<string, string> dama)
        {
            try
            {
                IWebElement ele = null;
                string yzmCode = this.GetAskValidCode(js, user, dama);
                if (yzmCode == null || yzmCode == "")
                {
                    return false;
                }

                ele = Utils.GetElementFormWebDriver(js, By.XPath("//input[@class=\"js-captcha-input\"]"), 2);
                if (Utils.IsValid(ele) == false)
                {
                    this.log(String.Format("系统提示: 账号: {0} 提交问题失败,原因: 验证码控件未获取到", user.LoginName));
                    return false;
                }

                ele.Clear();
                ele.SendKeys(yzmCode);
                return true;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 提交问题失败,原因: {1}", user.LoginName, ex.Message + "---" + ex.StackTrace));
                return false;
            }
        }

        private bool AskContentValid(PhantomJSDriver js, UserModel user)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在获取提问错误提示控件", user.LoginName));

                IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//div[@class=\"error-msg js-error-msg\"]"), 2);
                if (Utils.IsValid(ele) == true)
                {
                    this.DebugLog(String.Format("账号: {0} 获取提问错误提示控件成功", user.LoginName));
                    string label = ele.Text;
                    if (label == null)
                    {
                        this.log(String.Format("系统提示: 账号: {0} 提问失败,原因: 内容未通过审核", user.LoginName));
                        return false;
                    }
                    this.log(String.Format("系统提示: 账号: {0} 提问失败,原因: {1}", user.LoginName,label));
                    return false;
                }

                this.DebugLog(String.Format("账号: {0} 获取提问错误提示控件失败", user.LoginName));
                return true;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 提问失败,原因: {1}", user.LoginName, ex.Message));
                return false;
            }
        }

        private string IsAskSucess(PhantomJSDriver js,UserModel user)
        {
            try
            {
                Thread.Sleep(6000);
                Regex re = new Regex(@"<div class=""mod-q js-form"" ask_id=""(\d+)""");
                if (re.IsMatch(js.PageSource) == true)
                {
                    return re.Match(js.PageSource).Groups[1].Value;
                }
                return null;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 提问失败,原因: {1}", user.LoginName, ex.Message + "---" + ex.StackTrace));
                return null;
            }
        }

        private bool WriteAnswerContent(PhantomJSDriver js, UserModel user, string answerContent)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在获取回答内容输入控件", user.LoginName));
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.Id("detailUmeditor"),30);
                if (Utils.IsValid(ele) == false)
                {
                    this.log(String.Format("系统提示: 账号: {0} 回答失败,原因: 回答输入控件未找到", user.LoginName));
                    return false;
                }

                try
                {
                    ele.SendKeys(answerContent);
                }
                catch (Exception ex) { }

                this.DebugLog(String.Format("账号: {0} 获取回答内容输入控件，并已输入回答内容", user.LoginName));
                return true;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        private bool ClickSubmitAnswer(PhantomJSDriver js, UserModel user)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在获取回答提交触发控件", user.LoginName));
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.Id("submit"),30);
                if (Utils.IsValid(ele) == false)
                {
                    this.log(String.Format("系统提示: 账号: {0} 回答失败,原因: 回答提交控件未找到", user.LoginName));
                    return false;
                }
                ele.Click();
                Thread.Sleep(2000);
                this.DebugLog(String.Format("账号: {0} 获取回答提交触发控件成功,并已触发", user.LoginName));
                return true;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 回答失败,原因: {1}", user.LoginName, ex.Message + "---" + ex.StackTrace));
                return false;
            }
        }

        private bool ProcessAnswerValidCode(PhantomJSDriver js, UserModel user,Dictionary<string,string> dama)
        {
            try
            {
                string yzmCode = this.GetAskValidCode(js, user, dama);
                if (yzmCode == null || yzmCode == "")
                {
                    return false;
                }

                IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//input[@class=\"js-captcha-input\"]"), 2);
                if (Utils.IsValid(ele) == true)
                {
                    ele.Clear();
                    ele.SendKeys(yzmCode);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string GetAnserValidCode(PhantomJSDriver js, UserModel user,Dictionary<string,string> dama)
        {
            try
            {
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//img[@class=\"js-captcha-btn\"]"), 2);
                if (Utils.IsValid(ele) == false)
                {
                    this.log(String.Format("系统提示: 账号: {0} 回答失败,原因: 没有找到验证码控件", user.LoginName));
                    return null;
                }



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
                size.Height = size.Height;
                Point location = new Point(528, 1010);
                Rectangle crop = new Rectangle(location, size);
                MemoryStream ms = new MemoryStream();
                Bitmap validCode = bitmap2.Clone(crop, bitmap2.PixelFormat);
                validCode.Save(ms, ImageFormat.Jpeg);
                validCode.Save("yzm.jpeg");

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
                            {"typeid","2000"},
                            {"timeout",dama["timeout"]},
                            {"softid",dama["softid"]},
                            {"softkey",dama["softkey"]}
                        };

                string httpResult = RuoKuaiHttp.Post("http://api.ruokuai.com/create.xml", param, ms.ToArray());
                string yzmCode = Utils.GetImgString(httpResult);
                if (yzmCode == null || yzmCode == "")
                {
                    this.log(String.Format("系统提示 账号: {0} 获取验证码失败，原因: 打码平台返回验证码为空", user.LoginName));
                    return null;
                }

                ms.Close();
                ms.Dispose();
                return yzmCode;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private bool IsNeedAnserValidCode(PhantomJSDriver js, UserModel user)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在获取验证码录入控件", user.LoginName));
                IWebElement ele_input = Utils.GetElementFormWebDriver(js, By.XPath("//input[@class=\"js-captcha-input\"]"), 1);

                this.DebugLog(String.Format("账号: {0} 正在获取验证码图片控件", user.LoginName));
                IWebElement ele_img = Utils.GetElementFormWebDriver(js, By.XPath("//img[@class=\"js-captcha-btn\"]"), 1);

                if (Utils.IsValid(ele_input, ele_img) == true)
                {
                    this.DebugLog(String.Format("账号: {0} 获取验证码录入控件成功", user.LoginName));
                    this.DebugLog(String.Format("账号: {0} 获取验证码图片控件成功", user.LoginName));
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool AsnwerContentValid(PhantomJSDriver js, UserModel user)
        {
            try
            {
                this.DebugLog(String.Format("账号: {0} 正在获取回答错误提示控件", user.LoginName));
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//span[@class=\"err-cnt\"]"), 2);
                if (Utils.IsValid(ele) == true)
                {
                    this.DebugLog(String.Format("账号: {0} 获取回答错误提示控件成功", user.LoginName));
                    string label = ele.Text;
                    if (label == null || label=="")
                    {
                        return true;
                    }
                    this.log(String.Format("系统提示: 账号: {0} 回答失败,原因: {1}", user.LoginName, label));
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 回答失败,原因: {1}", user.LoginName, ex.Message + "---" + ex.StackTrace));
                return false;
            }
        }

        private bool AnswerProcessSuccess(PhantomJSDriver js, UserModel user,AskModel ask)
        {
            try
            {
                Utils.GoUrl(js, ask.qurl,5);

                this.DebugLog(String.Format("账号: {0} 正在获取回答ID信息控件", user.LoginName));
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.XPath("//a[@class=\"js-append-form add-ans\"]"), 2);
                if (Utils.IsValid(ele) == true)
                {
                    //回答成功
                    this.DebugLog(String.Format("账号: {0} 获取回答ID信息控件成功", user.LoginName));
                    string str = ele.GetAttribute("data");
                    Regex re = new Regex(@"ans_id:(\d+)");
                    if (re.IsMatch(str) == true)
                    {
                        string aid = re.Match(str).Groups[1].Value;
                        SQLiteHelper.ExecuteNonQuery(String.Format(@"insert into AnswerData(Id,qid,uid,aid,category,savedate)values('{0}','{1}','{2}','{3}',{4},'{5}')",
                Utils.GetGUID(),
                ask.Id,
                user.Id,
                aid,
                2,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                ), null);
                        this.log(String.Format("问题ID: {0} 操作成功，地址: {1}，回答ID: {2} 回答账号: {3}", ask.Id, ask.qurl, aid, user.LoginName));
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 账号: {0} 回答失败,原因: {1}", user.LoginName, ex.Message + "---" + ex.StackTrace));
                return false;
            }
        }

    }
}
