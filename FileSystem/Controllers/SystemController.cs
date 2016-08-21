using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using FileSystem.Models;

namespace FileSystem.Controllers {
    public class SystemController : ApiController {
        
         /**
         * Return folder content according to received path. 
         * If the path is not provided it returns the content of the first disk.
         */

        [HttpGet]
        public Structure GetFiles([FromUri] string path = null) {
            if (path == null) {
                DriveInfo[] disks = DriveInfo.GetDrives();
                path = disks[0].Name;
            }
            return BiuldStructure(path);
        }

        /**
         * Returns a list with disk names
         */

        [HttpGet]
        public Drives GetDisks() {
            DriveInfo[] disks = DriveInfo.GetDrives();
            string[] driveNames = new string[disks.Length];
            for (var i = 0; i < disks.Length; i++) {
                driveNames[i] = disks[i].Name;
            }
            Drives drives = new Drives {DiskNames = driveNames};
            return drives;
        }

        /**
         * Calculates files in folders and subfolders. 
         * Split them into 3 groups: small - up to 10Mb,
         * medium - from 10Mb to 50Mb, big - more than 100Mb
         */

        [HttpGet]
        public Files CalculateFiles(string path, CancellationToken token) {
            return Task.Run(() => FilesCalculation(token, path), token).Result;
        }

        private Files FilesCalculation(CancellationToken token, string path) {
            int smallFiles = 0;
            int mediumFiles = 0;
            int bigFiles = 0;
            List<string> folders = new List<string>(Directory.GetDirectories(path));

            while ((folders.Count > 0) && (!token.IsCancellationRequested)) {
                token.ThrowIfCancellationRequested();

                try {
                    var folder = folders[0];
                    folders.AddRange(Directory.GetDirectories(folder));

                    foreach (var file in new DirectoryInfo(folder).GetFiles()) {
                        checkFileSize(file, ref smallFiles, ref mediumFiles, ref bigFiles);
                    }
                }
                catch { }
                folders.RemoveAt(0);
            }

            return new Files() {Small = smallFiles, Medium = mediumFiles, Big = bigFiles};
        }

        private Structure BiuldStructure(string path) {
            if ("\\..".Equals(path.Substring(path.Length - 3))) {
                path = path.Substring(0, path.Length - 4);
                int pos = path.LastIndexOf("\\", StringComparison.Ordinal);
                path = path.Substring(0, pos + 1);
            }
            Structure structure = new Structure();
            string[] folders = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);

            getElementName(ref folders);
            getElementName(ref files);

            if (!"\\".Equals(path.Substring(path.Length - 1))) {
                structure.CurrentPath = path + "\\";
            }
            else {
                structure.CurrentPath = path;
            }

            structure.Folders = folders;
            structure.Files = files;

            return structure;
        }

        private void getElementName(ref string[] elements) {
            for (var i = 0; i < elements.Length; i++) {
                var pos = elements[i].LastIndexOf("\\", StringComparison.Ordinal);
                elements[i] = elements[i].Substring(pos + 1);
            }
        }

        private void checkFileSize(FileInfo fileInfo, ref int smallFiles, ref int mediumFiles, ref int bigFiles) {
            if (fileInfo.Length <= 10000000) {
                smallFiles++;
            }
            else if (fileInfo.Length > 10000000 && fileInfo.Length <= 50000000) {
                mediumFiles++;
            }
            else if (fileInfo.Length >= 100000000) {
                bigFiles++;
            }
        }
    }
}