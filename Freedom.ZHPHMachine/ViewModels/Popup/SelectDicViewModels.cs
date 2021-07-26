using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Freedom.Common;
using Freedom.Controls.Foundation;
using Freedom.Models;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.Command;
using Freedom.ZHPHMachine.Common;
using MachineCommandService;

namespace Freedom.ZHPHMachine.ViewModels.Popup
{
    public class SelectDicViewModels : ViewModelBase
    {
        public SelectDicViewModels(OperaType type, string key, string code = null, int status = 1)
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
        private int pageSize = 32;

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

        private string searchText;
        /// <summary>
        /// 绑定搜索字段
        /// </summary>
        public string SearchText
        {
            get { return searchText; }
            set { searchText = value; RaisePropertyChanged("SearchText"); }
        }

        private string tipsMsg;

        /// <summary>
        /// 提示信息
        /// </summary>
        public string TipsMsg
        {
            get { return tipsMsg; }
            set { tipsMsg = value; RaisePropertyChanged("TipsMsg"); }
        }

        public ICommand TextGotFocusCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (CommonHelper.PopupHandwritingInput("搜索前往地", SearchText, out string str))
                    {
                        Item = null;
                        //小写转大写
                        SearchText = str.ToUpper();
                        //取字典所有数据
                        listAll = GetDictionaryAsync(KindType.Destination, 1);
                        if (!string.IsNullOrEmpty(SearchText))
                        {
                            if (ValidationHelper.IsEnglishChar(SearchText))
                            {
                                //取字典中
                                // 1.包含搜索字符的内容
                                // 2.以搜索字符开始的拼音 的总集合
                                var GetDicPinYin = GetDictionaryWithPinYin(KindType.Destination, 1, SearchText);
                                listAll = GetDicPinYin;
                            }

                            if (ValidationHelper.IsChinaChar(SearchText))
                            {
                                //搜索框输入不为空则搜索
                                if (!string.IsNullOrEmpty(SearchText))
                                {
                                    //中文搜索功能
                                    listAll = listAll.Keys.Where(t => t.Contains(SearchText)).Select(t => listAll[t]).OfType<DictionaryTypeByPinYin>().ToList().ToDictionary(t => t.Description, t => (object)t);

                                    //护照搜索 筛选出 中国 香港/澳门 台湾地区

                                    listAll.Where(t => t.Key.Contains("CHN")).ToList();


                                    var abc = listAll.Where(t => t.Key.Contains("CHN")).ToList();



                                    //中文没有则搜索英文code
                                    if (!listAll.Any())
                                    {

                                        TipsMsg = "未查询到,请重新填写";
                                        return;

                                    }
                                    else
                                    {
                                        TipsMsg = "";
                                    }
                                }
                            }
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
                            else
                            {
                                NextShow = false;
                                PreviousShow = false;
                            }
                        }
                        DataInit(SearchText);
                        //BookingBaseInfo.UrgentName = str;
                    }

                });
            }
        }
        #endregion

        /// <summary>
        /// 根据字段类型查询字典所有数据
        /// </summary>
        /// <param name="type">字典类型</param>
        /// <param name="status">状态</param>
        /// <param name="code"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetDictionaryAsync(KindType type, int status = 1, string code = null)
        {
            var config = ServiceRegistry.Instance.Get<ElementManager>();
            List<DictionaryTypeByPinYin> lst = new List<DictionaryTypeByPinYin>();

            lst = OwnerViewModel.GetDictionaryTypeByPinYin();
            lst = lst?.Where(t => t.KindType == ((int)type).ToString() && t.Status == status)?.ToList();
            return lst?.ToDictionary(t => t.Description, t => (object)t);

        }

        /// <summary>
        /// 搜索英文简写和中文首字母
        /// </summary>
        /// <param name="type"></param>
        /// <param name="status"></param>
        /// <param name="SearchPinYin"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetDictionaryWithPinYin(KindType type, int status, string SearchPinYin)
        {
            if (!string.IsNullOrEmpty(SearchPinYin))
            {
                //英文code模糊搜索
                //例 CHN 中国
                listAll = listAll.Values.OfType<DictionaryTypeByPinYin>().Where(t => t.Code.Contains(SearchPinYin))
                    .ToDictionary(t => t.Description, t => (object)t);

                //中文首字母搜索
                //例 ZG 中国
                List<DictionaryTypeByPinYin> lst = new List<DictionaryTypeByPinYin>();

                lst = OwnerViewModel.GetDictionaryTypeByPinYin();

                lst?.ForEach(t => t.PinYin = Hz2Py.GetTotalPingYin(t.Description).FirstPingYin.FirstOrDefault());


                //护照搜索 筛选出 中国 香港/澳门 台湾地区
                //listAll.Where(t => t.Key.Contains("CHN")).ToList();

                var chn = lst.Where(t => t.Code.Contains("CHN")).ToList();
                var tw = lst.Where(t => t.Code.Contains("CHN")).ToList();
                var xg = lst.Where(t => t.Code.Contains("HKG")).ToList();
                var mac = lst.Where(t => t.Code.Contains("MAC")).ToList();

                var listAllPinYin = lst?.Where(t => t.PinYin.StartsWith(SearchPinYin)).ToDictionary(t => t.Description, t => (object)t);




                if (listAllPinYin != null && listAllPinYin.Any())
                {
                    foreach (var item in listAllPinYin)
                    {
                        if (!listAll.ContainsKey(item.Key))
                            listAll.Add(item.Key, item.Value);
                    }
                }
                //中文没有则搜索英文code
                if (!listAll.Any())
                {
                    Item = null;
                    TipsMsg = "未查询到,请重新填写";

                }
                else
                {
                    TipsMsg = "";
                }
            }


            return listAll;
        }


        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="code"></param>
        /// <param name="status"></param>
        private void Query(OperaType type, string key, string code = null, int status = 1)
        {
            try
            {
                switch ((int)type)
                {

                    //case (int)OperaType.Job:
                    //    listAll = GetDictionaryAsync(KindType.Job, status);
                    //    break;
                    //case (int)OperaType.DepartureReason:
                    //    listAll = GetDictionaryAsync(KindType.DepartureReason, status);
                    //    break;
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
                //搜索
                //var searchResult = itemsInfo.Select(t => t.Key.Equals(searchText));
                //if (searchResult != null)
                //{
                //    searchResult = itemsInfo.Select(t => t.Value.Equals(searchText));
                //}

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

    }
}
