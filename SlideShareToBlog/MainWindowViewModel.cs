using HtmlAgilityPack;
using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SlideShareToBlog
{
    class MainWindowViewModel : ViewModel
    {
        #region HTML Path
        private string htmlPath;
        public string HTMLPath
        {
            get
            {
                return htmlPath;
            }
            set
            {
                htmlPath = value;
                RaisePropertyChanged("HTMLPath");
            }
        }

        public ICommand BrowseCommand
        {
            get
            {
                return new CommandHelper(Browse);
            }
        }

        private void Browse()
        {
            var ofd = DialogsUtility.CreateOpenFileDialog();
            DialogsUtility.AddExtension(ofd, "Web Page", "*.html;*.htm");
            DialogsUtility.AddExtension(ofd, "All Files", "*.*");

            ofd.ShowDialog();

            if (ofd.FileName != null)
            {
                HTMLPath = ofd.FileName;
            }
        }

        #endregion

        #region Pre/Postfix

        private string prefix = "", postfix = "";
        public string PrefixText
        {
            get
            {
                return prefix;
            }
            set
            {
                prefix = value;

                if (sync)
                    SyncPostFix();

                RaisePropertyChanged("PrefixText");
            }
        }

        private void SyncPostFix()
        {
            if (PrefixText.Contains("<"))
            {
                string[] parts = PrefixText.Split('<');

                StringBuilder sb = new StringBuilder();

                foreach (var s in parts.Reverse())
                {
                    //TODO: would need to also trim attributes

                    if (s != "")
                        sb.Append("</" + s);
                }

                PostfixText = sb.ToString();
            }
            else //Not a tag
                PostfixText = PrefixText;
        }

        public string PostfixText
        {
            get
            {
                return postfix;
            }
            set
            {
                postfix = value;
                RaisePropertyChanged("PostfixText");
            }
        }

        private bool sync;
        public bool Sync
        {
            get
            {
                return sync;
            }
            set
            {
                sync = value;

                if (sync)
                    SyncPostFix();

                RaisePropertyChanged("Sync");
                RaisePropertyChanged("NotSync");
            }
        }

        public bool NotSync
        {
            get
            {
                return !sync;
            }
        }

        #endregion

        #region Generate HTML

        public string GeneratedText
        {
            get; private set;
        }
        public ICommand GenerateCommand
        {
            get
            {
                return new CommandHelper(GenerateHTML);
            }
        }

        private void GenerateHTML()
        {
            try
            {
                if (!File.Exists(htmlPath))
                {
                    throw new Exception("The entered path is invalid");
                }

                HtmlDocument doc = new HtmlDocument();
                doc.Load(htmlPath);

                //TODO: Extract logic into module/extension
                var nodes = doc.DocumentNode.SelectNodes("//div[@id='slideshows']/ul/li");

                StringBuilder sb = new StringBuilder();

                foreach (var node in nodes)
                {
                    var link = node.SelectSingleNode(".//a");

                    string title = link.Attributes["title"].Value;
                    string url = link.Attributes["href"].Value;

                    string result = "<a href='" + url + "'>" + title + "</a>";
                    result = PrefixText + result + PostfixText;

                    sb.AppendLine(result);
                }

                GeneratedText = sb.ToString();

                RaisePropertyChanged("GeneratedText");
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e, "Error while generating HTML");
            }
        }

        #endregion

    }
}
