using ByteSizeLib;
using Caliburn.Micro;
using HSLDownloader_m3u8_ts.ffmpeg_wrapper;
using M3U8_Downloader.EventModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace M3U8_Downloader.ViewModels {
    public class DownloadViewModel : Screen , IHandle<DownloadInfoEvent> {
        private readonly IEventAggregator _eventAggregator;
        private IWindowManager _windowManager;

        public DownloadViewModel() {
        }


        public DownloadViewModel(IEventAggregator eventAggregator, IWindowManager windowManager) {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
        }

        public void Handle(DownloadInfoEvent downloadInfoEvent) {
            // Handling event here.
            _ts_file_links = downloadInfoEvent.TsFileLinks;
            _download_folder_path = downloadInfoEvent.DownloadPath;
            downloadVideo();
    }

        protected override void OnActivate() {
            _eventAggregator.Subscribe(this);
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close) {
            _eventAggregator.Unsubscribe(this);
            base.OnDeactivate(close);
        }

        /* protected override void OnDeactivate(bool close) {
             //MessageBox.Show("Closed");
             //App.Current.MainWindow.Show();
         }*/

        /* public override void CanClose(Action<bool> callback) {
             //if(some logic...)
             callback(false); // will cancel close
             MessageBox.Show("tried to close");
         }*/


        //  
        private string _processText01 = "...";
        public string ProcessText01 {
            get { return _processText01; }
            set { 
                _processText01 = value;
                NotifyOfPropertyChange(() => ProcessText01);
            }
        }

        //  
        private string _processText02 = "...";
        public string ProcessText02 {
            get { return _processText02; }
            set {
                _processText02 = value;
                NotifyOfPropertyChange(() => ProcessText02);
            }
        }

        //  
        private Visibility _pbar02Visibility = Visibility.Collapsed;
        public Visibility Pbar02Visibility {
            get { return _pbar02Visibility; }
            set {
                _pbar02Visibility = value;
                NotifyOfPropertyChange(() => _pbar02Visibility);
            }
        }


        //  
        private int _pbar01Value = 0;
        public int Pbar01Value {
            get { return _pbar01Value; }
            set {
                _pbar01Value = value;
                NotifyOfPropertyChange(() => Pbar01Value);
            }
        }

        //  
        private int _pbar02Value = 0;
        public int Pbar02Value {
            get { return _pbar02Value; }
            set {
                _pbar02Value = value;
                NotifyOfPropertyChange(() => Pbar02Value);
            }
        }

        


        private string _temp_folder_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
        private string _ffmpeg_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ffmpeg\\ffmpeg.exe");
        private string[] _ts_file_links = new string[] { };
        private string _download_folder_path = "";
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private bool _canClose = false;

        private void downloadVideo() {
            CancellationToken ct = tokenSource.Token;
            try {
                Task t = Task.Run(async () => {
                    try {
                        string[] _ts_file_paths = await DownloadTsFiles(ct);
                        string _merged_file_path = await MergeTsFiles(ct, _ts_file_paths);
                        string _final_video_path = await ConvertIntoMp4Video(ct, _merged_file_path);
                        OnVideoDownloadCompleted(_final_video_path);
                    } catch (Exception e) {
                        OnErrorDownloading(e);
                    }

                }, ct);
            } catch (AggregateException) {
                OnCancelation();
            } catch (Exception e) {
                OnErrorDownloading(e);
            }
        }

        private async Task<string[]> DownloadTsFiles(CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            if (!Directory.Exists(_temp_folder_path)) {
                DirectoryInfo di = Directory.CreateDirectory(_temp_folder_path);
            } else {
                Directory.Delete(_temp_folder_path,true);
                DirectoryInfo di = Directory.CreateDirectory(_temp_folder_path);
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() => {
                ProcessText01 = String.Format("Downloading Ts Part Files.\n{0}/{1} parts downloaded\n{2}% Completed", 0, _ts_file_links.Length, 0);
                ProcessText02 = String.Format("Downloading Part {0}\n{1}% Completed", 1, 0);
                Pbar01Value = 0;
                Pbar02Value = 0;
                Pbar02Visibility = Visibility.Visible;
            }));

            string[] _ts_file_paths = new string[_ts_file_links.Length];

            //Making ts file paths
            int j = 0;
            foreach (string link in _ts_file_links) {
                j++;
                _ts_file_paths[j-1] = _temp_folder_path + "\\temp_ts_part_" + j + ".ts";
            }

            int i = 0;
            foreach (string link in _ts_file_links) {
                i++;
                using (WebClient client = new WebClient()) {
                    client.DownloadProgressChanged += (s, e) => {
                        string ratio = ByteSize.FromBytes(e.BytesReceived).ToString("KB") + "/" + ByteSize.FromBytes(e.TotalBytesToReceive).ToString("KB");
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() => {
                            ProcessText02 = String.Format("Downloading Part {0}\n{1} Downloaded\n{2}% Completed\nLink = {3}", i, ratio, e.ProgressPercentage,link);
                            Pbar02Value = e.ProgressPercentage;
                        }));
                    };  
                    client.DownloadFileCompleted += (s, e) => {
                        int p = (int) Math.Round((double)(100 * i) / _ts_file_links.Length);
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() => {
                            ProcessText01 = String.Format("Downloading Ts Part Files.\n{0}/{1} parts downloaded\n{2}% Completed", i, _ts_file_links.Length, p);
                            Pbar01Value = p;
                        }));
                    };
                    ct.ThrowIfCancellationRequested();
                    await client.DownloadFileTaskAsync(link, _ts_file_paths[i-1]);
                }
            }

            ct.ThrowIfCancellationRequested();
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() => {
                ProcessText02 = "...";
                Pbar01Value = 100;
                Pbar02Visibility = Visibility.Collapsed;
            }));
            return _ts_file_paths;
        }

        private async Task<string> MergeTsFiles(CancellationToken ct, string[] _ts_file_paths) {
            ct.ThrowIfCancellationRequested();

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() => {
                ProcessText01 = "Merging Ts Part Files.";
                ProcessText02 = String.Format("Merging Part {0}/{1}\nCurrent size - {2} , part size - {3}\n{4}% Completed", 1, _ts_file_paths.Length,0,0,0);
                Pbar01Value = 0;
                Pbar02Visibility = Visibility.Collapsed;
            }));



            string _merged_file_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Final_Merge_Output.ts");
            if (File.Exists(_merged_file_path)) {
                File.Delete(_merged_file_path);
            }

            using (var output = File.Create(_merged_file_path)) {
                int k = 0;
                foreach (var i_file in _ts_file_paths) {
                    k++;
                    ct.ThrowIfCancellationRequested();
                    using (var input = File.OpenRead(i_file)) {
                        input.CopyTo(output);
                        int p = (int)Math.Round((double)(100 * k) / _ts_file_paths.Length);
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() => {
                            ProcessText02 = String.Format("Merging Part {0}/{1}\nCurrent size - {2} , part size - {3}\n{4}% Completed", k, _ts_file_paths.Length, ByteSize.FromBytes(output.Length).ToString("MB"), ByteSize.FromBytes(input.Length).ToString("KB"), p);
                            Pbar01Value = p;
                        }));
                    }
                }
            }
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() => {
                ProcessText01 = "Merging Ts Part Files Completed.";
                ProcessText02 = "...";
                Pbar01Value = 100;
            }));
            ct.ThrowIfCancellationRequested();
            await Task.Delay(50);
            return _merged_file_path;
        }

        private async Task<string> ConvertIntoMp4Video(CancellationToken ct, string _merged_file_path) {
            ct.ThrowIfCancellationRequested();

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() => {
                ProcessText01 = "Converting and processing final output video";
                ProcessText02 = String.Format("{0}% Completed",0);
                Pbar01Value = 0;
                Pbar02Visibility = Visibility.Collapsed;
            }));

    
            string _final_video_file_name = "Final Mp4 Video " + DateTime.Now.ToString("(yyyy-MM-dd) (HH-mm-ss)") + ".mp4";
            string _final_video_file_path = Path.Combine(_download_folder_path, _final_video_file_name);
            string _ffmpeg_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ffmpeg\\ffmpeg.exe");

            string args = String.Format("-i \"{0}\" -c:v libx264 -c:a aac \"{1}\"", _merged_file_path, _final_video_file_path);
            EncodingEngine engine = new EncodingEngine(_ffmpeg_path);
            EncodingJob job = new EncodingJob();
            job.Arguments = args;
            engine.VideoEncoding += (sender, e) => {
                if (ct.IsCancellationRequested) {
                    engine.KillProcess();
                    ct.ThrowIfCancellationRequested();
                }
                int p = Convert.ToInt32(e.Progress);

                StringBuilder sb = new StringBuilder("Processing from ts to mp4\nFrame - ");
                sb.Append(e.Frame);
                sb.Append("\nFps - ");
                if (string.IsNullOrWhiteSpace(e.Fps)) {
                    sb.Append(e.Fps);
                } else {
                    sb.Append(Convert.ToDouble(e.Fps) / 10);
                }
                sb.Append("\nTime - ");
                sb.Append(e.Time);
                sb.Append("\nSize - ");
                if (string.IsNullOrWhiteSpace(e.Size)) {
                    sb.Append(e.Size);
                } else {
                    sb.Append(ByteSize.FromKiloBytes(Convert.ToDouble(e.Size)).ToString("MB"));
                }
                sb.Append("\nBitrate - ");
                sb.Append(e.Bitrate);
                sb.Append("kbit/s\nSpeed - ");
                sb.Append(e.Speed);
                sb.Append("x\nQuantizer - ");
                sb.Append(e.Quantizer);
                sb.Append("\n");
                sb.Append(p);
                sb.Append("% Completed");

                //ByteSize.FromKiloBytes(Convert.ToInt32(e.Size)).ToString("MB")

                string msg = sb.ToString();
                /*try {
                    msg = String.Format("Processing from ts to mp4\nFrame - {0}\nFps - {1}\nTime - {2}\nSize - {3}\nBitrate - {4}\nSpeed - {5}\nQuantizer - {6}\n{7}% Completed",
                    e.Frame,
                    e.Fps,
                    e.Time,
                    ByteSize.FromKiloBytes(Convert.ToDouble(e.Size)).ToString("MB"),
                    e.Bitrate + " kbit/s",
                    e.Speed,
                    e.Quantizer,
                    p
                    );
                } catch (System.FormatException) {
                    msg = "nullllllllllllllllllllllll";
                }*/


                //MessageBox.Show(msg);

                
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() => {
                    ProcessText01 = "Converting and processing final output video";
                    ProcessText02 = msg;
                    Pbar01Value = p;
                }));
            };
            engine.VideoEncoded += (sender, e) => {
                //encoding finished. do something
                if (ct.IsCancellationRequested) {
                    engine.KillProcess();
                    ct.ThrowIfCancellationRequested();
                }
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() => {
                    ProcessText01 = "Converting and processing final output video Completed\nPlease wait the for process to complete";
                    ProcessText02 = "This may take few minutes";
                    Pbar01Value = 100;
                }));

            };
            engine.ErrorReceived += (sender, e) => {
                OnErrorDownloading(e.GetException());
            };
            //engine.Exited += (sender, e) => {
            //};
            ct.ThrowIfCancellationRequested();
            engine.DoWork(job);

            if (File.Exists(_merged_file_path)) {
                File.Delete(_merged_file_path);
            }


            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() => {
                ProcessText01 = "Converting and processing final output video Completed.\nFinal video is ready";
                ProcessText02 = "...";
                Pbar01Value = 100;
            }));
            ct.ThrowIfCancellationRequested();
            await Task.Delay(50);
            return _final_video_file_path;
        }


        public override void CanClose(Action<bool> callback) {
            callback(_canClose);
            if (! _canClose) {
                CancelDownload();
            }
        }


        private void CancelDownload() {
            MessageBoxResult result = MessageBox.Show("Dow you want to cancel the download?", "Confirmation", MessageBoxButton.YesNo);
            switch (result) {
                case MessageBoxResult.Yes:
                    OnCancelation();
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        //
        private void OnCancelation() {
            tokenSource.Cancel();
            Pbar02Visibility = Visibility.Collapsed;
            Pbar01Value = 0;
            ProcessText01 = "Cancelling the download\nPlease wait...";
            DownloadCompleteEvent msg = new DownloadCompleteEvent(false, true, "", "");
            SendResultAndClose(msg);
        }


        //
        private void OnVideoDownloadCompleted(string _final_video_path) {
            DownloadCompleteEvent msg = new DownloadCompleteEvent(true, false, _final_video_path, "");
            SendResultAndClose(msg);
        }

      
        //
        private void OnErrorDownloading(Exception e) {
            DownloadCompleteEvent msg = new DownloadCompleteEvent(false,false,"",e.Message);
            SendResultAndClose(msg);
        }


        private void SendResultAndClose(DownloadCompleteEvent _event) {
            _eventAggregator.PublishOnUIThread(_event);
            _canClose = true;
            this.TryClose();
        }
    }
}
