﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF.Common
{
    /// <summary>
    /// This interface is implemented by both the 
    /// <see cref="ValidatingObject">ValidatingObject</see> and the
    /// <see cref="ViewModelBase">ViewModelBase</see> classes, and is used
    /// to expose the list of delegates that are currently listening to the
    /// <see cref="System.ComponentModel.INotifyPropertyChanged">INotifyPropertyChanged</see>
    /// PropertyChanged event. This is done so that the internal 
    /// <see cref="DataWrapper">DataWrapper</see> classes can notify their parent object
    /// when an internal <see cref="DataWrapper">DataWrapper</see> property changes
    /// </summary>
    public interface IParentablePropertyExposer
    {
        Delegate[] GetINPCSubscribers();
    }
}
