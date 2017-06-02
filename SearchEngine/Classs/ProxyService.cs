using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace 搜索引擎自动营销助手V1._0.Classs
{
    public class ProxyService
    {

        public int useCount { get; set; }

        public string ProxyMessage { get; set; }

        public ProxyService()
        {
            this.proxyList = new Stack<string>();
            this.useCount = 0;
        }

        private Stack<string> proxyList;

        /// <summary>
        /// 代理接口地址
        /// </summary>
        public string RemoteUrl { get; set; }


        /// <summary>
        /// 获取代理IP
        /// </summary>
        /// <returns></returns>
        public string GetProxyIP()
        {
            while (true)
            {
                if (this.proxyList.Count <= 0)
                {
                    if (this.FillProxyList() == false)
                    {
                        return null;
                    }
                }
                string ip = this.proxyList.Pop();
                if (Utils.IsProxyValid(ip) == true)
                {
                    return ip;
                }
            }
        }

        public bool FillProxyList()
        {
            string ipStr = this.GetHtml(this.RemoteUrl);
            if (ipStr == null)
            {
                return false;
            }

            //验证
            if (ipStr.Contains("提取数量已用完") == true)
            {
                this.ProxyMessage = ipStr;
                return false;
            }
            string[] proxyIP = ipStr.Split(new string[] { "\n","\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string ip in proxyIP)
            {
                this.proxyList.Push(ip);
            }
            return true;
        }


        public string GetHtml(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36";
            request.Timeout = 30000;
            request.ReadWriteTimeout = 30000;
            HttpWebResponse response = null;
            string proxy = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                proxy= this.StreamToString(response.GetResponseStream());
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (request != null)
                {
                    request.Abort();
                    request = null;
                }
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
            return proxy;
        }


        /// <summary>
        /// 转换流到字符串
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private string StreamToString(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int len = 0;
                    len = stream.Read(buffer, 0, 1024);
                    while (len > 0)
                    {
                        ms.Write(buffer, 0, len);
                        len = stream.Read(buffer, 0, 1024);
                    }
                    return System.Text.Encoding.UTF8.GetString(ms.ToArray());
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
    }
}
