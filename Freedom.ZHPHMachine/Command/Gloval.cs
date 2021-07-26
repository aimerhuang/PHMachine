using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freedom.ZHPHMachine.Command
{
    using Freedom.Models;
    using Freedom.Hardware.Visa;
    using Freedom.Hardware;
    using Freedom.Models.VisaModels;
    using Freedom.Models.CrjDataModels;
    using System.IO;
    using Freedom.Common; 
    using ViewModels;
    using System.Threading;
    using Freedom.Models.CrjCreateJsonModels; 
    using Freedom.Config;  
    using Freedom.Models.JgModels;
    using Freedom.Models.DataBaseModels;
    using MachineCommandService;

    public partial class Gloval
    {
        #region 加密身份证号码
        /// <summary>
        /// 加密身份证号码
        /// </summary>
        /// <param name="idcardinfo"></param>
        /// <returns></returns>
        public static string DESIdCardShowInfo(string idcardinfo)
        {
            if (string.IsNullOrEmpty(idcardinfo))
                return "";
            string str = new StrHelp().ChangID15to18(idcardinfo);
            string result = "";
            result = idcardinfo.Substring(0, 10);
            result += "****";
            result += idcardinfo.Substring(14, 4);
            return result;
        }
        #endregion
    }
}
