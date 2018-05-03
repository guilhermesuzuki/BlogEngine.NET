﻿angular.module('blogAdmin').controller('CategoriesController', ["$rootScope", "$scope", "$location", "$http", "$filter", "dataService", function ($rootScope, $scope, $location, $http, $filter, dataService) {
    $scope.items = [];
    $scope.lookups = [];
    $scope.category = {};
    $scope.id = ($location.search()).id;
    $scope.focusInput = false;

    if ($scope.id) {
        dataService.getItems('/api/categories', { Id: $scope.id })
            .then(
            function (response) {
                angular.copy(response.data, $scope.category);
            },
            function () {
                toastr.error($rootScope.lbl.errorLoadingCategory);
            });

        $("#modal-add-cat").modal();
        $scope.focusInput = true;
    }

    $scope.addNew = function () {
        $scope.clear();
        $("#modal-add-cat").modal();
        $scope.focusInput = true;
    }

    $scope.load = function () {
        dataService.getItems('/api/lookups')
            .then(function (response) { angular.copy(response.data, $scope.lookups); }, function () { toastr.error("Error loading lookups"); });

        var p = { skip: 0, take: 0 };
        dataService.getItems('/api/categories', p)
            .then(function (response) {
                angular.copy(response.data, $scope.items);
                gridInit($scope, $filter);
                spinOff();
            }, function () {
                toastr.error($rootScope.lbl.errorLoadingCategories);
            });
    }

    $scope.load();

    $scope.save = function () {
        if (!$('#form').valid()) {
            return false;
        }
        if ($scope.category.Id) {
            dataService.updateItem("/api/categories/update/" + $scope.category.Id, $scope.category)
                .then(function (data) {
                    toastr.success($rootScope.lbl.categoryUpdated);
                    $scope.load();
                }, function (response) { toastr.error(response.data); });
        }
        else {
            dataService.addItem("/api/categories", $scope.category)
                .then(function (response) {
                    toastr.success($rootScope.lbl.categoryAdded);
                    if (response.data.Id) {
                        angular.copy(response.data, $scope.category);
                        $scope.load();
                    }
                }, function (response) { toastr.error(response.data); });
        }
        $("#modal-add-cat").modal('hide');
        $scope.focusInput = false;
    }

    $scope.processChecked = function (action, itemsChecked) {
        if (itemsChecked) {
            processChecked("/api/categories/processchecked/", action, $scope, dataService);
        }
    }

    $scope.clear = function () {
        $scope.category = { "IsChecked": false, "Id": null, "Parent": null, "Title": "", "Description": "", "Count": 0 };
        $scope.id = null;
    }

    $(document).ready(function () {
        bindCommon();
        $('#form').validate({
            rules: {
                txtSlug: { required: true }
            }
        });
    });
}]);
