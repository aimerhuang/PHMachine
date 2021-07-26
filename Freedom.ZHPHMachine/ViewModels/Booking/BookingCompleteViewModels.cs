using Freedom.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freedom.ZHPHMachine.ViewModels
{
    public class BookingCompleteViewModels : ViewModelBase
    {
        public override void DoInitFunction(object obj)
        {
            OpenTimeOut(10);
        }

        public override void TimeOutCallBackExcuted()
        {
            this.DoExit?.Execute(null);
        } 
         
        public string DWName
        {
            get
            {
                return QJTConfig.QJTModel?.QJTDevInfo?.DeptInfo?.DeptName;
            }
        }
    }
}
