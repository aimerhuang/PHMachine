using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Freedom.Common;
using Freedom.Models;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.Common;
using MachineCommandService;

namespace Freedom.ZHPHMachine.Command
{
    /// <summary>
    /// 正则验证类
    /// </summary>
    public class ValidationHelper
    {

        /// <summary>
        /// 验证是否是姓名
        /// 1.可以是中文
        /// 2.可以是英文，允许输入点（英文名字中的那种点）， 允许输入空格
        /// 3.中文和英文不能同时出现
        /// 4.长度在12个字符以内
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns>是否</returns>
        public static bool IsName(string inputData)
        {
            Regex regName = new Regex("^([\\u4e00-\\u9fa5]{1,12})$");
            return regName.Match(inputData).Success;
        }

        /// <summary>
        /// 验证是否是身份号
        /// 15或18位号码
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns>是否</returns>
        public static bool IsIdNumber(string inputData)
        {
            Regex regName=new Regex("^\\d{15}|\\d{18}$");
            return regName.Match(inputData).Success;
        }

        /// <summary>
        /// 验证是否是邮政编码
        /// 15或18位号码
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns>是否</returns>
        public static bool IsYzbm(string inputData)
        {
            Regex regName = new Regex("^\\d{6}$");
            return regName.Match(inputData).Success;
        }

        /// <summary>
        /// 是否是电话号码
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns>是否</returns>
        public static bool IsPhoneNumber(string inputData)
        {
            Regex regName = new Regex("^1(3[0-9]|4[0-9]|5[0-9]|6[0-9]|7[0-9]|8[0-9]|9[0-9])[0-9]{8}$");
            return regName.Match(inputData).Success;
            
        }

        /// <summary>
        /// 是否是中文字符
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public static bool IsChinaChar(string inputData)
        {
            if (string.IsNullOrEmpty(inputData))
                return false;
            Regex regName=new Regex("^[^\x00-\xFF]");
            return regName.Match(inputData).Success;
        }

        /// <summary>
        /// 是否是英文字符
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public static bool IsEnglishChar(string inputData)
        {
            Regex regName = new Regex("^[a-zA-Z]");
            return regName.Match(inputData).Success;
        }

        /// <summary>
        /// 验证是否是地址
        /// 1.可以是中文
        /// 2.可以是英文，允许输入点
        /// 3.可以输入数字和_
        /// 4.长度在60个字符以内
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns>是否</returns>
        public static bool IsPlace(string inputData)
        {
            //Regex regName = new Regex("^([\\u4e00-\\u9fa5]{1,50})$");[\u4e00-\u9fa5_a-zA-Z0-9_]
            Regex regName = new Regex("^([\u4E00-\u9FA5A-Za-z0-9_]{2,60})$");
            return regName.Match(inputData).Success;
        }

        public static int Get_NomalAge(IdCardInfo info, out string msg)
        {
            msg = "";
            Log.Instance.WriteInfo("\n==========3、判断年龄是否可以取号==========");
            if (!string.IsNullOrEmpty(info.IDCardNo))
            {
                //取系统时间
                var mistime = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();
                //if (string.IsNullOrEmpty(mistime))
                //{
                //    Log.Instance.WriteInfo("获取系统时间返回为空");
                //    mistime = DateTime.Now.ToString();//返回空取本机时间
                //}
                //时间转换
                DateTime now = DateTime.Parse(mistime);
                if (!string.IsNullOrEmpty(now.ToString()))
                {
                    Log.Instance.WriteInfo("当前时间：" + now.ToString("yyyy年MM月dd日 HH:mm:ss"));
                    DateTime birth;
                    DateTime.TryParse(info.IDCardNo.Substring(6, 4) + "-" + info.IDCardNo.Substring(10, 2) + "-" +
                                      info.IDCardNo.Substring(12, 2), out birth);
                    int age = now.Year - birth.Year; //年龄
                    if (now.Month < birth.Month || (now.Month == birth.Month && now.Day < birth.Day))
                        age--;
                    Log.Instance.WriteInfo("计算出年龄为：" + age);
                    return age;
                }

            }

            msg = "计算年龄发生错误";
            return 0;
        }

        /// <summary>
        /// 是否是数字字符
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public static bool IsNumberChar(string inputData)
        {
            Regex regName = new Regex("^\\d{n}$");
            return regName.Match(inputData).Success;
        }

        /// <summary>
        /// 取民族
        /// </summary>
        /// <returns></returns>
        public static List<DictionaryType> GetMzTypes()
        {
            var config = ServiceRegistry.Instance.Get<ElementManager>();
            List<DictionaryType> lst = new List<DictionaryType>();

            lst = config.Get<List<DictionaryType>>();
            lst = lst?.Where(t => t.KindType == ((int)KindType.MZ).ToString() && t.Status == 1)?.ToList();

            return lst;
        }

    }
}
