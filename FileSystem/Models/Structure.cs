using System.Collections.Generic;

namespace FileSystem.Models {
    public class Structure {
        public string CurrentPath { get; set; }
        public string[] Folders { get; set; }
        public string[] Files { get; set; }
    }
}