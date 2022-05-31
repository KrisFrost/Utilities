using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WPF.Common;

namespace SHGLogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LogViewer _logViewer = new LogViewer();

        public MainWindow()
        {
            InitializeComponent();

            RootAppState = AppStateFactory.GetGlobalRootAppState();

            this.DataContext = this;
            //this.DataContext = new MainViewModel();

            // For testing, load default UC
            this.LoadRootUC(_logViewer);
        }

        private async void LoadRootUC(UserControl uc)
        {
            // Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            //{

            RootBusyIndicator.IsBusy = true;
            //RootAppState.IsBusy = true;


            //}));

            await Task.Run(() => Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                this.UCRootCenterContent.Content = uc;
            })));

            RootBusyIndicator.IsBusy = false;
            //RootAppState.IsBusy = false;


            //Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            //{
            //    RootAppState.IsBusy = false;

            //}));
        }

        public RootAppStateModel RootAppState { get; set; }

        public override void EndInit()
        {
            base.EndInit();

        }
    }
}
