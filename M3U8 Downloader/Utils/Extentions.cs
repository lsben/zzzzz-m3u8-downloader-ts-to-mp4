using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3U8_Downloader.Utils {
    public class Extentions {

        public static string[] getTSFileLinkListFromPath(string path) {
            return detectTsLinks(File.ReadAllLines(path));
        }

        public static string[] getTSFileLinksFromContent(string content) {
            return detectTsLinks(stringToLines(content));
        }

        public static string[] detectTsLinks(string[] lines) {
            List<string> result = new List<string>();
            foreach (string line in lines) {
                if (!line.Contains("#EXT") && line.Contains("http")) {
                    //Parsing link.  remove it if needed
                    result.Add(parseLink(line));
                }
            }
            return result.ToArray();
        }

        public static string[] stringToLines(string source) {
            return source.Split(new[] { Environment.NewLine },StringSplitOptions.None);
        }


        public static string parseLink(string link) {
            if (!link.Contains("_unsec")) {
                List<string> list = new List<string>(link.Split('/'));
                list.Insert(list.Count - 1, "_unsec");
                return String.Join("/", list);
            } else {
                return link;
            }
        }

    }
}
