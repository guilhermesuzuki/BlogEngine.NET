angular.module('blogAdmin').controller('CustomThemesController', ["$rootScope", "$scope", "$location", "$filter", "dataService", function ($rootScope, $scope, $location, $filter, dataService) {
    $scope.items = [];
    $scope.customFields = [];
    $scope.editId = "";
    $scope.package = {};
    $scope.activeTheme = ActiveTheme;
    $scope.IsPrimary = $rootScope.SiteVars.IsPrimary == "True";
    $scope.fltr = 'themes';
    $scope.order = 'DownloadCount desc';
    $scope.sortingOrder = 'DownloadCount';
    $scope.reverse = true;
    $scope.selectedRating = 0;
    $scope.author = UserVars.Name;

    if ($location.path().indexOf("/custom/themes/gallery") == 0) {
        $scope.fltr = 'all';
        $scope.galleryFilter = 'themes';
    }

    $scope.load = function () {
        spinOn();
        dataService.getItems('/api/packages', { take: 0, skip: 0, filter: $scope.fltr, order: 'LastUpdated desc' })
            .then(function (response) {
                angular.copy(response.data, $scope.items);
                gridInit($scope, $filter);
                if ($scope.galleryFilter) {
                    $scope.setFilter();
                }
                var pkgId = getFromQueryString('pkgId');
                if (pkgId != null) {
                    $scope.query = pkgId;
                    $scope.search();
                }
                spinOff();
            }, function () {
                toastr.error($rootScope.lbl.errorLoadingPackages);
                spinOff();
            });
    }

    $scope.showInfo = function (id) {
        dataService.getItems('/api/packages/' + id)
            .then(function (response) {
                angular.copy(response.data, $scope.package);
                $scope.selectedRating = $scope.package.Rating;
                $scope.removeEmptyReviews();
            }, function () {
                toastr.error($rootScope.lbl.errorLoadingPackages);
            });
        $("#modal-info").modal();
    }

    $scope.showSettings = function (id) {
        $scope.editId = id;

        dataService.getItems('/api/customfields', { filter: 'CustomType == "THEME" && ObjectId == "' + id + '"' })
            .then(function (response) {
                angular.copy(response.data, $scope.customFields);
                $("#modal-settings").modal();
            }, function () {
                toastr.error($rootScope.lbl.errorLoadingCustomFields);
            });
    }

    $scope.setDefaultTheme = function (id) {
        spinOn();
        dataService.updateItem("/api/packages/settheme/" + id, id)
            .then(function (data) {
                ActiveTheme = id;
                $scope.activeTheme = id;
                toastr.success($rootScope.lbl.completed);
                $scope.load();
            }, function () {
                toastr.error($rootScope.lbl.failed);
                spinOff();
            });
    }

    $scope.setFilter = function () {
        if ($scope.galleryFilter == 'extensions') {
            $scope.gridFilter('PackageType', 'Extension', 'pub');
        }
        if ($scope.galleryFilter == 'themes') {
            $scope.gridFilter('PackageType', 'Theme', 'dft');
        }
    }

    $scope.checkStar = function (item, rating) {
        if (item === rating) {
            return true;
        }
        return false;
    }

    $scope.setRating = function (rating) {
        $scope.selectedRating = rating;
    }

    $scope.submitRating = function () {
        var author = $("#txtAuthor").val().length > 0 ? $("#txtAuthor").val() : $scope.author;
        var review = { "Name": author, "Rating": $scope.selectedRating, "Body": $("#txtReview").val() };

        dataService.updateItem("/api/packages/rate/" + $scope.package.Extra.Id, review)
            .then(function (response) {
                //if (data != null) {
                //    data = JSON.parse(data);
                //}
                if (response.data.length === 0) {
                    toastr.success($rootScope.lbl.completed);
                }
                else {
                    toastr.error(response.data);
                }
                $("#modal-info").modal("hide");
            }, function () {
                toastr.error($rootScope.lbl.failed);
                $("#modal-info").modal("hide");
            });
    }

    $scope.sortBy = function (ord) {
        $scope.sortingOrder = ord;
        $scope.reverse = true;
        $scope.load();
    }

    $scope.processChecked = function (action) {
        processChecked("/api/packages/processchecked/", action, $scope, dataService);
    }

    $scope.save = function () {
        spinOn();
        dataService.updateItem("/api/customfields", $scope.customFields)
            .then(function (data) {
                $("#modal-settings").modal('hide');
                toastr.success($rootScope.lbl.completed);
                $scope.load();
            }, function () {
                toastr.error($rootScope.lbl.updateFailed);
                spinOff();
                $("#modal-settings").modal('hide');
            });
    }

    $scope.installPackage = function (pkgId) {
        spinOn();
        dataService.updateItem("/api/packages/install/" + pkgId, pkgId)
            .then(function (data) {
                toastr.success($rootScope.lbl.completed);
                $scope.load();
            }, function () {
                toastr.error($rootScope.lbl.failed);
                spinOff();
            });
    }

    $scope.uninstallPackage = function (pkgId) {
        spinOn();
        dataService.updateItem("/api/packages/uninstall/" + pkgId, pkgId)
            .then(function (data) {
                toastr.success($rootScope.lbl.completed);
                $scope.load();
            }, function () {
                toastr.error($rootScope.lbl.failed);
                spinOff();
            });
    }

    $scope.upgradePackage = function (pkgId) {
        spinOn();
        dataService.updateItem("/api/packages/uninstall/" + pkgId, pkgId)
            .then(function (data) {
                $scope.installPackage(pkgId);
            }, function () {
                toastr.error($rootScope.lbl.failed);
                spinOff();
            });
    }

    $scope.removeEmptyReviews = function () {
        if ($scope.package.Extra != null && $scope.package.Extra.Reviews != null) {
            var reviews = [];
            for (var i = 0; i < $scope.package.Extra.Reviews.length; i++) {
                var review = $scope.package.Extra.Reviews[i];
                if (review.Body.length > 0) {
                    reviews.push(review);
                }
            }
            $scope.package.Extra.Reviews = reviews;
        }
    }

    $scope.load();

    $(document).ready(function () {
        bindCommon();
    });
}]);