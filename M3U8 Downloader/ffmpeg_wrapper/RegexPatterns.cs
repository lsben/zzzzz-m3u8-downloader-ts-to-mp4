using System.Collections.Generic;

namespace HSLDownloader_m3u8_ts.ffmpeg_wrapper {
    public class RegexPatterns {
        private Dictionary<string, RegexSearchPattern> _patterns;

        public RegexPatterns() {
            _patterns = new Dictionary<string, RegexSearchPattern>();


            _patterns.Add("Frame", new RegexSearchPattern() { OriginalKey = "frame=", ReplaceKey = "frame= ", RegexSearchKey = @"(frame=) +(\d+)" });
            _patterns.Add("Fps", new RegexSearchPattern() { OriginalKey = "fps=", ReplaceKey = "fps= ", RegexSearchKey = @"(fps=) +(\d+)" });
            _patterns.Add("Time", new RegexSearchPattern() { OriginalKey = "time=", ReplaceKey = "time= ", RegexSearchKey = @"(time=) +(\d+:\d+:\d+.\d+)" });
            _patterns.Add("Size", new RegexSearchPattern() { OriginalKey = "size=", ReplaceKey = "size= ", RegexSearchKey = @"(size=) +(\d+)" });
            _patterns.Add("Bitrate", new RegexSearchPattern() { OriginalKey = "bitrate=", ReplaceKey = "bitrate= ", RegexSearchKey = @"(bitrate=) +(\d+.\d+)" });
            _patterns.Add("Speed", new RegexSearchPattern() { OriginalKey = "speed=", ReplaceKey = "speed= ", RegexSearchKey = @"(speed=) +(\d+.\d+)" });
            _patterns.Add("Quantizer", new RegexSearchPattern() { OriginalKey = "q=", ReplaceKey = "q= ", RegexSearchKey = @"(q=) +(\d+\.\d+)" });

        }


        public RegexSearchPattern GetValue(RegexKey regexKey) {

            RegexSearchPattern regexSearchPattern = new RegexSearchPattern();

            _patterns.TryGetValue(regexKey.ToString(), out regexSearchPattern);

            return regexSearchPattern;
        }

    }
}