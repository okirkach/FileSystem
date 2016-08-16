using System.Collections.Generic;

namespace FileSystem.Models {
    public class Structure {
        public string CurrentPath { get; set; }
        public string[] Folders { get; set; }
        public string[] Files { get; set; }
        public int SmallFiles { get; set; }
        public int MediumFiles { get; set; }
        public int LargeFiles { get; set; }
        public List<string> Exceptions { get; set; }
    }
}