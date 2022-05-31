using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WPF.Common;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SHGLogViewer.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public RootAppStateModel RootAppState { get; set; }
       // ApimContainerUC _apimContainerUc = new ApimContainerUC();

        private UserControl _loadedRootCenterContent = null;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (RootAppState == null)
                RootAppState = AppStateFactory.GetGlobalRootAppState();
        }
    }
}