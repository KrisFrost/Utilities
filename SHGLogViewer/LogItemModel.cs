using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SHGLogViewer
{
    public class LogItemModel
    {
        //        https://stackoverflow.com/questions/17660917/how-to-make-textbox-text-as-hyperlink-in-wpf
        private bool _hasValidURI;

        public bool HasValidURI
        {
            get { return _hasValidURI; }
            set { _hasValidURI = value; OnPropertyChanged("HasValidURI"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }

        public string LineColor { get; set; }
        public string Level { get; set; }
        public string TimeStamp { get; set; }

        public string Category { get; set; }

        public string Exception { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }

        public string StatusCode { get; set; }
        public string RequestId { get; set; }

        public string ContentType { get; set; }

        public string Recommendations { get; set; }

        public LogItemModel()
        {
            //LineColor.
        }
    }
}
