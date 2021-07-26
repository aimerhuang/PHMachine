using Freedom.Common.HsZhPjh.Enums;
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
using System.Windows.Input;
using Freedom.Common;

namespace Freedom.ZHPHMachine.ViewModels
{
    public class SelectGAViewModels : ObservableObject
    {
        #region 字段
        private ElementManager config;
        #endregion

        #region 构造函数
        public SelectGAViewModels()
        {
            config = Common.ServiceRegistry.Instance.Get<ElementManager>();

            //港澳签注种类
            var GAQZTypes = QueryQZTypes(KindType.QZType);
            Log.Instance.WriteInfo("选择港澳签注种类总数为：" + GAQZTypes.Count);
            //香港签注种类
            HKQZTypes = GAQZTypes;
            //澳门签注种类
            MACQZTypes = GAQZTypes;
        }

        #endregion

        #region 属性
        private List<DictionaryType> hkQZTypes;
        /// <summary>
        /// 香港签注申请类型集合
        /// </summary>
        public List<DictionaryType> HKQZTypes
        {
            get { return hkQZTypes; }
            set { hkQZTypes = value; RaisePropertyChanged("HKQZTypes"); }
        }

        private DictionaryType hkQZType;

        /// <summary>
        /// 选中香港签注类型
        /// </summary>
        public DictionaryType HKQZType
        {
            get { return hkQZType; }
            set
            {
                hkQZType = value;
                RaisePropertyChanged("HKQZType");
                //更新香港签注次数集合
                HKQZCounts = QueryQZCounts(EnumTypeQWD.HKG, hkQZType?.Code);
                //更新香港签注次数
                HKQZCount = HKQZCounts?.First();
            }
        }


        private List<DictionaryType> hkQZCounts;
        /// <summary>
        /// 香港签注次数类型集合
        /// </summary>
        public List<DictionaryType> HKQZCounts
        {
            get { return hkQZCounts; }
            set { hkQZCounts = value; RaisePropertyChanged("HKQZCounts"); }
        }

        private DictionaryType hkQZCount;

        /// <summary>
        /// 选中香港签注次数
        /// </summary>
        public DictionaryType HKQZCount
        {
            get { return hkQZCount; }
            set { hkQZCount = value; RaisePropertyChanged("HKQZCount"); }
        }

        private List<DictionaryType> macQZTypes;
        /// <summary>
        /// 澳门签注申请类型集合
        /// </summary>
        public List<DictionaryType> MACQZTypes
        {
            get { return macQZTypes; }
            set { macQZTypes = value; RaisePropertyChanged("MACQZTypes"); }
        }

        private DictionaryType macQZType;

        /// <summary>
        /// 选中澳门签注类型
        /// </summary>
        public DictionaryType MACQZType
        {
            get { return macQZType; }
            set
            {
                macQZType = value;
                RaisePropertyChanged("MACQZType");
                //更新澳门签注次数集合
                MACQZCounts = QueryQZCounts(EnumTypeQWD.MAC, macQZType?.Code);
                //更新澳门签注次数
                MACQZCount = MACQZCounts?.First();
            }
        }

        private List<DictionaryType> macQZCounts;
        /// <summary>
        /// 澳门签注次数类型集合
        /// </summary>
        public List<DictionaryType> MACQZCounts
        {
            get { return macQZCounts; }
            set { macQZCounts = value; RaisePropertyChanged("MACQZCounts"); }
        }

        private DictionaryType macQZCount;

        /// <summary>
        /// 选中澳门签注次数
        /// </summary>
        public DictionaryType MACQZCount
        {
            get { return macQZCount; }
            set { macQZCount = value; RaisePropertyChanged("MACQZCount"); }
        }

        public Dictionary<string, string> QWDList
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    ["HKMC"] = "香港+澳门",
                    ["HKG"] = "香港",
                    ["MAC"] = "澳门",
                };
            }
        }

        private string qwdSelected = EnumTypeQWD.HKMC.ToString();
        public string QWDSelected
        {
            get { return qwdSelected; }
            set { qwdSelected = value; RaisePropertyChanged("QWDSelected"); }
        }
        #endregion

        #region 方法
        private List<DictionaryType> QueryQZTypes(KindType type)
        {
            return config?.Get<List<DictionaryType>>()?.Where(t => t.KindType == ((int)type).ToString() && t.Status == 1)?.ToList();
        }

        private List<DictionaryType> QueryQZCounts(EnumTypeQWD qwd, string code)
        {
            var item = config?.Get<List<QZZLList>>()?.FirstOrDefault(t => t.CODE == code);
            return item?.QZLXList?.Where(t => t.QWD == (qwd).ToString() && t.STATUS == "1")?.Select(t => new DictionaryType()
            {
                Description = t.DESCRIPTION,
                Code = t.CODE
            }).ToList();
        }
        #endregion
    }
}
