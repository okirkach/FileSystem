var app = angular.module("FileSystem", ["ngResource"])
    .controller("FileSystemCtrl", [
        "$resource", "$timeout", "$q", "$http", function($resource, $timeout, $q, $http) {
            var fileSystem = this;
            var cancelRequest = $q.defer();
            fileSystem.folders = [];
            fileSystem.files = [];
            fileSystem.isRoot = true;
            fileSystem.loading = true;
            fileSystem.calculationInProcess = false;

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
                var structure = $resource("/api/system/getFiles/?path=:path", { path: "@path" });

                structure.get({ path: path }, function (resp) {
                    fileSystem.currentPath = resp.CurrentPath;
                    fileSystem.folders = resp.Folders;
                    fileSystem.files = resp.Files;
                    fileSystem.isRoot = isRootFunction(fileSystem.currentPath);
                    fileSystem.loading = false;
                }, function(error) {
                    fileSystem.loading = false;

                    if (error.data.ExceptionType == "System.UnauthorizedAccessException") {
                        bootbox.alert("You do not have rights to get access to this folder.");
                    } else if (error.data.ExceptionType == "System.IO.IOException") {
                        bootbox.alert("An error occurred during disk reading. Disk is not ready.");
                    } else {
                        bootbox.alert("Error during reading. Try a little bit later.");
                    }
                });

            };


            /**
             * Files calcultion in current folder. 
             * If the old calculation is still in process we stop it and start a new one.
             */
            var getFiles = function (path) {
                if (fileSystem.calculationInProcess) {
                    cancelRequest.resolve("New calculation started");
                }
                fileSystem.calculationInProcess = true;
                cancelRequest = $q.defer();
                $http({
                    method: "GET",
                    url: "/api/system/CalculateFiles/?path=" + path,
                    timeout: cancelRequest.promise
                }).then(function success(response) {
                    fileSystem.smallFiles = response.data.Small;
                    fileSystem.mediumFiles = response.data.Medium;
                    fileSystem.largeFiles = response.data.Big;
                    fileSystem.calculationInProcess = false;
                });

            }

            //This function assign to click event on the folder name
            fileSystem.getStructure = function(folder) {
                if (folder == undefined) {
                    folder = "..";
                }
                var path = fileSystem.currentPath + folder;
                getStructure(path);
                getFiles(path);
            };

            //This function assign to click event on the disk icon
            fileSystem.changeDisk = function (disk) {
                getStructure(disk);
                getFiles(disk);
            };

            //This function assign to click event on the disk refresh button
            fileSystem.getDisks = function() {
                getDisks();
            };

            //This function run on the application start up to get all disks
            getDisks();

            
        }
    ]);