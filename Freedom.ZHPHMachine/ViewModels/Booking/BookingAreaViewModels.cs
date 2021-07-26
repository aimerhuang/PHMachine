using Freedom.Common;
using Freedom.Config;
using Freedom.Models;
using MachineCommandService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Freedom.Models.HsZhPjhJsonModels;
using Freedom.Models.TJJsonModels;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.Command;

namespace Freedom.ZHPHMachine.ViewModels
{
    public class BookingAreaViewModels : ViewModelBase
    {
        #region 属性
        private List<PH_SERVICETYPE_TB> areaInfo;
        /// <summary>
        /// 区域信息
        /// </summary>
        public List<PH_SERVICETYPE_TB> AreaInfo
        {
            get { return areaInfo; }
            set { areaInfo = value; RaisePropertyChanged("AreaInfo"); }
        }
        /// <summary>
        /// 是否可进智慧大厅标识
        /// </summary>
        public bool IsGoZHDT = true;
        #endregion

        #region 方法
        public override void DoNextFunction(object obj)
        {
            if (BookingBaseInfo.SelectCardTypes.IsEmpty())
            {
                Log.Instance.WriteInfo("数据异常，切换到首页……");

                OwnerViewModel?.MessageTips("数据异常，返回首页！");
                base.DoNextFunction("MainPage");
                return;
            }

            if (TjConfig.TjModel.IsConnectionTj)
            {
                List<BookingInfo> ywlbList = new List<BookingInfo>();
                foreach (DictionaryType item in BookingBaseInfo.SelectCardTypes)
                {
                    var model = BookingBaseInfo.BookingInfo.Where(t => t.SQLB.Equals(item?.Code)).ToList().FirstOrDefault();

                    //是否使用太极规则生成业务编号
                    if (QJTConfig.QJTModel.IsTJYWID && BookingBaseInfo.AreaCode.IS_NEED_SELF != "1")//QJTConfig.QJTModel.IsTJYWID &&
                    {
                        //选中业务从太极获取业务编号
                        var barcode = new TaiJiHelper().Do_Common_MakeBarcode(QJTConfig.QJTModel?.QJTDevInfo?.TBDWBH?.Substring(0, 6), OwnerViewModel?.Xxly);
                        Log.Instance.WriteInfo("获取太极业务编号返回：" + barcode?.ywbh);
                        if (!string.IsNullOrEmpty(barcode?.ywbh))
                        {
                            model.YWBH = barcode.ywbh;
                            if (model.YYJZBLLIST != null && model.YYJZBLLIST.Count > 0)
                            {
                                foreach (var s in model.YYJZBLLIST)
                                {
                                    s.YWBH = barcode?.ywbh;
                                }
                                Log.Instance.WriteInfo("申请类别：" + model?.SQLB + "，加注信息：" + JsonHelper.ToJson(model.YYJZBLLIST));

                            }
                            if (model.YYQZLIST != null && model.YYQZLIST.Count > 0)
                            {
                                foreach (var i in model.YYQZLIST)
                                {
                                    i.YWBH = barcode?.ywbh;
                                }
                                Log.Instance.WriteInfo("申请类别：" + model?.SQLB + "，签注信息：" + JsonHelper.ToJson(model.YYQZLIST));
                            }

                        }

                    }
                    if (model != null)
                    {
                        ywlbList.Add(model);
                    }
                }
                BookingBaseInfo.BookingInfo = ywlbList;

            }

            //!OwnerViewModel?.IsShanXi == true && OwnerViewModel?.IsXuwen == true && 
            if (string.IsNullOrWhiteSpace(BookingBaseInfo?.AreaCode?.SERVICE_CODE) && !QJTConfig.QJTModel.IsTodayPH)
            {
                if (OwnerViewModel?.IsBeijing == true)
                    TTS.PlaySound("预约机-页面-请选择办理区域");
                OwnerViewModel?.MessageTips("请选择您的办理区域");
                return;
            }
            //关闭倒计时
            OnDispose();
            Log.Instance.WriteInfo($"选择办理区域：{BookingBaseInfo?.AreaCode?.SERVICE_PRINTDESC}");
            Log.Instance.WriteInfo("\n**********************************离开【选择派号区域】界面**********************************");
            //Log.Instance.WriteInfo("点击下一步按钮：进入派号界面");
            base.DoNextFunction("CheckPage");

        }
        public override void DoInitFunction(object obj)
        {
            Log.Instance.WriteInfo("\n**********************************进入【选择派号区域】界面**********************************");

            if (OwnerViewModel?.IsShenZhen == true && BookingBaseInfo?.AreaCode != null == true && BookingBaseInfo?.AreaCode?.SERVICE_CODE.IsNotEmpty() == true)
            {
                base.DoNextFunction("CheckPage");
            }
            OpenTimeOut(QJTConfig.QJTModel.TOutSeconds);
            QueryArea();
        }

        /// <summary>
        /// 查询派号区域
        /// </summary>
        private void QueryArea()
        {
            try
            {

                OwnerViewModel?.IsShowHiddenLoadingWait("正在查询请稍等......");
                //Log.Instance.WriteInfo("=====进入区域选择页面=====");
                if (OwnerViewModel?.IsBeijing == true)
                    TTS.PlaySound("预约机-页面-请选择办理区域");

                bool IsNormal = OwnerViewModel.CheckServeStatus();
                Log.Instance.WriteInfo("正在检测本地网络情况：" + IsNormal);
                if (!IsNormal)
                {
                    Log.Instance.WriteInfo("本地网络异常，切换到暂停页面……");
                    DoNext.Execute("NotificationPage");
                }

                //检测是否能连上服务器
                var sysTime = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();

                if (sysTime.IsEmpty())
                {
                    string msg = "网络连接出现异常，获取派号业务失败！请重试！";
                    OwnerViewModel?.MessageTips(msg, () => { DoExit.Execute(null); });
                    return;
                }

                Log.Instance.WriteInfo("检测当前时间返回：【" + sysTime + "】");

                if (sysTime.IsEmpty())
                {
                    string msg = "网络连接出现异常，获取派号业务失败！请重试！";
                    OwnerViewModel?.MessageTips(msg, () => { DoExit.Execute(null); });
                    Log.Instance.WriteInfo("网络连接出现异常，获取派号业务失败！请重试！：" + msg);
                    return;
                }

                Log.Instance.WriteInfo("检测当前时间返回：【" + sysTime + "】");
                Log.Instance.WriteInfo("正在检测应用服务器连接：" + sysTime);
                if (string.IsNullOrEmpty(sysTime))
                {
                    //增加10秒测试服务连接是否正常 by 2021年7月7日11:02:38 wei.chen
                    for (int i = 1; i <= 10; i++)
                    {
                        sysTime = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();
                        Log.Instance.WriteInfo("重新【" + i + "】获取应用服务器连接返回：" + sysTime);
                        if (sysTime.IsNotEmpty())
                            break;
                        System.Threading.Thread.Sleep(1000);
                    }
                    if (string.IsNullOrEmpty(sysTime))
                    {
                        string msg = "应用服务器连接出现异常，获取派号业务失败！请重试！";
                        OwnerViewModel?.MessageTips(msg, () =>
                        {
                            DoExit.Execute(null);
                        });
                        Log.Instance.WriteInfo("应用服务器连接出现异常，获取派号业务失败！请重试！：" + msg);
                        Log.Instance.WriteError("应用服务器连接出现异常，获取派号业务失败！请重试！：" + msg);
                        //切换到暂停页面
                        base.DoNextFunction("NotificationPage");
                    }
                }

                PlanPhServers();

                if (string.IsNullOrWhiteSpace(BookingBaseInfo?.AreaCode?.SERVICE_CODE) && OwnerViewModel?.IsBeijing == false && OwnerViewModel?.IsWuHan == false)//
                {
                    Log.Instance.WriteInfo("未找到派号业务，请重新配置派号业务!");
                    OwnerViewModel?.MessageTips("办理方式配置出现错误！请联系工作人员！", () => { DoExit.Execute(null); });
                    //OwnerViewModel?.MessageTips("办理方式配置出现错误！请联系工作人员！");
                    //return;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo($"[区域分配派号业务异常]");
                Log.Instance.WriteError($"[区域分配派号业务异常]" + ex.Message);
                OwnerViewModel?.MessageTips(ex.Message, (() =>
                {
                    this.DoExit.Execute(null);
                }));
            }
            finally
            {
                OwnerViewModel?.IsShowHiddenLoadingWait();
            }

        }

        /// <summary>
        /// 判断能否进自助大厅
        /// </summary>
        public void IsConnectTaiji()
        {
            List<YwlbInfo> ywlbList = new List<YwlbInfo>();
            YwlbInfo info = new YwlbInfo();

            //判断是否能进自助大厅
            if (TjConfig.TjModel.IsConnectionTj)
            {
                try
                {
                    Log.Instance.WriteInfo("1、开始核查【太极】【是否可进智慧大厅】接口");
                    if (string.IsNullOrEmpty(OwnerViewModel?.YWID))
                    {
                        Log.Instance.WriteInfo("阿 o 导引ID 弄丢了~~~系统查询到您的导服数据异常！");
                        OwnerViewModel?.MessageTips("系统查询到您的导服数据异常！", (() =>
                        {
                            Log.Instance.WriteInfo("点击【确定】按钮，进入选择派号区域");
                        }), null, 10, "确定");
                    }

                    //List<BookingInfo> booking = new List<BookingInfo>();
                    foreach (DictionaryType item in BookingBaseInfo.SelectCardTypes)
                    {
                        info = new YwlbInfo();
                        info.sqlb = item?.Code;//申请类型
                        var model = BookingBaseInfo.BookingInfo.Where(t => t.SQLB.Equals(item?.Code)).ToList()[0];
                        info.bzlb = model?.ApplyType?.Code;//办证类型
                        info.qzblbs = model?.ApplyType?.Status.ToString();//签注办理标识 后面修改

                        string logo = model?.YYJZBLLIST.Count > 0 ? "1" : "0";
                        info.jzblbs = logo;//加注办理标识
                        Log.Instance.WriteInfo("申请类型：" + info?.sqlb + "，办证类型：" + info?.bzlb + "，签注办理标识：" + info?.qzblbs?.ToString());
                        //if (item.Code == "104")
                        //continue;
                        ywlbList.Add(info);

                    }

                    Json_I_DY_Result dyResult = new Json_I_DY_Result
                    {
                        ywid = OwnerViewModel?.YWID,
                        ydrysf = OwnerViewModel?.ydrysf ?? "",
                        sjbzlbs = ywlbList.ToArray(),
                        jqbh = TjConfig.TjModel.TJMachineNo
                    };
                    Log.Instance.WriteInfo("【是否可进智慧大厅】参数：" + JsonHelper.ToJson(dyResult));
                    var count = new TaiJiHelper().DY_result(dyResult);//这里不能直接返回null，又不加null判断 by wei.chen
                    if (count.IsEmpty())
                        Log.Instance.WriteInfo("【是否可进智慧大厅】返回为null.");

                    if (count.IsNotEmpty() && count?.sfkyzz != null && count.sfkyzz == "1")
                    {
                        Log.Instance.WriteInfo("【太极】查询到" + CommandTools.ReplaceWithSpecialChar(BookingBaseInfo?.CardInfo?.FullName) + "可进入智慧大厅");
                        foreach (var item in BookingBaseInfo?.BookingInfo)
                        {
                            item.SFKYZZ = "1";
                            item.BKJYY = "";
                            item.YWID = OwnerViewModel.YWID;
                        }
                        //Log.Instance.WriteInfo("太极业务编号：" + BookingBaseInfo?.BookingInfo?.FirstOrDefault()?.YWID);
                    }
                    else
                    {
                        Log.Instance.WriteInfo("【太极】查询到" + CommandTools.ReplaceWithSpecialChar(BookingBaseInfo?.CardInfo?.FullName) + "不可进入智慧大厅，原因：" + count?.bkjyy?.ToString());
                        foreach (var item in BookingBaseInfo?.BookingInfo)
                        {
                            item.SFKYZZ = "0";
                            item.BKJYY = count?.bkjyy;
                            item.YWID = OwnerViewModel.YWID;
                        }
                        //Log.Instance.WriteInfo("太极业务编号：" + BookingBaseInfo?.BookingInfo?.FirstOrDefault()?.YWID);

                    }
                    if (!string.IsNullOrEmpty(count?.bkjyy))
                    {
                        // Log.Instance.WriteInfo("系统查询到您不可进入自助大厅办理，原因：" + count?.bkjyy);
                        //历史办证申请信息与人口信息主项不一致
                        if (count?.bkjyy.Contains("人口信息主项与历史申请信息不一致") == true)
                        {
                            Log.Instance.WriteInfo("包含：【人口信息主项与历史申请信息不一致】，提示语提示");
                            OwnerViewModel.isVALIDXCYY = "1";
                            OwnerViewModel?.MessageTips("系统查询到您不可进入自助大厅办理，原因：" + count?.bkjyy, (() =>
                            {
                                Log.Instance.WriteInfo("点击【确定】按钮，进入选择派号区域");
                            }), null, 10, "确定");
                            //return false;
                        }
                    }

                    if (BookingBaseInfo?.byslyy?.IsNotEmpty() == true && BookingBaseInfo.byslyy.Contains("人口信息主项与历史申请信息不一致"))
                    {
                        OwnerViewModel.isVALIDXCYY = "1";
                        OwnerViewModel?.MessageTips("系统查询到您不可进入自助大厅办理，原因：" + count?.bkjyy, (() =>
                        {
                            Log.Instance.WriteInfo("点击【确定】按钮，进入选择派号区域");
                        }), null, 10, "确定");
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteInfo("核查导服数据发生异常：" + ex.Message);
                    Log.Instance.WriteError("核查导服数据发生异常：" + ex.Message);
                    OwnerViewModel?.MessageTips("系统查询到您的导服数据异常！", (() =>
                    {
                        Log.Instance.WriteInfo("点击【确定】按钮，进入选择派号区域");
                    }), null, 10, "确定");
                }
                finally
                {
                    Log.Instance.WriteInfo("1、结束核查【太极】【是否可进智慧大厅】接口");
                }
            }
        }

        /// <summary>
        /// 核查是否可进智慧大厅
        /// </summary>
        public void PlanPhServers()
        {
            //北京连接太极判断能否进自助大厅
            IsConnectTaiji();
            if (OwnerViewModel?.IsBeijing != true)
            {
                Log.Instance.WriteInfo("准备核查智慧办理条件并分配派号区域/业务……");
                //核查智慧办理条件 并分配派号区域
                if (BookingBaseInfo.IsGetPHNO == "0" && !PH_CheckZHPH(BookingBaseInfo.CardInfo, out string msg1))
                {
                    IsGoZHDT = false;
                    //OwnerViewModel?.MessageTips(msg1, () =>
                    //{
                    //    DoExit.Execute(null);
                    //});
                    Log.Instance.WriteInfo("核查到不能进入智慧大厅原因：" + msg1);
                    //return;
                }
            }

            if (QJTConfig.QJTModel.IsTodayPH)
            {
                Log.Instance.WriteInfo("不派号跳过派号区域判断，下一步：进入预约完成界面");
                //this.DoNext.Execute("CheckPage");
                DoNextFunction("CheckPage");
                return;
            }
            Log.Instance.WriteInfo("2、开始获取【派号业务】接口");

            var models = BookingBaseInfo?.CardInfo;
            //if(OwnerViewModel?.IsShanXi==false)
            var list = CheckService(models?.IDCardNo, OwnerViewModel.IsBeijingRegister, OwnerViewModel?.isVALIDXCYY, models.IsOfficial, models.RSZGDW, OwnerViewModel?.SFKZDX);
            Log.Instance.WriteInfo("获取【派号业务】接口返回：" + list?.Count);
            if (list?.Count == 1)
            {
                BookingBaseInfo.AreaCode = list.FirstOrDefault();
                //关闭倒计时 by wei.chen
                Log.Instance.WriteInfo($"关闭倒计时：【1】");
                //OnDispose();
                this.DoNext.Execute("CheckPage");
            }

            //北京地区区域分配
            if (OwnerViewModel?.IsBeijing == true)
            {
                try
                {
                    //var model = BookingBaseInfo?.CardInfo;
                    //var lst = ZHPHMachineWSHelper.ZHPHInstance.S_PlanPHSERVICETYPE(model?.IDCardNo, OwnerViewModel.IsBeijingRegister, OwnerViewModel?.isVALIDXCYY, model.IsOfficial, model.RSZGDW, OwnerViewModel?.SFKZDX);
                    if (list != null && list.Count > 0)
                    {
                        Log.Instance.WriteInfo($"获取派号业务成功，业务编号：【{list?.FirstOrDefault()?.SERVICE_CODE}】");
                        AreaInfo = list;
                        //if (List.Count == 1)
                        //{
                        //    BookingBaseInfo.AreaCode = List[0];
                        //    this.DoNext.Execute("CheckPage");
                        //}
                    }
                    else
                    {
                        OwnerViewModel?.MessageTips("获取派号业务失败，请联系工作人员检查！", (() =>
                        {
                            this.DoExit.Execute(null);
                        }));
                    }

                }
                catch (Exception ex)
                {
                    Log.Instance.WriteInfo($"[区域分配派号业务异常]");
                    Log.Instance.WriteError(ex.Message);
                    OwnerViewModel?.MessageTips(ex.Message, (() =>
                    {
                        this.DoExit.Execute(null);
                    }));
                }
                finally
                {
                    Log.Instance.WriteInfo("2、结束获取【派号业务】接口");
                    OwnerViewModel?.IsShowHiddenLoadingWait();
                }
            }

            //陕西地区查询区域分配
            if (OwnerViewModel?.IsShanXi == true)
            {
                //by wei.chen 处理null返回
                var lst = ZHPHMachineWSHelper.ZHPHInstance.GetPHServiceType(QJTConfig.QJTModel?.ZhDeviceInfo?.QYCODE?.ToString(), "", QJTConfig.QJTModel.IS_NEED_SELF);
                Log.Instance.WriteInfo("陕西地区查询区域分配返回：" + lst.Count);

                if (OwnerViewModel?.isVALIDXCYY == "1" && QJTConfig.QJTModel?.IsManual == true)
                {
                    Log.Instance.WriteInfo($"获取派号业务成功，业务编号：【{lst[0].SERVICE_CODE}】");
                    AreaInfo = lst;

                    if (AreaInfo.Count > 0)
                    {
                        AreaInfo = lst.Where(t => t.PREFIX == "Y").ToList();
                        Log.Instance.WriteInfo("获取人工派号业务数量：" + AreaInfo.Count);
                        //Log.Instance.WriteInfo("获取人工派号业务：" + AreaInfo[0].SERVICE_CODE);
                        BookingBaseInfo.AreaCode = AreaInfo[0];
                        //this.DoNext.Execute("CheckPage");
                        DoNextFunction("CheckPage");
                    }

                    //this.DoNext.Execute("CheckPage");
                    DoNextFunction("CheckPage");
                }
                else
                {
                    if (lst != null && lst.Count > 0)
                    {
                        Log.Instance.WriteInfo($"获取派号业务成功，业务编号：【{lst[0].SERVICE_CODE}】");
                        AreaInfo = lst;

                        if (AreaInfo.Count > 0)
                        {
                            AreaInfo = lst.Where(t => t.PREFIX == "A").ToList();

                            BookingBaseInfo.AreaCode = AreaInfo[0];
                            //this.DoNext.Execute("CheckPage");

                            // DoNextFunction("CheckPage");
                        }

                        //this.DoNext.Execute("CheckPage");
                        DoNextFunction("CheckPage");
                    }
                }

            }

            //湛江徐闻区域分配
            if (OwnerViewModel?.IsXuwen == true)
            {
                if (OwnerViewModel?.IsLeiZhou == true)
                {
                    OwnerViewModel.isVALIDXCYY = "0";
                    Log.Instance.WriteInfo("雷州地区不派人工号，人工号标识为:" + OwnerViewModel.isVALIDXCYY);
                }
                try
                {
                    //var model = BookingBaseInfo?.CardInfo;
                    //var lst = ZHPHMachineWSHelper.ZHPHInstance.S_PlanPHSERVICETYPE(model?.IDCardNo, OwnerViewModel.IsBeijingRegister, OwnerViewModel?.isVALIDXCYY, model.IsOfficial, model.RSZGDW, OwnerViewModel?.SFKZDX);
                    if (list != null && list.Count > 0)
                    {
                        Log.Instance.WriteInfo($"获取派号业务成功，业务编号：【{list?.FirstOrDefault()?.SERVICE_CODE}】");
                        AreaInfo = list;

                        //if (List.Count == 1)
                        //{
                        //    BookingBaseInfo.AreaCode = List?.FirstOrDefault();
                        //    this.DoNext.Execute("CheckPage");
                        //}
                        //else
                        //{
                        if (OwnerViewModel?.isVALIDXCYY == "1")//雷州不派人工号
                        {
                            AreaInfo = list.Where(t => t.IS_NEED_SELF == "1")?.ToList();
                            Log.Instance.WriteInfo("获取人工派号业务区域数量为：" + AreaInfo?.Count);

                            if (AreaInfo.Count > 0)
                            {
                                BookingBaseInfo.AreaCode = AreaInfo?.FirstOrDefault();
                                //this.DoNext.Execute("CheckPage");
                                DoNextFunction("CheckPage");
                            }
                        }
                        else
                        {
                            AreaInfo = list.Where(t => t.IS_NEED_SELF == "0").ToList();
                            Log.Instance.WriteInfo("获取派号业务区域数量为：" + AreaInfo?.Count);
                            if (AreaInfo.Count > 0)
                            {
                                if (BookingBaseInfo.Book_Type == "0" && OwnerViewModel?.IsLeiZhou == true)
                                {
                                    AreaInfo = list.Where(t => t.PREFIX == "B").ToList();
                                }
                                BookingBaseInfo.AreaCode = AreaInfo?.FirstOrDefault();
                                Log.Instance.WriteInfo($"获取派号业务成功，业务编号：【{BookingBaseInfo?.AreaCode?.SERVICE_CODE}】,下一步：派号页面");
                                //this.DoNext.Execute("CheckPage");
                                DoNextFunction("CheckPage");
                            }
                        }
                        //}
                    }
                    else
                    {
                        OwnerViewModel?.MessageTips("获取派号业务失败，请联系工作人员检查！", (() =>
                        {
                            this.DoExit.Execute(null);
                        }));
                    }

                }
                catch (Exception ex)
                {
                    Log.Instance.WriteInfo($"[区域分配派号业务异常]");
                    Log.Instance.WriteError(ex.Message);
                    OwnerViewModel?.MessageTips(ex.Message, (() =>
                    {
                        this.DoExit.Execute(null);
                    }));
                }
                finally
                {
                    //this.DoNext.Execute("CheckPage");
                    OwnerViewModel?.IsShowHiddenLoadingWait();
                }

            }

            //武汉地区查询区域分配
            if (OwnerViewModel?.IsWuHan == true)
            {
                if (list != null && list.Count > 0)
                {
                    Log.Instance.WriteInfo($"武汉获取派号业务成功，业务编号：【{list?.FirstOrDefault()?.SERVICE_CODE}】");
                    AreaInfo = list;

                    //if (BookingBaseInfo.Book_Type == "0")
                    //{
                    //    AreaInfo = list.Where(t => t.PREFIX == "G").ToList();
                    //}

                    //var age = ValidationHelper.Get_NomalAge(models, out string msg);
                    //if (age > 70 || age < 16)
                    //{
                    //    AreaInfo = list.Where(t => t.PREFIX == "V").ToList();
                    //}
                    //else
                    //{
                    //    AreaInfo = list.Where(t => t.PREFIX == "A").ToList();
                    //}
                    //BookingBaseInfo.AreaCode = AreaInfo.FirstOrDefault();
                    //this.DoNext.Execute("CheckPage");
                }
                else
                {
                    OwnerViewModel?.MessageTips("获取派号业务失败，请联系工作人员检查！", (() =>
                    {
                        this.DoExit.Execute(null);
                    }));
                }
                //this.DoNext.Execute("CheckPage");
                OwnerViewModel?.IsShowHiddenLoadingWait();
            }

            if (OwnerViewModel?.IsGuangDong == true)
            {
                try
                {
                    if (BookingBaseInfo.AreaCode == null)
                    {
                        if (OwnerViewModel?.isVALIDXCYY != "1" && !IsGoZHDT) //
                        {
                            Log.Instance.WriteInfo("广东地区核查到不可进入智慧大厅，人工号标识为1");
                            OwnerViewModel.isVALIDXCYY = "1";
                        }

                        //var model = BookingBaseInfo?.CardInfo;
                        //var lst = ZHPHMachineWSHelper.ZHPHInstance.S_PlanPHSERVICETYPE(model?.IDCardNo, true,
                        //    OwnerViewModel?.isVALIDXCYY, model.IsOfficial, model.RSZGDW, OwnerViewModel?.SFKZDX);
                        if (list != null && list.Count > 0)
                        {
                            Log.Instance.WriteInfo($"获取派号业务成功，业务编号：【{list?.FirstOrDefault()?.SERVICE_CODE}】");
                            AreaInfo = list;
                            if (AreaInfo.Count == 1)
                            {
                                //关闭倒计时 by wei.chen
                                Log.Instance.WriteInfo($"关闭倒计时：【2】");
                                OnDispose();
                                BookingBaseInfo.AreaCode = AreaInfo.FirstOrDefault();
                                this.DoNext.Execute("CheckPage");
                            }
                            else
                            {
                                if (OwnerViewModel?.isVALIDXCYY == "1")
                                {
                                    AreaInfo = list.Where(t => t.IS_NEED_SELF == "1").ToList();
                                    Log.Instance.WriteInfo("获取人工派号业务区域数量为：" + AreaInfo?.Count);

                                    if (AreaInfo.Count > 0)
                                    {
                                        //关闭倒计时 by wei.chen
                                        Log.Instance.WriteInfo($"关闭倒计时：【3】");
                                        OnDispose();
                                        Log.Instance.WriteInfo("获取人工派号业务数量：" + AreaInfo?.Count);
                                        Log.Instance.WriteInfo("获取人工派号业务：" + AreaInfo?.FirstOrDefault()?.SERVICE_CODE);
                                        BookingBaseInfo.AreaCode = AreaInfo?.FirstOrDefault();
                                        this.DoNext.Execute("CheckPage");
                                    }
                                }
                                else
                                {
                                    AreaInfo = list.Where(t => t.IS_NEED_SELF == "0").ToList();

                                    if (AreaInfo.Count > 0)
                                    {
                                        //Log.Instance.WriteInfo("获取人工派号业务：" + AreaInfo.Count);
                                        //Log.Instance.WriteInfo("获取人工派号业务：" + AreaInfo[0].SERVICE_CODE);
                                        //关闭倒计时 by wei.chen
                                        Log.Instance.WriteInfo($"关闭倒计时：【4】");
                                        OnDispose();
                                        BookingBaseInfo.AreaCode = AreaInfo?.FirstOrDefault();
                                        Log.Instance.WriteInfo(
                                            $"获取派号业务成功，业务编号：【{BookingBaseInfo?.AreaCode?.SERVICE_CODE}】");
                                        this.DoNext.Execute("CheckPage");
                                    }
                                }

                                //BookingBaseInfo.AreaCode = AreaInfo.FirstOrDefault();
                                //this.DoNext.Execute("CheckPage");
                            }
                        }
                        else
                        {
                            OwnerViewModel?.MessageTips("获取派号业务失败，请联系工作人员检查！", (() => { OwnerViewModel.DoExitFunction(null); }));
                        }

                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteInfo($"[区域分配派号业务异常]");
                    Log.Instance.WriteError(ex.Message);
                    OwnerViewModel?.MessageTips(ex.Message, (() =>
                    {
                        OwnerViewModel.DoExitFunction(null);
                    }));
                }
                finally
                {
                    OwnerViewModel?.IsShowHiddenLoadingWait();
                }

            }
        }

        /// <summary>
        /// 核查智慧办理条件
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool PH_CheckZHPH(IdCardInfo info, out string msg1)
        {
            Log.Instance.WriteInfo("开始核查智慧办理条件……");
            msg1 = string.Empty;
            if (BookingBaseInfo?.BookingInfo == null)
            {
                msg1 = "未查询到预约信息";
                Log.Instance.WriteInfo(msg1);
                return false;
            }
            if (info.IDCardNo.IsEmpty())
            {
                msg1 = "未查询到申请人ID信息";
                Log.Instance.WriteInfo(msg1);
                return false;
            }
            if (OwnerViewModel?.IsXuwen == true)
            {
                Log.Instance.WriteInfo("徐闻直接返回成功。");
                return true;
            }
            try
            {
                //实际办理类别信息集合
                List<SJBZLBInfo> sjbzlbInfo = new List<SJBZLBInfo>();
                SJBZLBInfo bzlbInfo = new SJBZLBInfo();

                //证件信息集合
                List<ZjxxInfo> zjxxInfos = new List<ZjxxInfo>();
                ZjxxInfo zjxx = new ZjxxInfo();

                foreach (var item in BookingBaseInfo.BookingInfo)
                {
                    //实际办理类别
                    bzlbInfo = new SJBZLBInfo
                    {
                        SQLB = item.SQLB,
                        BZLB = item.ApplyType.Code,
                        QWD = item.QWD,
                        ZJZL = item.XCZJZL?.ToString(),
                        ZJHM = item.XCZJHM?.ToString(),
                        QZZL = item.YYQZLIST?.Count > 0 ? item.YYQZLIST[0].QZType.Code : null,
                        QZYXQ = item.YYQZLIST?.Count > 0 ? item.YYQZLIST[0].QZYXQ : null,
                        QZYXCS = item.YYQZLIST?.Count > 0 ? item.YYQZLIST[0].QZYXCS.ToInt() : 0,
                        QZYXQDW = item.YYQZLIST?.Count > 0 ? item.YYQZLIST[0].QZYXQDW?.ToString() : null,
                        TBDWBH = QJTConfig.QJTModel.QJTDevInfo.TBDWBH,
                    };
                    sjbzlbInfo.Add(bzlbInfo);
                    if (!string.IsNullOrEmpty(item.XCZJHM) && !string.IsNullOrEmpty(item.XCZJYXQZ))
                    {
                        //证件信息
                        zjxx = new ZjxxInfo
                        {
                            zjzl = item.XCZJZL?.ToString(),
                            zjhm = item.XCZJHM?.ToString(),
                            zjqfrq = item.XCZJQFRQ?.ToString(),
                            zjyxqz = item.XCZJYXQZ?.ToString(),
                            zjzt = item.DJSY?.ToString(),
                        };
                        zjxxInfos.Add(zjxx);
                    }
                }

                Json_I_PH_CheckZHPH phCheck = new Json_I_PH_CheckZHPH();
                phCheck = new Json_I_PH_CheckZHPH
                {
                    SFZHORYWBH = info.IDCardNo,//身份证号码
                    ZWXM = info.Zwx + info.Zwm,//中文姓名
                    MZ = info.pNational ?? "01",//民族
                    HKSZD = BookingBaseInfo.Address.Code ?? info.IDCardNo.Substring(0, 6),//户口所在地代码
                    YDRYSF = OwnerViewModel.ydrysf ?? "15",
                    SJBZLBS = sjbzlbInfo,
                    zbywxxs = null,
                    SLDW = QJTConfig.QJTModel.QJTDevInfo.TBDWBH,
                    ISVALIDXCYY = OwnerViewModel.isVALIDXCYY,
                    ISOFFICIAL = info.IsOfficial ? "1" : "0",
                    RSZGDW = info.RSZGDW,
                    SFKZDX = info.IsCheckSpecil ? "1" : "0",
                    BYQK = "0",
                    ZJXXS = zjxxInfos,
                    ZP = null
                };

                //Log.Instance.WriteInfo("核查智慧办理条件参数：" + JsonHelper.ToJson(phCheck));

                var result = ZHPHMachineWSHelper.ZHPHInstance.PH_CheckZHPH(phCheck, out string message);
                //if (!string.IsNullOrEmpty(message))
                //{
                //    OwnerViewModel.isVALIDXCYY = "1";
                //    msg1 = message;
                //    Log.Instance.WriteInfo("核查结果：不可进入智慧大厅，原因：" + message);
                //    return false;
                //}
                if (result != null)
                {
                    if (string.IsNullOrEmpty(result?.BFHBLYY) && string.IsNullOrEmpty(result?.BFHBLYYDETAIL))
                        Log.Instance.WriteInfo("核查结果：可进入智慧大厅");
                    else
                    {
                        msg1 = result.BFHBLYY;
                        Log.Instance.WriteInfo("核查结果：不可进入智慧大厅，原因：" + result?.BFHBLYYDETAIL);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("核查智慧办理条件发生异常：" + ex.Message);
                Log.Instance.WriteError("核查智慧办理条件发生异常：" + ex.Message);
            }
            finally
            {
                Log.Instance.WriteInfo("结束核查智慧办理条件……");
            }
            return true;
        }

        /// <summary>
        /// 区域分配派号业务编码
        /// </summary>
        /// <param name="sfzh">身份证号码</param>
        /// <param name="isLOACLHJ">是否北京户籍 0：否 1：是 </param>
        /// <param name="isVALIDXCYY">是否现场预约 0：否 1：是</param>
        /// <param name="isOFFICIAL">是否国家工作人员 0：否 1：是</param>
        /// <param name="rszgdw">人事主管单位（是：国家工作人员时需传入）</param>
        /// <param name="sfkzdx">是否控制对象 0：否 1：是</param>
        /// <returns></returns>
        public List<PH_SERVICETYPE_TB> CheckService(string sfzh, bool isLOACLHJ, string isVALIDXCYY,
            bool isOFFICIAL, string rszgdw, string sfkzdx)
        {
            var lst = ZHPHMachineWSHelper.ZHPHInstance.S_PlanPHSERVICETYPE(sfzh, isLOACLHJ, OwnerViewModel?.isVALIDXCYY, isOFFICIAL, rszgdw, sfkzdx);

            //if (lst != null && lst.Count > 0)
            //{
            //    Log.Instance.WriteInfo($"获取派号业务成功，业务编号：【{lst[0]?.SERVICE_CODE}】");
            //    AreaInfo = lst;

            //    if (lst.Count == 1)
            //    {
            //        BookingBaseInfo.AreaCode = lst[0];
            //        this.DoNext.Execute("CheckPage");
            //    }
            //}
            //else
            //{
            //    OwnerViewModel?.MessageTips("获取派号业务异常，请联系工作人员检查！", (() =>
            //    {
            //        Log.Instance.WriteInfo("派号业务返回为空！！！");
            //        this.DoExit.Execute(null);
            //    }));
            //}

            return lst;
        }

        #endregion
    }
}
