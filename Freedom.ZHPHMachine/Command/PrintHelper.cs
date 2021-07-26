using Aspose.Words;
using Aspose.Words.Drawing;
using Freedom.Common;
using Freedom.Common.HsZhPjh.Enums;
using Freedom.Config;
using Freedom.Hardware;
using Freedom.Models;
using Freedom.Models.JgModels;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.ViewModels;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Freedom.Models.DataBaseModels;
using MachineCommandService;

namespace Freedom.ZHPHMachine.Command
{
    public class PrintHelper
    {
        /// <summary>
        /// 是否陕西地区
        /// </summary>
        public static bool IsShanXi
        {
            get
            {
                string DWCode = QJTConfig.QJTModel?.QJTDevInfo?.TBDWBH;
                if (!string.IsNullOrWhiteSpace(DWCode) &&
                    DWCode.Length > 2 &&
                    DWCode.Substring(0, 2).Equals("61"))
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否广州地区
        /// </summary>
        public static bool IsGuangZhou
        {
            get
            {
                string DWCode = QJTConfig.QJTModel?.QJTDevInfo?.TBDWBH;
                if (!string.IsNullOrWhiteSpace(DWCode) &&
                    DWCode.Length > 4 &&
                    DWCode.Substring(0, 4).Equals("4401"))
                {
                    return true;
                }
                return false;
            }
        }

        public static Dev_AlarmInfo DevAlarm = new Dev_AlarmInfo();//设备故障信息

        /// <summary>
        /// 校验打印机是否可用
        /// </summary>
        /// <param name="msg">返回状态</param>
        /// <returns></returns>
        public static bool CheckPrinter(out string msg)
        {
            bool result = false;
            msg = string.Empty;

            if (QJTConfig.QJTModel.PT_Is_USB || QJTConfig.QJTModel.Is_ConnPTDevice)
            {
                if (TxPrintServer.Instance.OpenDev())
                {
                    int value = Print_TxDll.TxGetStatus2();
                    if (value != 0)
                    {
                        foreach (TX_STAT item in Enum.GetValues(typeof(TX_STAT)))
                        {
                            if (item == TX_STAT.TX_STAT_NOERROR && (value & (int)item) != 0)
                            {
                                result = true;
                                break;
                            }
                            else if ((value & (int)item) != 0)
                            {
                                msg += $"{EnumType.GetEnumDescription(item)} ";
                            }
                        }
                    }
                    else
                    {
                        msg = "获取状态失败";
                    }
                    msg = string.IsNullOrWhiteSpace(msg) ? "未知打印机错误" : msg;
                    TxPrintServer.Instance.CloseDev();
                }
                else
                {
                    msg = TipMsgResource.PrintInitializationTipMsg;
                }
            }
            else { result = true; }


            return result;
        }

        /// <summary>
        /// 打印模板
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        private static bool TemplatePrint(List<JGPrintData> lst)
        {
            bool result = false;
            try
            {
                if (QJTConfig.QJTModel.PT_Is_USB || QJTConfig.QJTModel.Is_ConnPTDevice)
                {
                    //打开串口
                    if (TxPrintServer.Instance.OpenDev())
                    {
                        foreach (JGPrintData item in lst)
                        {
                            switch (item.DataType)
                            {
                                case 0://文本打印
                                       //设置对齐方式 
                                    Print_TxDll.TxDoFunction((int)TXFunEnum.TX_ALIGN, item.AlignmentMode, 0);
                                    if (!string.IsNullOrWhiteSpace(item.Style))
                                    {
                                        if (item.Style.Contains("Boldface"))//文本加粗 
                                            Print_TxDll.TxDoFunction((int)TXFunEnum.TX_FONT_BOLD, (int)Enum_TX_ONOFF.TX_ON, 0);
                                        if (item.Style.Contains("Underline"))//下滑下 
                                            Print_TxDll.TxDoFunction((int)TXFunEnum.TX_FONT_ULINE, (int)Enum_TX_ONOFF.TX_ON, 0);
                                    }
                                    Print_TxDll.TxOutputStringLn(item.Text);
                                    Print_TxDll.TxResetFont();//恢复字体效果 
                                    break;
                                case 1://打印二维码 
                                    Print_TxDll.TxDoFunction((int)TXFunEnum.TX_ALIGN, item.AlignmentMode, 0);
                                    Print_TxDll.TxDoFunction((int)TXFunEnum.TX_QR_DOTSIZE, 9, 0);
                                    Print_TxDll.TxDoFunction((int)TXFunEnum.TX_QR_ERRLEVEL, (int)TX_QR_ERRLEVEL.TX_QR_ERRLEVEL_M, 0);
                                    Print_TxDll.TxPrintQRcode(item.Text, item.Text.Length);
                                    break;
                                case 9://走纸
                                    Print_TxDll.TxDoFunction((int)TXFunEnum.TX_FEED, int.Parse(item.Text), 0);
                                    break;
                            }
                        }
                        //全切
                        Print_TxDll.TxDoFunction((int)TXFunEnum.TX_CUT, (int)Enum_TX_CUT.TX_CUT_FULL, 0);
                        //关闭串口
                        Print_TxDll.TxClosePrinter();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("打印凭条发生异常，异常信息：" + ex.Message);
            }
            return result;
        }

        /// <summary>
        ///  打印预约小票
        /// </summary>
        /// <param name="sendNo">派号码</param>
        /// <param name="id">派号id</param>
        /// <param name="msg">提示语句</param>
        /// <param name="name">单位名称</param>
        /// <param name="model">预约信息</param>
        /// <returns></returns>
        public static bool BookingPrint(string sendNo, string id, string msg, string name, BookingModel model)
        {
            bool result = false;
            try
            {
                List<JGPrintData> pringDataList = new List<JGPrintData>()
                        {
                            new JGPrintData() { DataType=0, Text =$"<{sendNo}>", AlignmentMode = 1, Style = "Boldface"},
                            new JGPrintData() { DataType=9, Text ="24"},
                            new JGPrintData() { DataType=0, Text =$"姓名：{model?.CardInfo?.FullName}", AlignmentMode = 0},
                            new JGPrintData() { DataType=0, Text =$"预约时间：{model.BookingTarget.BookingDt?.ToString("yyyy-MM-dd")} {model.BookingTarget.Title}", AlignmentMode = 0},
                            new JGPrintData() { DataType=0, Text =$"为您办理：{model.StrCardTypes}", AlignmentMode = 0},
                            new JGPrintData() { DataType=9, Text ="24"},
                            new JGPrintData() { DataType=0, Text ="指导语", AlignmentMode = 0, Style = "Boldface|Underline"},
                            new JGPrintData() { DataType=0, Text =$"{msg}", AlignmentMode = 0, Style = "Boldface|Underline"},
                            new JGPrintData() { DataType=9, Text ="24"},
                            new JGPrintData() { DataType=0, Text =$"{QJTConfig.QJTModel.QJTDevInfo.DeptInfo.DeptName}", AlignmentMode = 1, Style = "Boldface"},
                            new JGPrintData() { DataType=9, Text ="24"},
                            new JGPrintData() { DataType=0, Text ="=======温馨提示========", AlignmentMode = 1},
                            new JGPrintData() { DataType=9, Text ="24"},
                            new JGPrintData() { DataType=0, Text ="二维条码", AlignmentMode = 1},
                            new JGPrintData() { DataType=1, Text =$"{id}", AlignmentMode = 1},
                            new JGPrintData() { DataType=9, Text ="24"},
                            new JGPrintData() { DataType=0, Text ="过号作废，请留意屏幕叫号！！！", AlignmentMode = 1, Style = "Boldface"},
                            new JGPrintData() { DataType=9, Text ="24"},
                            new JGPrintData() { DataType=0, Text =$"<{QJTConfig.QJTModel.QJTDevInfo.DEV_NAME}-{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}>", AlignmentMode = 1},
                            new JGPrintData() { DataType=9, Text ="200"},
                        };
                result = TemplatePrint(pringDataList);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("打印凭条发生异常，异常信息：" + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="info"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool CreateQrCode(string info, string path)
        {

            if (string.IsNullOrWhiteSpace(info) || string.IsNullOrWhiteSpace(path))
            {
                return false;
            }
            try
            {
                QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.Q);
                QrCode qrCode = qrEncoder.Encode(info);
                GraphicsRenderer renderer = new GraphicsRenderer(new FixedModuleSize(10, QuietZoneModules.Two), System.Drawing.Brushes.Black, System.Drawing.Brushes.White);
                using (MemoryStream ms = new MemoryStream())
                {
                    renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, ms);
                    Image img = Image.FromStream(ms);
                    img.Save(path);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("生成二维码异常" + ex.Message);
            }
            return false;
        }

        /// <summary>
        /// word模板打印
        /// </summary>
        /// <returns></returns>
        public static bool BookingWordPrint(string sendNo, string id, string msg, string name, int waitNums, BookingModel model, JsonPH_SendOut SendInfo)
        {
            try
            {

                //获取计算机所有打印机
                var printers = PrinterSettings.InstalledPrinters;
                if (printers.Count < 1)
                {
                    //msg = $"未安装{PrintPHPrinterName}打印机";
                    //LoadWS.dev_Alarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault20;
                    //LoadWS.dev_Alarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_10000015);
                    //LoadWS.dev_Alarm.AlarmInfo = msg;
                    //ZHWSBase.Instance.InsertDeviceAutoFault(LoadWS.dev_Alarm.DevID.ToString(), LoadWS.dev_Alarm.AlarmTypeID.ToString(), LoadWS.dev_Alarm.AlarmInfo.ToString(), LoadWS.dev_Alarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_10000015).ToString());
                    //Log.Instance.WriteInfo(msg);
                    //return msg;
                }

                //string printpath = @"win32_printer.DeviceId='" + LoadWS.PrintPHPrinterName + "'";
                //string printName = "EPSON BA-T500II Receipt";
                //if (printers == null || printers.Count < 1)
                //{
                //    Log.Instance.WriteError($"未安装{printName}打印机");
                //    Log.Instance.WriteInfo($"未安装{printName}打印机");
                //    return false;
                //}
                ////判断是否安装所需打印机
                //foreach (string item in printers)
                //{
                //    if (item == printName)
                //    {
                //        result = true;
                //        break;
                //    }
                //}
                //if (!result)
                //{
                //    Log.Instance.WriteError($"未安装{printName}打印机");
                //    Log.Instance.WriteInfo($"未安装{printName}打印机");
                //    return false;
                //}
                //string printpath = @"win32_printer.DeviceId='" + printName + "'";

                //ManagementObject printer = new ManagementObject(printpath);
                ////判断打印机是否离线
                //if (Convert.ToBoolean(printer.Properties["WorkOffline"].Value))
                //{
                //    Log.Instance.WriteError($"{printName}打印机已离线");
                //    Log.Instance.WriteInfo($"{printName}打印机已离线");
                //    return false;
                //}
                //判断是否是默认打印机
                //PrintDocument fPrintDocument = new PrintDocument();
                //if (fPrintDocument?.PrinterSettings.PrinterName != printName)
                //{
                //    Log.Instance.WriteError($"请设置{printName}打印机为默认打印机");
                //    return false;
                //}

                //模板路径
                string path = Path.Combine(FileHelper.GetLocalPath(), "PHPrint.docx");

                if (File.Exists(path))
                {
                    object oMissing = System.Reflection.Missing.Value;

                    //创建一个Word应用程序实例
                    Microsoft.Office.Interop.Word.Application oWord = new Microsoft.Office.Interop.Word.Application();
                    oWord.DisplayAlerts = Microsoft.Office.Interop.Word.WdAlertLevel.wdAlertsNone;
                    //设置为不可见
                    oWord.Visible = false;
                    //模板文件地址，这里假设在X盘根目录
                    object oTemplate = path;
                    object otrue = true;
                    object ofalse = false;
                    //不保存
                    Object saveChanges = WdSaveOptions.wdDoNotSaveChanges;
                    //生产二维码图片 
                    string replacePic = Path.Combine(FileHelper.GetLocalPath(), "QRCode.png");
                    if (!CreateQrCode(id, replacePic))
                    {
                        Log.Instance.WriteInfo("生成二维码异常");
                        return false;
                    }
                    //以模板为基础生成文档
                    Microsoft.Office.Interop.Word.Document oDoc = oWord.Documents.Add(ref oTemplate, ref oMissing, ref oMissing, ref oMissing);
                    if (oDoc.Bookmarks["Title"] != null)
                        oDoc.Bookmarks.get_Item("Title").Range.Text = SendInfo.ZWXM + "  " + SendInfo.PREFIX + SendInfo.PH_NUMBER;
                    if (oDoc.Bookmarks["BookingDateTime"] != null)
                        oDoc.Bookmarks.get_Item("BookingDateTime").Range.Text = SendInfo.YY_TIME?.ToString();
                    if (oDoc.Bookmarks["BookingType"] != null)
                        oDoc.Bookmarks.get_Item("BookingType").Range.Text = model.StrCardTypes;

                    if (oDoc.Bookmarks["GuidTipMsg"] != null)
                        oDoc.Bookmarks.get_Item("GuidTipMsg").Range.Text = msg;
                    if (oDoc.Bookmarks["DeptName"] != null)
                        oDoc.Bookmarks.get_Item("DeptName").Range.Text = name;//QJTConfig.QJTModel?.QJTDevInfo?.DeptInfo?.DeptName;
                    if (oDoc.Bookmarks["MsgTip"] != null)
                        oDoc.Bookmarks.get_Item("MsgTip").Range.Text = "过号作废，请留意屏幕叫号!!!";
                    if (oDoc.Bookmarks["Describe"] != null)
                        oDoc.Bookmarks.get_Item("Describe").Range.Text = $"{QJTConfig.QJTModel?.QJTDevInfo?.DEV_NAME}-{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
                    if (oDoc.Bookmarks["WaitNums"] != null)
                        oDoc.Bookmarks.get_Item("WaitNums").Range.Text = waitNums.ToString();
                    if (oDoc.Bookmarks["QRCode"] != null)
                    {
                        oDoc.Bookmarks.get_Item("QRCode").Select();

                        InlineShape inlineShape =
                            oWord.Selection.InlineShapes.AddPicture(replacePic, otrue, otrue, oMissing);
                        inlineShape.Width = 80;
                        inlineShape.Height = 80;
                    }
                    //打印
                    oDoc.PrintOutOld(ref otrue, ref ofalse, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                    oDoc.Close(saveChanges, ref oMissing, ref oMissing);
                    //等待打印状态
                    while (oWord.BackgroundPrintingStatus > 0)
                    {
                        System.Threading.Thread.Sleep(200);
                    }
                    //关闭word
                    oWord.Quit(ref oMissing, ref oMissing, ref oMissing);
                    Log.Instance.WriteInfo("打印派号单时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return true;
                }
                else
                {
                    Log.Instance.WriteInfo("未找到PHPrint.docx打印模板");
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteError("Word模板打印凭条失败" + ex.Message);
            }
            return false;
        }

        public static bool print(string strWordFile)
        {
            //return true;
            object fn = strWordFile;
            object mt = Type.Missing;
            object otrue = true;
            object ofalse = false;
            try
            {
                Microsoft.Office.Interop.Word._Application app = new Microsoft.Office.Interop.Word.Application();
                //app.ActivePrinter
                app.DisplayAlerts = Microsoft.Office.Interop.Word.WdAlertLevel.wdAlertsNone;
                Microsoft.Office.Interop.Word._Document doc = app.Documents.OpenOld(ref fn, ref mt, ref mt, ref mt, ref mt, ref mt, ref mt, ref mt, ref mt, ref mt);
                //by 2021年7月6日16:31:07
                if (doc.IsEmpty())
                    return false;
                doc.PrintOutOld(ref otrue, ref ofalse, ref mt, ref mt, ref mt, ref mt, ref mt, ref mt, ref mt, ref mt, ref mt, ref mt, ref mt, ref mt);
                doc.Close(ref mt, ref mt, ref mt);
                while (app.BackgroundPrintingStatus > 0)
                {
                    System.Threading.Thread.Sleep(500);
                }
                app.Quit(ref mt, ref mt, ref mt);
                return true;
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("office打印异常 :" + ex.Message);
                return false;
            }
            finally
            {

            }
        }

        /// <summary>
        /// 打印派号单模板
        /// </summary>
        /// <returns></returns>
        public static bool BookingWordPrintNew(string sendNo, string id, string msg, string name, int waitNums, BookingModel model, JsonPH_SendOut SendInfo)
        {

            //获取计算机所有打印机
            Log.Instance.WriteInfo("\n**********************************开始【打印派号单】界面**********************************");
            string replacePic = string.Empty;
            string PhTableName = "";

            if (!string.IsNullOrEmpty(QJTConfig.QJTModel.QJTDevInfo.DeptInfo.Remark))
            {
                PhTableName = QJTConfig.QJTModel.QJTDevInfo.DeptInfo.Remark;
                //PhTableName = "phTableWH.doc"; //QJTConfig.QJTModel.QJTDevInfo.DeptInfo.Remark;
            }
            if (SendInfo.IsEmpty())
            {
                Log.Instance.WriteInfo("《《《《《阿o 派号数据弄丢了》》》》》");
            }
            string PrintPhTableName = "phTableBakup.doc";
            string docPath = Path.Combine(FileHelper.GetLocalPath(), PhTableName);
            string printPath = Path.Combine(FileHelper.GetLocalPath(), PrintPhTableName);
            try
            {
                if (!File.Exists(docPath))
                {
                    PhTableName = "phTable.doc";
                    docPath = Path.Combine(FileHelper.GetLocalPath(), PhTableName);
                }
                if (File.Exists(docPath))
                {
                    Aspose.Words.Document awPrintDoc = new Aspose.Words.Document(docPath);
                    Log.Instance.WriteInfo("身份证号为【" + sendNo + " - " + name?.ToString() + "】，开始打印派号单。" + docPath);
                    if (File.Exists(docPath))
                    {
                        //生产二维码图片 
                        replacePic = Path.Combine(FileHelper.GetLocalPath(), "phQr.png");
                        Log.Instance.WriteInfo("二维码：" + replacePic);
                        try
                        {
                            if (File.Exists(Path.Combine(FileHelper.GetLocalPath(), PrintPhTableName)))
                            {
                                Log.Instance.WriteInfo("开始删除历史打印文档，生成新文档...");
                                File.Delete(Path.Combine(FileHelper.GetLocalPath(), PrintPhTableName));

                                //不删二维码文件 by wei.chen 2021年7月6日16:26:41
                                //if (File.Exists(replacePic))
                                //    File.Delete(replacePic);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            Log.Instance.WriteError("删除打印派号单发生异常：" + ex.Message);
                            Log.Instance.WriteInfo("删除打印派号单发生异常：" + ex.Message);
                            //不直接把异常往上抛 by wei.chen 2021年7月6日16:26:41
                            //throw;
                        }


                        //武汉二维码为 messageinfo返回的二维码  
                        //其他地区为id生成的二维码
                        if (SendInfo.MessageInfo.IsNotEmpty() && SendInfo.QY_CODE?.Substring(0, 2) == "42")
                        {
                            if (!CreateQrCode(SendInfo.MessageInfo, replacePic))
                            {
                                msg = id.ToDecimal(0).ToString() + "生成二维码发生异常！";
                                replacePic = Path.Combine(FileHelper.GetLocalPath(), "ewm.png"); //生成失败则打印公众号
                                Log.Instance.WriteInfo(msg);
                            }
                            else
                                replacePic = Path.Combine(FileHelper.GetLocalPath(), "phQr.png");

                        }
                        else
                        {
                            if (!CreateQrCode(id.ToDecimal(0).ToString(), replacePic))
                            {
                                msg = id.ToDecimal(0).ToString() + "生成二维码发生异常！";
                                replacePic = Path.Combine(FileHelper.GetLocalPath(), "ewm.png"); //生成失败则打印公众号
                                Log.Instance.WriteInfo(msg);
                            }
                            else
                                replacePic = Path.Combine(FileHelper.GetLocalPath(), "phQr.png");
                        }


                        string bookmarkName = string.Empty;
                        string DeptName = name;
                        string WaitTips = "过号作废，请留意屏幕叫号!!!";
                        string GuidTipMsg = SendInfo.PRINT_GUIDE?.ToString();

                        foreach (Aspose.Words.Bookmark bookmark in awPrintDoc.Range.Bookmarks)
                        {
                            try
                            {
                                bookmarkName = bookmark.Name;
                                switch (bookmark.Name)
                                {
                                    case "Title":
                                        bookmark.Text = SendInfo.ZWXM + "  " + SendInfo.PREFIX + SendInfo.PH_NUMBER;
                                        break;
                                    case "GuidTipMsg":
                                        bookmark.Text = GuidTipMsg;
                                        break; // "请您移步到A区受理区域等候办理！";
                                    case "DeptName":
                                        bookmark.Text = DeptName;
                                        break;
                                    case "MsgTip":
                                        bookmark.Text = WaitTips;
                                        break;
                                    case "lblQhTimeStr":
                                        bookmark.Text = SendInfo.PH_TIME?.ToString();
                                        break;
                                    case "BookingDateTime":
                                        bookmark.Text = SendInfo.YY_TIME?.ToString();
                                        break;
                                    case "lblYyTimeStr":
                                        bookmark.Text = !string.IsNullOrEmpty(SendInfo.YY_TIME) ? SendInfo.YY_TIME : "";
                                        break;
                                    case "Describe":
                                        bookmark.Text =
                                            $"{QJTConfig.QJTModel?.QJTDevInfo?.DEV_NAME}-";//{SendInfo.PH_TIME}
                                        break;
                                    case "BookingType":
                                        bookmark.Text = model.StrCardTypes;
                                        break;
                                    case "WaitNums":
                                        bookmark.Text = SendInfo.PH_WAITNUMS.ToString();
                                        break;
                                    case "lblIDCardNo":
                                        if (IsShanXi)
                                            bookmark.Text = "身份证号：";
                                        break;
                                    case "IDCardNo":
                                        if (IsShanXi)
                                        {
                                            if (!string.IsNullOrEmpty(SendInfo.SFZH))
                                            {
                                                string idNumber = ReplaceWithSpecialChar(SendInfo.SFZH);
                                                bookmark.Text = idNumber;
                                            }
                                        }
                                        break;
                                    case "QRCode":
                                        bookmark.Text = "";
                                        if (File.Exists(replacePic))
                                        {
                                            if (!IsShanXi && !IsGuangZhou)
                                            {
                                                DocumentBuilder builder = new DocumentBuilder(awPrintDoc);
                                                Aspose.Words.Drawing.Shape shape =
                                                    new Aspose.Words.Drawing.Shape(awPrintDoc, ShapeType.Image);
                                                shape.ImageData.SetImage(replacePic);
                                                shape.Width = 80;
                                                shape.Height = 80;
                                                shape.HorizontalAlignment = Aspose.Words.Drawing.HorizontalAlignment.Center;
                                                builder.MoveToBookmark("QRCode");
                                                builder.InsertNode(shape);
                                            }

                                        }
                                        break;
                                    default: break;
                                }
                            }
                            catch (Exception ex)
                            {
                                msg = "打印设置 【" + bookmarkName + "】书签时发生异常：" + ex.Message;
                                Log.Instance.WriteError(msg);
                                Log.Instance.WriteInfo(msg);
                            }
                        }


                        awPrintDoc.Save(printPath);
                        //打印设置
                        if (File.Exists(printPath))
                        {
                            msg = "准备开始打印 【" + printPath + "】 派号单。";
                            Log.Instance.WriteInfo(msg);
                            if (print(printPath))
                            {
                                Log.Instance.WriteInfo("打印派号单时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                msg = "成功打印 【" + PrintPhTableName + "】 派号单。";
                                Log.Instance.WriteInfo(msg);
                                msg = string.Empty;
                            }
                            else
                            {
                                msg = " 打印派号单失败。";
                                Log.Instance.WriteInfo(msg);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = "打印word派号单发生异常：" + ex.Message;
                Log.Instance.WriteInfo(msg);
                Log.Instance.WriteError(msg);

                //上报故障：预约数据提交失败
                //DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault21;
                //DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_20000005);
                //DevAlarm.AlarmInfo = "打印派号单回执发生异常：" + msg;//返回信息
                //ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm?.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_20000005).ToString());
            }
            finally
            {
                //将申请表统一保存到服务器后删除，暂时不做。
                if (File.Exists(printPath))
                {
                    //File.Delete(Application.StartupPath + "\\" + PrintPhTableName);
                    if (File.Exists(replacePic))
                    {
                        //先不做强制回收 by 2021年7月6日16:47:12 wei.chen
                        //killWinWordProcess();
                        File.Delete(replacePic);
                    }

                    if (msg.IsEmpty())
                        msg = "打印word派号单完成，删除打印word派号单：" + PrintPhTableName;
                    Log.Instance.WriteInfo(msg);
                    msg = string.Empty;
                }

                Log.Instance.WriteInfo("\n**********************************结束【打印派号单】界面**********************************");
                //先不做强制回收 by 2021年7月6日16:47:12 wei.chen
                //Thread.Sleep(500);
                //GC.Collect();
            }

            return false;
        }


        public static async Task<bool> WordPrintNew(string sendNo, string id, string msg, string name, int waitNums, BookingModel model, JsonPH_SendOut SendInfo)
        {

            //获取计算机所有打印机

            string replacePic = string.Empty;
            string PhTableName = "";
            if (!string.IsNullOrEmpty(QJTConfig.QJTModel.QJTDevInfo.DeptInfo.Remark))
            {
                PhTableName = QJTConfig.QJTModel.QJTDevInfo.DeptInfo.Remark;
                //PhTableName = "phTableWH.doc"; //QJTConfig.QJTModel.QJTDevInfo.DeptInfo.Remark;
            }
            string PrintPhTableName = "phTableBakup.doc";
            string docPath = Path.Combine(FileHelper.GetLocalPath(), PhTableName);
            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    if (!File.Exists(docPath))
                    {
                        PhTableName = "phTable.doc";
                        docPath = Path.Combine(FileHelper.GetLocalPath(), PhTableName);
                    }
                    if (File.Exists(docPath))
                    {
                        Aspose.Words.Document awPrintDoc = new Aspose.Words.Document(docPath);
                        Log.Instance.WriteInfo("身份证号为【" + sendNo + " - " + name?.ToString() + "】，开始打印派号单。" + docPath);
                        if (File.Exists(docPath))
                        {
                            //生产二维码图片 
                            replacePic = Path.Combine(FileHelper.GetLocalPath(), "phQr.png");
                            Log.Instance.WriteInfo("二维码：" + replacePic);
                            //将申请表统一保存到服务器后删除。
                            if (File.Exists(Path.Combine(FileHelper.GetLocalPath(), PrintPhTableName)))
                            {
                                try
                                {
                                    //Log.Instance.WriteInfo("开始删除历史打印文档，生成新文档...");
                                    File.Delete(Path.Combine(FileHelper.GetLocalPath(), PrintPhTableName));
                                    if (File.Exists(replacePic))
                                        File.Delete(replacePic);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                    throw;
                                }

                            }
                            //武汉二维码为 messageinfo返回的二维码  
                            //其他地区为id生成的二维码
                            if (SendInfo.MessageInfo.IsNotEmpty() && SendInfo.QY_CODE.Substring(0, 2) == "42")
                            {
                                if (!CreateQrCode(SendInfo.MessageInfo, replacePic))
                                {
                                    msg = id.ToDecimal(0).ToString() + "生成二维码发生异常！";
                                    replacePic = Path.Combine(FileHelper.GetLocalPath(), "ewm.png"); //生成失败则打印公众号
                                    Log.Instance.WriteInfo(msg);
                                }
                                else
                                    replacePic = Path.Combine(FileHelper.GetLocalPath(), "phQr.png");

                            }
                            else
                            {
                                if (!CreateQrCode(id.ToDecimal(0).ToString(), replacePic))
                                {
                                    msg = id.ToDecimal(0).ToString() + "生成二维码发生异常！";
                                    replacePic = Path.Combine(FileHelper.GetLocalPath(), "ewm.png"); //生成失败则打印公众号
                                    Log.Instance.WriteInfo(msg);
                                }
                                else
                                    replacePic = Path.Combine(FileHelper.GetLocalPath(), "phQr.png");
                            }


                            string bookmarkName = string.Empty;
                            string DeptName = name;
                            string WaitTips = "过号作废，请留意屏幕叫号!!!";
                            string GuidTipMsg = SendInfo.PRINT_GUIDE?.ToString();

                            foreach (Aspose.Words.Bookmark bookmark in awPrintDoc.Range.Bookmarks)
                            {
                                try
                                {
                                    bookmarkName = bookmark.Name;
                                    switch (bookmark.Name)
                                    {
                                        case "Title":
                                            bookmark.Text = SendInfo.ZWXM + "  " + SendInfo.PREFIX + SendInfo.PH_NUMBER;
                                            break;
                                        case "GuidTipMsg":
                                            bookmark.Text = GuidTipMsg;
                                            break; // "请您移步到A区受理区域等候办理！";
                                        case "DeptName":
                                            bookmark.Text = DeptName;
                                            break;
                                        case "MsgTip":
                                            bookmark.Text = WaitTips;
                                            break;
                                        case "lblQhTimeStr":
                                            bookmark.Text = SendInfo.PH_TIME?.ToString();
                                            break;
                                        case "BookingDateTime":
                                            bookmark.Text = SendInfo.YY_TIME?.ToString();
                                            break;
                                        case "lblYyTimeStr":
                                            bookmark.Text = !string.IsNullOrEmpty(SendInfo.YY_TIME) ? SendInfo.YY_TIME : "";
                                            break;
                                        case "Describe":
                                            bookmark.Text =
                                                $"{QJTConfig.QJTModel?.QJTDevInfo?.DEV_NAME}-";//{SendInfo.PH_TIME}
                                            break;
                                        case "BookingType":
                                            bookmark.Text = model.StrCardTypes;
                                            break;
                                        case "WaitNums":
                                            bookmark.Text = SendInfo.PH_WAITNUMS.ToString();
                                            break;
                                        case "lblIDCardNo":
                                            if (IsShanXi)
                                                bookmark.Text = "身份证号：";
                                            break;
                                        case "IDCardNo":
                                            if (IsShanXi)
                                            {
                                                if (!string.IsNullOrEmpty(SendInfo.SFZH))
                                                {
                                                    string idNumber = ReplaceWithSpecialChar(SendInfo.SFZH);
                                                    bookmark.Text = idNumber;
                                                }
                                            }
                                            break;
                                        case "QRCode":
                                            bookmark.Text = "";
                                            if (File.Exists(replacePic))
                                            {
                                                if (!IsShanXi && !IsGuangZhou)
                                                {
                                                    DocumentBuilder builder = new DocumentBuilder(awPrintDoc);
                                                    Aspose.Words.Drawing.Shape shape =
                                                        new Aspose.Words.Drawing.Shape(awPrintDoc, ShapeType.Image);
                                                    shape.ImageData.SetImage(replacePic);
                                                    shape.Width = 80;
                                                    shape.Height = 80;
                                                    shape.HorizontalAlignment = Aspose.Words.Drawing.HorizontalAlignment.Center;
                                                    builder.MoveToBookmark("QRCode");
                                                    builder.InsertNode(shape);
                                                }

                                            }
                                            break;
                                        default: break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    msg = "打印设置 【" + bookmarkName + "】书签时发生异常：" + ex.Message;
                                    Log.Instance.WriteError(msg);
                                    Log.Instance.WriteInfo(msg);
                                }
                            }

                            awPrintDoc.Save(Path.Combine(FileHelper.GetLocalPath(), PrintPhTableName));
                            //打印设置
                            if (File.Exists(Path.Combine(FileHelper.GetLocalPath(), PrintPhTableName)) &&
                        print(Path.Combine(FileHelper.GetLocalPath(), PrintPhTableName)))
                            {

                                Log.Instance.WriteInfo("打印派号单时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                msg = "成功打印 【" + PrintPhTableName + "】 申请表。";
                                Log.Instance.WriteInfo(msg);
                                msg = string.Empty;
                            }

                        }
                    }
                });
            }
            catch (Exception ex)
            {
                msg = "打印word派号单发生异常：" + ex.Message;
                Log.Instance.WriteInfo(msg);
                Log.Instance.WriteError(msg);

                //上报故障：预约数据提交失败
                //DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault21;
                //DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_20000005);
                //DevAlarm.AlarmInfo = "打印派号单回执发生异常：" + msg;//返回信息
                //ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm?.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_20000005).ToString());
            }
            finally
            {

                //将申请表统一保存到服务器后删除，暂时不做。
                if (File.Exists(Path.Combine(FileHelper.GetLocalPath(), PrintPhTableName)))
                {
                    //File.Delete(Application.StartupPath + "\\" + PrintPhTableName);
                    if (File.Exists(replacePic))
                    {
                        killWinWordProcess();
                        File.Delete(replacePic);
                    }

                    if (msg.IsEmpty())
                        msg = "打印word派号单完成，删除打印word派号单：" + PrintPhTableName;
                    Log.Instance.WriteInfo(msg);
                    msg = string.Empty;
                }
                GC.Collect();
            }

            return false;
        }


        //}

        /// <summary>
        /// 将传入的字符串中间部分字符替换成特殊字符
        /// </summary>
        /// <param name="value">需要替换的字符串</param>
        /// <param name="startLen">前保留长度</param>
        /// <param name="endLen">尾保留长度</param>
        /// <param name="specialChar">替换的字符</param>
        /// <returns>被特殊字符替换的字符串</returns>
        public static string ReplaceWithSpecialChar(string value, int startLen = 4, int endLen = 4, char specialChar = '*')
        {
            try
            {

                int lenth = value.Length - startLen - endLen;

                if (lenth <= 0)
                {
                    string start = value.Substring(0, 1);
                    string end = value.Substring(1, 1);
                    value = start + "*" + end;
                }
                else
                {
                    string replaceStr = value.Substring(startLen, lenth);
                    string specialStr = string.Empty;
                    for (int i = 0; i < replaceStr.Length; i++)
                    {
                        specialStr += specialChar;
                    }
                    value = value.Replace(replaceStr, specialStr);
                }

            }
            catch (Exception)
            {
                throw;
            }

            return value;
        }

        /// <summary>
        /// 关闭所有进程,会关闭Application
        /// </summary>
        public static void killWinWordProcess()
        {
            try
            {
                Log.Instance.WriteInfo("开始检查是否存在未关闭打印文档...");
                //获取所有的word进程
                System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName("WINWORD");
                if (processes.Length > 0)
                {
                    foreach (System.Diagnostics.Process process in processes)
                    {
                        if (process.MainWindowTitle == "")
                        {
                            Log.Instance.WriteInfo("关闭未打印文档成功！");
                            process.Kill();

                            //程序休息0.5秒，等待进程关闭
                            System.Threading.Thread.Sleep(500);
                        }

                        GC.Collect();
                    }
                }

                Process[] processeswps = System.Diagnostics.Process.GetProcessesByName("WPS");
                if (processeswps.Length > 0)
                {
                    foreach (System.Diagnostics.Process process in processeswps)
                    {
                        if (process.MainWindowTitle == "")
                        {
                            Log.Instance.WriteInfo("关闭未打印文档成功！");
                            process.Kill();

                            //程序休息0.5秒，等待进程关闭
                            System.Threading.Thread.Sleep(500);
                        }
                        GC.Collect();
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("关闭word进程失败：" + ex);
                Log.Instance.WriteError("关闭word进程失败：" + ex);
            }
        }
    }
}
