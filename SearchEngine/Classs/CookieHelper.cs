using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace 搜索引擎自动营销助手V1._0.Classs
{
    /// <summary>
    /// Cookie自动管理容器
    /// </summary>
    public class CookieAutoContainer
    {
        /// <summary>
        /// Cookie字典
        /// </summary>
        private Dictionary<string,CookieModel> cookies;

        public CookieAutoContainer()
        {
            this.cookies = new Dictionary<string, CookieModel>();
        }


        public void UpdateMsgListCookie(string cookie)
        {
            //ms_tk
            Regex re = new Regex(@"ms_tk=([\w+ \-]+)");
            CookieModel ms_tk = new CookieModel() { Name = "ms_tk", Value = re.Match(cookie).Groups[1].Value };
            this.AddCookie(ms_tk);

            //qun_t1
            re = new Regex(@"qun_t1=([\w+ \-]+)");
            CookieModel qun_t1 = new CookieModel() { Name = "qun_t1", Value = re.Match(cookie).Groups[1].Value };
            this.AddCookie(qun_t1);

            //qun_tl
            re = new Regex(@"qun_tl=([\w+ \-]+)");
            CookieModel qun_tl = new CookieModel() { Name = "qun_tl", Value = re.Match(cookie).Groups[1].Value };
            this.AddCookie(qun_tl);

            //qun_t0
            re = new Regex(@"qun_t0=([\w+ \-]+)");
            CookieModel qun_t0 = new CookieModel() { Name = "qun_t0", Value = re.Match(cookie).Groups[1].Value };
            this.AddCookie(qun_t0);

            //qun_to
            re = new Regex(@"qun_to=([\w+ \-]+)");
            CookieModel qun_to = new CookieModel() { Name = "qun_to", Value = re.Match(cookie).Groups[1].Value };
            this.AddCookie(qun_to);
        }

        #region 使用指定的匹配正则更新Cookie字符串到容器中
        /// <summary>
        /// 使用指定的匹配正则更新Cookie字符串到容器中
        /// </summary>
        /// <param name="re"></param>
        /// <param name="cookieStr"></param>
        public void UpdateCookieByRegex(Regex re,string cookieStr)
        {
            if (re.IsMatch(cookieStr) == true)
            {
                CookieModel cookie = new CookieModel() { Name = re.Match(cookieStr).Groups[1].Value, Value = re.Match(cookieStr).Groups[2].Value,Path= re.Match(cookieStr).Groups[3].Value,Domain= re.Match(cookieStr).Groups[4].Value,Expires= re.Match(cookieStr).Groups[5].Value };
                this.AddCookie(cookie);
            }
        }
        #endregion

        #region 更新Cookie字符串到容器中
        /// <summary>
        /// 更新Cookie字符串到容器中
        /// </summary>
        /// <param name="cookie"></param>
        public List<CookieModel> UpdateCookie(string cookie)
        {
            if (cookie == null || cookie.Length < 2)
            {
                return null;
            }
            List<CookieModel> list = this.StringToCookieModel(cookie);
            foreach (CookieModel cm in list)
            {
                this.AddCookie(cm);
            }
            return list;
        }
        #endregion

        #region 获取当前Cookie容器中存放的Cookie数量
        /// <summary>
        /// 获取当前Cookie容器中存放的Cookie数量
        /// </summary>
        /// <returns></returns>
        public int GetCookieCount()
        {
            return this.cookies.Count;
        }
        #endregion

        #region 添加或更新一个Cookie
        /// <summary>
        /// 添加一个Cookie
        /// </summary>
        /// <param name="cm"></param>
        public void AddCookie(CookieModel cm)
        {
            if (this.cookies.Keys.Contains(cm.Name) == true)
            {
                CookieModel ck = this.cookies[cm.Name];
                ck.Value = cm.Value;
                ck.Path = cm.Path;
                ck.Domain = cm.Domain;
                ck.Expires = cm.Expires;
                return;
            }
            this.cookies.Add(cm.Name, cm);
            return;
        }
        #endregion

        #region 获取指定的Cookie头
        /// <summary>
        /// 获取指定的Cookie头
        /// </summary>
        /// <param name="arr">指定的Cookie</param>
        /// <returns></returns>
        public string GetCookieHeader(List<string> arrKeys)
        {
            StringBuilder sb = new StringBuilder();
            if (arrKeys != null)
            {
                foreach (string keystr in arrKeys)
                {
                    if (this.cookies.Keys.Contains(keystr) == true)
                    {
                        CookieModel cm = this.cookies[keystr];
                        sb.Append(cm.Name + "=" + cm.Value + "; ");
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<string,CookieModel> kv in this.cookies)
                {
                    CookieModel cm = kv.Value;
                    sb.Append(cm.Name + "=" + cm.Value + "; ");
                }
            }
            string tmp = sb.ToString();
            tmp = tmp.Substring(0, tmp.LastIndexOf(';'));
            return tmp;
        }
        #endregion

        #region 获取指定的Cookie实例
        /// <summary>
        /// 获取指定的Cookie实例
        /// </summary>
        /// <param name="arr">指定的Cookie</param>
        /// <returns></returns>
        public List<CookieModel> GetCookieModel(List<string> arrKeys)
        {
            List<CookieModel> list = new List<CookieModel>();
            if (arrKeys != null)
            {
                foreach (string keystr in arrKeys)
                {
                    if (this.cookies.Keys.Contains(keystr) == true)
                    {
                        list.Add(this.cookies[keystr]);
                    }
                }
            }
            
            return list;
        }
        #endregion

        #region 返回一个Cookie
        /// <summary>
        /// 返回一个Cookie
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CookieModel GetCookie(string name)
        {
            return this.cookies[name];
        }
        #endregion

        #region 解析Cookie字符串到Cookie实体类
        /// <summary>
        /// 解析Cookie字符串到Cookie实体类
        /// </summary>
        /// <param name="cookieStr"></param>
        /// <returns></returns>
        private List<CookieModel> StringToCookieModel(string cookieStr)
        {
            Regex re = new Regex(@"([\w \- \s]+)=([\w \. \* \- \s : / % = |]+)");
            if (re.IsMatch(cookieStr) == true)
            {
                List<CookieModel> list = new List<CookieModel>();
                MatchCollection matchs = re.Matches(cookieStr);
                CookieModel cm = null;
                foreach (Match m in matchs)
                {
                    if (m.Groups[1].Value.ToLower().Trim() == "domain")
                    {
                        cm.Domain = m.Groups[2].Value.Trim();
                        continue;
                    }

                    if (m.Groups[1].Value.ToLower().Trim() == "path")
                    {
                        cm.Path = m.Groups[2].Value.Trim();
                        continue;
                    }

                    if (m.Groups[1].Value.ToLower().Trim() == "expires")
                    {
                        cm.Expires = m.Groups[2].Value.Trim();
                        continue;
                    }

                    cm = new CookieModel();
                    cm.Name = m.Groups[1].Value.Trim();
                    cm.Value = m.Groups[2].Value.Trim();
                    list.Add(cm);
                }
                return list;
            }
            return null;
        }
        #endregion

        #region 遍历CookieContainer
        /// <summary>
        /// 遍历CookieContainer
        /// </summary>
        /// <param name="cc"></param>
        /// <returns></returns>
        public List<Cookie> GetAllCookies(CookieContainer cc)
        {
            List<Cookie> lstCookies = new List<Cookie>();

            Hashtable table = (Hashtable)cc.GetType().InvokeMember("m_domainTable",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField |
            System.Reflection.BindingFlags.Instance, null, cc, new object[] { });
            foreach (object pathList in table.Values)
            {
                SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField
                | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
                foreach (CookieCollection colCookies in lstCookieCol.Values)
                    foreach (Cookie c in colCookies) lstCookies.Add(c);
            }

            return lstCookies;
        }
        #endregion

        #region 获取所有的Cookie
        /// <summary>
        /// 获取所有的Cookie
        /// </summary>
        /// <returns></returns>
        public List<CookieModel> GetCookies()
        {
            List<CookieModel> list = new List<CookieModel>();
            foreach (KeyValuePair<string, CookieModel> kv in this.cookies)
            {
                list.Add(kv.Value);
            }
            return list;
        }
        #endregion

    }

    #region Cookie实体类
    /// <summary>
    /// Cookie实体类
    /// </summary>
    public class CookieModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Domain { get; set; }
        public string Path { get; set; }
        public string Expires { get; set; }
    }
    #endregion
}
