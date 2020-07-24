using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HSLDownloader_m3u8_ts.ffmpeg_wrapper {
    public class EncodingEngine {
        private Process _process;
        private string _encoderPath;

        public event EventHandler<EncodedEventArgs> VideoEncoded;
        public event EventHandler<EncodingEventArgs> VideoEncoding;
        public event EventHandler<ExitedEventArgs> Exited;
        public event EventHandler<ErrorEventArgs> ErrorReceived;

        public EncodingEngine(string encoderPath) {
            _encoderPath = encoderPath;
            _process = new Process();

        }

        public void Cancel() {

            StreamWriter myStreamWriter = this._process.StandardInput;
            myStreamWriter.WriteLine("q");


        }

        public void KillProcess() {
            try {
                this._process.Kill();
                this._process.Dispose();
            } catch (Exception) {

            }
        }

        public void DoWork(EncodingJob encodingJob) {

            this._process.EnableRaisingEvents = true;

            this._process.OutputDataReceived += new DataReceivedEventHandler(this.GetStandardOutputDataReceived);

            //subscribe to event
            this._process.ErrorDataReceived += new DataReceivedEventHandler(this.GetStandardErrorDataReceived);

            this._process.Exited += new EventHandler(ProcessExited);

            this._process.StartInfo.FileName = _encoderPath;

            this._process.StartInfo.Arguments = encodingJob.Arguments;

            this._process.StartInfo.UseShellExecute = false;
            this._process.StartInfo.RedirectStandardError = true;
            this._process.StartInfo.RedirectStandardOutput = true;
            this._process.StartInfo.RedirectStandardInput = true;
            this._process.StartInfo.CreateNoWindow = true;
            this._process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            this._process.Start();
            this._process.BeginErrorReadLine();
            this._process.BeginOutputReadLine();

            this._process.WaitForExit();
            this._process.Close();

            //raise event
            OnVideoEncoded(new EncodedEventArgs() { EncodingJob = encodingJob });

        }


        protected virtual void OnVideoEncoded(EncodedEventArgs e) {

            VideoEncoded?.Invoke(this, e);

        }


        protected virtual void OnVideoEncoding(EncodingEventArgs e) {

            VideoEncoding?.Invoke(this, e);

        }

        protected virtual void OnExit(ExitedEventArgs e) {

            Exited?.Invoke(this, e);   

        }


        protected virtual void OnErrorReceived(ErrorEventArgs e) {

            ErrorReceived?.Invoke(this, e);

        }


        private void ProcessExited(object sender, EventArgs e) {
            ExitedEventArgs exitedEventArgs = new ExitedEventArgs();
            string _exit_code;
            try {
                _exit_code = _process.ExitCode.ToString();
            }catch(Exception ex) {
                _exit_code = "0";
            }
            
            exitedEventArgs.ExitCode = _exit_code;
            OnExit(exitedEventArgs);
                
            /*try {
                OnExit(new ExitedEventArgs() { ExitCode = _process.ExitCode.ToString() });
            } catch (Exception) {

            }*/


        }

        private void GetStandardErrorDataReceived(object sender, DataReceivedEventArgs e) {
            //MessageBox.Show(e.Data);
            //raise event        
            OnVideoEncoding(new EncodingEventArgs() {

                Frame = e.Data.GetRegexValue(RegexKey.Frame, RegexGroup.Two),
                Fps = e.Data.GetRegexValue(RegexKey.Fps, RegexGroup.Two),
                Size = e.Data.GetRegexValue(RegexKey.Size, RegexGroup.Two),
                Time = e.Data.GetRegexValue(RegexKey.Time, RegexGroup.Two),
                Bitrate = e.Data.GetRegexValue(RegexKey.Bitrate, RegexGroup.Two),
                Speed = e.Data.GetRegexValue(RegexKey.Speed, RegexGroup.Two),
                Quantizer = e.Data.GetRegexValue(RegexKey.Quantizer, RegexGroup.Two),
                Progress = e.Data.GetRegexValue(RegexKey.Time, RegexGroup.Two).ParseTotalSeconds(),
                Data = e.Data
            });

        }

        private void GetStandardOutputDataReceived(object sender, DataReceivedEventArgs e) {
            //Console.WriteLine(e.Data);
            //MessageBox.Show(e.Data);
        }


    }

    public class EncodingEventArgs : EventArgs {
        public string Frame { get; set; }
        public string Fps { get; set; }
        public string Size { get; set; }
        public string Time { get; set; }
        public string Bitrate { get; set; }
        public string Speed { get; set; }
        public string Quantizer { get; set; }
        public string Data { get; set; }
        public double Progress { get; set; }
    }

    public class EncodedEventArgs : EventArgs {
        public EncodingJob EncodingJob { get; set; }
    }

    public class ExitedEventArgs : EventArgs {
        public string ExitCode { get; set; }

    }
}
