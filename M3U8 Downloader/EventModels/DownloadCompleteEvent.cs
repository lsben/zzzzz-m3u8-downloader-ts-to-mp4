using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3U8_Downloader.EventModels {
    public class DownloadCompleteEvent {
        public bool IsDownloadSuccessful { get; }

        public bool IsDownloadCancelled { get; }

        public string DownloadedFilePath { get; }

        public string ErrorMsg { get; }

        public DownloadCompleteEvent(bool IsDownloadSuccessful, bool IsDownloadCancelled, string DownloadedFilePath, string ErrorMsg) {
            this.IsDownloadSuccessful = IsDownloadSuccessful;
            this.IsDownloadCancelled = IsDownloadCancelled;
            this.DownloadedFilePath = DownloadedFilePath; 
            this.ErrorMsg = ErrorMsg;
        }
    }
}
