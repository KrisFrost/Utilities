using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHGLogViewer
{
    public class LogItemModel
    {
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
