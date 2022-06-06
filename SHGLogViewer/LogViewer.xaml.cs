using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

                    _ret.StatusCode = _tempLine.Substring(_indexStartStatusCodeMessage + _statusCodeLength, _indexEndStatusCodeMessage - (_indexStartStatusCodeMessage + _statusCodeLength)).Trim();

                    // Remove exception
                    _tempLine = _tempLine.Remove(_indexStartStatusCodeMessage, _indexEndStatusCodeMessage + 1).Trim();
                }

                // Left off here, need to get message containing more than one ,
                // Possibly look for : and the from there back to the last previous ,  But also have to check for the escape as well
                /*  LocalBackupLocationNotFound 17:02.009 is an example.  So is [MyAddresses]
                 *  Or maybe we just show source and remove message:
                 */
                const string _source = "source:";
                if (_tempLine.Contains(_source))
                {
                    int _sourceLength = _source.Length;

                    int _indexStartSource = _tempLine.IndexOf(_source);
                    int _indexEndSource = _tempLine.IndexOf(",", _indexStartSource + _sourceLength);

                    if(_indexEndSource == -1)
                    {
                        _indexEndSource = _tempLine.IndexOf(ESCAPE, _indexStartSource + _sourceLength);
                    }

                    _ret.Source = _tempLine.Substring(_indexStartSource + _sourceLength, _indexEndSource - (_indexStartSource + _sourceLength)).Trim();

                    // Remove Source
                    _tempLine = _tempLine.Remove(_indexStartSource, (_indexEndSource - _indexStartSource)).Trim();
                }
                
                const string _contentType = "Content-Type:";

                if(_tempLine.Contains(_contentType))
                {
                    int _statusContentTypeLength = _contentType.Length;

                    int _indexStartContentTypeMessage = _tempLine.IndexOf(_contentType);
                    int _indexEndContentTypeMessage = _tempLine.IndexOf(COLORRESET, _indexStartContentTypeMessage + _statusContentTypeLength);

                    _ret.ContentType = _tempLine.Substring(_indexStartContentTypeMessage + _statusContentTypeLength, _indexEndContentTypeMessage - (_indexStartContentTypeMessage + _statusContentTypeLength));

                    _ret.ContentType = _ret.ContentType.Replace("\r\n", "").Replace("[0m", "").Trim();

                    // Remove exception
                    _tempLine = _tempLine.Remove(_indexStartContentTypeMessage, _indexEndContentTypeMessage + 1);                    
                }
                
                _tempLine = _tempLine.Replace("[0m", "").Replace("message:", "").Trim();


                // The above should assign everything, in the event it doesn't assign everything else to message.
                // We'll have to use this to clean up and do more parsing when needed.

                if (_ret.Level == "Info" || _ret.Level == "Debug")
                {
                    _ret.Message = _tempLine;
                }
                else
                    _ret.ContentType = _tempLine;

                if(!string.IsNullOrEmpty(_ret.StatusCode))
                {

                    _ret.Recommendations = ErrorRepository.CheckForRecommendations(_ret.StatusCode);
                }


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
                //MessageBox.Show("Please select a valid file", "Warning", MessageBoxButton.OK);
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
                MessageBox.Show("Please select a valid file", "Warning", MessageBoxButton.OK);
                ShowFileDialog();
            }

        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            ShowFileDialog();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //https://social.msdn.microsoft.com/Forums/vstudio/en-US/ce6e0c8a-07f4-46fb-a56c-d412c4d4e20d/how-to-make-textbox-text-as-hyperlink-in-wpf
            //TextBox _txtBox = (sender as TextBox);
            //RichTextBox _txtBox = (sender as RichTextBox);
            //TextRange _range = new TextRange(_txtBox.Document.ContentStart, _txtBox.Document.ContentEnd);
            //string _text = _txtBox.Text;
                        

            //Uri uri;
            //HasValidURI = Uri.TryCreate(_text, UriKind.Absolute, out uri);
            // Here see if you can build a Hyperlink and remove the the link from text and populate
            // Links below of how to do this with a textblock, trying to get it to work with RichTextBox if you can bind text to it inxamel

            /*<ScrollViewer>
   <RichTextBox>
        ...
   </RichTextBox>
</ScrollViewer>
            */
          //  if(HasValidURI)
          //  {
                /*
                _txtBox.Document.Blocks.Clear();

                Paragraph _para = new Paragraph();
                _txtBox.Document.Blocks.Add(_para);
                _text = _text.Replace(uri.ToString(), "");
                _para.Inlines.Add(_text);

                Hyperlink _hl = new Hyperlink();
                _hl.NavigateUri = uri;
                _hl.RequestNavigate += Hyperlink_RequestNavigate;
                _hl.Inlines.Add("Click for More Info");

                _para.Inlines.Add(_hl);
                */

                /* TextBlock
                 * https://stackoverflow.com/questions/2092890/add-hyperlink-to-textblock-wpf
                 * TextBlockWithHyperlink.Inlines.Clear();
TextBlockWithHyperlink.Inlines.Add("Some text ");
Hyperlink hyperLink = new Hyperlink() {
    NavigateUri = new Uri("http://somesite.com")
};
hyperLink.Inlines.Add("some site");
hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
TextBlockWithHyperlink.Inlines.Add(hyperLink);
TextBlockWithHyperlink.Inlines.Add(" Some more text");
                 */
        //}
    }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Uri uri;
            if (Uri.TryCreate((sender as TextBox).Text, UriKind.Absolute, out uri))
            {
                //     var _links = uri.AbsoluteUri.Split("\t\n ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(s => s.StartsWith("http://") || s.StartsWith("https://"));
                //var _regExUrl = new Regex(@"/(?:(?:((http|ftp|https):){0,1}\/\/)|www.|)([\w_-]+(?:(?:\.[\w_-]+)*))(\.(aaa|aarp|abarth|abb|abbott|abbvie|abc|able|abogado|abudhabi|ac|academy|accenture|accountant|accountants|aco|active|actor|ad|adac|ads|adult|ae|aeg|aero|aetna|af|afamilycompany|afl|africa|ag|agakhan|agency|ai|aig|aigo|airbus|airforce|airtel|akdn|al|alfaromeo|alibaba|alipay|allfinanz|allstate|ally|alsace|alstom|am|amazon|americanexpress|americanfamily|amex|amfam|amica|amsterdam|an|analytics|android|anquan|anz|ao|aol|apartments|app|apple|aq|aquarelle|ar|arab|aramco|archi|army|arpa|art|arte|as|asda|asia|associates|at|athleta|attorney|au|auction|audi|audible|audio|auspost|author|auto|autos|avianca|aw|aws|ax|axa|az|azure|ba|baby|baidu|banamex|bananarepublic|band|bank|bar|barcelona|barclaycard|barclays|barefoot|bargains|baseball|basketball|bauhaus|bayern|bb|bbc|bbt|bbva|bcg|bcn|bd|be|beats|beauty|beer|bentley|berlin|best|bestbuy|bet|bf|bg|bh|bharti|bi|bible|bid|bike|bing|bingo|bio|biz|bj|bl|black|blackfriday|blanco|blockbuster|blog|bloomberg|blue|bm|bms|bmw|bn|bnl|bnpparibas|bo|boats|boehringer|bofa|bom|bond|boo|book|booking|boots|bosch|bostik|boston|bot|boutique|box|bq|br|bradesco|bridgestone|broadway|broker|brother|brussels|bs|bt|budapest|bugatti|build|builders|business|buy|buzz|bv|bw|by|bz|bzh|ca|cab|cafe|cal|call|calvinklein|cam|camera|camp|cancerresearch|canon|capetown|capital|capitalone|car|caravan|cards|care|career|careers|cars|cartier|casa|case|caseih|cash|casino|cat|catering|catholic|cba|cbn|cbre|cbs|cc|cd|ceb|center|ceo|cern|cf|cfa|cfd|cg|ch|chanel|channel|charity|chase|chat|cheap|chintai|chloe|christmas|chrome|chrysler|church|ci|cipriani|circle|cisco|citadel|citi|citic|city|cityeats|ck|cl|claims|cleaning|click|clinic|clinique|clothing|cloud|club|clubmed|cm|cn|co|coach|codes|coffee|college|cologne|com|comcast|commbank|community|company|compare|computer|comsec|condos|construction|consulting|contact|contractors|cooking|cookingchannel|cool|coop|corsica|country|coupon|coupons|courses|cpa|cr|credit|creditcard|creditunion|cricket|crown|crs|cruise|cruises|csc|cu|cuisinella|cv|cw|cx|cy|cymru|cyou|cz|dabur|dad|dance|data|date|dating|datsun|day|dclk|dds|de|deal|dealer|deals|degree|delivery|dell|deloitte|delta|democrat|dental|dentist|desi|design|dev|dhl|diamonds|diet|digital|direct|directory|discount|discover|dish|diy|dj|dk|dm|dnp|do|docs|doctor|dodge|dog|doha|domains|doosan|dot|download|drive|dtv|dubai|duck|dunlop|duns|dupont|durban|dvag|dvr|dz|earth|eat|ec|eco|edeka|edu|education|ee|eg|eh|email|emerck|energy|engineer|engineering|enterprises|epost|epson|equipment|er|ericsson|erni|es|esq|estate|esurance|et|etisalat|eu|eurovision|eus|events|everbank|exchange|expert|exposed|express|extraspace|fage|fail|fairwinds|faith|family|fan|fans|farm|farmers|fashion|fast|fedex|feedback|ferrari|ferrero|fi|fiat|fidelity|fido|film|final|finance|financial|fire|firestone|firmdale|fish|fishing|fit|fitness|fj|fk|flickr|flights|flir|florist|flowers|flsmidth|fly|fm|fo|foo|food|foodnetwork|football|ford|forex|forsale|forum|foundation|fox|fr|free|fresenius|frl|frogans|frontdoor|frontier|ftr|fujitsu|fujixerox|fun|fund|furniture|futbol|fyi|ga|gal|gallery|gallo|gallup|game|games|gap|garden|gay|gb|gbiz|gd|gdn|ge|gea|gent|genting|george|gf|gg|ggee|gh|gi|gift|gifts|gives|giving|gl|glade|glass|gle|global|globo|gm|gmail|gmbh|gmo|gmx|gn|godaddy|gold|goldpoint|golf|goo|goodhands|goodyear|goog|google|gop|got|gov|gp|gq|gr|grainger|graphics|gratis|green|gripe|grocery|group|gs|gt|gu|guardian|gucci|guge|guide|guitars|guru|gw|gy|hair|hamburg|hangout|haus|hbo|hdfc|hdfcbank|health|healthcare|help|helsinki|here|hermes|hgtv|hiphop|hisamitsu|hitachi|hiv|hk|hkt|hm|hn|hockey|holdings|holiday|homedepot|homegoods|homes|homesense|honda|honeywell|horse|hospital|host|hosting|hot|hoteles|hotels|hotmail|house|how|hr|hsbc|ht|htc|hu|hughes|hyatt|hyundai|ibm|icbc|ice|icu|id|ie|ieee|ifm|iinet|ikano|il|im|imamat|imdb|immo|immobilien|in|inc|industries|infiniti|info|ing|ink|institute|insurance|insure|int|intel|international|intuit|investments|io|ipiranga|iq|ir|irish|is|iselect|ismaili|ist|istanbul|it|itau|itv|iveco|iwc|jaguar|java|jcb|jcp|je|jeep|jetzt|jewelry|jio|jlc|jll|jm|jmp|jnj|jo|jobs|joburg|jot|joy|jp|jpmorgan|jprs|juegos|juniper|kaufen|kddi|ke|kerryhotels|kerrylogistics|kerryproperties|kfh|kg|kh|ki|kia|kim|kinder|kindle|kitchen|kiwi|km|kn|koeln|komatsu|kosher|kp|kpmg|kpn|kr|krd|kred|kuokgroup|kw|ky|kyoto|kz|la|lacaixa|ladbrokes|lamborghini|lamer|lancaster|lancia|lancome|land|landrover|lanxess|lasalle|lat|latino|latrobe|law|lawyer|lb|lc|lds|lease|leclerc|lefrak|legal|lego|lexus|lgbt|li|liaison|lidl|life|lifeinsurance|lifestyle|lighting|like|lilly|limited|limo|lincoln|linde|link|lipsy|live|living|lixil|lk|llc|llp|loan|loans|locker|locus|loft|lol|london|lotte|lotto|love|lpl|lplfinancial|lr|ls|lt|ltd|ltda|lu|lundbeck|lupin|luxe|luxury|lv|ly|ma|macys|madrid|maif|maison|makeup|man|management|mango|map|market|marketing|markets|marriott|marshalls|maserati|mattel|mba|mc|mcd|mcdonalds|mckinsey|md|me|med|media|meet|melbourneand Innovation|meme|memorial|men|menu|meo|merckmsd|metlife|mf|mg|mh|miami|microsoft|mil|mini|mint|mit|mitsubishi|mk|ml|mlb|mls|mm|mma|mn|mo|mobi|mobile|mobily|moda|moe|moi|mom|monash|money|monster|montblanc|mopar|mormon|mortgage|moscow|moto|motorcycles|mov|movie|movistar|mp|mq|mr|ms|msd|mt|mtn|mtpc|mtr|mu|museum|music|mutual|mutuelle|mv|mw|mx|my|mz|na|nab|nadex|nagoya|name|nationwide|natura|navy|nba|nc|ne|nec|net|netbank|netflix|network|neustar|new|newholland|news|next|nextdirect|nexus|nf|nfl|ng|ngo|nhk|ni|nico|nike|nikon|ninja|nissan|nissay|nl|no|nokia|northwesternmutual|norton|now|nowruz|nowtv|np|nr|nra|nrw|ntt|nu|nycTelecommunications|nz|obi|observer|off|office|okinawa|olayan|olayangroup|oldnavy|ollo|om|omega|one|ong|onl|online|onyourside|ooo|open|oracle|orange|org|organic|orientexpress|origins|osaka|otsuka|ott|ovh|pa|page|pamperedchef|panasonic|panerai|paris|pars|partners|parts|party|passagens|pay|pccw|pe|pet|pf|pfizer|pg|ph|pharmacy|phd|philips|phone|photo|photography|photos|physio|piaget|pics|pictet|pictures|pid|pin|ping|pink|pioneer|pizza|pk|pl|place|play|playstation|plumbing|plus|pm|pn|pnc|pohl|poker|politie|porn|post|pr|pramerica|praxi|press|prime|pro|prod|productions|prof|progressive|promo|properties|property|protection|pru|prudential|ps|pt|pub|pw|pwc|py|qa|qpon|quebec|quest|qvc|racing|radio|raid|re|read|realestate|realtor|realty|recipes|red|redstone|redumbrella|rehab|reise|reisen|reit|reliance|ren|rent|rentals|repair|report|republican|rest|restaurant|review|reviews|rexroth|rich|richardli|ricoh|rightathome|ril|rio|rip|rmit|ro|rocher|rocks|rodeo|rogers|room|rs|rsvp|ru|rugby|ruhr|run|rw|rwe|ryukyu|sa|saarland|safe|safety|sakura|sale|salon|samsclub|samsung|sandvik|sandvikcoromant|sanofi|sap|sapo|sarl|sas|save|saxo|sb|sbi|sbs|sc|sca|scb|schaeffler|schmidt|scholarships|school|schule|schwarz|science|scjohnson|scor|scot|sd|se|search|seat|secure|security|seek|select|sener|services|ses|seven|sew|sex|sexy|sfr|sg|sh|shangrila|sharp|shaw|shell|shia|shiksha|shoes|shop|shopping|shouji|show|showtime|shriram|si|silk|sina|singles|site|sj|sk|ski|skin|sky|skype|sl|sling|sm|smart|smile|sn|sncf|so|soccer|social|softbank|software|sohu|solar|solutions|song|sony|soy|spa|space|spiegel|sport|spot|spreadbetting|sr|srl|srt|ss|st|stada|staples|star|starhub|statebank|statefarm|statoil|stc|stcgroup|stockholm|storage|store|stream|studio|study|style|su|sucks|supplies|supply|support|surf|surgery|suzuki|sv|swatch|swiftcover|swiss|sx|sy|sydney|symantec|systems|sz|tab|taipei|talk|taobao|target|tatamotors|tatar|tattoo|tax|taxi|tc|tci|td|tdk|team|tech|technology|tel|telecity|telefonica|temasek|tennis|teva|tf|tg|th|thd|theater|theatre|tiaa|tickets|tienda|tiffany|tips|tires|tirol|tj|tjmaxx|tjx|tk|tkmaxx|tl|tm|tmall|tn|to|today|tokyo|tools|top|toray|toshiba|total|tours|town|toyota|toys|tp|tr|trade|trading|training|travel|travelchannel|travelers|travelersinsurance|trust|trv|tt|tube|tui|tunes|tushu|tv|tvs|tw|tz|ua|ubank|ubs|uconnect|ug|uk|um|unicom|university|uno|uol|ups|us|uy|uz|va|vacations|vana|vanguard|vc|ve|vegas|ventures|verisign|versicherung|vet|vg|vi|viajes|video|vig|viking|villas|vin|vip|virgin|visa|vision|vista|vistaprint|viva|vivo|vlaanderen|vn|vodka|volkswagen|volvo|vote|voting|voto|voyage|vu|vuelos|wales|walmart|walter|wang|wanggou|warman|watch|watches|weather|weatherchannel|webcam|weber|website|wed|wedding|weibo|weir|wf|whoswho|wien|wiki|williamhill|win|windows|wine|winners|wme|wolterskluwer|woodside|work|works|world|wow|ws|wtc|wtf|xbox|xerox|xfinity|xihuan|xin|xperia|xxx|xyz|yachts|yahoo|yamaxun|yandex|ye|yodobashi|yoga|yokohama|you|youtube|yt|yun|za|zappos|zara|zero|zip|zippo|zm|zone|zuerich|zw))(?:([\w.,@?^=%&:\/~+#-]*[\w@?^=%&\/~+#-]){0,1})/i");

                //var _links = _regExUrl.Match(uri.AbsoluteUri);
                var _links = Regex.Match(uri.AbsoluteUri, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");

                if (_links != null)
                {
                    Process.Start(new ProcessStartInfo(_links.Value));
                }
                /*
                if (_links.Any())
                {
                    Process.Start(new ProcessStartInfo(_links.First()));
                }
                */
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // for .NET Core you need to add UseShellExecute = true
            // see https://docs.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute#property-value
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            Process wordProcess = new Process();
            wordProcess.StartInfo.FileName = "GenerateStandardOutputLogs.rtf";
            wordProcess.StartInfo.UseShellExecute = true;
            wordProcess.Start();
        }
    }
}
