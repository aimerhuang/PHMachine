using Freedom.Common;
using Freedom.Common.HsZhPjh.Enums;
using Freedom.Controls.Foundation;
using Freedom.Models;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.Common;
using MachineCommandService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Freedom.Models.TJJsonModels;

namespace Freedom.ZHPHMachine.ViewModels
{
    public class SelectOperaViewModels : ViewModelBase
    {
        public SelectOperaViewModels(OperaType type, string key, string code = null, int status = 1)
        {
            Title = Freedom.Common.HsZhPjh.Enums.EnumType.GetEnumDescription(type);
            Query(type, key, code, status);
        }

        #region 字段
        /// <summary>
        /// 当前页
        /// </summary>
        private int currentPage = 1;

        /// <summary>
        /// 每页显示条数
        /// </summary>
        private int pageSize = 7;

        /// <summary>
        /// 全部数据
        /// </summary>
        private Dictionary<string, object> listAll;

        /// <summary>
        /// 总页数
        /// </summary>
        private int allPage = 0;

        #endregion

        #region 属性  
        private string title;

        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged("Title"); }
        }
        public Dictionary<string, object> itemsInfo;

        /// <summary>
        /// 集合
        /// </summary>
        public Dictionary<string, object> ItemsInfo
        {
            get { return itemsInfo; }
            set { itemsInfo = value; RaisePropertyChanged("ItemsInfo"); }
        }

        private object item;

        /// <summary>
        /// 选中项
        /// </summary>
        public object Item
        {
            get { return item; }
            set { item = value; RaisePropertyChanged("Item"); }
        }

        private bool nextShow;

        /// <summary>
        /// 是否显示下一页按钮
        /// </summary>
        public bool NextShow
        {
            get { return nextShow; }
            set { nextShow = value; RaisePropertyChanged("NextShow"); }
        }

        private bool previousShow;

        /// <summary>
        /// 是否显示上一页按钮
        /// </summary>
        public bool PreviousShow
        {
            get { return previousShow; }
            set { previousShow = value; RaisePropertyChanged("PreviousShow"); }
        }

        private bool nextEnabled = true;

        /// <summary>
        /// 下一页是否可用
        /// </summary>
        public bool NextEnabled
        {
            get { return nextEnabled; }
            set { nextEnabled = value; RaisePropertyChanged("NextEnabled"); }
        }

        private bool previousEnabled = true;

        /// <summary>
        /// 上一页是否可用
        /// </summary>
        public bool PreviousEnabled
        {
            get { return previousEnabled; }
            set { previousEnabled = value; RaisePropertyChanged("PreviousEnabled"); }
        }
        #endregion

        #region 方法

        /// <summary>
        /// 日期选择(只能选择七天)
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<string, object>> GetDateAsync()
        {
            try
            {
                var result = await ZHPHMachineWSHelper.ZHPHInstance.S_BookingZB();
                if (result != null)
                {
                    var lstResult = result.GroupBy(t => t.BookingDt).
                   ToDictionary(t => t.Key?.ToString("yyyy年MM月dd日"), t => (object)new Tuple<DateTime?, bool, string>(t.Key, t.Sum(y => y.IsEnable ? 1 : 0) > 0,
                   t.Sum(y => y.IsEnable ? 1 : 0) > 0 ? string.Empty : "已满"));
                    return lstResult;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("弹出层出境事由选择报错");
                Log.Instance.WriteError("弹出层出境事由选择报错" + ex.Message);

            }
            return null;
        }

        /// <summary>
        /// 字典类型查询
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetDictionaryAsync(KindType type, int status = 1, string code = null)
        {

            var config = ServiceRegistry.Instance.Get<ElementManager>();
            List<DictionaryType> lst = new List<DictionaryType>();
            if (type == KindType.ApplyCategory)
            {
                //获取护照申请类型
                if (status == 1) lst = config.Get<List<DictionaryType>>("ApplyCategoryHZ");
                //获取台湾申请类型
                else if (status == 3) lst = config.Get<List<DictionaryType>>("ApplyCategoryTWN");
                //获取港澳申请类型
                else lst = config.Get<List<DictionaryType>>("ApplyCategoryGA");

                if (OwnerViewModel?.KbywInfos?.Length > 0)
                {
                    //根据证件获取对应办证类别
                    var applyType = "";
                    switch (status)
                    {
                        case 1:
                            applyType = "101";
                            break;
                        case 2:
                            applyType = "102";
                            break;
                        case 3:
                            applyType = "104";
                            break;
                        default:
                            break;
                    }

                    List<DictionaryType> cardList = new List<DictionaryType>();
                    //太极可办业务
                    List<KbywInfo> cardListByKbywInfos = new List<KbywInfo>();
                    //指定的 可办的 申请类型
                    foreach (var dictionaryType in lst)
                    {
                        var dicCode = dictionaryType.Code;
                        var carInfos = OwnerViewModel.KbywInfos.Where(t => t.bzlb.Contains(dicCode) && t.sqlb == applyType)?.ToList();
                        if (carInfos.Count > 0)
                        {
                            cardListByKbywInfos.Add(carInfos.FirstOrDefault());
                        }
                        
                    }
                    foreach (var kbywInfo in cardListByKbywInfos)
                    {
                        var bzlb = kbywInfo.bzlb;
                        if (kbywInfo?.sqlb == "101" && bzlb == "21") continue;
                        var kbtype = lst.Where(t => t.Code == bzlb).ToList().FirstOrDefault();

                        cardList.Add(kbtype);

                    }

                    lst = cardList;
                }
            }
            else if (type == KindType.QZCount)
            {
                if (Enum.IsDefined(typeof(EnumTypeQWD), status))
                {
                    var item = config.Get<List<QZZLList>>()?.FirstOrDefault(t => t.CODE == code);
                    lst = item?.QZLXList?.Where(t => t.QWD == ((EnumTypeQWD)status).ToString() && t.STATUS == "1")?.Select(t => new DictionaryType()
                    {
                        Description = t.DESCRIPTION,
                        Code = t.CODE
                    }).ToList();

                }
            }
            else
            {
                lst = config.Get<List<DictionaryType>>();
                lst = lst?.Where(t => t.KindType == ((int)type).ToString() && t.Status == status)?.ToList();
            }
            return lst?.ToDictionary(t => t.Description, t => (object)t);
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="code"></param>
        /// <param name="status"></param>
        private async void Query(OperaType type, string key, string code = null, int status = 1)
        {
            try
            {
                switch ((int)type)
                {
                    case (int)OperaType.Date:
                        listAll = await GetDateAsync();
                        break;
                    case (int)OperaType.ApplyType:
                        listAll = GetDictionaryAsync(KindType.ApplyCategory, status);
                        break;
                    case (int)OperaType.TW:
                        listAll = GetDictionaryAsync(KindType.QZType, 2);
                        break;
                    case (int)OperaType.GA:
                        listAll = GetDictionaryAsync(KindType.QZType, 1);
                        break;
                    case (int)OperaType.QZCount:
                        listAll = GetDictionaryAsync(KindType.QZCount, status, code);
                        break;
                    case (int)OperaType.RelationType:
                        listAll = GetDictionaryAsync(KindType.RelationType, status);
                        break;
                    case (int)OperaType.Job:
                        listAll = GetDictionaryAsync(KindType.Job, status);
                        break;
                    case (int)OperaType.DepartureReason:
                        listAll = GetDictionaryAsync(KindType.DepartureReason, status);
                        break;
                    case (int)OperaType.Destination:
                        listAll = GetDictionaryAsync(KindType.Destination, status);
                        break;
                }
                //设置分页基础数据
                if (listAll != null)
                {
                    allPage = (int)Math.Ceiling((double)listAll.Count / pageSize);
                    PageQuery(currentPage, pageSize);
                    if (allPage > 1)
                    {
                        NextShow = true;
                        PreviousShow = true;
                        PreviousEnabled = currentPage != 1;
                    }
                }
                DataInit(key);
            }
            catch (Exception ex)
            {
                Freedom.Common.Log.Instance.WriteError($"查询字典{(int)type}报错");
                Freedom.Common.Log.Instance.WriteError($"查询字典{(int)type}异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pageSize"></param>
        public void PageQuery(int index, int pageSize)
        {
            try
            {
                if (listAll == null) { ItemsInfo = null; return; }
                if (index > allPage && index < 1) { return; }
                var result = listAll.Skip(pageSize * (index - 1)).Take(pageSize).AsEnumerable();
                itemsInfo = result.ToDictionary(t => t.Key, t => t.Value);
                var selectItem = itemsInfo?.FirstOrDefault(t => t.Value.Equals(item)).Value;
                if (selectItem != null)
                {
                    Item = selectItem;
                }
                ItemsInfo = itemsInfo;

                if (selectItem != null)
                {
                    Item = selectItem;
                }

            }
            catch (Exception ex)
            {

            }

        }
        /// <summary>
        /// 下一页
        /// </summary>
        /// <param name="obj"></param>
        public override void DoNextFunction(object obj)
        {
            if (currentPage < allPage)
            {
                currentPage++;
                PageQuery(currentPage, pageSize);
            }
            NextEnabled = currentPage != allPage;
            PreviousEnabled = currentPage != 1;
        }

        /// <summary>
        /// 上一页
        /// </summary>
        /// <param name="obj"></param>
        public override void DoBackFunction(object obj)
        {
            if (currentPage > 1)
            {
                currentPage--;
                PageQuery(currentPage, pageSize);
            }
            NextEnabled = currentPage != allPage;
            PreviousEnabled = currentPage != 1;
        }

        public void DataInit(string key)
        {
            if (listAll == null || listAll.Count < 1) { return; }
            for (int i = 0; i < listAll.Count; i++)
            {
                if (listAll.ToList()[i].Key == key)
                {
                    currentPage = (int)Math.Ceiling((double)(i + 1) / pageSize);
                    if (currentPage != 1)
                    {
                        PageQuery(currentPage, pageSize);
                    }
                    Item = listAll.ToList()[i].Value;
                    NextEnabled = currentPage != allPage;
                    PreviousEnabled = currentPage != 1;
                    break;
                }
            }

        }
        #endregion
    }

    public enum OperaType
    {
        [Description("选择预约办证日期")]
        Date = 0,//日期
        [Description("办证类别")]
        ApplyType = 1,//办证类别
        [Description("台湾签注种类")]
        TW = 2,
        [Description("香港/澳门签注种类")]
        GA = 3,
        [Description("签注种类")]
        QZCount = 4,
        [Description("与申请人关系")]
        RelationType = 5,
        [Description("职业")]
        Job = 6,
        [Description("出境事由")]
        DepartureReason = 7,
        [Description("前往地")]
        Destination = 8

    }
}
