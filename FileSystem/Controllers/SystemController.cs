using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;
using FileSystem.Models;

namespace FileSystem.Controllers {
    public class SystemController : ApiController {
        private static bool _cancelCalculation;
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
         * Used to cancel the file calculation
         */

        [HttpGet]
        public void CancelCalculation() {
            _cancelCalculation = true;
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

            int small = 0;
            int medium = 0;
            int big = 0;
            List<string> exceptionList = new List<string>();

            _cancelCalculation = false;
            CalculateFiles(path, ref small, ref medium, ref big, ref exceptionList);

            structure.SmallFiles = small;
            structure.MediumFiles = medium;
            structure.LargeFiles = big;
            structure.Exceptions = exceptionList;

            return structure;
        }

        private void getElementName(ref string[] elements) {
            for (var i = 0; i < elements.Length; i++) {
                var pos = elements[i].LastIndexOf("\\", StringComparison.Ordinal);
                elements[i] = elements[i].Substring(pos + 1);
            }
        }

        private void CalculateFiles(string path, ref int smallFiles, ref int mediumFiles, ref int bigFiles,
            ref List<string> exceptions) {
            if (_cancelCalculation) {
                return;
            }
            foreach (var file in new DirectoryInfo(path).GetFiles()) {
                try {
                    checkFileSize(file, ref smallFiles, ref mediumFiles, ref bigFiles);
                }
                catch (Exception e) {
                    exceptions.Add(file.FullName);
                }
            }

            foreach (var folder in Directory.GetDirectories(path)) {
                if (_cancelCalculation) {
                    break;
                }
                try {
                    CalculateFiles(folder, ref smallFiles, ref mediumFiles, ref bigFiles, ref exceptions);
                }
                catch (Exception e) {
                    exceptions.Add(folder);
                }
            }
        }

        private void checkFileSize(FileInfo fileInfo, ref int smallFiles, ref int mediumFiles, ref int bigFiles) {
            if (_cancelCalculation) {
                return;
            }
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