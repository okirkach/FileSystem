var app = angular.module("FileSystem", ["ngResource"])
    .controller("FileSystemCtrl", [
        "$resource", "$timeout", function($resource, $timeout) {
            var fileSystem = this;
            fileSystem.folders = [];
            fileSystem.files = [];
            fileSystem.isRoot = true;
            fileSystem.loading = true;

            //if the current path is shorter than 4 characters, it means that we are in root folder
            var isRootFunction = function(path) {
                if (path.length < 4) {
                    return true;
                } else {
                    return false;
                }
            };

            //return array of disks which are installed on the server
            var getDisks = function() {
                $resource("/api/system/getDisks/").get(function(resp) {
                    fileSystem.disks = resp.DiskNames;
                    fileSystem.loading = false;
                });

                
            };

            /**
             * path: path which is necesarry to check
             * success: set fileSystem parameter to all folders and files according to path and files quantities
            */
            var getStructure = function(path) {
                fileSystem.loading = true;
                fileSystem.calculationCanceled = false;
                var timeout = $timeout(function() {
                    fileSystem.cancelCalculation = true;
                }, 5000);
                var structure = $resource("/api/system/getFiles/?path=:path", { path: "@path" });
                structure.get({ path: path }, function(resp) {
                    fileSystem.currentPath = resp.CurrentPath;
                    fileSystem.folders = resp.Folders;
                    fileSystem.files = resp.Files;
                    fileSystem.isRoot = isRootFunction(fileSystem.currentPath);
                    fileSystem.smallFiles = resp.SmallFiles;
                    fileSystem.mediumFiles = resp.MediumFiles;
                    fileSystem.largeFiles = resp.LargeFiles;
                    fileSystem.loading = false;
                    fileSystem.cancelCalculation = false;
                    $timeout.cancel(timeout);
                }, function(error) {
                    fileSystem.loading = false;
                    fileSystem.cancelCalculation = false;
                    $timeout.cancel(timeout);

                    if (error.data.ExceptionType == "System.UnauthorizedAccessException") {
                        bootbox.alert("You do not have rights to get access to this folder.");
                    } else if (error.data.ExceptionType == "System.IO.IOException") {
                        bootbox.alert("An error occurred during disk reading. Disk is not ready.");
                    } else {
                        bootbox.alert("Error during reading. Try a little bit later.");
                    }
                });


            };

            //This function assign to click event on the folder name
            fileSystem.getStructure = function(folder) {
                if (folder == undefined) {
                    folder = "..";
                }
                var path = fileSystem.currentPath + folder;
                getStructure(path);
            };

            //This function assign to click event on the disk icon
            fileSystem.changeDisk = function(disk) {
                getStructure(disk);
            };

            //This function assign to click event on the disk refresh button
            fileSystem.getDisks = function() {
                getDisks();
            };

            //This function hide the window with question about cancellation of file calculation
            fileSystem.refuseCancelation = function() {
                fileSystem.cancelCalculation = false;
            };

            //This function send request to cancell the files calculation
            fileSystem.cancel = function() {
                $resource("/api/system/cancelCalculation/").get(function() {
                    fileSystem.calculationCanceled = true;
                });
            };

            //This function run on the application start up to get all disks
            getDisks();

            
        }
    ]);