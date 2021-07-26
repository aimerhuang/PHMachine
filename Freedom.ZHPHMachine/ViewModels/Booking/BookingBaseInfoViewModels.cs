using Freedom.BLL;
using Freedom.Common;
using Freedom.Common.HsZhPjh.Enums;
using Freedom.Config;
using Freedom.Controls.Foundation;
using Freedom.Models.CrjCreateJsonModels;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.Common;
using MachineCommandService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Freedom.Models;
using Freedom.ZHPHMachine.Command;
using System.ComponentModel;
using System.IO;
using Freedom.Models.TJJsonModels;

namespace Freedom.ZHPHMachine.ViewModels
{
    public class BookingBaseInfoViewModels : ViewModelBase
    {
        public BookingBaseInfoViewModels()
        {
            if (crjManager == null) { crjManager = new CrjPreapplyManager(); }
            var result = ServiceRegistry.Instance.Get<ElementManager>()?.Get<List<DictionaryType>>();
            CardTypes = result?.Where(t => t.KindType == ((int)KindType.ApplyType).ToString()).OrderBy(t => t.Code).ToList();
            Log.Instance.WriteInfo("\n======进入填写基础信息界面======");
            TTS.PlaySound("预约机-页面-请完善个人申请信息");
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

        #region 字段
        private CrjPreapplyManager crjManager;
        #endregion
        #region 属性 

        private List<DictionaryType> cardTypes;
        public Dictionary<int, string> Current = new Dictionary<int, string>();//业务类型，当前环节
        /// <summary>
        /// 办证类型
        /// </summary>
        public List<DictionaryType> CardTypes
        {
            get { return cardTypes; }
            set { cardTypes = value; RaisePropertyChanged("CardTypes"); }
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


        public ICommand TextBoxGotFocusCommand
        {
            get
            {
                return new RelayCommand<string>((param) =>
                {

                    if (string.IsNullOrWhiteSpace(param)) return;
                    string content = string.Empty;
                    switch (param)
                    {
                        case "本人联系电话":
                            content = BookingBaseInfo.Telephone;
                            break;
                        case "紧急联系人电话":
                            content = BookingBaseInfo.UrgentTelephone;
                            break;
                        case "收件人电话":
                            content = BookingBaseInfo.RecipientTelephone;
                            break;
                        case "邮政编码":
                            content = BookingBaseInfo.EMSCode;
                            break;

                    }
                    //string content = param == "本人联系电话" ? BookingBaseInfo.Telephone : BookingBaseInfo.UrgentTelephone;
                    if (CommonHelper.PopupNumKeyboard(param, content, out string str))
                    {
                        switch (param)
                        {
                            case "本人联系电话":
                                BookingBaseInfo.Telephone = str;
                                break;
                            case "紧急联系人电话":
                                BookingBaseInfo.UrgentTelephone = str;
                                break;
                            case "收件人电话":
                                BookingBaseInfo.RecipientTelephone = str;
                                BookingBaseInfo.HasRecipientTelephone = str;
                                break;
                            case "邮政编码":
                                BookingBaseInfo.EMSCode = str;
                                BookingBaseInfo.HasEMSCode = str;
                                break;

                        }
                        //if (param == "本人联系电话")
                        //    BookingBaseInfo.Telephone = str;
                        //else
                        //    BookingBaseInfo.UrgentTelephone = str;
                    }
                    //Log.Instance.WriteInfo("填写本人联系电话");
                });
            }
        }

        public ICommand ProvinceSelectCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (CommonHelper.PopupProvince("户口所在地", BookingBaseInfo?.Address?.Code, out DictionaryType model, out string HasAddress))
                    {
                        if (DjzConfig.DjzModel.IsConnectionDjz)
                        {
                            if (BookingBaseInfo?.Address?.Code != null)
                            {
                                if (BookingBaseInfo?.Address?.Code != model.Code)
                                {
                                    OwnerViewModel?.MessageTips("您所选户口所在地：[" + model.Description + "] 与人口库[" + BookingBaseInfo.Address.Description + "]不符，请确认填写无误！");
                                    Log.Instance.WriteInfo("所选户口所在地：" + model.Code + ",与人口库：" + BookingBaseInfo?.Address?.Code + "户口所在地不符，请确认填写无误！");

                                    //OwnerViewModel?.MessageTips(TipMsgResource.AddressTipMsg);
                                    //return;
                                }
                            }
                        }

                        BookingBaseInfo.HasAddress = HasAddress;
                        BookingBaseInfo.Address = model;
                    }

                });
            }
        }

        public ICommand HandInputCommand
        {
            get
            {
                return new RelayCommand<string>((param) =>
                {
                    if (param == "紧急联系人姓名")
                    {
                        if (CommonHelper.PopupHandwritingInput("紧急联系人姓名", BookingBaseInfo.UrgentName, out string str))
                        {
                            BookingBaseInfo.UrgentName = str;
                        }
                    }
                    if (param == "邮寄地址")
                    {
                        if (CommonHelper.PopupHandwritingInput("邮寄地址", BookingBaseInfo.EMSAddress, out string str, 30))
                        {
                            BookingBaseInfo.HasEMSAddress = str;
                            BookingBaseInfo.EMSAddress = str;
                            // BookingBaseInfo.EMSCode = BookingBaseInfo.CardInfo.hkszd;
                        }
                    }
                    if (param == "收件人姓名")
                    {
                        if (CommonHelper.PopupHandwritingInput("收件人姓名", BookingBaseInfo.EMSAddress, out string str, 30))
                        {
                            BookingBaseInfo.HasRecipientName = str;
                            BookingBaseInfo.RecipientName = str;
                            // BookingBaseInfo.EMSCode = BookingBaseInfo.CardInfo.hkszd;
                        }
                    }

                    //
                });
                //return new RelayCommand(() =>
                //{
                //    if (CommonHelper.PopupHandwritingInput("紧急联系人姓名", BookingBaseInfo.UrgentName, out string str))
                //    {
                //        BookingBaseInfo.UrgentName = str;
                //    }
                //    if (CommonHelper.PopupHandwritingInput("邮寄地址", BookingBaseInfo.UrgentName, out str))
                //    {
                //        BookingBaseInfo.EMSAddress = str;
                //    }
                //});
            }
        }

        /// <summary>
        /// 选择职业
        /// </summary>
        public ICommand JobCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (CommonHelper.PopupDictionaryByPage(OperaType.Job, BookingBaseInfo?.Job?.Description, out DictionaryType model))
                    {
                        BookingBaseInfo.Job = model;
                        if (BookingBaseInfo.Job != null)
                            BookingBaseInfo.HasJob = model.Description;
                    }
                });
            }
        }


        #endregion

        #region 方法  

        private void QueryAsync()
        {

            // Task.Run(() =>
            //{
            try
            {
                OwnerViewModel?.IsShowHiddenLoadingWait("正在查询请稍等......");
                if (!DjzConfig.DjzModel.IsConnectionDjz) { return; }
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
                        BookingBaseInfo.UrgentName = item.jjqklxr;//紧急联系人
                        BookingBaseInfo.UrgentTelephone = CommandTools.FilterStrByNumber(item.jjqklxrdh);//紧急联系人电话
                        BookingBaseInfo.Telephone = CommandTools.FilterStrByNumber(item.lxdh);//本人联系电话 

                        Log.Instance.WriteInfo("[申请信息查询]紧急联系人：" + CommandTools.ReplaceWithSpecialChar(item.jjqklxr, 3, 4) + "本人电话：" + CommandTools.ReplaceWithSpecialChar(item.lxdh, 3, 4));
                    }
                    //出境申请信息
                    var cjsqxx = (result.ReturnValue as SqxxModel)?.cjsqxxs;
                    //cjsqxx[0].sqlb = "101";
                    //cjsqxx[0].currenthj = "000";

                    if (cjsqxx != null && cjsqxx.Length > 0)
                    {
                        //查询历史申请信息中，环节状态为归档096，和环节结束999
                        foreach (var sqxx in cjsqxx)
                        {
                            var sqlb = sqxx.sqlb;
                            var current = sqxx.currenthj;
                            //Log.Instance.WriteInfo("查询历史申请信息返回：申请类别" + sqlb + "，当前环节状态：" + current);
                            if (current != null && current != "096" && current != "999")
                            {
                                Log.Instance.WriteInfo("检查到环节状态为：" + current + "，办证类别：" + sqlb);
                                //if (!Current.ContainsKey(sqlb.ToInt()))
                                //    Current.Add(sqlb.ToInt(), current);
                            }
                        }
                        //Log.Instance.WriteInfo("在办业务总数："+Current.Count);
                    }
                }
                else
                {
                    Log.Instance.WriteInfo($"[申请信息查询]{result.MessageInfo}");
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo($"[申请信息查询发生异常]");
                Log.Instance.WriteError($"[申请信息查询发生异常]{ex.Message}");
            }
            finally
            {
                Log.Instance.WriteInfo("-----结束查询历史申请信息-----");
                //根据配置判断是否跳过基本信息页面
                IsSkipBasicInfo();

                OwnerViewModel?.IsShowHiddenLoadingWait();
            }
            //});

        }

        protected override void OnDispose()
        {
            CommonHelper.OnDispose();
            base.OnDispose();
        }
        public override void DoNextFunction(object obj)
        {
            if (!DataVerification(BookingBaseInfo, out string msg) && OwnerViewModel.IsTakePH_No == false && OwnerViewModel?.IsShenZhen == false)// && OwnerViewModel?.IsBeijing == false
            {
                OwnerViewModel?.MessageTips(msg);
                return;
            }
            if (!ZHPHMachineWSHelper.ZHPHInstance.CreateBookingInfo(BookingBaseInfo, out string msg1))
            {
                OwnerViewModel?.MessageTips(msg1);
                return;
            }
            //测试模式外省户口判断
            if (!DjzConfig.DjzModel.IsConnectionDjz && OwnerViewModel?.IsLocalRegister == false)
            {
                OwnerViewModel.isVALIDXCYY = "1";
                Log.Instance.WriteInfo("非本省户籍，不符合自助办理条件，派人工号标识为True");
            }

            //关闭倒计时  
            OnDispose();

            BookingBaseInfo.StrCardTypes = msg1.Substring(0, msg1.Length - 1);

            string str = BookingBaseInfo.IsExpress == 0 ? "公安机关领取" : "邮政";
            //Log.Instance.WriteInfo($"办证类型:{msg} 取证方式:{str}");
            Log.Instance.WriteInfo($"办证类型:{BookingBaseInfo.StrCardTypes} 取证方式:{str}");
            Log.Instance.WriteInfo("=====离开填写基础信息页面=====");
            Log.Instance.WriteInfo("下一步：进入选择证件类型界面");
            base.DoNextFunction("Booking/BookingInfo");
        }

        private bool DataVerification(BookingModel model, out string msg)
        {
            msg = "";
            if (string.IsNullOrWhiteSpace(model?.Telephone))
            {
                TTS.PlaySound("预约机-提示-请完善本人联系电话");
                msg = string.Format(TipMsgResource.NonEmptyTipMsg, "本人联系电话");

                return false;
            }
            if (model?.Address == null || string.IsNullOrWhiteSpace(model?.Address?.Code))
            {
                TTS.PlaySound("预约机-提示-请完善户口所在地信息");
                msg = string.Format(TipMsgResource.NonEmptyTipMsg, "户口所在地");
                return false;
            }

            if (string.IsNullOrWhiteSpace(model?.UrgentName))
            {
                TTS.PlaySound("预约机-提示-请完善紧急联系人姓名");
                msg = string.Format(TipMsgResource.NonEmptyTipMsg, "紧急联系人姓名");
                return false;
            }
            if (string.IsNullOrWhiteSpace(model?.UrgentTelephone))
            {
                TTS.PlaySound("预约机-提示-请完善紧急联系人电话");
                msg = string.Format(TipMsgResource.NonEmptyTipMsg, "紧急联系人电话");
                return false;
            }
            if (model?.SelectCardTypes == null || model?.SelectCardTypes?.Count <= 0)
            {
                TTS.PlaySound("预约机-提示-请选择办证类型");
                msg = TipMsgResource.NonEmptySelectsTipMsg;
                return false;
            }
            //电话格式验证
            if (!string.IsNullOrEmpty(model?.Telephone))
            {

                if (!Freedom.Common.Validate.IsValidMobile(model?.Telephone))
                {
                    TTS.PlaySound("预约机-提示-请完善本人联系电话");
                    BookingBaseInfo.Telephone = "";
                    msg = "本人联系电话格式填写不正确，请重新填写";
                    return false;
                }
            }
            //职业格式 设置为空
            if (string.IsNullOrEmpty(model?.Job?.Code))
            {
                TTS.PlaySound("预约机-提示-请完善职业信息");
                if (model.Job != null) model.Job.Code = "";
                msg = "职业不能为空，请重新填写";
                return false;

            }
            //紧急联系人姓名格式验证 设置为空
            if (!string.IsNullOrEmpty(model?.UrgentName))
            {
                if (!ValidationHelper.IsName(model?.UrgentName))
                {
                    TTS.PlaySound("预约机-提示-请完善紧急联系人姓名");
                    model.UrgentName = "";
                    msg = "紧急联系人姓名格式填写不正确，请重新填写";
                    return false;
                }
            }
            //紧急联系人电话不对 设置为空
            if (!string.IsNullOrEmpty(model?.UrgentTelephone))
            {
                //if (!ValidationHelper.IsPhoneNumber(model?.UrgentTelephone))
                if (!Freedom.Common.Validate.IsValidMobile(model?.UrgentTelephone))
                {
                    TTS.PlaySound("预约机-提示-请完善紧急联系人电话");
                    model.UrgentTelephone = "";
                    msg = "紧急联系人电话格式填写不正确，请重新填写";
                    return false;
                }
            }

            if (BookingBaseInfo.IsExpress == 1)
            {
                if (BookingBaseInfo.EMSCode.IsEmpty())
                {
                    model.EMSCode = "";
                    msg = "邮政编码格式填写不正确，请重新填写";
                    return false;
                }

                if (BookingBaseInfo.RecipientName.IsEmpty())
                {
                    model.EMSCode = "";
                    msg = "收件人格式填写不正确，请重新填写";
                    return false;
                }

                if (BookingBaseInfo.RecipientTelephone.IsEmpty())
                {
                    model.EMSCode = "";
                    msg = "收件人电话格式填写不正确，请重新填写";
                    return false;
                }

                if (BookingBaseInfo.EMSAddress.IsEmpty())
                {
                    model.EMSCode = "";
                    msg = "邮寄地址格式填写不正确，请重新填写";
                    return false;
                }
            }
            Log.Instance.WriteInfo("紧急联系人：" +
                                   CommandTools.ReplaceWithSpecialChar(BookingBaseInfo?.UrgentName, 1, 1) + "，本人电话：" +
                                   CommandTools.ReplaceWithSpecialChar(BookingBaseInfo?.Telephone, 3, 3) + ",紧急联系人电话：" +
                                   CommandTools.ReplaceWithSpecialChar(BookingBaseInfo?.UrgentTelephone, 3, 3) + "，职业：" +
                                   BookingBaseInfo?.HasJob);
            return true;
        }

        /// <summary>
        /// 根据区域和取号模式显示跳转页面
        /// </summary>
        public void IsSkipBasicInfo()
        {
            try
            {
                Log.Instance.WriteInfo(OwnerViewModel?.IsTakePH_No == true ? "取号模式：直接取号" : "取号模式：现场预约+取号");

                //人员类别显示
                if (!string.IsNullOrEmpty(OwnerViewModel?.IsOfficial))
                {
                    TipsMsg = OwnerViewModel?.IsOfficial;
                }

                //显示照片
                if (!string.IsNullOrWhiteSpace(OwnerViewModel?.djzPhoto))
                {
                    ImageStr = OwnerViewModel?.djzPhoto;
                }

                //ImageStr = Path.Combine(FileHelper.GetLocalPath() + "\\ZZTakePhoto" + "\\110111199512160099.jpg");

                //过滤无预约
                if (DjzConfig.DjzModel.IsConnectionDjz)
                {
                    // List<DictionaryType> types = new List<DictionaryType>();
                    //删除未完结业务
                    if (Current.Count > 0)
                    {
                        Log.Instance.WriteInfo("核查未完结业务总数为：" + Current.Count);
                        foreach (var item in Current)
                        {
                            var over = CardTypes.Where(t => t.Code == item.Key.ToString()).ToList().FirstOrDefault();
                            if (over != null)
                            {
                                Log.Instance.WriteInfo("未完结业务为：" + over.Code);
                                //types.Add(over);
                                CardTypes.Remove(over);

                            }
                        }

                        Log.Instance.WriteInfo("可办业务总数为：" + CardTypes.Count);
                        if (CardTypes.Count == 0)
                        {
                            Log.Instance.WriteInfo("===是这里===");
                            //OwnerViewModel?.MessageTips("系统查询到您已办理业务【护照、港澳、赴台】，不能重复办理，请您再次核对，若有疑问，请移步人工窗口咨询！");
                            //改为弹出直接退出回首页 by 2021年7月7日15:35:24 wei.chen
                            OwnerViewModel?.MessageTips("系统查询到您已办理业务【护照、港澳、赴台】，不能重复办理，请您再次核对，若有疑问，请移步人工窗口咨询！", (() =>
                            {
                                //提示框内的倒计时卡死 by 2021年7月10日18:37:30
                                this.DoNextFunction(null);
                            }));
                            return;
                        }
                    }

                    //过滤已办结
                    if (OwnerViewModel?.zbywList?.Count > 0)
                    {
                        Log.Instance.WriteInfo("核查已办结业务总数为：" + OwnerViewModel?.zbywList?.Count);

                        foreach (var item in OwnerViewModel?.zbywList)
                        {
                            var over = CardTypes.Where(t => t.Code == item).ToList().FirstOrDefault();
                            if (over != null)
                            {
                                Log.Instance.WriteInfo("已办结业务为：" + over.Code);
                                //types.Add(over);
                                CardTypes.Remove(over);
                            }
                        }
                    }
                }


                //从配置项获取：取号模式为直接取号
                if (!QJTConfig.QJTModel.IsBasicInfo && OwnerViewModel?.IsTakePH_No == true || OwnerViewModel?.IsShenZhen == true)
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
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("填写基础信息发生错误：" + ex);
                Log.Instance.WriteError("填写基础信息发生错误：" + ex);
                //Console.WriteLine(ex);
                //throw;
            }
        }

        #endregion
    }

}
