using Freedom.Controls.Foundation;
using Freedom.Models;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.Common;
using MachineCommandService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freedom.ZHPHMachine.ViewModels
{
    public class SelectProvinceViewModels : ObservableObject
    {
        #region 构造函数
        public SelectProvinceViewModels(string title)
        {
            ProvinceLst = ServiceRegistry.Instance.Get<ElementManager>().Get<List<ProvinceList>>();  
            Title = title;
        }
        #endregion

        #region 属性
        public List<ProvinceList> provinceLst;

        /// <summary>
        /// 省份集合
        /// </summary>
        public List<ProvinceList> ProvinceLst
        {
            get { return provinceLst; }
            set { provinceLst = value; RaisePropertyChanged("ProvinceLst"); }
        }

        private string title;
        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged("Title"); }
        }

        private ProvinceList selectProvince;
        /// <summary>
        /// 选择省份
        /// </summary>
        public ProvinceList SelectProvince
        {
            get { return selectProvince; }
            set { selectProvince = value; RaisePropertyChanged("SelectProvince"); }
        }

        private CityList selectCity;
        /// <summary>
        /// 选择市
        /// </summary>
        public CityList SelectCity
        {
            get { return selectCity; }
            set { selectCity = value; RaisePropertyChanged("SelectCity"); }
        }

        private PH_XTZD_TB selectArea;
        /// <summary>
        /// 选择区
        /// </summary>
        public PH_XTZD_TB SelectArea
        {
            get { return selectArea; }
            set { selectArea = value; RaisePropertyChanged("SelectArea"); }
        }

        public string hasAddress;
        public string HasAddress
        {
            get { return hasAddress; }
            set { hasAddress = value; RaisePropertyChanged("HasAddress"); }
        }

        /// <summary>
        /// 省市区
        /// </summary>
        public DictionaryType Result
        {
            get
            {
                string str = string.Empty;
                string code = string.Empty;
                if (selectProvince != null)
                {
                    str += selectProvince.DESCRIPTION;
                    code = selectProvince.CODE;
                }
                if (selectCity != null)
                {
                    str += selectCity.DESCRIPTION;
                    code = selectCity.CODE;
                }
                if (selectArea != null)
                {
                    str += selectArea.DESCRIPTION;
                    code = selectArea.CODE;
                }
                return new DictionaryType()
                {
                    Code = code,
                    Description = str
                };
            }
        }
        #endregion

        #region 方法
        public void DateInit(string code)
        {
            SelectProvince = provinceLst?.FirstOrDefault(t => t?.CityList?.FirstOrDefault(k => k.CountryList?.FirstOrDefault(z => z.CODE == code) != null) != null);
            SelectCity = selectProvince?.CityList?.FirstOrDefault(t => t.CountryList?.FirstOrDefault(k => k.CODE == code) != null);
            SelectArea = selectCity?.CountryList?.FirstOrDefault(t => t.CODE == code);
        }
        #endregion

    }
}
