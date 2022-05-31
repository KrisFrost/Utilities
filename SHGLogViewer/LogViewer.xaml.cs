using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    /// Interaction logic for LogViewer.xaml
    /// </summary>
    public partial class LogViewer : UserControl
    {
        const string COLORRESET = "\u001b[0m";
        const string ESCAPE = "\u001b";
        static readonly string[] Levels =
        {
            "     ",
            "[Fatal]",
            "[Error]",
            "[Warn] ",
            "[Info] ",
            "[Debug]"
        };

        string _logFileName = "";

        RootAppStateModel RootAppState { get; set; }

        public ObservableCollection<LogItemModel> LogItems { get; set; }

        public LogViewer()
        {
            InitializeComponent();
            LogItems = new ObservableCollection<LogItemModel>();

            rgLogs.ItemsSource = LogItems;

            RootAppState = AppStateFactory.GetGlobalRootAppState();

            ShowFileDialog();

            RootAppState.IsBusy = true;
          //  LoadLogFile(_logFileName);

            RootAppState.IsBusy = false;

        }

        public void LoadLogFile()
        {

            //  Red: \u001b[31m
            //  Reset: \u001b[0m
            // https://www.lihaoyi.com/post/BuildyourownCommandLinewithANSIescapecodes.html

            string _version = "";
            string _commit = "";

            var _log = File.ReadAllLines(_logFileName);

            int _lineCount = _log.Count();
            StringBuilder _sb = new StringBuilder();

            for (int i = 0; i < _lineCount; i++)
            { 
                string line = _log[i];

                if (line.StartsWith("version:"))
                {
                    _version = line;
                }
                else if (line.StartsWith("commit:"))
                {
                    _commit = line;
                }
                else if (line.StartsWith(ESCAPE))
                {
                    _sb.Clear();

                    //\u001b[37m[Info] 2022-05-12T06: 16:58.790[HostBootstrapperStarting]\u001b[0m
                    // Now we need to read all string lines associated with the lineitem
                    _sb.AppendLine(line);

                    // Read lines till we get to the end color reset
                    if (!line.Contains(COLORRESET))
                    {
                        for (int ii = i+1; ii < _lineCount; ii++)
                        {
                            _sb.AppendLine(_log[ii]);
                            i++;

                            if (_log[ii].Contains(COLORRESET))
                                break;
                        }
                    }

                    LogItems.Add(ConvertToLogItem(_sb.ToString()));
                }
            }

            // Build out a class, Level
            /*
            foreach(var line in _log)
            {
                if(line.StartsWith("version:"))
                {
                    _version = line;
                }
                else if(line.StartsWith("commit:"))
                {
                    _commit = line;
                }
                else if(line.StartsWith($"\u001b"))
                {
                    
                        ConvertToLogItem(line);
                }
            }
            */           

        }

        private LogItemModel ConvertToLogItem(string lineItem)
        {
            var _ret = new LogItemModel();
            // What we're parsing
            //\u001b[37m[Info] 2022-05-12T06: 16:58.790[HostBootstrapperStarting]\u001b[0m
            // \u001b[37m[Info] 2022-05-12T06:16:58.790 [HostBootstrapperStarting]\u001b[0m
            // Get Color

            var _indexOfColor = lineItem.IndexOf("m", 0)+1;

            _ret.LineColor = lineItem.Substring(0, _indexOfColor);
            _indexOfColor++;

            var _indexOfLevel = lineItem.IndexOf("]", 0);
                        
            _ret.Level = lineItem.Substring(_indexOfColor, (_indexOfLevel - _indexOfColor));

            // This gets us up to the start of category in the line
            var _indexOfCategoryStart = lineItem.IndexOf("[", _indexOfLevel);

            _indexOfLevel++;
            // Get TimeStamp
            _ret.TimeStamp = lineItem.Substring(_indexOfLevel, _indexOfCategoryStart - _indexOfLevel).Trim();

            _indexOfCategoryStart++;

            // The file I have, there is a comma after category
            var _indexOfStartMessage = lineItem.IndexOf(",", _indexOfCategoryStart);

            // If there is a message or exception, we see a , after category.
            if(_indexOfStartMessage > _indexOfCategoryStart)
            {
                // Get Category
                _ret.Category = lineItem.Substring(_indexOfCategoryStart, (_indexOfStartMessage - _indexOfCategoryStart) - 1);

                // Get everything after category for this line
                // There are variances as to how exception is within the file, some upper, some lo
                string _tempLine = lineItem.Remove(0, _indexOfStartMessage + 1).Trim();

                // See how many different elements are in the string, they appear to be delimited with a ,
                // However, as in this case, there may be multiple comma's within an element as well.
                //[37m[Info] 2022 - 05 - 12T06: 16:58.803[MyAddresses], message: 127.0.0.1, 192.168.0.2, source: P2PRateLimitSignalListenerService, correlationId: 6cf0f128 - 408b - 40b8 - 934a - 21145dd4b333

                // Check for Exception
                string _exception = "exception:";
                if(_tempLine.Contains(_exception))
                {
                    int _exceptionLength = _exception.Length;

                    int _indexStartExceptionMessage = _tempLine.IndexOf(_exception);
                    int _indexEndExceptionMessage = _tempLine.IndexOf(":", _indexStartExceptionMessage + _exceptionLength);

                    _ret.Exception = _tempLine.Substring(_indexStartExceptionMessage + 10, _indexEndExceptionMessage - (_indexStartExceptionMessage + _exceptionLength));

                    // Remove exception
                    _tempLine = _tempLine.Remove(_indexStartExceptionMessage, _indexEndExceptionMessage + 1).Trim();

                }

                const string _statusCode = "StatusCode:";

                // Check for StatusCode
                if(_tempLine.Contains(_statusCode))
                {
                    int _statusCodeLength = _statusCode.Length;

                    int _indexStartStatusCodeMessage = _tempLine.IndexOf(_statusCode);
                    int _indexEndStatusCodeMessage = _tempLine.IndexOf("\r", _indexStartStatusCodeMessage + _statusCodeLength);

                    _ret.StatusCode = _tempLine.Substring(_indexStartStatusCodeMessage + _statusCodeLength, _indexEndStatusCodeMessage - (_indexStartStatusCodeMessage + _statusCodeLength));

                    // Remove exception
                    _tempLine = _tempLine.Remove(_indexStartStatusCodeMessage, _indexEndStatusCodeMessage + 1).Trim();
                }

                const string _contentType = "Content-Type:";

                if(_tempLine.Contains(_contentType))
                {
                    int _statusContentTypeLength = _contentType.Length;

                    int _indexStartContentTypeMessage = _tempLine.IndexOf(_contentType);
                    int _indexEndContentTypeMessage = _tempLine.IndexOf(COLORRESET, _indexStartContentTypeMessage + _statusContentTypeLength);

                    _ret.ContentType = _tempLine.Substring(_indexStartContentTypeMessage + _statusContentTypeLength, _indexEndContentTypeMessage - (_indexStartContentTypeMessage + _statusContentTypeLength));

                    _ret.ContentType = _ret.ContentType.Replace("\r\n", "").Trim();

                    // Remove exception
                    _tempLine = _tempLine.Remove(_indexStartContentTypeMessage, _indexEndContentTypeMessage + 1).Replace("[0m", "").Trim();                    
                }

                // The above should assign everything, in the event it doesn't assign everything else to message.
                // We'll have to use this to clean up and do more parsing when needed.
                _ret.Message = _tempLine;             



                // There are some different patterns we're going to have to try and address
                // message could have Exception, message seems to always have a, but exception, source etc do now to trying to
                // get those is going to be challengine.
                // May have to make assumptions like exception will be followed by StatusCode, is there an unauthorized.
                // So we may want to first check with Message and if present take logic
                // If warn or error and no message then handle exception.
                // We're going to have to determine what sections are next and if there are multiples, how much of the string do we parse to get all the
                // different elements.
                /* for example, there may just be a source and that goes to the end of the line.
                 * Ther may be message and it tends to end with a ,.
                 * But then there could be exception which seems to end with :, but after it is StatusCode which doesn't have a ending delimiter and may be followed by Content-Type.
                 */

                // Now we check to Content-Type:  The presumption here is everything else other is on a single line, this typically contains a call stack
                // which will span over multiple lines.
                // I think if there are other 

            }
            

            return _ret;
        }
        public void ShowFileDialog()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "StandardOutput.txt"; // Default file name
            dialog.DefaultExt = ".txt"; // Default file extension
            dialog.Filter = "Any Log File (*.*)|*.*"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                _logFileName = dialog.FileName;
                txtFileName.Text = _logFileName;
            }
            else
            {
                MessageBox.Show("Please select a valid file", "Warning", MessageBoxButton.OK);
            }
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_logFileName))
            {
                LoadLogFile();
            }
            else
            {
                ShowFileDialog();
            }

        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            ShowFileDialog();
        }
    }
}
