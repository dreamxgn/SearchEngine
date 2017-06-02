using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web.Security;

namespace 搜索引擎自动营销助手V1._0.Classs
{
    public class QQPwdNew
    {

        public static string GetSSUid()
        {
            string str2 = File.ReadAllText("common.js");
            string result = ExecuteScript("getsuuid()",str2);

            return result;
        }

        public static string GetDTUUID()
        {
            string str2 = File.ReadAllText("common.js");
            string result = ExecuteScript("getdtsuuid()", str2);

            return result;
        }

        public static string GetSalt(string qq)
        {
            //string getsalt = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "js", "getsalt.js");

            // string path = AppDomain.CurrentDomain.BaseDirectory + "getsalt.js";
            string str2 = File.ReadAllText("getsalt.js");

            string fun = string.Format(@"uin2hex('{0}')", qq);
            string result = ExecuteScript(fun, str2);

            return result;
        }

        public static string GetPwd(string password, string salt, string vcode)
        {
            //string getpwd = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "js", "getpwd.js");
            // string path = AppDomain.CurrentDomain.BaseDirectory + "getpwd.js";
            string str2 = File.ReadAllText("getpwd.js");

            string fun = string.Format(@"getEncryption('{0}','{1}','{2}')", password, salt, vcode);
            string result = ExecuteScript(fun, str2);

            return result;
        }

        /// <summary>
        /// 执行JS
        /// </summary>
        /// <param name="sExpression">参数体</param>
        /// <param name="sCode">JavaScript代码的字符串</param>
        /// <returns></returns>
        private static string ExecuteScript(string sExpression, string sCode)
        {
            MSScriptControl.ScriptControl scriptControl = new MSScriptControl.ScriptControl();
            scriptControl.UseSafeSubset = true;
            scriptControl.Language = "JScript";
            scriptControl.AddCode(sCode);
            try
            {
                string str = scriptControl.Eval(sExpression).ToString();
                return str;
            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
            return null;
        }
    }
}
