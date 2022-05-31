using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace SHGLogViewer
{
    public class GridRowStyleSelector : System.Windows.Controls.StyleSelector
    {
        /*
         *
         *      (EventLevel.Critical, ConsoleColor.Magenta, 35),
                (EventLevel.Error, ConsoleColor.Red, 31),
                (EventLevel.Warning,ConsoleColor.Yellow,  33),
                (EventLevel.Verbose, ConsoleColor.Green, 32),
                (EventLevel.Informational,ConsoleColor.Gray,  37) 
         */

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is LogItemModel)
            {
                LogItemModel _item = item as LogItemModel;

                switch(_item.Level.ToLower())
                {
                    case "fatal":
                        return CriticalStyle;

                    case "error":
                        return ErrorStyle;

                    case "warn":
                        return WarningStyle;

                    case "info":
                        //return InfoStyle;
                        return null;

                    //case "debug":
                        ///return DebugStyle
                        

                    default:
                        return null;

                        
                }
            }

            return null;
            
        }
        public Style WarningStyle { get; set; }
        public Style CriticalStyle { get; set; }
        public Style ErrorStyle { get; set; }
        public Style VerboseStyle { get; set; }
        public Style InfoStyle { get; set; }

    }
}
