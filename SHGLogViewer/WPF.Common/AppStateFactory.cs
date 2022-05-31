using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF.Common
{
    public class AppStateFactory
    {
        //RootAppStateModel GetGlobalRootAppState();

        private volatile static RootAppStateModel RootAppState = new RootAppStateModel();

        public static RootAppStateModel GetGlobalRootAppState()
        {
            return RootAppState;
        }
    }
}
