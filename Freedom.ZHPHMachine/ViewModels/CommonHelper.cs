using Freedom.Common.HsZhPjh.Enums;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.View;
using Freedom.ZHPHMachine.View.Popup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Freedom.Common;

namespace Freedom.ZHPHMachine.ViewModels
{
    public class CommonHelper
    {
        static NumberKeyboard win;
        static SelectOpera winSelect;
        static SelectProvince winSelectProvince;
        static NumKeyboard winNum;
        static HandwritingInput _winHand;
        static SelectGA winSelectGA;
        static KeyBorderWindow winKeyBorder;
        private static SelectDictionary winSelectDictionary;
        private static SelectDic winSelectDic;
        /// <summary>
        /// 弹出层数字键盘输入
        /// </summary>
        /// <param name="type"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool PopupNumberKeyboard(KeyboardType type, string content, out string str, string title = null)
        {
            bool result = false;
            str = string.Empty;
            win = new NumberKeyboard(type)
            {
                Owner = App.Current.MainWindow
            };
            var viewModel = win.ViewModel;
            viewModel.Title = title;
            viewModel.Content = content;
            if (win.ShowDialog() == true)
            {
                str = win.ViewModel.Content;
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 弹出层纯数字键盘
        /// </summary>
        /// <param name="content"></param>
        /// <param name="str"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static bool PopupNumKeyboard(string title, string content, out string str)
        {
            bool result = false;
            str = string.Empty;

            if (title == "邮政编码")
            {
                winNum = new NumKeyboard(KeyboardType.EMSCode, title)
                {
                    Owner = App.Current.MainWindow
                };
            }
            else
            {
                winNum = new NumKeyboard(KeyboardType.TelePhone, title)
                {
                    Owner = App.Current.MainWindow
                };
            }
           
            winNum.ViewModel.Content = content;
            if (winNum.ShowDialog() == true)
            {
                str = winNum.ViewModel.Content;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 弹出层选择日期
        /// </summary>
        /// <param name="type"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool PopupDate(OperaType type, string key, out DateTime str)
        {
            bool result = false;
            str = DateTime.Now;
            App.Current.MainWindow.IsHitTestVisible = false;
            winSelect = new SelectOpera(type, key)
            {
                Owner = App.Current.MainWindow
            };
            if (winSelect.ShowDialog() == true)
            {
                var item = (Tuple<DateTime?, bool, string>)winSelect.ViewModel.Item;
                str = item?.Item1 ?? DateTime.Now;
                result = true;
            }
            App.Current.MainWindow.IsHitTestVisible = true;
            return result;
        }

        /// <summary>
        /// 弹出层选择字典信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="model"></param>
        /// <param name="code"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static bool PopupDictionary(OperaType type, string key, out DictionaryType model, string code = null, int status = 1)
        {
            bool result = false;
            model = null;
            winSelect = new SelectOpera(type, key, code, status)
            {
                Owner = App.Current.MainWindow
            };

            if (winSelect.ShowDialog() == true)
            {
                model = (DictionaryType)winSelect.ViewModel?.Item;
                //Verification = model.Description;
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 弹出层选择户口所在地
        /// </summary>
        /// <param name="title"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool PopupProvince(string title, string code, out DictionaryType model, out string HasAddress)
        {
            bool result = false;
            model = null;
            HasAddress = "";
            winSelectProvince = new SelectProvince(title, code)
            {
                Owner = App.Current.MainWindow
            };

            if (winSelectProvince.ShowDialog() == true)
            {
                model = winSelectProvince.ViewModel.Result;
                HasAddress = winSelectProvince.ViewModel.HasAddress;
                result = true;
            }
            return result;
        }

        public static bool PopupHandwritingInput(string title, string content, out string strContent, int maxLength = 15)
        {
            strContent = string.Empty;
            //winHand = new HandwritingInput()
            //{
            //    Owner = App.Current.MainWindow
            //};
            //winHand.Title = title;
            //winHand.MaxLength = maxLength;
            //winHand.StrContent = content;
            //if (winHand.ShowDialog() == true)
            //{
            //    strContent = winHand.StrContent;
            //    return true;
            //}
            //return false;
            winKeyBorder = new KeyBorderWindow()
            {
                Owner = App.Current.MainWindow
            };
            winKeyBorder.oldTextInfo = content;
            winKeyBorder.topTitle = title;
            winKeyBorder.strTextName = title;
            winKeyBorder.InitKeyBorderWindow();
            if (winKeyBorder.ShowDialog() == true)
            {
                strContent = winKeyBorder.KeyBorderContent;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 港澳签注选择
        /// </summary>
        /// <param name="param">item1:前往地 item2:香港签注种类 item3:香港签注次数 item4:澳门签注种类 item5:澳门签注次数</param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool PopupSelectGA(Tuple<string, string, string, string, string> param, out SelectGAViewModels model)
        {
            model = null;
           
            winSelectGA = new SelectGA()
            {
                Owner = App.Current.MainWindow
            };
           
            var viewModel = winSelectGA.ViewModel;
            viewModel.QWDSelected = param.Item1 == ((int)EnumTypeQWD.HKMC).ToString() ? EnumTypeQWD.HKMC.ToString() : param.Item1;

           // Log.Instance.WriteInfo("前往地：" + viewModel?.QWDSelected);
            viewModel.HKQZType = viewModel.HKQZTypes.FirstOrDefault(t => t.Code == param.Item2);
            viewModel.MACQZType = viewModel.MACQZTypes.FirstOrDefault(t => t.Code == param.Item4);
            if (viewModel.HKQZType != null && viewModel.MACQZType != null)
            {
                viewModel.HKQZCount = viewModel.HKQZCounts.FirstOrDefault(t => t.Code == param.Item3);
                viewModel.MACQZType = viewModel.MACQZTypes.FirstOrDefault(t => t.Code == param.Item4);
                viewModel.MACQZCount = viewModel.MACQZCounts.FirstOrDefault(t => t.Code == param.Item5);
                
            }


            if (winSelectGA.ShowDialog() == true)
            {
                model = viewModel;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 弹出层选择字典带翻页
        /// </summary>
        /// <param name="key"></param>
        /// <param name="model"></param>
        /// <param name="type"></param>
        /// <param name="code"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static bool PopupDictionaryByPage(OperaType type, string key, out DictionaryType model, string code = null, int status = 1)
        {
            bool result = false;
            model = null;
            winSelectDictionary = new SelectDictionary(type, key, code, status)
            {
                Owner = App.Current.MainWindow
            };

            if (winSelectDictionary.ShowDialog() == true)
            {
                model = (DictionaryType)winSelectDictionary.ViewModel?.Item;
                //Verification = model.Description;
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 弹出层选择字典带翻页带搜索功能
        /// </summary>
        /// <param name="key"></param>
        /// <param name="model"></param>
        /// <param name="type"></param>
        /// <param name="code"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static bool PopupDicByPage(OperaType type, string key, out DictionaryTypeByPinYin model, string code = null, int status = 1)
        {
            bool result = false;
            model = null;
            winSelectDic = new SelectDic(type, key, code, status)
            {
                Owner = App.Current.MainWindow
            };

            if (winSelectDic.ShowDialog() == true)
            {
                model = (DictionaryTypeByPinYin)winSelectDic.ViewModel?.Item;
                //Verification = model.Description;
                result = true;
            }
            return result;
        }


        public static void OnDispose()
        {
            if (win != null) win.Close();
            if (winSelect != null) winSelect.Close();
            if (winSelectProvince != null) winSelectProvince.Close();
            if (winNum != null) winNum.Close();
            if (_winHand != null) _winHand.Close();
            if (winSelectGA != null) winSelectGA.Close();
            if (winKeyBorder != null) winKeyBorder.Close();
            if (winSelectDictionary != null) winSelectDictionary.Close();
            if (winSelectDic != null) winSelectDic.Close();
        }
    }
}
