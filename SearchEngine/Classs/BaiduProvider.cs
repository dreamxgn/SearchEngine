using OpenQA.Selenium;
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
    public class BaiduProvider
    {
        public delegate void Log(string msg);
        public delegate void ShowImg(byte[] buff);

        private Log log;
        private ShowImg showimg;

        public BaiduProvider(Log _log, ShowImg _showimg)
        {
            this.log = _log;
            this.showimg = _showimg;
        }

        #region 登录百度
        /// <summary>
        /// 登录百度
        /// </summary>
        /// <param name="js">PhantomJSDriver对象</param>
        /// <param name="jsService">PhantomJSDriverService 对象</param>
        /// <param name="sleep">操作延时</param>
        /// <param name="user">登录用户</param>
        /// <param name="dama">若快打码</param>
        public void Login(PhantomJSDriver js, UserModel user, Dictionary<string, string> dama)
        {
            this.log(String.Format("帐号：{0} 正在登录", user.LoginName));
            js.SwitchTo().DefaultContent();
            js.Manage().Cookies.DeleteAllCookies();
            user.isLogin = false;


            Utils.GoUrl(js, "https://zhidao.baidu.com/",2000);

            IWebElement ele = Utils.GetElementFormWebDriver(js, By.Id("userbar-login"), 3);
            if (Utils.IsValid(ele) == false)
            {
                this.log(String.Format("系统提示: 帐号: {0} 显示登录控件没有找到", user.LoginName));
                return;
            }
            ele.Click();

            if (this.InputAccountAndPwd(js, user) == false)
            {
                return;
            }

            if (this.ClickLoginButton(js, user) == false)
            {
                return;
            }

            string msg = "";

            //登录验证码
            if (this.IsNeedLoginValidCode(js, user) == true)
            {
                this.log(String.Format("系统提示: 帐号: {0} 需要登录验证码，正在处理...", user.LoginName));
                //验证码重试次数3次
                int codeCount = 0;
                while (true)
                {
                    codeCount++;
                    if (codeCount > 3)
                    {
                        this.log(String.Format("系统提示: 帐号: {0} 验证码重试次数超过3次已跳过...", user.LoginName));
                        return;
                    }

                    if (this.ProcessValidCode(js, user, dama) == false)
                    {
                        return;
                    }

                    if (this.ClickLoginButton(js, user) == false)
                    {
                        return;
                    }

                    msg = this.LoginErrorMessage(js, user);
                    if (msg != null && msg != "")
                    {
                        if (msg.Contains("密码") == true)
                        {
                            this.log(String.Format("系统提示: 帐号: {0} 登录密码错误", user.LoginName));
                            return;
                        }
                        if (msg.Contains("验证码") == true)
                        {
                            this.log(String.Format("系统提示: 帐号: {0} 验证码错误重试中...", user.LoginName));
                            Thread.Sleep(1000);
                            continue;
                        }
                        this.log(String.Format("系统提示: 帐号: {0} {1}", user.LoginName, msg));
                        return;
                    }

                    if (this.IsLoginSuccess(js, user) == true)
                    {
                        this.log(String.Format("系统提示: 帐号: {0} 登录成功", user.LoginName));
                        return;
                    }
                    else
                    {
                        this.log(String.Format("系统提示: 帐号: {0} 登录失败", user.LoginName));
                        return;
                    }


                }
            }

            //错误消息
            msg=this.LoginErrorMessage(js,user);
            if (msg != null && msg != "")
            {
                if (msg.Contains("密码有误") == true)
                {
                    this.log(String.Format("系统提示: 帐号: {0} 登录密码错误", user.LoginName));
                    return;
                }
                this.log(String.Format("系统提示: 帐号: {0} {1}", user.LoginName,msg));
                return;
            }


            if (this.IsLoginSuccess(js, user) == true)
            {
                this.log(String.Format("系统提示: 帐号: {0} 登录成功", user.LoginName));
                return;
            }
            else
            {
                this.log(String.Format("系统提示: 帐号: {0} 登录失败", user.LoginName));
                return;
            }
        }
        #endregion

        #region 百度知道 提问
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
        public AskModel SendQuestion(PhantomJSDriver js, UserModel user, QuestionInfoModel quesion, Dictionary<string, string> dama, out bool outerr)
        {
            this.log(String.Format("系统提示: 账号: {0} 开始提交问题", user.LoginName));
            outerr = true;
            return null;
        }
        #endregion

        #region 百度 回答
        /// <summary>
        /// 360问答 回答
        /// </summary>
        /// <param name="js">PhantomJSDriver</param>
        /// <param name="jsService">PhantomJSDriverService</param>
        /// <param name="sleep">操作延时</param>
        /// <param name="user">提问用户</param>
        /// <param name="answerContent">回答内容</param>
        /// <param name="ask">提问对象</param>
        public bool Answer(PhantomJSDriver js, UserModel user, string answerContent, AskModel ask, Dictionary<string, string> dama, out bool err)
        {
            Utils.GoUrl(js, ask.qurl, 300);
            err = false;
            return false;
        }
        #endregion

        #region 百度 点赞
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
                return false;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 帐号{0} 点赞失败 地址: {1} 原因: {1}", user.LoginName, answer.QuesionUrl, ex.Message + "---" + ex.StackTrace));
                return false;
            }
        }
        #endregion

        #region 百度 选择最佳答案
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
                return false;

            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 帐号{0} 选择答案失败 地址: {1} 原因: {1}", user.LoginName, answer.QuesionUrl, ex.Message + "---" + ex.StackTrace));
                return false;
            }
        }
        #endregion

        private bool InputAccountAndPwd(PhantomJSDriver js, UserModel user)
        {
            try
            {
                IWebElement account = Utils.GetElementFormWebDriver(js, By.Id("TANGRAM__PSP_8__userName"),10);
                if (Utils.IsValid(account) == false)
                {
                    this.log(String.Format("系统提示: 帐号: {0} 帐号输入控件没有找到", user.LoginName));
                    return false;
                }
                account.Clear();
                account.SendKeys(user.LoginName);

                IWebElement password = Utils.GetElementFormWebDriver(js, By.Id("TANGRAM__PSP_8__password"),10);
                if (Utils.IsValid(password) == false)
                {
                    this.log(String.Format("系统提示: 帐号: {0} 密码输入控件没有找到", user.LoginName));
                    return false;
                }
                password.Clear();
                password.SendKeys(user.LoginPwd);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool IsNeedLoginValidCode(PhantomJSDriver js, UserModel user)
        {
            try
            {
                string errStr = this.LoginErrorMessage(js, user);
                if (errStr != null)
                {
                    if (errStr.Contains("验证码")==true)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string LoginErrorMessage(PhantomJSDriver js, UserModel user)
        {
            try
            {
                Object obj= ((IJavaScriptExecutor)js).ExecuteScript("return document.getElementById('TANGRAM__PSP_8__error').innerText", null);
                if (obj != null)
                {
                    string errStr = obj.ToString();
                    return errStr;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private bool ClickLoginButton(PhantomJSDriver js, UserModel user)
        {
            try
            {
                IWebElement btn_login = Utils.GetElementFormWebDriver(js, By.Id("TANGRAM__PSP_8__submit"), 3);
                if (Utils.IsValid(btn_login) == true)
                {
                    btn_login.Click();
                    Thread.Sleep(500);
                    return true;
                }
                this.log(String.Format("系统提示: 帐号: {0} 登录提交控件没有找到", user.LoginName));
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool IsLoginSuccess(PhantomJSDriver js, UserModel user)
        {
            try
            {
                string html=js.PageSource;
                Regex re = new Regex(@"id=""user-name"">(.+)<i", RegexOptions.IgnoreCase);
                if (re.IsMatch(html) == true)
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

        private byte[] GetValidCodeBuffer(PhantomJSDriver js, UserModel user)
        {
            try
            {
                IWebElement ele = Utils.GetElementFormWebDriver(js, By.Id("TANGRAM__PSP_8__verifyCodeImg"), 10);
                if (Utils.IsValid(ele) == false)
                {
                    this.log(String.Format("系统提示: 帐号: {0} 验证码图像控件没有找到", user.LoginName));
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
                size.Height = size.Height + 5;
                Point location = new Point(609, 1560);

                Rectangle crop = new Rectangle(location, size);
                MemoryStream ms = new MemoryStream();
                Bitmap validCode = bitmap2.Clone(crop, bitmap2.PixelFormat);
                validCode.Save(ms, ImageFormat.Jpeg);

                //删除图片
                validCode.Dispose();
                validCode = null;
                bitmap2.Dispose();
                bitmap2 = null;
                File.Delete(pageImg);

                byte[] data = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return data;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示: 帐号: {0} 验证码图像控件获取出错,错误消息: {1}", user.LoginName, ex.Message));
                return null;
            }
        }

        private bool ProcessValidCode(PhantomJSDriver js, UserModel user,Dictionary<string,string> dama)
        {
            try
            {
                byte[] buffer = this.GetValidCodeBuffer(js, user);
                if (buffer == null)
                {
                    return false;
                }
                this.showimg(buffer);
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

                string httpResult = RuoKuaiHttp.Post("http://api.ruokuai.com/create.xml", param, buffer);
                string yzmCode = Utils.GetImgString(httpResult);
                if (yzmCode == null || yzmCode == "")
                {
                    this.log(String.Format("系统提示 账号: {0} 获取验证码失败，原因: 打码平台返回验证码为空", user.LoginName));
                    return false;
                }
                this.log(String.Format("系统提示 账号: {0} 识别验证码内容: {1}", user.LoginName, yzmCode));
                
                //输入验证码
                IWebElement yzmInput = Utils.GetElementFormWebDriver(js, By.Id("TANGRAM__PSP_8__verifyCode"), 5);
                if (Utils.IsValid(yzmInput) == false)
                {
                    this.log(String.Format("系统提示 账号: {0} 获取验证码输入控件失败", user.LoginName));
                    return false;
                }

                IWebElement password = Utils.GetElementFormWebDriver(js, By.Id("TANGRAM__PSP_8__password"), 10);
                if (Utils.IsValid(password) == false)
                {
                    this.log(String.Format("系统提示: 帐号: {0} 处理验证码过程中,密码输入控件没有找到", user.LoginName));
                    return false;
                }
                password.Clear();
                password.SendKeys(user.LoginPwd);

                yzmInput.Clear();
                yzmInput.SendKeys(yzmCode);
                return true;
            }
            catch (Exception ex)
            {
                this.log(String.Format("系统提示 账号: {0} 获取验证码失败，原因: {1}", user.LoginName, ex.Message));
                return false;
            }
        }

    }
}
