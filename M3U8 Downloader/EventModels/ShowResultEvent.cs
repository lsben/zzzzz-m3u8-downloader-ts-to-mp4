using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3U8_Downloader.EventModels {
    public class ShowResultEvent {

        public string DownloadFilePath { get; }

        public ShowResultEvent( string DownloadFilePath) {
            this.DownloadFilePath = DownloadFilePath;
        }
    }
}
