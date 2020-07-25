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
    public class ShellViewModel : Screen , IHandle<DownloadCompleteEvent> {

        private readonly IEventAggregator _eventAggregator;
        private DownloadViewModel _downloadViewModel;
        private ResultViewModel _resultViewModel;
        private IWindowManager _windowManager;

        //
        public ShellViewModel() {
        }

        public ShellViewModel(IEventAggregator eventAggregator, DownloadViewModel downloadViewModel, ResultViewModel resultViewModel, IWindowManager windowManager) {
            _eventAggregator = eventAggregator;
            _downloadViewModel = downloadViewModel;
            _windowManager = windowManager;
            _resultViewModel = resultViewModel;
            _eventAggregator.Subscribe(this);
        }

        //
        private string _downloadPath = "...";

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
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                    DownloadPath = fbd.SelectedPath;
                }
            }
        }

        //
        public void BrowseAndSetM3u8FilePath() {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".m3u8";
            dlg.Filter = "M3U8 Files (*.m3u8) |*.m3u8";
            dlg.Title = "Select the m3u8 file";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true) {
                M3u8Path = dlg.FileName;
            }
        }

        //
        public void PasteM3u8FileContent() {
            M3u8FileContent = Clipboard.GetText();
        }

        //
        private bool _isDownloading = false;

        public bool IsDownloading {
            get { return _isDownloading; }
            set {
                _isDownloading = value;
                NotifyOfPropertyChange(() => DownloadEnable);
            }
        }

        public bool DownloadEnable => !IsDownloading;

        //
        public void DownloadVideo() {

            if (IsUsingPath && !File.Exists(M3u8Path)) {
                MessageBox.Show("Please select valid m3u8 file", "M3u8 file not selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!IsUsingPath && string.IsNullOrWhiteSpace(M3u8FileContent)) {
                MessageBox.Show("Please enter  m3u8 file content", "Enter M3u8 file content", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (DownloadPath.Equals("...") || !Directory.Exists(DownloadPath)) {
                MessageBox.Show("Please select a Download Folder", "Select Download Folder", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }


            string[] tsLinks = new string[]{};
            if (IsUsingPath) {
                tsLinks = Extentions.getTSFileLinkListFromPath(M3u8Path);
            }else {
                tsLinks = Extentions.getTSFileLinksFromContent(M3u8FileContent);
            }


            if(tsLinks.Length == 0) {
                MessageBox.Show("The entered m3u8 file or m3u8 content is invalid", "Invalid Data entered", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            } else {
                IsDownloading = true;
                _windowManager.ShowWindow(_downloadViewModel, null, null);
                _eventAggregator.PublishOnUIThread(new DownloadInfoEvent(tsLinks, DownloadPath));
            }

            //App.Current.MainWindow.Hide();
            //ActivateItem(_downloadViewModel);
            //_windowManager.ShowWindow(_downloadViewModel, null, null);
            
            //_windowManager.ShowDialog(_downloadViewModel, null, null);
            //_eventAggregator.PublishOnUIThread("Hello World");
        }

        public void Handle(DownloadCompleteEvent message) {
            IsDownloading = false;
            if (message.IsDownloadCancelled) {
                MessageBox.Show("Download Cancelled", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            } else if(message.IsDownloadSuccessful){
                OnDownloadSuccess(message.DownloadedFilePath);
            } else {
                OnDownloadError(message.ErrorMsg);
            }
        }

        private void OnDownloadSuccess(string path) {
            _windowManager.ShowWindow(_resultViewModel, null, null);
            _eventAggregator.PublishOnUIThread(new ShowResultEvent(path));
        }

        private void OnDownloadError(string errorMsg) {
            MessageBox.Show("Error occured while downloading\nError message - "+errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnActivate() {
            _eventAggregator.Subscribe(this);
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close) {
            _eventAggregator.Unsubscribe(this);
            base.OnDeactivate(close);
        }

    }       
}
