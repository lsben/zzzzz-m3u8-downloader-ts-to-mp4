using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3U8_Downloader.EventModels {
    public class DownloadInfoEvent {
        public string[] TsFileLinks { get; }

        public string DownloadPath { get; }

        public DownloadInfoEvent(string[] TsFileLinks, string DownloadPath) {
            this.TsFileLinks = TsFileLinks;
            this.DownloadPath = DownloadPath;
        }
    }
}
