using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace 搜索引擎自动营销助手V1._0.Classs
{
    public class SogouService
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        private int traceIndex { get; set; }

        private CookieContainer cookies = new CookieContainer();

        private CookieAutoContainer cookieAutoContainer;


        public SogouService()
        {
            this.cookieAutoContainer = new CookieAutoContainer();
            this.traceIndex = 0;
        }

        #region 登录搜狗问问
        /// <summary>
        /// 登录搜狗
        /// </summary>
        public void Login_ww()
        {
            HttpWebResponse res = null;
            HttpWebRequest req = null;
            List<CookieModel> list = null;
            string url = "";

            //访问搜狗问问首页获取Cookie
            req = this.GetRequest("http://wenwen.sogou.com/?p=web2ww");
            string html = this.GetHtml(req, out res);
            this.cookieAutoContainer.UpdateCookie(res.Headers["Set-Cookie"]);

            
            req = this.GetRequest("http://www.sogou.com/index/images/ico_beta.png");
            res = (HttpWebResponse)req.GetResponse();
            this.cookieAutoContainer.UpdateCookie(res.Headers["Set-Cookie"]);
            req.Abort();
            res.Close();
            


            this.cookieAutoContainer.AddCookie(new CookieModel() { Name = "ssuid", Value = QQPwdNew.GetSSUid(), Domain = ".sogou.com", Path = "/" });
            this.cookieAutoContainer.AddCookie(new CookieModel() { Name = "dt_ssuid", Value = QQPwdNew.GetDTUUID(), Domain = ".sogou.com", Path = "/" });

            req = this.GetRequest("http://xui.ptlogin2.qq.com/cgi-bin/xlogin?appid=6000201&daid=210&hide_uin_tip=1&style=20&hide_close_icon=0&pt_no_auth=1&target=self&qtarget=0&hide_title_bar=1&s_url=http%3A%2F%2Fwenwen.sogou.com%2Flogin%2FpopLogin");
            //req.CookieContainer = this.cookies;
            req.Host = "xui.ptlogin2.qq.com";
            req.Referer = "http://wenwen.sogou.com/?p=web2ww";
            req.Headers["Cookie"] = this.cookieAutoContainer.GetCookieHeader(null);
            html = this.GetHtml(req, out res);
            this.cookieAutoContainer.UpdateCookie(res.Headers["Set-Cookie"]);
            string login_sig = this.cookieAutoContainer.GetCookie("pt_login_sig").Value;

            //验证码
            url = String.Format("http://check.ptlogin2.qq.com/check?regmaster=&pt_tea=2&pt_vcode=1&uin={0}&appid=6000201&js_ver=10203&js_type=1&login_sig={1}&u1=http%3A%2F%2Fwenwen.sogou.com%2Flogin%2FpopLogin&r=0.18177199340700656&pt_uistyle=40", this.UserName, login_sig);
            req = this.GetRequest(url);
            req.Host = "check.ptlogin2.qq.com";
            req.Referer = "http://xui.ptlogin2.qq.com/cgi-bin/xlogin?appid=6000201&daid=210&hide_uin_tip=1&style=20&hide_close_icon=0&pt_no_auth=1&target=self&qtarget=0&hide_title_bar=1&s_url=http%3A%2F%2Fwenwen.sogou.com%2Flogin%2FpopLogin";
            html = this.GetHtml(req, out res);
            this.cookieAutoContainer.UpdateCookie(res.Headers["Set-Cookie"]);

            Console.WriteLine("当前帐号验证码返回：" + html);

            this.cookieAutoContainer.AddCookie(new CookieModel() { Name = "ptui_loginuin", Value = this.UserName, Domain = "qq.com", Expires = DateTime.Now.AddDays(30).ToShortDateString() });
            this.cookies.Add(new Cookie("ptui_loginuin", this.UserName, "/", "qq.com"));

            if (html.Contains("ptui_checkVC('1'"))
            {
                Console.WriteLine("当前QQ帐号需要验证码");
                return;
            }

            Regex checkVCRegex = new Regex(@"ptui_checkVC\('0','(?<vc1>[^']+)','(?<vc2>[^']+)','(?<vc3>[^']+)'");
            string checkVC1 = checkVCRegex.Match(html).Groups["vc1"].Value;
            string checkVC2 = checkVCRegex.Match(html).Groups["vc2"].Value;
            string checkVC3 = checkVCRegex.Match(html).Groups["vc3"].Value;

            string salt = QQPwdNew.GetSalt(this.UserName); // salt和checkVC2相同
            string rspwd = QQPwdNew.GetPwd(this.Password, salt, checkVC1);



            //登录授权
            url = String.Format("http://ptlogin2.qq.com/login?u={0}&verifycode={1}&pt_vcode_v1=0&pt_verifysession_v1={2}&p={3}&pt_randsalt=2&u1=http%3A%2F%2Fwenwen.sogou.com%2Flogin%2FpopLogin&ptredirect=0&h=1&t=1&g=1&from_ui=1&ptlang=2052&action=5-11-{4}&js_ver=10203&js_type=1&login_sig={5}&pt_uistyle=40&aid=6000201&daid=210&"
                , this.UserName, checkVC1, checkVC3, rspwd, JsTool.GetLongFromTime(), login_sig);


            req = this.GetRequest(url);
            req.Host = "ptlogin2.qq.com";
            req.Headers["Cookie"] = this.cookieAutoContainer.GetCookieHeader(null);
            req.Referer = " http://xui.ptlogin2.qq.com/cgi-bin/xlogin?appid=6000201&daid=210&hide_uin_tip=1&style=20&hide_close_icon=0&pt_no_auth=1&target=self&qtarget=0&hide_title_bar=1&s_url=http%3A%2F%2Fwenwen.sogou.com%2Flogin%2FpopLogin";
            html = this.GetHtml(req, out res);
            this.cookieAutoContainer.UpdateCookie(res.Headers["Set-Cookie"]);

            if (!html.Contains("登录成功") == true)
            {
                Console.WriteLine(html);
                return;
            }

            //获取跳转URL
            Regex re = new Regex(@"ptuiCB\('0','0','(.+)','0'");
            url = re.Match(html).Groups[1].Value;
            req = this.GetRequest(url);

            req.Headers["Cookie"] = this.cookieAutoContainer.GetCookieHeader(null);
            req.Host = "ssl.ptlogin2.graph.qq.com";
            req.Referer = "http://xui.ptlogin2.qq.com/cgi-bin/xlogin?appid=6000201&daid=210&hide_uin_tip=1&style=20&hide_close_icon=0&pt_no_auth=1&target=self&qtarget=0&hide_title_bar=1&s_url=http%3A%2F%2Fwenwen.sogou.com%2Flogin%2FpopLogin";
            req.AllowAutoRedirect = false;
            res = (HttpWebResponse)req.GetResponse();
            this.cookieAutoContainer.UpdateCookie(res.Headers["Set-Cookie"]);
            req.Abort();
            res.Close();

            //302
            url = res.Headers["Location"];

            req = this.GetRequest(url);
            req.Headers["Cookie"] = this.cookieAutoContainer.GetCookieHeader(null);
            req.Host = "wenwen.sogou.com";
            req.Referer = "http://xui.ptlogin2.qq.com/cgi-bin/xlogin?appid=6000201&daid=210&hide_uin_tip=1&style=20&hide_close_icon=0&pt_no_auth=1&target=self&qtarget=0&hide_title_bar=1&s_url=http%3A%2F%2Fwenwen.sogou.com%2Flogin%2FpopLogin";
            req.AllowAutoRedirect = false;
            res = (HttpWebResponse)req.GetResponse();

            req.Abort();
            res.Close();

            this.cookieAutoContainer.UpdateCookie(res.Headers["Set-Cookie"]);

            req = this.GetRequest("http://wenwen.sogou.com/?p=web2ww");
            req.Headers["Cookie"] = this.cookieAutoContainer.GetCookieHeader(new List<string>() { "p_uin", "p_skey", "pt4_token", "sg_uname", "sw_uuid", "IPLOC", "SUID", "sg_mu", "sg_lu", "sg_upic", "sg_uname", "ms_tk", "qun_t1", "qun_tl", "qun_t0", "qun_to" });
            html = this.GetHtml(req, out res);
            return;
        }
        #endregion

        #region 提交问题
        /// <summary>
        /// 提交问题
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public string Ask(string title, string content)
        {
            List<string> cheader= new List<string>() { "p_uin", "p_skey", "pt4_token", "sg_uname", "sw_uuid", "IPLOC", "SUID", "sg_mu", "sg_lu", "sg_upic", "sg_uname", "ms_tk", "qun_t1", "qun_tl", "qun_t0" };

            HttpWebRequest req = null;
            HttpWebResponse res = null;
            string html = "";

            //提问页面
            req = this.GetRequest("http://wenwen.sogou.com/question/ask");
            req.Headers["Cookie"] = this.cookieAutoContainer.GetCookieHeader(cheader);
            req.Host = "wenwen.sogou.com";
            req.Referer = "http://wenwen.sogou.com/";
            html = this.GetHtml(req, out res);

            //userId
            Regex re = new Regex(@"userId: '(\d+)'");
            string userId = re.Match(html).Groups[1].Value;
            //clbUid
            re = new Regex(@"clbUid: '(\d+)'");
            string clbUid = re.Match(html).Groups[1].Value;
            //orig
            re = new Regex(@"orig: (\d+)");
            string orig = re.Match(html).Groups[1].Value;
            //title
            string _title = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(title));
            //content
            string _content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content));
            //tag
            string tag = "462491";

            //traceId
            re = new Regex(@"traceId: '(\w+)'");
            string traceId = re.Match(html).Groups[1].Value;

            this.DownLoadImage("http://wenwen.sogou.com/wapi/ms/captcha?_0.3468338583637548", traceId);
            Console.Write("输入验证码>>>");
            string yzm = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Console.ReadLine()));

            string data = "{" + String.Format("\"userId\":\"{0}\",\"clbUid\":\"{1}\",\"orig\":{2},\"content\":\"{3}\",\"title\":\"{4}\",\"tags\":[462491],\"images\":[],\"score\":\"0\",\"anonymous\":false,\"seekHelpUid\":null,\"code\":\"{5}\"",
                userId, clbUid, orig, _content, _title, yzm) + "}";

            req = this.GetRequest("http://wenwen.sogou.com/submit/ms/ask?groupUin=undefined");
            req.Headers["Cookie"] = this.cookieAutoContainer.GetCookieHeader(cheader);
            req.Host = "wenwen.sogou.com";
            req.Referer = "http://wenwen.sogou.com/question/ask";
            req.ContentType = "application/json; charset=UTF-8";
            req.Headers["Origin"] = "http://wenwen.sogou.com";
            req.Headers["x-wenwen-trace-id"] = traceId;
            html = this.PostRequestString(req,data);
            return html;
        }

        #endregion

        #region 点赞
        /// <summary>
        /// 点赞
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="answerId"></param>
        public string MarkGood(string questionId, string answerId)
        {
            List<string> cheader = new List<string>() { "p_uin", "p_skey", "pt4_token", "sg_uname", "sw_uuid", "IPLOC", "SUID", "sg_mu", "sg_lu", "sg_upic", "sg_uname", "ms_tk", "qun_t1", "qun_tl", "qun_t0" };

            string url = String.Format("http://wenwen.sogou.com/question/?qid={0}", questionId);

            HttpWebRequest req = null;
            HttpWebResponse res = null;
            string html = "";

            req = this.GetRequest(url);
            req.Headers["Cookie"] = this.cookieAutoContainer.GetCookieHeader(cheader);
            req.Host = "wenwen.sogou.com";
            html= this.GetHtml(req, out res);
            this.cookieAutoContainer.UpdateCookie(res.Headers["Set-Cookie"]);

            //userId
            Regex re = new Regex(@"userId: '(\d+)'");
            string userId = re.Match(html).Groups[1].Value;
            //clbUid
            re = new Regex(@"clbUid: '(\d+)'");
            string clbUid = re.Match(html).Groups[1].Value;
            //orig
            re = new Regex(@"orig: (\d+)");
            string orig = re.Match(html).Groups[1].Value;

            //traceId
            re = new Regex(@"traceId: '(\w+)'");
            string traceId = re.Match(html).Groups[1].Value;

            string data = "{" + String.Format("\"userId\":\"{0}\",\"clbUid\":\"{1}\",\"orig\":{2},\"threadId\":\"{3}\",\"type\":1", userId, clbUid, orig, answerId) + "}";

            req = this.GetRequest("http://wenwen.sogou.com/submit/ms/vote?groupUin=undefined");
            req.Headers["Cookie"] = this.cookieAutoContainer.GetCookieHeader(cheader);
            req.Host = "wenwen.sogou.com";
            req.Referer = "http://wenwen.sogou.com/question/ask";
            req.ContentType = "application/json; charset=UTF-8";
            req.Headers["Origin"] = "http://wenwen.sogou.com";
            req.Headers["x-wenwen-trace-id"] = traceId;
            html = this.PostRequestString(req, data);

            return html;


        }
        #endregion

        #region 选择最佳答案
        /// <summary>
        /// 选择最佳答案
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="answerId"></param>
        public string TakeBestAnswer(string questionId, string answerId)
        {
            List<string> cheader = new List<string>() { "p_uin", "p_skey", "pt4_token", "sg_uname", "sw_uuid", "IPLOC", "SUID", "sg_mu", "sg_lu", "sg_upic", "sg_uname", "ms_tk", "qun_t1", "qun_tl", "qun_t0" };

            string url = String.Format("http://wenwen.sogou.com/question/?qid={0}", questionId);

            HttpWebRequest req = null;
            HttpWebResponse res = null;
            string html = "";

            req = this.GetRequest(url);
            req.Headers["Cookie"] = this.cookieAutoContainer.GetCookieHeader(cheader);
            req.Host = "wenwen.sogou.com";
            html = this.GetHtml(req, out res);
            this.cookieAutoContainer.UpdateCookie(res.Headers["Set-Cookie"]);

            //userId
            Regex re = new Regex(@"userId: '(\d+)'");
            string userId = re.Match(html).Groups[1].Value;
            //clbUid
            re = new Regex(@"clbUid: '(\d+)'");
            string clbUid = re.Match(html).Groups[1].Value;
            //orig
            re = new Regex(@"orig: (\d+)");
            string orig = re.Match(html).Groups[1].Value;

            //traceId
            re = new Regex(@"traceId: '(\w+)'");
            string traceId = re.Match(html).Groups[1].Value;

            string data = String.Format("userId={0}&clbUid={1}&orig={2}&asid={3}", userId, clbUid, answerId);

            req = this.GetRequest("http://wenwen.sogou.com/submit/ms/adopt?groupUin=undefined");
            req.Headers["Cookie"] = this.cookieAutoContainer.GetCookieHeader(cheader);
            req.Host = "wenwen.sogou.com";
            req.Referer = "http://wenwen.sogou.com/question/ask";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.Headers["Origin"] = "http://wenwen.sogou.com";
            req.Headers["x-wenwen-trace-id"] = traceId;
            html = this.PostRequestString(req, HttpUtility.UrlEncode(data));

            return html;
        }
        #endregion

        #region 回答问题
        /// <summary>
        /// 回答问题
        /// </summary>
        /// <param name="questionId"></param>
        public string Answer(string questionId,string content)
        {
            List<string> cheader = new List<string>() { "p_uin", "p_skey", "pt4_token", "sg_uname", "sw_uuid", "IPLOC", "SUID", "sg_mu", "sg_lu", "sg_upic", "sg_uname", "ms_tk", "qun_t1", "qun_tl", "qun_t0" };

            string url = String.Format("http://wenwen.sogou.com/question/?qid={0}", questionId);

            HttpWebRequest req = null;
            HttpWebResponse res = null;
            string html = "";

            req = this.GetRequest(url);
            req.Headers["Cookie"] = this.cookieAutoContainer.GetCookieHeader(cheader);
            req.Host = "wenwen.sogou.com";
            html = this.GetHtml(req, out res);
            this.cookieAutoContainer.UpdateCookie(res.Headers["Set-Cookie"]);

            //userId
            Regex re = new Regex(@"userId: '(\d+)'");
            string userId = re.Match(html).Groups[1].Value;
            //clbUid
            re = new Regex(@"clbUid: '(\d+)'");
            string clbUid = re.Match(html).Groups[1].Value;
            //orig
            re = new Regex(@"orig: (\d+)");
            string orig = re.Match(html).Groups[1].Value;

            //traceId
            re = new Regex(@"traceId: '(\w+)'");
            string traceId = re.Match(html).Groups[1].Value;

            string data = "{" + String.Format("\"userId\":\"{0}\",\"clbUid\":\"{1}\",\"orig\":{2},\"content\":\"{3}\",\"questionId\":\"{4}\",\"anonymous\":false", userId, clbUid, orig, Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content)),questionId) + "}";

            req = this.GetRequest("http://wenwen.sogou.com/submit/ms/answer?groupUin=undefined");
            req.Headers["Cookie"] = this.cookieAutoContainer.GetCookieHeader(cheader);
            req.Host = "wenwen.sogou.com";
            req.Referer = "http://wenwen.sogou.com/question/ask";
            req.ContentType = "application/json; charset=UTF-8";
            req.Headers["Origin"] = "http://wenwen.sogou.com";
            req.Headers["x-wenwen-trace-id"] = traceId;
            html = this.PostRequestString(req, data);

            return html;
        }
        #endregion

        #region 获取一个HttpWebRequest对象
        private HttpWebRequest GetRequest(string url)
        {
            HttpWebRequest request = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version11;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls; //SecurityProtocolType.Tls1.2;
                request.KeepAlive = false;
                ServicePointManager.CheckCertificateRevocationList = true;
                ServicePointManager.DefaultConnectionLimit = 100;
            }
            else
            {
                request = (HttpWebRequest)WebRequest.Create(url);
            }
            request.Proxy = null;
            request.Timeout = 10000;
            request.ReadWriteTimeout = 10000;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36";
            request.Method = "GET";
            request.KeepAlive = true;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.AllowAutoRedirect = true;
            return request;
        }
        #endregion

        #region 访问连接获取响应的字符串
        private string GetHtml(HttpWebRequest request, out HttpWebResponse res)
        {
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                res = response;
                var sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string ss = sr.ReadToEnd();
                sr.Close();
                request.Abort();
                response.Close();
                return ss;
            }
            catch (WebException ex)
            {
                res = null;
                return "";
            }
        }
        #endregion

        #region 将数据流转换为字节数组
        /// <summary>
        /// 将数据流转换为字节数组
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public byte[] StreamToBytes(Stream s)
        {
            int len = 0;
            MemoryStream ms = new MemoryStream();
            byte[] buff = new byte[1024];
            len = s.Read(buff, 0, 1024);
            while (len > 0)
            {
                ms.Write(buff, 0, len);
                len = s.Read(buff, 0, 1024);
            }
            byte[] data = ms.ToArray();
            ms.Close();
            ms.Dispose();
            return data;
        }
        #endregion

        #region 获取GTK
        /// <summary>
        /// 获取GTK
        /// </summary>
        /// <param name="sKey"></param>
        /// <returns></returns>
        private long GetG_tk(string sKey)
        {
            int hash = 5381;
            for (int i = 0, len = sKey.Length; i < len; ++i)
            {
                hash += (hash << 5) + (int)sKey[i];
            }
            return (hash & 0x7fffffff);
        }
        #endregion

        #region POST提交数据
        /// <summary>
        /// POST提交数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="postData"></param>
        private void PostRequest(HttpWebRequest request, string postData)
        {
            HttpWebResponse response = null;
            try
            {
                request.Method = "POST";
                //提交请求  
                byte[] postdatabytes = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = postdatabytes.Length;
                Stream stream;
                stream = request.GetRequestStream();
                //设置POST 数据
                stream.Write(postdatabytes, 0, postdatabytes.Length);
                stream.Close();
                //接收响应  
                response = (HttpWebResponse)request.GetResponse();
                var cookieCollection = response.Cookies;//拿到bduss 说明登录成功
                                                        //保存返回cookie  
                                                        //取下一次GET跳转地址  
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string content = sr.ReadToEnd();

                sr.Close();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (request != null) request.Abort();
                if (response != null) response.Close();
            }
            return;
        }
        #endregion

        #region 下载图片
        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="url"></param>
        /// <param name="tarceId"></param>
        public void DownLoadImage(string url, string tarceId)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Headers["Cookie"] = this.cookieAutoContainer.GetCookieHeader(new List<string>() { "p_uin", "p_skey", "pt4_token", "sg_uname", "sw_uuid", "IPLOC", "SUID", "sg_mu", "sg_lu", "sg_upic", "sg_uname", "ms_tk", "qun_t1", "qun_tl", "qun_t0" });
            req.Host = "wenwen.sogou.com";
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:52.0) Gecko/20100101 Firefox/52.0";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Headers["x-wenwen-trace-id"] = tarceId;

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            this.cookieAutoContainer.UpdateCookie(res.Headers["Set-Cookie"]);
            File.WriteAllBytes("yzm.jpeg", this.StreamToBytes(res.GetResponseStream()));
            req.Abort();
            res.Close();
        }
        #endregion

        #region 提交字符串数据POST
        /// <summary>
        /// 提交字符串数据POST
        /// </summary>
        /// <param name="request"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        private string PostRequestString(HttpWebRequest request, string postData)
        {
            HttpWebResponse response = null;
            try
            {
                request.Method = "POST";
                //提交请求  
                byte[] postdatabytes = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = postdatabytes.Length;
                Stream stream;
                stream = request.GetRequestStream();
                //设置POST 数据
                stream.Write(postdatabytes, 0, postdatabytes.Length);
                stream.Close();
                //接收响应  
                response = (HttpWebResponse)request.GetResponse();
                //保存返回cookie  
                //取下一次GET跳转地址  
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string content = sr.ReadToEnd();

                sr.Close();
                return content;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (request != null) request.Abort();
                if (response != null) response.Close();
            }
            return "";
        }
        #endregion
    }
}
