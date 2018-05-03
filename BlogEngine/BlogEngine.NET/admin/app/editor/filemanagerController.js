angular.module('blogAdmin').controller('FileManagerController', ["$rootScope", "$scope", "$location", "$filter", "$log", "dataService", function ($rootScope, $scope, $location, $filter, $log, dataService) {
    $scope.data = dataService;
    $scope.items = [];
    $scope.itemsPerPage = 45; // page size - pass into grid on init
    $scope.sortingOrder = 'SortOrder';
    $scope.reverse = false;
    $scope.id = {};
    $scope.file = {};
    $scope.dirName = '';    
    $scope.root = $rootScope.SiteVars.ApplicationRelativeWebRoot;
    $scope.rootStorage = SiteVars.BlogStorageLocation + SiteVars.BlogFilesFolder;
    $scope.currentPath = '/';
    $scope.focusInput = false;

    $scope.load = function (path) {
        var p = path ? { take: 0, skip: 0, path: path } : { take: 0, skip: 0 };
        dataService.getItems('/api/filemanager', p)
            .then(function (response) {
                angular.copy(response.data, $scope.items);
                gridInit($scope, $filter);
                $scope.currentPath = path ? path : $scope.rootStorage;
                $('#file-spinner').hide();
            }, function (data) {
                toastr.error($rootScope.lbl.Error);
                $('#file-spinner').hide();
            });
    }

    $scope.processChecked = function (action) {
        var i = $scope.items.length;
        var checked = [];
        while (i--) {
            var item = $scope.items[i];
            if (item.IsChecked === true) {
                checked.push(item);
            }
        }
        if (checked.length < 1) {
            return false;
        }

        if (action === "append") {
            var j = checked.length;
            while (j--) {
                var item = checked[j];
                var editorHtml = editorGetHtml();
                var tag = item.HTML;

                editorSetHtml(editorHtml + tag);
            }
            toastr.success($rootScope.lbl.completed);
            $("#modal-file-manager").modal('hide');
        }
        if (action === "delete") {
            spinOn();
            dataService.processChecked("/api/filemanager/processchecked/delete", checked)
                .then(function (data) {
                    $scope.load($scope.currentPath);
                    gridInit($scope, $filter);
                    toastr.success($rootScope.lbl.completed);
                    if ($('#chkAll')) {
                        $('#chkAll').prop('checked', false);
                    }
                    spinOff();
                }, function () {
                    toastr.error($rootScope.lbl.failed);
                    spinOff();
                });
        }
    }

    $scope.uploadFile = function (files) {
        var fd = new FormData();
        fd.append("file", files[0]);
        $('#file-spinner').show();

        dataService.uploadFile("/api/upload?action=filemgr&dirPath=" + $scope.currentPath, fd)
            .then(function (data) {
                $scope.load($scope.currentPath);
                gridInit($scope, $filter);
                toastr.success($rootScope.lbl.completed);
                $('#file-spinner').hide();
            }, function () {
                toastr.error($rootScope.lbl.failed);
                $('#file-spinner').hide();
            });
    }

    $scope.addFolder = function () {
        $("#modal-form").modal();
        $scope.focusInput = true;
    }

    $scope.createFolder = function () {
        if (!$('#form').valid()) {
            return false;
        }
        spinOn();
        dataService.updateItem("/api/filemanager/addfolder/add", { Name: $scope.dirName, FullPath: $scope.currentPath })
            .then(function (data) {
                $scope.load($scope.currentPath);
                gridInit($scope, $filter);
                toastr.success($rootScope.lbl.completed);
                spinOff();
                $("#modal-form").modal('hide');
            }, function () {
                toastr.error($rootScope.lbl.failed);
                spinOff();
            });
    }

    $(document).ready(function () {
        $('#form').validate({
            rules: {
                txtFolder: { required: true }
            }
        });
    });

    $scope.hasChecked = function () {
        var i = $scope.items.length;
        var checked = [];
        while (i--) {
            var item = $scope.items[i];
            if (item.IsChecked === true) {
                return true;
            }
        }
        return false;
    }

    $scope.insertFile = function (file) {
        // get hold on TinyMce editor and inject link
        var wm = top.tinymce.activeEditor.windowManager;
        wm.getParams().ed.insertContent(file.HTML);
        wm.getWindows()[0].close();
    }

    $scope.load('');
}]);
