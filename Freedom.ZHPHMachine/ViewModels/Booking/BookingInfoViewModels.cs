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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Freedom.Models.TJJsonModels;

namespace Freedom.ZHPHMachine.ViewModels
{
    public class BookingInfoViewModels : ViewModelBase
    {
        #region 字段
        private CrjPreapplyManager crjManager;
        private List<DictionaryType> applyTypes;
        private List<DictionaryType> departureReasonTypes;
        private List<DictionaryType> destinationTypes;
        #endregion

        #region 构造函数
        public BookingInfoViewModels()
        {
            if (crjManager == null) { crjManager = new CrjPreapplyManager(); }
            //获取申请类型
            applyTypes = GetApplyTypes();
            departureReasonTypes = GetDepartureReasonTypes();
            destinationTypes = GetDestinationTypes();
        }
        #endregion

        #region 属性 

        /// <summary>
        /// 申请类型选择
        /// </summary>
        public ICommand TextBoxGotFocusCommand
        {
            get
            {
                return new RelayCommand<String>((param) =>
                {
                    if (int.TryParse(param, out int type))
                    {
                        string key = "";
                        switch (type)
                        {
                            case 1:
                                key = HZBookingInfo?.ApplyType?.Description;
                                break;
                            case 2:
                                key = GABookingInfo?.ApplyType?.Description;
                                break;
                            case 3:
                                key = TWBookingInfo?.ApplyType?.Description;
                                break;
                        }

                        if (CommonHelper.PopupDictionary(OperaType.ApplyType, key, out DictionaryType model, null, type))
                        {
                            string msg = "";
                            switch (type)
                            {
                                case 1:
                                    HZBookingInfo.ApplyType = model;
                                    DataVerification(EnumTypeSQLB.HZ, HZBookingInfo, out msg);
                                    if (TipMsg(msg))
                                    {
                                        HZBookingInfo.ApplyType = null;
                                    }
                                    break;
                                case 2:
                                    GABookingInfo.ApplyType = model;
                                    DataVerification(EnumTypeSQLB.HKGMAC, GABookingInfo, out msg);
                                    if (TipMsg(msg))
                                    {
                                        GABookingInfo.ApplyType = null;
                                    }
                                    break;
                                case 3:
                                    TWBookingInfo.ApplyType = model;
                                    DataVerification(EnumTypeSQLB.TWN, TWBookingInfo, out msg);
                                    if (TipMsg(msg))
                                    {
                                        TWBookingInfo.ApplyType = null;
                                    }
                                    break;
                            }
                        }
                    }

                });
            }
        }

        /// <summary>
        /// 签注种类选择
        /// </summary>
        public ICommand ApplyCommand
        {
            get
            {
                return new RelayCommand<String>((param) =>
                {
                    if (int.TryParse(param, out int type))
                    {
                        if (type == (int)EnumTypeSQLB.TWN)
                        {
                            var item = TWBookingInfo.YYQZLIST[0];
                            if (CommonHelper.PopupDictionary(OperaType.TW, item?.QZType?.Description, out DictionaryType model))
                            {
                                if (item.QZType == null || !item.QZType.Code.Equals(model.Code))
                                {
                                    item.QZType = model;
                                    item.QZCount = null;
                                }
                            }

                        }
                    }
                });
            }
        }

        /// <summary>
        /// 出境事由
        /// </summary>
        public ICommand CJSYCommand
        {
            get
            {
                return new RelayCommand<String>((param) =>
                {
                    if (int.TryParse(param, out int type))
                    {
                        var item = HZBookingInfo.CJSYDescription;
                        if (CommonHelper.PopupDictionary(OperaType.DepartureReason, item?.Description, out DictionaryType model))
                        {
                            //if (item == null || !item.Code.Equals(model.Code))
                            //{
                            //    item = model;

                            //}


                            HZBookingInfo.CJSYDescription = model;
                            HZBookingInfo.IsUse = true;
                            HZBookingInfo.CJSY = model.Code;

                        }
                    }

                });
            }
        }

        /// <summary>
        /// 护照前往地
        /// </summary>
        public ICommand QwdCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var item = HZBookingInfo?.QwdDescription?.Description;
                    if (CommonHelper.PopupDicByPage(OperaType.Destination, item, out DictionaryTypeByPinYin model))
                    {
                        HZBookingInfo.QWD = model?.Code;
                        HZBookingInfo.IsUse = true;
                        HZBookingInfo.QwdDescription = model;

                    }

                });
            }
        }



        /// <summary>
        /// 签注次数选择
        /// </summary>
        public ICommand QZTypeCommand
        {
            get
            {
                return new RelayCommand<String>((param) =>
                {
                    var item = TWBookingInfo.YYQZLIST[0];
                    if (int.TryParse(param, out int type))
                    {
                        if (CommonHelper.PopupDictionary(OperaType.QZCount, item?.QZCount?.Description, out DictionaryType model, item.QZType.Code, type))
                        {
                            item.QZCount = model;
                            DataVerification(EnumTypeSQLB.TWN, TWBookingInfo, out string msg, false);
                            if (TipMsg(msg))
                            {
                                item.QZCount = null;
                            }

                        }
                    }
                });
            }
        }


        /// <summary>
        /// 与申请人关系
        /// </summary>
        public ICommand RelationCommand
        {
            get
            {
                return new RelayCommand<String>((param) =>
                {
                    if (CommonHelper.PopupDictionary(OperaType.RelationType, GABookingInfo?.RelationType?.Description, out DictionaryType model))
                    {
                        GABookingInfo.RelationType = model;
                    }
                });
            }
        }

        /// <summary>
        /// 弹出键盘
        /// </summary>
        public ICommand NumberKeyboardCommand
        {
            get
            {
                return new RelayCommand<String>((param) =>
                {
                    string content = "";
                    if (param == "港澳身份证号码")
                    {
                        content = GABookingInfo.GAQSSFZHM;
                    }
                    else if (param == "曾持照加注")
                    {
                        content = HZBookingInfo.YYJZBLLIST[2].BGJZXM;
                    }
                    else
                    {
                        content = GABookingInfo?.GAJMLWNDTXHM;
                    }
                    if (CommonHelper.PopupNumberKeyboard(KeyboardType.ID, content, out string str, param))
                    {
                        if (param == "港澳身份证号码")
                        {
                            GABookingInfo.GAQSSFZHM = str;
                        }
                        else if (param == "曾持照加注")
                        {
                            HZBookingInfo.YYJZBLLIST[2].BGJZXM = str;
                        }
                        else
                        {
                            GABookingInfo.GAJMLWNDTXHM = str;
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 港澳前往地
        /// </summary>
        public ICommand GAQWDCommand
        {
            get
            {
                return new RelayCommand<string>((param) =>
                {
                    if (gABookingInfo.SQOp == 2) { return; }
                    EndorsementInfo model = null;
                    if (param == "HK")
                    {
                        GABookingInfo.QWD = EnumTypeQWD.HKG.ToString();
                        model = gABookingInfo.YYQZLIST.FirstOrDefault(t => t.QWD.Equals(EnumTypeQWD.MAC.ToString()));
                    }
                    else if (param == "MC")
                    {
                        GABookingInfo.QWD = EnumTypeQWD.MAC.ToString();
                        model = gABookingInfo.YYQZLIST.FirstOrDefault(t => t.QWD.Equals(EnumTypeQWD.HKG.ToString()));
                    }
                    else GABookingInfo.QWD = "999";
                    if (model != null)
                        model.IsUse = false;
                    else
                    {
                        foreach (var item in gABookingInfo.YYQZLIST)
                        {
                            item.IsUse = true;
                        }
                    }

                });
            }
        }

        /// <summary>
        /// 操作 0通行证和签注 1签注 2 通行证
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
                        bool isUse = true;
                        if (value == 2) isUse = false;
                        foreach (var item in GABookingInfo.YYQZLIST)
                        {
                            item.IsUse = isUse;
                        }
                        if (value == 1)
                        {
                            GABookingInfo.ApplyType = applyTypes.FirstOrDefault(t => t.Code == "92");
                        }
                        if (value == 2 && GABookingInfo?.ApplyType?.Code == "92")
                        {
                            GABookingInfo.ApplyType = null;
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 操作 0通行证和签注 1签注 2 通行证
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
                        bool isUse = true;
                        if (value == 2) isUse = false;
                        foreach (var item in TWBookingInfo.YYQZLIST)
                        {
                            item.IsUse = isUse;
                        }
                        if (value == 1)
                        {
                            TWBookingInfo.ApplyType = applyTypes.FirstOrDefault(t => t.Code == "92");
                        }
                        if (value == 2 && TWBookingInfo?.ApplyType?.Code == "92")
                        {
                            TWBookingInfo.ApplyType = null;
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 手写板
        /// </summary>
        public ICommand HandwritingCommand
        {
            get
            {
                return new RelayCommand<string>((param) =>
                {
                    string content = "";
                    if (param == "曾用名")
                    {
                        content = HZBookingInfo.YYJZBLLIST[0].BGJZXM;
                    }
                    else if (param == "姓名")
                    {
                        content = HZBookingInfo.YYJZBLLIST[1].BGJZXM;
                    }
                    else if (param == "曾持照加注")
                    {
                        content = HZBookingInfo.YYJZBLLIST[2].BGJZXM;
                    }
                    else if (param == "港澳亲属姓名")
                    {
                        content = GABookingInfo.GAQSZWXM;
                    }
                    if (CommonHelper.PopupHandwritingInput(param, content, out string str))
                    {
                        if (param == "曾用名")
                        {
                            HZBookingInfo.YYJZBLLIST[0].BGJZXM = str;
                        }
                        else if (param == "姓名")
                        {
                            HZBookingInfo.YYJZBLLIST[1].BGJZXM = str;
                        }
                        else if (param == "曾持照加注")
                        {
                            HZBookingInfo.YYJZBLLIST[2].BGJZXM = str;
                        }
                        else if (param == "港澳亲属姓名")
                        {
                            GABookingInfo.GAQSZWXM = str;
                        }
                    }
                });
            }
        }

        public ICommand SelectGACommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var hk = this.GABookingInfo?.YYQZLIST?[0];
                    var mac = this.GABookingInfo?.YYQZLIST?[1];

                    //TextBoxGotFocusCommandLog.Instance.WriteInfo("点击香港澳门签注：");
                    var param = new Tuple<string, string, string, string, string>(GABookingInfo?.QWD, hk?.QZType?.Code, hk?.QZCount?.Code, mac?.QZType?.Code, mac?.QZCount?.Code);
                    //Log.Instance.WriteInfo("点击香港澳门签注：" + param);

                    if (CommonHelper.PopupSelectGA(param, out SelectGAViewModels model))
                    {
                        hk.QZType = model?.HKQZType;
                        hk.QZCount = model?.HKQZCount;
                        mac.QZType = model?.MACQZType;
                        mac.QZCount = model?.MACQZCount;
                        var qwdType = model?.QWDSelected;

                        if (qwdType == EnumTypeQWD.HKG.ToString())
                        {
                            mac.IsUse = false;
                            GABookingInfo.QWD = qwdType;
                        }
                        else if (qwdType == EnumTypeQWD.MAC.ToString())
                        {
                            hk.IsUse = false;
                            GABookingInfo.QWD = qwdType;
                        }
                        else
                        {
                            mac.IsUse = true;
                            hk.IsUse = true;
                            GABookingInfo.QWD = ((int)EnumTypeQWD.HKMC).ToString();
                        }
                        DataVerification(EnumTypeSQLB.HKGMAC, GABookingInfo, out string msg, false);
                        TipMsg(msg);
                    }
                });
            }
        }

        public BookingInfo hZBookingInfo;
        /// <summary>
        /// 护照预约信息
        /// </summary>
        public BookingInfo HZBookingInfo
        {
            get { return hZBookingInfo; }
            set { hZBookingInfo = value; RaisePropertyChanged("HZBookingInfo"); }
        }

        public BookingInfo tWBookingInfo;
        /// <summary>
        /// 台湾预约信息
        /// </summary>
        public BookingInfo TWBookingInfo
        {
            get { return tWBookingInfo; }
            set { tWBookingInfo = value; RaisePropertyChanged("TWBookingInfo"); }
        }

        public BookingInfo gABookingInfo;
        /// <summary>
        /// 港澳预约信息
        /// </summary>
        public BookingInfo GABookingInfo
        {
            get { return gABookingInfo; }
            set { gABookingInfo = value; RaisePropertyChanged("GABookingInfo"); }
        }

        #endregion

        #region 方法
        /// <summary>
        /// 状态初始化
        /// </summary>
        public void StateInitializationAsync()
        {
            OwnerViewModel?.IsShowHiddenLoadingWait("正在查询,请稍等.....");
            TTS.PlaySound("预约机-页面-请完善个人申请信息");
            Log.Instance.WriteInfo("=====进入证件类型页面=====");
            if (!OwnerViewModel.CheckServeStatus())
            {
                DoNext.Execute("NotificationPage");
            }
            //初始化状态
            HZBookingInfo = TWBookingInfo = GABookingInfo = null;
            if (BookingBaseInfo?.BookingInfo != null && BookingBaseInfo.BookingInfo.Count > 0)
            {
                //有预约信息且在使用状态中
                HZBookingInfo = BookingBaseInfo.BookingInfo.FirstOrDefault(t => t.SQLB.Equals(((int)EnumTypeSQLB.HZ).ToString()) && t.IsUse);
                TWBookingInfo = BookingBaseInfo.BookingInfo.FirstOrDefault(t => t.SQLB.Equals(((int)EnumTypeSQLB.TWN).ToString()) && t.IsUse);
                GABookingInfo = BookingBaseInfo.BookingInfo.FirstOrDefault(t => t.SQLB.Equals(((int)EnumTypeSQLB.HKGMAC).ToString()) && t.IsUse);
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


                //根据护照信息，给护照签注默认值
                DataVerification(EnumTypeSQLB.HZ, HZBookingInfo, out string msg);
                //给出境事由 前往地赋值
                //var CJSY = HZBookingInfo.CJSY;


            }
            if (TWBookingInfo != null && TWBookingInfo.ApplyType == null)
            {
                //存在台湾预约信息
                //获取加注信息
                var item = TWBookingInfo.YYQZLIST[0];

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


                //根据台湾证件信息，给台湾签注默认值
                DataVerification(EnumTypeSQLB.TWN, TWBookingInfo, out string msg);
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


                //根据港澳信息，给港澳签注默认值
                DataVerification(EnumTypeSQLB.HKGMAC, GABookingInfo, out string msg);
            }
            OwnerViewModel?.IsShowHiddenLoadingWait();

            if (OwnerViewModel?.IsTakePH_No == true || OwnerViewModel?.IsShenZhen == true)
            {
                Log.Instance.WriteInfo("直接取号模式跳过证件类型页面");
                this.DoNextFunction(null);
            }
            //根据配置项，北京地区跳过基本信息填写，默认护照，台湾，港澳信息
            //if (!QJTConfig.QJTModel.IsBasicInfo && OwnerViewModel?.IsBeijing == true && OwnerViewModel?.IsBeijingRegister == true || !QJTConfig.QJTModel.IsBasicInfo && BookingBaseInfo?.BookingSource == 0)
            //{
            //    Log.Instance.WriteInfo("北京本市户口预约跳过证件类型页面");
            //    this.DoNextFunction(null);
            //}


        }

        /// <summary>
        /// 查询证件信息
        /// </summary>
        private PaperInfo QueryDocumentInfo(DocumentType type, string idCardNo)
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

                #region MyRegion

                //不能通过身份证号码查询
                //if (!result.IsSucceed)
                //{
                //    string zjzt = string.Empty;
                //    //查询异地全国公民证件信信息查询接口
                //    result = crjManager.Qg_cn_queryPaperExact(idCardNo, ((int)type).ToString(), out zjzt);
                //    string msg = string.Empty;
                //    if (result.IsSucceed && (zjzt == "31" || zjzt == "32" || zjzt == "33" || zjzt == "34"))
                //    {
                //        msg = "抱歉，您证件状态无效，不符合自助办理证件的条件，请到人工窗口咨询！";
                //        Log.Instance.WriteInfo(msg);
                //        MainWindowViewModels.Instance.MessageTips(msg, () =>
                //        {
                //            this.DoExit.Execute(null);
                //        });
                //        return;
                //    }
                //    if (!result.IsSucceed)
                //    {
                //        msg = "抱歉，获取证件信息失败，请稍候再试！";
                //        Log.Instance.WriteInfo(msg);
                //        MainWindowViewModels.Instance.MessageTips(msg, () =>
                //        {
                //            this.DoExit.Execute(null);
                //        });
                //        return;
                //    }
                //}

                #endregion

                if (result.ReturnValue != null && result.ReturnValue is List<PaperInfo>)
                {
                    var lst = result.ReturnValue as List<PaperInfo>;
                    model = lst?.OrderByDescending(t => t.zjyxqz)?.First();
                    Log.Instance.WriteInfo("全国证件信息接口查询成功：" + CommandTools.ReplaceWithSpecialChar(model?.zwxm, 1, 1) + ",号码：" + CommandTools.ReplaceWithSpecialChar(model?.zjhm) + "，状态：" + model?.zjzt + "，有效期至：" + model?.zjyxqz);

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

        protected override void OnDispose()
        {
            CommonHelper.OnDispose();
            base.OnDispose();
        }

        /// <summary>
        /// 前往地
        /// </summary>
        /// <param name="qwd"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        private List<DictionaryType> QueryHKQZCount(EnumTypeQWD qwd, string code)
        {
            var config = ServiceRegistry.Instance.Get<ElementManager>();
            var item = config.Get<List<QZZLList>>()?.FirstOrDefault(t => t.CODE == code);
            return item?.QZLXList?.Where(t => t.QWD == qwd.ToString() && t.STATUS == "1")?.Select(t => new DictionaryType()
            {
                Description = t.DESCRIPTION,
                Code = t.CODE
            })?.ToList();
        }

        public override void DoNextFunction(object obj)
        {
            string msg = string.Empty;
            if (OwnerViewModel?.IsShenZhen == false && OwnerViewModel?.IsTakePH_No == false && !DataVerification(out msg))
            {
                OwnerViewModel?.MessageTips(msg);
                return;
            }
            try
            {
                if (HZBookingInfo != null)
                {

                    if (string.IsNullOrEmpty(HZBookingInfo?.CJSY))
                    {
                        HZBookingInfo.CJSY = "19";
                        HZBookingInfo.CJSYDescription = new DictionaryType() { Code = "19", Description = "旅游", KindType = "102" };

                    }

                    //护照申请类别描述
                    //非首次、换发、失效重领 派人工号
                    msg += $" [护照] 申请类型:{HZBookingInfo?.ApplyType?.Description}，出境事由：{hZBookingInfo?.CJSYDescription?.Description}，前往地:{HZBookingInfo?.QwdDescription?.Description}，";

                    if (HZBookingInfo?.ApplyType?.Code != "11" && HZBookingInfo?.ApplyType?.Code != "13" && HZBookingInfo?.ApplyType?.Code != "31")
                    {
                        OwnerViewModel.isVALIDXCYY = "1";
                        Log.Instance.WriteInfo("护照申请类型非首次、换发、失效重领，派人工号标识为1");
                    }

                    //护照加注判断
                    if (HZBookingInfo.ApplyType != null && HZBookingInfo.ApplyType.Code == "93")
                    {
                        var lst = HZBookingInfo.YYJZBLLIST?.Where(t => string.IsNullOrWhiteSpace(t.BGJZXM) == true).ToList();
                        if (lst.Count() >= 3)
                        {
                            OwnerViewModel?.MessageTips("请填写加注事由");
                            return;
                        }
                        else
                        {
                            foreach (var item in lst)
                            {
                                HZBookingInfo.YYJZBLLIST.Remove(item);
                            }

                        }
                    }
                    else
                    {
                        //非持证加注清空加注信息
                        HZBookingInfo.YYJZBLLIST.Clear();
                    }
                }

                if (TWBookingInfo != null)
                {
                    if (TWBookingInfo.ApplyType.Code == "92")
                    {
                        
                        Log.Instance.WriteInfo("检测到台湾持证签注，人工号判断为True");
                        if (OwnerViewModel?.IsXuwen == true || OwnerViewModel?.IsWuHan == true)
                            OwnerViewModel.isVALIDXCYY = "1";
                    }

                    if (TWBookingInfo.YYQZLIST.Count > 0 && TWBookingInfo.YYQZLIST[0].QZType.Code == "21")
                    {
                        OwnerViewModel.IsManual = true;
                        Log.Instance.WriteInfo("检测到台湾探亲签注，人工号判断为True");
                        if (OwnerViewModel?.IsWuHan == true)
                            OwnerViewModel.isVALIDXCYY = "1";
                    }

                    //非首次、换发、失效重领 派人工号
                    if (TWBookingInfo?.ApplyType?.Code != "11" && TWBookingInfo?.ApplyType?.Code != "13" && TWBookingInfo?.ApplyType?.Code != "31")//换发补发 派人工号
                    {
                        OwnerViewModel.isVALIDXCYY = "1";
                        Log.Instance.WriteInfo("台湾通行证申请类型非首次、换发、失效重领，派人工号标识为1");
                    }

                    //台湾申请类别描述
                    var item = TWBookingInfo.YYQZLIST[0];
                    msg += $" [台湾] 办证类别:{TWBookingInfo.ApplyType.Description} 申请事由:{item.QZType.Description} 签注种类:{item.QZCount.Description}";
                }
                if (GABookingInfo != null)
                {
                    if (GABookingInfo.ApplyType.Code == "92")
                    {
                        Log.Instance.WriteInfo("检测到港澳持证签注，人工号判断为True");
                        if (OwnerViewModel?.IsXuwen == true)
                            OwnerViewModel.isVALIDXCYY = "1";
                    }

                    //if (GABookingInfo.YYQZLIST.Count > 0 && GABookingInfo.YYQZLIST[0].QZType.Code == "11")
                    //{
                    //    OwnerViewModel.IsManual = true;
                    //    Log.Instance.WriteInfo("检测到港澳探亲签注，人工号判断为True");
                    //    if (OwnerViewModel?.IsWuHan == true)
                    //        OwnerViewModel.isVALIDXCYY = "1";
                    //}

                    //非首次、换发、失效重领 派人工号
                    if (GABookingInfo?.ApplyType?.Code != "11" && GABookingInfo?.ApplyType?.Code != "13" && GABookingInfo?.ApplyType?.Code != "31")//换发补发 派人工号
                    {
                        OwnerViewModel.isVALIDXCYY = "1";
                        Log.Instance.WriteInfo("港澳通行证申请类型非首次、换发、失效重领，派人工号标识为1");
                    }

                    //港澳通行证申请类别描述
                    msg += $" [香港+澳门] 办证类别:{GABookingInfo.ApplyType.Description}";

                    //加注信息判断
                    var itemHK = GABookingInfo.YYQZLIST[0];
                    var itemMAC = GABookingInfo.YYQZLIST[1];
                    if ((itemHK.QZType.Code == "11" && itemHK.IsUse) ||
                        (itemMAC.QZType.Code == "11" && itemMAC.IsUse))
                    {
                        if (string.IsNullOrWhiteSpace(GABookingInfo.GAQSZWXM) ||
                           string.IsNullOrWhiteSpace(GABookingInfo.GAQSXB) ||
                           GABookingInfo.RelationType == null ||
                           string.IsNullOrWhiteSpace(GABookingInfo.GAQSSFZHM) ||
                           string.IsNullOrWhiteSpace(GABookingInfo.GAJMLWNDTXHM))
                        {
                            OwnerViewModel?.MessageTips("请填写探亲详细信息");
                            return;
                        }
                    }
                    else
                    {
                        GABookingInfo.GAQSZWXM = null;
                        GABookingInfo.GAQSXB = null;
                        GABookingInfo.RelationType = null;
                        GABookingInfo.GAQSSFZHM = null;
                        GABookingInfo.GAJMLWNDTXHM = null;
                    }
                    if (itemHK.IsUse)
                    {
                        msg += $@" [香港] 香港签注:{itemHK.QZType.Description} 签注次数:{itemHK.QZCount.Description}";
                    }
                    if (itemMAC.IsUse)
                    {
                        msg += $@" [澳门] 澳门签注:{itemMAC.QZType.Description} 签注次数:{itemMAC.QZCount.Description}";
                    }
                }
                //关闭倒计时
                OnDispose();
            }
            catch (Exception ex)
            {
                OwnerViewModel?.MessageTips("信息填写不完整");
                Log.Instance.WriteError("信息填写错误：" + ex);
                return;
            }
            Log.Instance.WriteInfo(msg);
            Log.Instance.WriteInfo("=====离开证件类型页面=====");
            Log.Instance.WriteInfo("点击下一步按钮：进入扫描回执界面");
            base.DoNextFunction("ScanningPhotoReceipt");
        }

        private bool DataVerification(out string msg)
        {
            msg = "";
            //2021-5-11 14:09:07 增加 证件有效期 < 3个月30天 不显示仅签注
            int daysInMonth = GetDaysInMonth(DateTime.Now, 3) + 30;
            if (HZBookingInfo != null)
            {
                if (HZBookingInfo.ApplyType == null)
                {
                    TTS.PlaySound("预约机-提示-请完善办证类别信息");
                    msg = string.Format(TipMsgResource.NonEmptyTipMsg, "普通护照中申请类型");
                    return false;
                }
                //加注信息判断
                if (HZBookingInfo.ApplyType != null && HZBookingInfo.ApplyType.Code == "93")
                {
                    var lst = HZBookingInfo.YYJZBLLIST?.Where(t => string.IsNullOrWhiteSpace(t.BGJZXM) == true).ToList();
                    if (lst.Count() >= 3)
                    {
                        TTS.PlaySound("预约机-提示-请完善加注信息");
                        msg = TipMsgResource.HZAddApplyTipMsg;
                        return false;
                    }
                }

                if (hZBookingInfo.CJSY == null)
                {
                    TTS.PlaySound("预约机-提示-请完善出境事由");
                    msg = string.Format(TipMsgResource.NonEmptyTipMsg, "普通护照出境事由");
                    return false;
                }

                if (hZBookingInfo.QWD == null)
                {
                    TTS.PlaySound("预约机-提示-请完善前往地信息");
                    msg = string.Format(TipMsgResource.NonEmptyTipMsg, "普通护照前往地");
                    return false;

                }
            }
            if (GABookingInfo != null)
            {
                
                var itemHK = GABookingInfo.YYQZLIST[0];
                var itemMAC = GABookingInfo.YYQZLIST[1];

                if (GABookingInfo.ApplyType == null)
                {
                    TTS.PlaySound("预约机-提示-请完善办证类别信息");
                    msg = string.Format(TipMsgResource.NonEmptyTipMsg, "往来港澳通行证中办证类别");
                    return false;
                }
                if (itemHK.IsUse && (itemHK.QZType == null || itemHK.QZCount == null))
                {
                    TTS.PlaySound("预约机-提示-请完善香港签注信息");
                    msg = string.Format(TipMsgResource.NonEmptyTipMsg, "往来港澳通行证中香港签注");
                    return false;
                }
                if (itemMAC.IsUse && (itemMAC.QZType == null || itemMAC.QZCount == null))
                {
                    TTS.PlaySound("预约机-提示-请完善澳门签注信息");
                    msg = string.Format(TipMsgResource.NonEmptyTipMsg, "往来港澳通行证中澳门签注");
                    return false;
                }
                if (OwnerViewModel.IsXuwen == true)
                {
                    if ((itemHK.QZType.Code == "11") || (itemMAC.QZType.Code == "11"))//探亲
                    {
                        msg = "您不符合自助受理条件，请移步到人工窗口办理！";
                        return false;
                    }
                    if ((itemHK.QZType.Code == "13") || (itemMAC.QZType.Code == "13"))//商务
                    {
                        msg = "您不符合自助受理条件，请移步到人工窗口办理！";
                        return false;
                    }
                }
                if ((itemHK.QZType.Code == "11" && itemHK.IsUse) || (itemMAC.QZType.Code == "11" && itemMAC.IsUse))
                {
                    if (string.IsNullOrWhiteSpace(GABookingInfo.GAQSZWXM))
                    {
                        TTS.PlaySound("预约机-提示-请完善港澳亲属姓名");
                        msg = string.Format(TipMsgResource.NonEmptyTipMsg, "往来港澳通行证中港澳亲属姓名");
                        return false;
                    }
                    if (string.IsNullOrWhiteSpace(GABookingInfo.GAQSXB))
                    {
                        TTS.PlaySound("预约机-提示-请完善港澳亲属性别");
                        msg = string.Format(TipMsgResource.NonEmptyTipMsg, "往来港澳通行证中港澳亲属性别");
                        return false;
                    }
                    if (string.IsNullOrWhiteSpace(GABookingInfo.GAQSSFZHM))
                    {
                        TTS.PlaySound("预约机-提示-请完善港澳身份证号码");
                        msg = string.Format(TipMsgResource.NonEmptyTipMsg, "往来港澳通行证中港澳身份证号码");
                        return false;
                    }
                    if (GABookingInfo.RelationType == null)
                    {
                        TTS.PlaySound("预约机-提示-请完善与申请人关系信息");
                        msg = string.Format(TipMsgResource.NonEmptyTipMsg, "往来港澳通行证中与申请人关系");
                        return false;
                    }
                    if (string.IsNullOrWhiteSpace(GABookingInfo.GAJMLWNDTXHM))
                    {
                        TTS.PlaySound("预约机-提示-请完善旅行证件号码");
                        msg = string.Format(TipMsgResource.NonEmptyTipMsg, "往来港澳通行证中旅行证件号码");
                        return false;
                    }
                }

                //获取有效天数
                //int days = GetValidDay(DateTime.ParseExact(TWBookingInfo?.XCZJYXQZ, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture));
                //if (days <= daysInMonth)
                //{
                //    msg = "您的证件有效期不符合办理签注，请选择换、补发业务！";
                //    return false;
                //}
            }
            if (TWBookingInfo != null)
            {
                
                var item = TWBookingInfo.YYQZLIST[0];
                if (TWBookingInfo.ApplyType == null)
                {
                    TTS.PlaySound("预约机-提示-请完善办证类别信息");
                    msg = string.Format(TipMsgResource.NonEmptyTipMsg, "往来台湾通行证中办证类别");
                    return false;
                }
                if (OwnerViewModel.IsXuwen)
                {
                    if (item.QZType.Code == "21")//探亲
                    {
                        msg = "您不符合自助受理条件，请移步到人工窗口办理！";
                        return false;
                    }
                    if (item.QZType.Code == "27")//商务
                    {
                        msg = "您不符合自助受理条件，请移步到人工窗口办理！";
                        return false;
                    }
                }
                if (item.IsUse && item.QZType == null)
                {
                    TTS.PlaySound("预约机-提示-请完善台湾签注信息");
                    msg = string.Format(TipMsgResource.NonEmptyTipMsg, "往来台湾通行证中申请事由");
                    return false;
                }
                if (item.IsUse && item.QZCount == null)
                {
                    TTS.PlaySound("预约机-提示-请完善台湾签注信息");
                    msg = string.Format(TipMsgResource.NonEmptyTipMsg, "往来台湾通行证中签注种类");
                    return false;
                }

                //获取有效天数
                //int days = GetValidDay(DateTime.ParseExact(TWBookingInfo?.XCZJYXQZ, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture));
                //if (days <= daysInMonth)
                //{
                //    msg = "您的证件有效期不符合办理签注，请选择换、补发业务！";
                //    return false;
                //}

            }

            //选中总数
            var selCount = BookingBaseInfo.SelectCardTypes.Count;
            //跳过回执页标识 
            //仅签注：仅持证申请签注 
            int DirectCount = 0;
            //仅签注默认照片
            foreach (DictionaryType item in BookingBaseInfo.SelectCardTypes)
            {
                //仅签注业务
                var directVerification = BookingBaseInfo.BookingInfo.Where(t =>
                    t.SQLB == item?.Code && t.SQLB != ((int)EnumSqlType.HZ).ToString() && t.ApplyType?.Code == "92" || t.ApplyType?.Code == "93").ToList();

                //仅签注业务总数
                if (directVerification.Count > 0)
                    DirectCount++;

            }

            //全部为仅签注业务
            if (selCount == DirectCount)
            {
                OwnerViewModel.IsDirectNumber = true;
                Log.Instance.WriteInfo("系统查询办证类型与仅签注业务对应，跳过扫描回执页。");
            }

            return true;
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

        /// <summary>
        /// 返回自然月天数(比如当前月三个月)
        /// </summary>
        /// <param name="dt">时间</param>
        /// <param name="count">次数</param>
        /// <returns></returns>
        private int GetDaysInMonth(DateTime dt, int count = 1)
        {
            int days = DateTime.DaysInMonth(dt.Year, dt.Month);
            for (int i = 1; i < count; i++)
            {
                dt = dt.AddMonths(1);
                days += DateTime.DaysInMonth(dt.Year, dt.Month);
            }
            return days;
        }

        /// <summary>
        /// 数据赋值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="info"></param>
        /// <param name="msg"></param>
        /// <param name="isApply"></param>
        private void DataVerification(EnumTypeSQLB type, BookingInfo info, out string msg, bool isApply = true)
        {
            //获取证件有效期
            string zjyxq = info.XCZJYXQZ;
            msg = string.Empty;
            //现在至到期时间间隔
            int days = DaysInterval(zjyxq);
            //取当前签注类型信息
            var applyType = CheckApplyType(type, info, out msg);

            if (applyType != null && info.ApplyType == null)
            {
                info.ApplyType = applyType;
            }
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
        /// 给当前预约签注信息签注
        /// </summary>
        /// <param name="type">申请类别</param>
        /// <param name="info">预约信息</param>
        /// <param name="msg">返回消息</param>
        /// <returns>申请类别</returns>
        private DictionaryType CheckApplyType(EnumTypeSQLB type, BookingInfo info, out string msg)
        {
            DictionaryType applyType = null;
            msg = "";
            //证件有效期
            string zjyxq = info.XCZJYXQZ;
            //获取申请类型
            var lst = applyTypes;
            //获取间隔时间
            int days = DaysInterval(zjyxq);
            //获取自然月三个月零30天天数
            int daysInMonth = GetDaysInMonth(DateTime.Now, 3) + 30;


            if (string.IsNullOrWhiteSpace(zjyxq))
            {
                //首次申请
                if (info.ApplyType?.Code != "11")
                {
                    applyType = lst.FirstOrDefault(t => t.Code == "11");
                    // TTS.PlaySound("预约机-提示-办证类型填写有误，请重新填写");
                    msg = TipMsgResource.FirstApplyTipMsg;
                }
                info.IsQZShow = false;
            }
            else if (days < 0 && info.ApplyType?.Code != "13")
            {
                //失效重领 
                applyType = lst.FirstOrDefault(t => t.Code == "13");
                //TTS.PlaySound("预约机-提示-办证类型填写有误，请重新填写");
                msg = TipMsgResource.DocumentExpirationTipMsg;
                info.IsQZShow = false;
            }
            else if (days >= 0 && (info.ApplyType?.Code == "13" || info.ApplyType?.Code == "11"))
            {
                //TTS.PlaySound("预约机-提示-办证类型填写有误，请重新填写");
                msg = TipMsgResource.DocumentValidTipMsg;
            }
            else if (type == EnumTypeSQLB.HZ && days >= 0 && days <= 180)
            {
                //换发+补发+加注 默认换发
                applyType = lst.FirstOrDefault(t => t.Code == "31");
            }
            else if (type == EnumTypeSQLB.HZ && days > 180)
            {
                //补发+换发+加注 默认补发
                //修改 遗失补发无权限修改为到期换发
                applyType = applyTypes.Exists(x => x.Code == "21") ? lst.FirstOrDefault(t => t.Code == "31") : lst.FirstOrDefault(t => t.Code == "31");

            }
            else if (type != EnumTypeSQLB.HZ && days >= 0 && days <= daysInMonth && info.ApplyType?.Code != "31" && info.ApplyType?.Code != "21")
            {
                //换发+补发 默认换发
                applyType = lst.FirstOrDefault(t => t.Code == "31");
                //TTS.PlaySound("预约机-提示-办证类型填写有误，请重新填写");
                msg = TipMsgResource.DocumentMonthTipMsg;
                info.IsQZShow = false;
            }
            else if (type != EnumTypeSQLB.HZ && days >= 180 && days <= 365)
            {
                //补发+持证签注+换发 默认补发
                applyType = lst.FirstOrDefault(t => t.Code == "21");
            }
            else if (type != EnumTypeSQLB.HZ)
            {
                //默认持证签注
                applyType = lst.FirstOrDefault(t => t.Code == "92");
            }

            //选择仅签注只能选择持证签注
            if (info.SQOp == 1 && info.ApplyType?.Code != "92")
            {
                applyType = lst.FirstOrDefault(t => t.Code == "92");
                //TTS.PlaySound("预约机-提示-办证类型填写有误，请重新填写");
                msg = TipMsgResource.QZTipMsg;
            }
            //选择仅通行证不能选择持证签注
            if (info.SQOp == 2 && info.ApplyType?.Code == "92")
            {
                //TTS.PlaySound("预约机-提示-办证类型填写有误，请重新填写");
                msg = TipMsgResource.TXZTipMsg;
            }
            //太极核查可办业务 默认赋值可办理第一个
            //if (TjConfig.TjModel.IsConnectionTj)
            //{
            //    if (OwnerViewModel?.KbywInfos != null && OwnerViewModel?.KbywInfos.Length > 0)
            //    {
            //        Log.Instance.WriteInfo("核查到办证类别为：" + type.ToString());
            //        var Hz = OwnerViewModel.KbywInfos.Where(t => t.sqlb == ((int)type).ToString()).ToList();
            //        Log.Instance.WriteInfo("核查可办业务：" + Hz.Count + "条");
            //        if (Hz.Count > 0)
            //        {
            //            var Bzlb = Hz.FirstOrDefault().bzlb;
            //            Log.Instance.WriteInfo("护照可办业务为：" + Bzlb + "");
            //            applyType = lst.FirstOrDefault(t => t.Code == Bzlb);
            //        }

            //    }
            //}
            //补发+换发 陕西地区派号到人工区域 后期修改
            //if (OwnerViewModel.IsShanXi && applyType.Code == "21" || applyType.Code == "31")
            //{
            //    OwnerViewModel.IsManual = true;
            //}
            return applyType;
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
            var lstAll = hzApplyTypes.Union(gaApplyTypes).Union(twApplyTypes)?.GroupBy(t => new { t.Code, t.Description }).Select(t => new DictionaryType()
            {
                Description = t.Key.Description,
                Code = t.Key.Code
            }).ToList();
            return lstAll;
        }

        /// <summary>
        /// 取出境事由列表
        /// </summary>
        /// <returns>列表集合</returns>
        private List<DictionaryType> GetDepartureReasonTypes()
        {
            var config = ServiceRegistry.Instance.Get<ElementManager>();
            List<DictionaryType> lst = new List<DictionaryType>();

            lst = config.Get<List<DictionaryType>>();
            lst = lst?.Where(t => t.KindType == ((int)KindType.DepartureReason).ToString() && t.Status == 1)?.ToList();

            return lst;
        }

        /// <summary>
        /// 取前往地列表
        /// </summary>
        /// <returns>列表集合</returns>
        private List<DictionaryType> GetDestinationTypes()
        {
            var config = ServiceRegistry.Instance.Get<ElementManager>();
            List<DictionaryType> lst = new List<DictionaryType>();

            lst = config.Get<List<DictionaryType>>();
            lst = lst?.Where(t => t.KindType == ((int)KindType.Destination).ToString() && t.Status == 1)?.ToList();

            return lst;
        }

        /// <summary>
        /// 获取证件有效天数
        /// </summary>
        /// <param name="dt">证件有效期</param>
        /// <returns>天数（不小于0）</returns>
        private int GetValidDay(DateTime dt)
        {
            DateTime oldDate = dt;
            DateTime newDate = DateTime.Now;
            TimeSpan ts = oldDate - newDate;
            int differenceInDays = ts.Days;
            if (differenceInDays <= 0)
                return 0;
            return differenceInDays;
        }
        #endregion
    }
}
