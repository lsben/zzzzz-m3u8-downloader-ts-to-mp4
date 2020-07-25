using Caliburn.Micro;
using M3U8_Downloader.EventModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace M3U8_Downloader.ViewModels {
    public class ResultViewModel : Screen , IHandle<ShowResultEvent> {

        private readonly IEventAggregator _eventAggregator;

        private string _downlodedFilePath = "...";

        public string DownlodedFilePath {
            get { return _downlodedFilePath; }
            set {
                _downlodedFilePath = value;
                NotifyOfPropertyChange(() => DownlodedFilePath);
            }
        }

                
        public ResultViewModel() {
        }

        public ResultViewModel(IEventAggregator eventAggregator, DownloadViewModel downloadViewModel, IWindowManager windowManager) {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
        }

        protected override void OnActivate() {
            _eventAggregator.Subscribe(this);
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close) {
            _eventAggregator.Unsubscribe(this);
            base.OnDeactivate(close);
        }


        public void Handle(ShowResultEvent message) {
            DownlodedFilePath = message.DownloadFilePath;
        }

        public void CopyPath() {
            if (DownlodedFilePath.Equals("...")) return;
            Clipboard.SetText(DownlodedFilePath);
           
        }

        public void PlayVideo() {
            if (DownlodedFilePath.Equals("...")) return;
            Process.Start(DownlodedFilePath);
        }

        public void OpenFileLocation() {
            if (DownlodedFilePath.Equals("...")) return;
            Process.Start("explorer.exe", string.Format("/select,\"{0}\"", DownlodedFilePath));
        }
    }
}
