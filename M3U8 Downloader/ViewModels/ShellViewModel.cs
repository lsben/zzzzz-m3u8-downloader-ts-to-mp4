using Caliburn.Micro;
using M3U8_Downloader.EventModels;
using M3U8_Downloader.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace M3U8_Downloader.ViewModels {
    public class ShellViewModel : Screen {

        private readonly IEventAggregator _eventAggregator;
        private DownloadViewModel _downloadViewModel;
        private IWindowManager _windowManager;

        //
        public ShellViewModel() {
        }

        public ShellViewModel(IEventAggregator eventAggregator, DownloadViewModel downloadViewModel, IWindowManager windowManager) {
            _eventAggregator = eventAggregator;
            _downloadViewModel = downloadViewModel;
            _windowManager = windowManager;
        }

        //
        private string _downloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");

        public string DownloadPath {
            get { return _downloadPath; }
            set { 
                _downloadPath = value;
                NotifyOfPropertyChange(() => DownloadPath);
            }
        }

        //
        private string _m3u8Path = "...";

        public string M3u8Path {
            get { return _m3u8Path; }
            set {
                _m3u8Path = value;
                NotifyOfPropertyChange(() => M3u8Path);
            }
        }

        //
        private string _m3u8FileContent = "";

        public string M3u8FileContent {
            get { return _m3u8FileContent; }
            set {
                _m3u8FileContent = value;
                NotifyOfPropertyChange(() => M3u8FileContent);
            }
        }

        //
        private bool _isUsingPath = true;

        public bool IsUsingPath {
            get { return _isUsingPath; }
            set { 
                _isUsingPath = value;
                NotifyOfPropertyChange(() => VisibilityPath);
                NotifyOfPropertyChange(() => VisibilityPaste);
            }
        }

        //
        public Visibility VisibilityPath {
            get {
                return IsUsingPath ? Visibility.Visible : Visibility.Collapsed;
            }
        }


        //
        public Visibility VisibilityPaste {
            get {
                return IsUsingPath ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        //
        public void BrowseAndSetDownloadPath() {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog()) {
                fbd.Description = "Select the download folder ";
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();
                if(result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                    DownloadPath = fbd.SelectedPath;
                }
            }
        }

        //
        public void BrowseAndSetM3u8FilePath() {
            OpenFileDialog dlg =  new OpenFileDialog();
            dlg.DefaultExt = ".m3u8";
            dlg.Filter = "M3U8 Files (*.m3u8) |*.m3u8";
            dlg.Title = "Select the m3u8 file";
            Nullable<bool> result = dlg.ShowDialog();
            if(result == true) {
                M3u8Path = dlg.FileName;
            }
        }

        //
        public void PasteM3u8FileContent() {
            M3u8FileContent = Clipboard.GetText();
        }

        public void DownloadVideo() {
            string[] tsLinks = new string[]{};
            if (IsUsingPath) {
                tsLinks = Extentions.getTSFileLinkListFromPath(M3u8Path);
            }else {
                tsLinks = Extentions.getTSFileLinksFromContent(M3u8FileContent);
            }


            if(tsLinks.Length == 0) {
                MessageBox.Show("Please enter valid m3u8 file or valid content", "Invalid Data", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            } else {
                _windowManager.ShowWindow(_downloadViewModel, null, null);
                _eventAggregator.PublishOnUIThread(new DownloadInfoEvent(tsLinks, DownloadPath));
            }


            //App.Current.MainWindow.Hide();
            //ActivateItem(_downloadViewModel);
            //_windowManager.ShowWindow(_downloadViewModel, null, null);
            
            //_windowManager.ShowDialog(_downloadViewModel, null, null);
            //_eventAggregator.PublishOnUIThread("Hello World");
        }

        

    }       
}
