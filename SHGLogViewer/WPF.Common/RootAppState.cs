using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF.Common
{
    [Export]
    public class RootAppStateModel : EditableRuleValidate<RootAppStateModel>
    {
        //public event Action OnChange;


        
        private string _busyContent;
        protected bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    RaisePropertyChanged(ReflectionUtility.GetPropertyName(() => IsBusy));
                }
            }
        }


        public string BusyContent
        {
            get { return _busyContent; }
            set
            {
                if (_busyContent != value)
                {
                    _busyContent = value;
                    RaisePropertyChanged(ReflectionUtility.GetPropertyName(() => BusyContent));
                }

            }
        }



        public RootAppStateModel()
        {
            BusyContent = "Loading";
        }
    }
}
