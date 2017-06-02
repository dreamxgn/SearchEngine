///之前这篇加密帮助类觉得不够严谨,不够强，所以特意修改,也对自己负责一点，如果对你有帮助可以看看,
///呵呵！~这个类都经过本人测试过都没问题.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Utility
{
    /// <summary>
    /// @Author:梁继龙
    /// @Date:2012/7/30
    /// @Descripte:EncryptHelper加密帮助类.
    /// </summary>
    public class EncryptHelper
    {
        /// <summary>
        ///方法一:
        ///此种加密之后的字符串是三十二位的(字母加数据)字符串  
        /// Example: password是admin 加密变成后21232f297a57a5a743894a0e4a801fc3
        /// </summary>
        /// <param name="beforeStr"></param>
        /// <returns></returns>
        public string MD5Encrypt(string beforeStr)
        {
            string afterString = "";
            try
            {
                MD5 md5 = MD5.Create();
                byte[] hashs = md5.ComputeHash(Encoding.UTF8.GetBytes(beforeStr));

                foreach (byte by in hashs)
                    //这里是字母加上数据进行加密.//3y 可以,y3不可以或 x3j等应该是超过32位不可以
                    afterString += by.ToString("x2");
            }
            catch (Exception ex)
            {
                
            }
            return afterString;
        }

        /// <summary>
        /// 方法二
        /// HashAlgorithm加密
        /// 这种加密是  字母加-加字符  
        /// Example: password是admin 加密变成后19-A2-85-41-44-B6-3A-8F-76-17-A6-F2-25-01-9B-12
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public String HashEncrypt(string password)
        {
            Byte[] hashedBytes = null;
            try
            {
                Byte[] clearBytes = new UnicodeEncoding().GetBytes(password);
                hashedBytes = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(clearBytes);
            }
            catch (Exception ex)
            {
                
            }
            return BitConverter.ToString(hashedBytes);//MD5加密
        }

        /// <summary>
        /// 方法三:
        /// MD5  +   HashCode加密
        /// Example: password是admin 加密变成后 895b7da64943134be17b825ce118456c
        /// </summary>
        /// <returns></returns>
        public String MD5HashCodeEncrypt(string EncryptPwd)
        {
            return MD5Encrypt(HashEncrypt(EncryptPwd)); //在HashEncrypt基础上再MD5
        }

        /// <summary>
        /// 方法四:
        /// HashCode +MD5 加密
        /// Example: password是admin 加密变成后EB-1D-6D-E2-FC-F1-CD-94-4D-75-78-E6-3D-7A-12-32
        /// </summary>
        /// <param name="EncryptPwd"></param>
        /// <returns></returns>
        public String HashCodeMD5Encrypt(string EncryptPwd)
        {
            return HashEncrypt(MD5Encrypt(EncryptPwd)); //在MD5基础再HashCode
        }
        /// <summary>
        /// 方法五
        /// </summary>
        /// <param name="EncryptPwd"></param>
        /// <returns></returns>

        public String HashMD5Encrypt(string EncryptPwd)
        {
            return HashCodeMD5Encrypt(HashCodeMD5Encrypt(EncryptPwd)); //在HashCodeMD5Encrypt基础再HashCode
        }
        /// <summary>
        /// 方法六
        /// 哈哈是不是有点晕呢？
        /// 大家伙可以继续写.
        /// </summary>
        /// <param name="EncryptPwd"></param>
        /// <returns></returns>
        public String MD5HashEncrypt(string EncryptPwd)
        {
            return MD5HashCodeEncrypt(MD5HashCodeEncrypt(EncryptPwd)); //在MD5基础再HashCode
        }
        /// <summary>
        /// 64位双重MD5小写
        /// </summary>
        /// <returns></returns>
        public static string Last64(string s)
        {
            if (s.Length != 32)
                return "";
            string s1 = s.Substring(0, 16);
            string s2 = s.Substring(16, 16);
            return Lower32(s1) + Lower32(s2);
        }
        /// <summary>
        /// 32位大写
        /// </summary>
        /// <returns></returns>
        public static string Upper32(string s)
        {
            s = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(s, "md5").ToString();
            return s.ToUpper();
        }
        /// <summary>
        /// 32位小写
        /// </summary>
        /// <returns></returns>
        public static string Lower32(string s)
        {
            s = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(s, "md5").ToString();
            return s.ToLower();
        }
        /// <summary>
        /// 16位大写
        /// </summary>
        /// <returns></returns>
        public static string Upper16(string s)
        {
            s = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(s, "md5").ToString();
            return s.ToUpper().Substring(8, 16);
        }
        /// <summary>
        /// 16位小写
        /// </summary>
        /// <returns></returns>
        public static string Lower16(string s)
        {
            s = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(s, "md5").ToString();
            return s.ToLower().Substring(8, 16);
        }
    }

}