using Caliburn.Micro;
using M3U8_Downloader.EventModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3U8_Downloader.ViewModels {
    public class ResultViewModel : Screen , IHandle<ShowResultEvent> {

        private readonly IEventAggregator _eventAggregator;

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
            throw new NotImplementedException();
        }
    }
}
