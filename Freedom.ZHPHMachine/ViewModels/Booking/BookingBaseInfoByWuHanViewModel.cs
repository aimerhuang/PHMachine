using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Freedom.BLL;
using Freedom.Common;
using Freedom.Common.HsZhPjh.Enums;
using Freedom.Config;
using Freedom.Controls.Foundation;
using Freedom.Models;
using Freedom.Models.CrjCreateJsonModels;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.Common;
using MachineCommandService;

namespace Freedom.ZHPHMachine.ViewModels.Booking
{
    public class BookingBaseInfoByWuHanViewModel : ViewModelBase
    {

        #region 字段

        private CrjPreapplyManager crjManager;
        private List<DictionaryType> cardTypes;
        private List<DictionaryType> applyTypes;

        public bool hzSelect;
        /// <summary>
        /// 是否填写港澳签注类型
        /// </summary>
        public bool gaQzType;

        /// <summary>
        /// 是否填写港澳签注类型
        /// </summary>
        public bool twQzType;
        /// <summary>
        /// 是否选择港澳签注类型
        /// </summary>
        public bool gaValues;


        /// <summary>
        /// 是否选择台湾签注类型
        /// </summary>
        public bool twValues;


        /// <summary>
        /// 办证类型
        /// </summary>
        public List<DictionaryType> CardTypes
        {
            get { return cardTypes; }
            set
            {
                cardTypes = value;
                RaisePropertyChanged("CardTypes");
            }
        }

        private string tipsMsg;

        /// <summary>
        /// 提示信息
        /// </summary>
        public string TipsMsg
        {
            get { return tipsMsg; }
            set
            {
                tipsMsg = value;
                RaisePropertyChanged("TipsMsg");
            }
        }

        public string imageStr;

        /// <summary>
        /// 太极人口照片路径
        /// </summary>
        public string ImageStr
        {
            get { return imageStr; }
            set
            {
                imageStr = value;
                RaisePropertyChanged("ImageStr");
            }
        }


        public BookingInfo hZBookingInfo;

        /// <summary>
        /// 护照预约信息
        /// </summary>
        public BookingInfo HZBookingInfo
        {
            get { return hZBookingInfo; }
            set
            {
                hZBookingInfo = value;
                RaisePropertyChanged("HZBookingInfo");
            }
        }

        public BookingInfo tWBookingInfo;

        /// <summary>
        /// 台湾预约信息
        /// </summary>
        public BookingInfo TWBookingInfo
        {
            get { return tWBookingInfo; }
            set
            {
                tWBookingInfo = value;
                RaisePropertyChanged("TWBookingInfo");
            }
        }

        public BookingInfo gABookingInfo;

        /// <summary>
        /// 港澳预约信息
        /// </summary>
        public BookingInfo GABookingInfo
        {
            get { return gABookingInfo; }
            set
            {
                gABookingInfo = value;
                RaisePropertyChanged("GABookingInfo");
            }
        }




        #endregion




        public BookingBaseInfoByWuHanViewModel()
        {
            //OwnerViewModel.IsOfficial = "查询到国家工作人员信息，请移步人工窗口办理！";
            if (crjManager == null)
            {
                crjManager = new CrjPreapplyManager();
            }

            gaQzType = twQzType = hzSelect = false;
            var result = ServiceRegistry.Instance.Get<ElementManager>()?.Get<List<DictionaryType>>();
            CardTypes = result?.Where(t => t.KindType == ((int)KindType.ApplyType).ToString()).OrderBy(t => t.Code)
                .ToList();
            applyTypes = GetApplyTypes();
            Log.Instance.WriteInfo("=====进入填写基础信息界面=====");
            //TTS.PlaySound("完善个人基本信息");
            if (!OwnerViewModel.CheckServeStatus())
            {
                DoNext.Execute("NotificationPage");
            }

            //当没有信息时查询
            if (BookingBaseInfo?.BookingSource == 1)
            {
                Log.Instance.WriteInfo("无预约信息，前往查询历史申请信息");
                QueryAsync();
            }
            else
            {
                Log.Instance.WriteInfo("存在预约信息，前往判断取号模式");
                //根据配置判断是否跳过基本信息页面
                IsSkipBasicInfo();
            }


        }

        /// <summary>
        /// 港澳台签注默认值
        /// </summary>
        public ICommand SQOpCommand
        {
            get
            {
                return new RelayCommand<string>((param) =>
                {
                    if (int.TryParse(param, out int value))
                    {
                        GABookingInfo.SQOp = value;
                        gaValues = true;
                        foreach (var item in GABookingInfo.YYQZLIST)
                        {
                            item.IsUse = true;
                            GetNormalInfo(item, value);
                            switch (value)
                            {
                                //旅游签注
                                case 0:
                                    OwnerViewModel.isVALIDXCYY = "0";
                                    Log.Instance.WriteInfo("核查到旅游签注业务，人工号标识为0");
                                    break;
                                //其他签注
                                case 1:
                                    OwnerViewModel.isVALIDXCYY = "1";
                                    Log.Instance.WriteInfo("核查到其他签注业务，人工号标识为1");
                                    break;
                            }
                        }

                        var a = GABookingInfo.YYQZLIST;
                    }
                });
            }
        }

        /// <summary>
        /// 台湾签注默认值
        /// </summary>
        public ICommand SQOpTWNCommand
        {
            get
            {
                return new RelayCommand<string>((param) =>
                {
                    if (int.TryParse(param, out int value))
                    {
                        TWBookingInfo.SQOp = value;
                        twValues = true;
                        foreach (var item in TWBookingInfo.YYQZLIST)
                        {
                            item.IsUse = true;
                            GetNormalInfo(item, value);
                            switch (value)
                            {
                                //旅游签注
                                case 0:
                                    OwnerViewModel.isVALIDXCYY = "0";
                                    Log.Instance.WriteInfo("核查到旅游签注业务，人工号标识为0");
                                    break;
                                //其他签注
                                case 1:
                                    OwnerViewModel.isVALIDXCYY = "1";
                                    Log.Instance.WriteInfo("核查到其他签注业务，人工号标识为1");
                                    break;
                            }
                        }

                    }
                });
            }
        }

        /// <summary>
        /// 默认签注信息
        /// </summary>
        /// <param name="info">签注信息</param>
        /// <param name="value">类型0:旅游 1：其他</param>
        public void GetNormalInfo(EndorsementInfo info, int value)
        {
            if (value == 0)
            {
                if (info.QWD == "MAC" || info.QWD == "HKG")
                {

                    //默认团签注
                    info.QZType = new DictionaryType() { Code = "12", Description = "团队旅游签注" };
                    info.QZCount = new DictionaryType() { Code = "HKG1Y1T", Description = "1年一次" };

                }

                if (info.QWD == "TWN")
                {
                    info.QZType = new DictionaryType() { Code = "25", Description = "赴台团队旅游" };
                    info.QZCount = new DictionaryType() { Code = "TWN6M1T", Description = "6个月一次" };
                }
            }

            if (value == 1)
            {
                if (info.QWD == "MAC" || info.QWD == "HKG")
                {

                    //默认团签注
                    info.QZType = new DictionaryType() { Code = "19", Description = "其他签注" };
                    info.QZCount = new DictionaryType() { Code = "HKG1Y1T", Description = "1年一次" };
                }

                if (info.QWD == "TWN")
                {
                    info.QZType = new DictionaryType() { Code = "29", Description = "赴台其他签注" };
                    info.QZCount = new DictionaryType() { Code = "TWN6M1T", Description = "6个月一次" };
                }
            }
        }

        private void QueryAsync()
        {
            OwnerViewModel?.IsShowHiddenLoadingWait("正在查询请稍等......");
            Task.Run(() =>
            {
                try
                {

                    if (!DjzConfig.DjzModel.IsConnectionDjz)
                    {
                        return;
                    }

                    QueryApplyParameter model = new QueryApplyParameter()
                    {
                        sfzh = BookingBaseInfo.CardInfo.IDCardNo
                    };
                    //查询申请信息
                    Log.Instance.WriteInfo("-----开始查询历史申请信息-----");
                    var result = crjManager.QueryApply(model);
                    if (result.IsSucceed && result.ReturnValue != null)
                    {
                        var item = (result.ReturnValue as SqxxModel)?.cjsqxxs?[0];
                        if (item != null)
                        {
                            BookingBaseInfo.UrgentName = item.jjqklxr; //紧急联系人
                            BookingBaseInfo.UrgentTelephone = item.jjqklxrdh; //紧急联系人电话
                            BookingBaseInfo.Telephone = item.lxdh; //本人联系电话 

                            Log.Instance.WriteInfo("[申请信息查询]紧急联系人：" + CommandTools.FilterStrByNumber(item.jjqklxr) + "本人电话：" + CommandTools.FilterStrByNumber(item.lxdh));
                        }

                    }
                    else
                    {
                        Log.Instance.WriteInfo($"[申请信息查询]{result.MessageInfo}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteInfo($"[申请信息异常]");
                    Log.Instance.WriteError($"[申请信息异常]{ex.Message}");
                }
                finally
                {
                    Log.Instance.WriteInfo("-----结束查询历史申请信息-----");

                    //根据配置判断是否跳过基本信息页面
                    IsSkipBasicInfo();

                    OwnerViewModel?.IsShowHiddenLoadingWait();
                }
            });

        }

        public override void DoNextFunction(object obj)
        {

            if (!DataVerification(BookingBaseInfo, out string msg) && OwnerViewModel.IsTakePH_No == false) // && OwnerViewModel?.IsBeijing == false
            {
                OwnerViewModel?.MessageTips(msg);
                return;
            }

            //北京户口默认值
            if (OwnerViewModel?.IsWuHan == true && BookingBaseInfo.Address == null ||
                OwnerViewModel?.IsWuHan == true && string.IsNullOrWhiteSpace(BookingBaseInfo?.Address?.Code))
            {
                BookingBaseInfo.Address = new DictionaryType()
                {
                    Code = BookingBaseInfo.CardInfo.IDCardNo.Substring(0, 6)
                };
            }

            //初始化预约数据
            if (!ZHPHMachineWSHelper.ZHPHInstance.CreateBookingInfo(BookingBaseInfo, out string msg1))
            {
                OwnerViewModel?.MessageTips(msg1);
                return;
            }

            BookingBaseInfo.StrCardTypes = msg1.Substring(0, msg1.Length - 1);
            string str = BookingBaseInfo.IsExpress == 0 ? "公安机关领取" : "邮政";
            //Log.Instance.WriteInfo($"办证类型:{msg} 取证方式:{str}");
            Log.Instance.WriteInfo($"办证类型:{BookingBaseInfo.StrCardTypes}");
            Log.Instance.WriteInfo("=====离开填写基础信息页面=====");
            Log.Instance.WriteInfo("下一步：进入选择证件类型界面");
            //base.DoNextFunction("Booking/BookingInfo");
            base.DoNextFunction("ScanningPhotoReceipt");
        }

        /// <summary>
        /// 数据校验
        /// </summary>
        /// <param name="model"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool DataVerification(BookingModel model, out string msg)
        {
            msg = "";
            if (model?.SelectCardTypes == null || model?.SelectCardTypes?.Count <= 0)
            {
                TTS.PlaySound("预约机-提示-请选择办证类型");
                msg = TipMsgResource.NonEmptySelectsTipMsg;
                return false;
            }


            foreach (DictionaryType item in BookingBaseInfo.SelectCardTypes)
            {
                if (HZBookingInfo != null && item.Code.Contains(HZBookingInfo.SQLB))
                {
                    if (HZBookingInfo.ApplyType == null)
                    {
                        //修改 申请类型为空默认可办第一个

                        HZBookingInfo.ApplyType = GetApplyTypes().FirstOrDefault();

                    }

                    if (string.IsNullOrEmpty(HZBookingInfo?.CJSY))
                    {
                        HZBookingInfo.CJSY = "19";
                        HZBookingInfo.CJSYDescription = new DictionaryType() { Code = "19", Description = "旅游", KindType = "102" };
                    }

                    hzSelect = true;
                    //护照申请类别描述
                    msg += $" [护照] 申请类型:{HZBookingInfo?.ApplyType?.Description}";
                    //护照加注判断
                    if (HZBookingInfo.ApplyType != null && HZBookingInfo.ApplyType?.Code == "93")
                    {

                        string ywbh = "";
                        HZBookingInfo.YYJZBLLIST = new List<PassportNote>();
                        HZBookingInfo.YYJZBLLIST.Add(new PassportNote()
                        {
                            XH = 1,
                            YWBH = ywbh,
                            ZJZL = HZBookingInfo.XCZJZL?.ToString(),
                            ZJHM = HZBookingInfo.XCZJHM?.ToString(),
                            BGJZZL = "1A",
                            BGJZXM = HZBookingInfo.XCZJHM?.ToString(),
                            TBDWBH = QJTConfig.QJTModel?.QJTDevInfo?.TBDWBH
                        });


                    }
                    else
                    {
                        //非持证加注清空加注信息
                        HZBookingInfo.YYJZBLLIST.Clear();


                    }
                }

                if (TWBookingInfo != null && item.Code.Contains(TWBookingInfo.SQLB))
                {
                    if (TWBookingInfo.ApplyType == null)
                    {
                        TWBookingInfo.ApplyType = GetApplyTypes().FirstOrDefault();
                    }

                    twQzType = true;
                    //台湾申请类别描述
                    //var t = TWBookingInfo.YYQZLIST[0];
                    msg += $" [台湾] 办证类别:{TWBookingInfo.ApplyType.Description}";
                }

                if (GABookingInfo != null && item.Code.Contains(GABookingInfo.SQLB))
                {
                    if (GABookingInfo != null)
                    {
                        if (GABookingInfo.ApplyType == null)
                        {

                            GABookingInfo.ApplyType = GetApplyTypes().FirstOrDefault();

                        }
                    }

                    gaQzType = true;
                    //港澳通行证申请类别描述
                    msg += $" [香港+澳门] 办证类别:{GABookingInfo?.ApplyType?.Description}";

                }
            }

            //if (gaQzType)
            //{
            //    if (!gaValues)
            //    {
            //        msg = "请选择港澳通行证签注类型";
            //        return false;
            //    }

            //}

            //if (twQzType)
            //{
            //    if (!twValues)
            //    {
            //        msg = "请选择台湾通行证签注类型";
            //        return false;
            //    }
            //}


            //所有选择的都是首次 派人工号
            var selCount = model.SelectCardTypes.Count;
            int firstCount = 0;
            foreach (DictionaryType item in model.SelectCardTypes)
            {
                var firstVerification = model.BookingInfo.Where(t => t.SQLB == item?.Code && t.ApplyType?.Code == "11").ToList();

                if (firstVerification.Count > 0)
                    firstCount++;

                //武汉选择包含护照类，派人工号（A区）
                if (OwnerViewModel?.IsWuHan == true && item.Code == "101")
                {
                    OwnerViewModel.isVALIDXCYY = "1";
                    Log.Instance.WriteInfo("系统查询到选择护照类，区域参数isVALIDXCYY修改为1");
                }
            }

            return true;
        }

        /// <summary>
        /// 根据区域和取号模式显示跳转页面
        /// </summary>
        public void IsSkipBasicInfo()
        {
            Log.Instance.WriteInfo(OwnerViewModel?.IsTakePH_No == true ? "取号模式：直接取号" : "取号模式：现场预约+取号");

            if (!string.IsNullOrEmpty(OwnerViewModel?.IsOfficial))
            {
                TipsMsg = OwnerViewModel?.IsOfficial;
            }

            //显示照片
            if (!string.IsNullOrWhiteSpace(OwnerViewModel?.djzPhoto))
            {
                ImageStr = OwnerViewModel?.djzPhoto;
            }
            else
                Log.Instance.WriteInfo("显示人口照片为空！！！");


            //在办业务 在办理类型里删除
            if (OwnerViewModel?.zbywxx != null)
            {
                CardTypes = GetSQTypes();
                //Log.Instance.WriteInfo("核查在办业务不为空：");
                List<string> zbyw = new List<string>(OwnerViewModel?.zbywxx);
                if (OwnerViewModel?.zbywxx != null && zbyw.Count > 0)
                {
                    foreach (var item in zbyw)
                    {
                        Log.Instance.WriteInfo("核查在办业务：" + item);
                        var UnOver = CardTypes.Where(t => t.Code == item).ToList()[0];
                        CardTypes.Remove(UnOver);
                        //如果预约在办业务 删除预约业务
                        if (BookingBaseInfo.SelectCardTypes != null && BookingBaseInfo.SelectCardTypes.Count > 0 &&
                            BookingBaseInfo.SelectCardTypes.Contains(UnOver))
                        {
                            Log.Instance.WriteInfo("核查到预约存在在办业务：" + item);
                            BookingBaseInfo.SelectCardTypes.Remove(UnOver);
                            Log.Instance.WriteInfo("删除在办业务：" + item);
                        }

                    }

                    if (CardTypes.Count == 0)
                    {

                        //OwnerViewModel?.MessageTips("系统查询到您已办理业务【护照、港澳、赴台】，不能重复办理，请您再次核对，若有疑问，请移步人工窗口咨询！");
                        //改为弹出直接退出回首页 by 2021年7月7日15:35:24 wei.chen
                        OwnerViewModel?.MessageTips("系统查询到您已办理业务【护照、港澳、赴台】，不能重复办理，请您再次核对，若有疑问，请移步人工窗口咨询！", (() =>
                        {
                            //提示框内的倒计时卡死 by 2021年7月10日18:37:30
                            this.DoNextFunction(null);
                        }), null, 10, "返回");
                    }
                }
            }

            //修改北京外网预约 删除非预约业务
            if (BookingBaseInfo?.BookingSource == 0 && OwnerViewModel?._hasYYXX.Count > 0 && BookingBaseInfo?.Book_Type == "0") //OwnerViewModel?.IsBeijingRegister == false && OwnerViewModel?.IsBeijingRegister == false
            {
                List<DictionaryType> carTypesCope = new List<DictionaryType>();
                //if (OwnerViewModel?._hasYYXX.Count > 0 && BookingBaseInfo?.Book_Type == "0")//修改 非网上预约的
                //{
                var cardtype = CardTypes.Select(s => s.Code).ToList(); //可办业务
                var hasyyxx = OwnerViewModel?._hasYYXX.Select(t => t).ToList(); //预约业务
                foreach (var item in cardtype)
                {
                    //var a = unyyTypes.Where(t => t.Code != item).ToList();
                    if (hasyyxx.Contains(item))
                    {
                        var yyTypes = CardTypes.Where(t => t.Code == item).ToList()[0];
                        //预约业务
                        carTypesCope.Add(yyTypes);
                    }
                }

                //北京本地。预约业务已经办理，则可以办理非预约业务
                if (OwnerViewModel?.zbywxx != null && hasyyxx.Count > 0)
                {
                    foreach (var zbyw in OwnerViewModel?.zbywxx)
                    {
                        var bjyw = hasyyxx.Where(t => t == zbyw).ToList().FirstOrDefault();//办结业务  预约业务=在办业务
                        if (!string.IsNullOrEmpty(bjyw))
                        {
                            if (carTypesCope.Count == 0)
                            {
                                BookingBaseInfo.Book_Type = "1";
                                BookingBaseInfo.BookingSource = 1;
                                BookingBaseInfo.IsExpress = 0;
                                BookingBaseInfo.RecipientName = null;
                                BookingBaseInfo.RecipientTelephone = null;
                                BookingBaseInfo.EMSCode = null;
                                BookingBaseInfo.EMSAddress = null;
                                OwnerViewModel.Xxly = "00";//00现场预约 99移民局  15公安
                                //预约业务非在办 ，则显示所有可办业务
                                carTypesCope = CardTypes;
                            }

                        }
                        //var UnOver = CardTypes.Where(t => t.Code == bjyw).ToList()[0];
                        //CardTypes.Remove(UnOver);
                    }

                }

                CardTypes = carTypesCope;
                //}
            }


            StateInitializationAsync();

            //}

            //从配置项获取：取号模式为直接取号
            if (!QJTConfig.QJTModel.IsBasicInfo && OwnerViewModel?.IsTakePH_No == true)
            {
                if (BookingBaseInfo.Address == null || string.IsNullOrWhiteSpace(BookingBaseInfo.Address?.Code))
                {
                    BookingBaseInfo.Address = new DictionaryType()
                    {
                        Code = BookingBaseInfo.CardInfo.IDCardNo.Substring(0, 6)
                    };
                }

                if (BookingBaseInfo?.SelectCardTypes == null || BookingBaseInfo?.SelectCardTypes.Count <= 0)
                {
                    BookingBaseInfo.SelectCardTypes = CardTypes;
                }

                Log.Instance.WriteInfo("直接取号模式跳过基本信息填写");
                this.DoNextFunction(null);
            }

        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void StateInitializationAsync()
        {
            if (BookingBaseInfo.SelectCardTypes == null)
            {
                BookingBaseInfo.SelectCardTypes = BookingBaseInfo?.BookingSource == 1 ? null : CardTypes;
            }


            //Log.Instance.WriteInfo("初始化办证类型成功！！！" + BookingBaseInfo.SelectCardTypes?.Count);
            if (OwnerViewModel?.IsWuHan == true && BookingBaseInfo.Address == null ||
                OwnerViewModel?.IsWuHan == true && string.IsNullOrWhiteSpace(BookingBaseInfo?.Address?.Code))
            {
                BookingBaseInfo.Address = new DictionaryType()
                {
                    Code = BookingBaseInfo.CardInfo.IDCardNo.Substring(0, 6)
                };
            }

            //申请类型初始化
            string msgstr = "";
            if (BookingBaseInfo.SelectCardTypes.IsNotEmpty())
                ZHPHMachineWSHelper.ZHPHInstance.CreateBookingInfo(BookingBaseInfo, out msgstr);

            //Log.Instance.WriteInfo("初始化办证类型成功！！！" + BookingBaseInfo.SelectCardTypes?.Count);
            //初始化证件
            //初始化状态
            HZBookingInfo = TWBookingInfo = GABookingInfo = null;
            if (BookingBaseInfo?.BookingInfo != null && BookingBaseInfo.BookingInfo.Count > 0)
            {
                //有预约信息且在使用状态中
                HZBookingInfo = BookingBaseInfo.BookingInfo.FirstOrDefault(t =>
                    t.SQLB.Equals(((int)EnumTypeSQLB.HZ).ToString()) && t.IsUse);
                TWBookingInfo = BookingBaseInfo.BookingInfo.FirstOrDefault(t =>
                    t.SQLB.Equals(((int)EnumTypeSQLB.TWN).ToString()) && t.IsUse);
                GABookingInfo = BookingBaseInfo.BookingInfo.FirstOrDefault(t =>
                    t.SQLB.Equals(((int)EnumTypeSQLB.HKGMAC).ToString()) && t.IsUse);
                //Log.Instance.WriteInfo("初始化证件状态成功！！！");
            }

            if (HZBookingInfo != null && HZBookingInfo.ApplyType == null)
            {
                //启动新线程 根据身份证查询护照信息
                var paperInfo = QueryDocumentInfo(DocumentType.HZ, BookingBaseInfo?.CardInfo.IDCardNo);

                if (paperInfo != null)
                {
                    //存在护照信息
                    HZBookingInfo.XCZJZL = paperInfo.zjzl;
                    HZBookingInfo.XCZJHM = paperInfo.zjhm;
                    HZBookingInfo.XCZJQFRQ = paperInfo.qfrq;
                    HZBookingInfo.XCZJYXQZ = paperInfo.zjyxqz;
                    HZBookingInfo.DJSY = paperInfo.zjzt;
                    //更新护照加注信息
                    foreach (var item in HZBookingInfo.YYJZBLLIST)
                    {
                        item.ZJHM = paperInfo.zjhm;
                    }
                }

                HZBookingInfo.ApplyType = GetApplyTypes().FirstOrDefault();

                //根据护照信息，给护照签注默认值
                DataVerificate(EnumTypeSQLB.HZ, HZBookingInfo, out string msg);
                //if (!string.IsNullOrWhiteSpace(msg))
                //    HZBookingInfo.ApplyType = null;

            }

            if (TWBookingInfo != null && TWBookingInfo.ApplyType == null)
            {

                //查询台湾证件信息
                var paperInfo = QueryDocumentInfo(DocumentType.TW, BookingBaseInfo.CardInfo.IDCardNo);
                if (paperInfo != null)
                {
                    //存在台湾证件信息
                    TWBookingInfo.XCZJZL = paperInfo.zjzl;
                    TWBookingInfo.XCZJHM = paperInfo.zjhm;
                    TWBookingInfo.XCZJQFRQ = paperInfo.qfrq;
                    TWBookingInfo.XCZJYXQZ = paperInfo.zjyxqz;
                    TWBookingInfo.DJSY = paperInfo.zjzt;
                }

                TWBookingInfo.ApplyType = GetApplyTypes().FirstOrDefault();
                //根据台湾证件信息，给台湾签注默认值
                DataVerificate(EnumTypeSQLB.TWN, TWBookingInfo, out string msg);
                //if (!string.IsNullOrWhiteSpace(msg))
                //    TWBookingInfo.ApplyType = null;
            }

            if (GABookingInfo != null && GABookingInfo.ApplyType == null)
            {
                //存在港澳信息
                //获取港澳加注信息
                var itemHK = GABookingInfo.YYQZLIST[0];
                var itemMAC = GABookingInfo.YYQZLIST[1];


                //查询港澳通行证信息
                var paperInfo = QueryDocumentInfo(DocumentType.GA, BookingBaseInfo.CardInfo.IDCardNo);
                if (paperInfo != null)
                {
                    //存在港澳通行证信息
                    GABookingInfo.XCZJZL = paperInfo.zjzl;
                    GABookingInfo.XCZJHM = paperInfo.zjhm;
                    GABookingInfo.XCZJQFRQ = paperInfo.qfrq;
                    GABookingInfo.XCZJYXQZ = paperInfo.zjyxqz;
                    GABookingInfo.DJSY = paperInfo.zjzt;
                }

                GABookingInfo.ApplyType = GetApplyTypes().FirstOrDefault();
                DataVerificate(EnumTypeSQLB.HKGMAC, GABookingInfo, out string msg);
                //if (!string.IsNullOrWhiteSpace(msg))
                //    GABookingInfo.ApplyType = null;
            }

        }

        /// <summary>
        /// 数据验证
        /// </summary>
        /// <param name="type">办理类别</param>
        /// <param name="info"></param>
        /// <param name="msg"></param>
        /// <param name="isApply"></param>
        private void DataVerificate(EnumTypeSQLB type, BookingInfo info, out string msg, bool isApply = true)
        {
            msg = "";
            var PaperInfo = OwnerViewModel?.PaperInfos;
            //获取证件有效期
            string zjyxq = info.XCZJYXQZ;
            //获取间隔时间
            int days = DaysInterval(info.XCZJYXQZ);

            if (type == EnumTypeSQLB.TWN && info != null)
            {
                var twnitem = info.YYQZLIST[0];
                //校验签注类型
                if (twnitem.QZType == null)
                {
                    //默认团签注
                    twnitem.QZType = new DictionaryType() { Code = "25", Description = "赴台团队旅游" };

                }
                //校验签注次数
                if (twnitem.QZCount == null)
                {
                    //默认6个月一次
                    twnitem.QZCount = new DictionaryType() { Code = "TWN6M1T", Description = "6个月一次" };
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(zjyxq) && days < 365 &&
                        twnitem.QZCount.Code != "TWN6M1T" && info.ApplyType.Code == "92" && twnitem.IsUse == true)
                    {
                        //TTS.PlaySound("预约机-提示-办证类型填写有误，请重新填写");
                        msg = TipMsgResource.DocumentTWNYearTipMsg;
                    }
                }
            }
            if (type == EnumTypeSQLB.HKGMAC && info != null)
            {
                var hkitem = info.YYQZLIST[0];
                var macitem = info.YYQZLIST[1];

                if (hkitem.QZType == null)
                {
                    //广东省非全国异地办证默认个人旅游签注
                    if (info?.YDBZBS != "2" && BookingBaseInfo?.Address?.Code?.Substring(0, 2).Equals("44") == true)
                    {
                        //默认个签注
                        hkitem.QZType = new DictionaryType() { Code = "1B", Description = "个人旅游签注" };
                    }
                    else
                    {
                        //默认团签注
                        hkitem.QZType = new DictionaryType() { Code = "12", Description = "团队旅游签注" };
                    }
                }
                if (macitem.QZType == null)
                {
                    //广东省非全国异地办证默认个人旅游签注
                    if (info?.YDBZBS != "2" && BookingBaseInfo?.Address?.Code?.Substring(0, 2).Equals("44") == true)
                    {
                        //默认个签注
                        macitem.QZType = new DictionaryType() { Code = "1B", Description = "个人旅游签注" };
                    }
                    else
                    {
                        //默认团签注
                        macitem.QZType = new DictionaryType() { Code = "12", Description = "团队旅游签注" };
                    }
                }
                if (hkitem.QZCount == null)
                {

                    if (!string.IsNullOrWhiteSpace(zjyxq) && days < 365)
                    {
                        //证件有效期小于1年默认3个月一次
                        hkitem.QZCount = new DictionaryType() { Code = "HKG3M1T", Description = "3个月一次" };
                    }
                    else
                    {
                        //广东省非全国异地办证且深圳户口证件有效期大于一年默认1年多次
                        if (info?.YDBZBS != "2" && BookingBaseInfo?.Address?.Code?.Substring(0, 4).Equals("4403") == true)
                        {
                            hkitem.QZCount = new DictionaryType() { Code = "HKG1Y9T", Description = "1年多次" };
                        }
                        else
                        {
                            hkitem.QZCount = new DictionaryType() { Code = "HKG1Y1T", Description = "1年一次" };
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(zjyxq) && days < 365 &&
                        hkitem.QZCount.Code != "HKG3M1T" && info.ApplyType.Code == "92" && hkitem.IsUse == true)
                    {
                        //TTS.PlaySound("预约机-提示-办证类型填写有误，请重新填写");
                        msg = TipMsgResource.DocumentGAYearTipMsg;
                        hkitem.QZCount = null;
                    }
                    //非全国异地办证 且 广东省内 才能签注一年多次
                    else if ((info?.YDBZBS == "2" || BookingBaseInfo?.Address?.Code?.Substring(0, 4).Equals("4403") == false) &&
                        hkitem.QZCount?.Code == "HKG1Y9T")
                    {
                        //TTS.PlaySound("预约机-提示-办证类型填写有误，请重新填写");
                        msg = TipMsgResource.HKGErrorTipMsg;
                        hkitem.QZCount = null;
                    }
                }
                if (macitem.QZCount == null)
                {
                    if (!string.IsNullOrWhiteSpace(zjyxq) && days < 365)
                    {
                        macitem.QZCount = new DictionaryType() { Code = "MAC3M1T", Description = "3个月一次" };
                    }
                    else
                    {
                        macitem.QZCount = new DictionaryType() { Code = "MAC1Y1T", Description = "1年一次" };
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(zjyxq) && days < 365 &&
                        macitem.QZCount.Code != "MAC3M1T" && info.ApplyType.Code == "92" && macitem.IsUse == true)
                    {
                        //TTS.PlaySound("预约机-提示-办证类型填写有误，请重新填写");
                        msg = TipMsgResource.DocumentGAYearTipMsg;
                        macitem.QZCount = null;
                    }
                }
            }

        }

        /// <summary>
        /// 查询证件信息
        /// </summary>
        public PaperInfo QueryDocumentInfo(DocumentType type, string idCardNo)
        {

            PaperInfo model = null;
            try
            {
                if (!DjzConfig.DjzModel.IsConnectionDjz)
                {
                    string url = "";
                    if (type == DocumentType.HZ)
                    {
                        url = "ViewModels\\Test\\hz.json";
                    }
                    else if (type == DocumentType.GA)
                    {
                        url = "ViewModels\\Test\\ga.json";
                    }
                    else
                    {
                        url = "ViewModels\\Test\\twn.json";
                    }
                    string path = Path.Combine(FileHelper.GetLocalPath(), url);
                    if (File.Exists(path))
                    {
                        string str = FileHelper.ReadFileContent(path);
                        model = JsonHelper.ConvertToObject<PaperInfo>(str);

                    }

                    return model;
                }
                //查询全国公民证件信信息查询接口
                var result = crjManager.QueryDocumentInfo(((int)type).ToString(), idCardNo, string.Empty, string.Empty, string.Empty, string.Empty);

                if (result.ReturnValue != null && result.ReturnValue is List<PaperInfo>)
                {
                    var lst = result.ReturnValue as List<PaperInfo>;
                    model = lst?.OrderByDescending(t => t.zjyxqz)?.First();
                    Log.Instance.WriteInfo("全国证件信息接口查询成功：" + CommandTools.FilterStrByNumber(model?.zwxm) + ",号码：" + CommandTools.FilterStrByNumber(model?.zjhm) + "，状态：" + model?.zjzt + "，有效期至：" + model?.zjyxqz);

                    //model = lst.FirstOrDefault(t => t.zjzt == "1" && t.zjyxqz.CompareTo(DateTime.Now.ToString("yyyyMMdd")) > 0);
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("查询证件信息异常");
                Log.Instance.WriteError($"[查询证件信息异常]{ex.Message}");
            }
            return model;

        }


        private bool TipMsg(string msg)
        {
            if (!string.IsNullOrWhiteSpace(msg))
            {
                OwnerViewModel?.MessageTips(msg);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 取申请类型
        /// </summary>
        /// <returns></returns>
        private List<DictionaryType> GetSQTypes()
        {
            return ServiceRegistry.Instance.Get<ElementManager>()?.Get<List<DictionaryType>>()
                ?.Where(t => t.KindType == ((int)KindType.ApplyType).ToString()).OrderBy(t => t.Code).ToList();
        }

        /// <summary>
        /// 取接口所有申请类别
        /// </summary>
        /// <returns>申请类别集合</returns>
        private List<DictionaryType> GetApplyTypes()
        {
            var config = ServiceRegistry.Instance.Get<ElementManager>();
            //护照申请类别
            var hzApplyTypes = config.Get<List<DictionaryType>>("ApplyCategoryHZ") ?? new List<DictionaryType>();
            //港澳申请类别
            var gaApplyTypes = config.Get<List<DictionaryType>>("ApplyCategoryGA") ?? new List<DictionaryType>();
            //台湾申请类别
            var twApplyTypes = config.Get<List<DictionaryType>>("ApplyCategoryTWN") ?? new List<DictionaryType>();
            var lstAll = hzApplyTypes.Union(gaApplyTypes).Union(twApplyTypes)?.GroupBy(t => new { t.Code, t.Description })
                .Select(t => new DictionaryType()
                {
                    Description = t.Key.Description,
                    Code = t.Key.Code
                }).ToList();
            return lstAll;
        }


        /// <summary>
        /// 获取时间与当前时间间隔
        /// </summary>
        /// <param name="zjyxq"></param>
        /// <returns></returns>
        private int DaysInterval(string zjyxq)
        {
            if (string.IsNullOrWhiteSpace(zjyxq)) { return 0; }
            //证件有效日期
            DateTime dt = DateTime.ParseExact(zjyxq, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            //当前日期
            DateTime currentdt = DateTime.Now;

            return dt.Subtract(currentdt).Days;
        }

        protected override void OnDispose()
        {
            CommonHelper.OnDispose();
            base.OnDispose();
        }


    }
}
