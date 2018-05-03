﻿
angular.module('blogAdmin').controller('BlogListController', ["$rootScope", "$scope", "$filter", "dataService", function ($rootScope, $scope, $filter, dataService) {
    $scope.items = [];
    $scope.editItem = {};
    $scope.newItem = {};
    $scope.modalTitle = $rootScope.lbl.addNewBlog;
    $scope.focusInput = false;
    $scope.blogsPage = true;

    $scope.modalNew = function () {
        $scope.modalTitle = $rootScope.lbl.addNewBlog;
        $scope.editItem = {};
        $("#modal-add").modal();
        $scope.focusInput = true;
    }

    $scope.modalEdit = function (id) {
        $scope.modalTitle = $rootScope.lbl.editExistingBlog;
        spinOn();
        dataService.getItems('/api/blogs/' + id)
            .then(function (response) {
                angular.copy(response.data, $scope.editItem);
                $("#modal-edit").modal();
                spinOff();
            }, function () {
                toastr.error($rootScope.lbl.errorLoadingBlog);
                spinOff();
            });
    }

    $scope.load = function (callback) {
        dataService.getItems('/api/blogs', { take: 0, skip: 0, filter: "1 == 1", order: "Name" })
            .then(function (response) {
                angular.copy(response.data, $scope.items);
                gridInit($scope, $filter);
                callback;
                spinOff();
            }, function () {
                toastr.error($rootScope.lbl.errorLoadingBlogs);
            });
    }

    $scope.save = function () {
        spinOn();
        dataService.updateItem("/api/blogs/update/item", $scope.editItem)
            .then(function (data) {
                toastr.success($rootScope.lbl.blogSaved);
                $scope.load();
                spinOff();
                $("#modal-edit").modal('hide');
            }, function () {
                toastr.error($rootScope.lbl.failedAddingNewRole);
                spinOff();
                $("#modal-edit").modal('hide');
            });
    }

    $scope.saveNew = function () {
        if (!$('#form').valid()) {
            return false;
        }
        spinOn();
        dataService.addItem("/api/blogs", $scope.newItem)
            .then(function (data) {
                toastr.success($rootScope.lbl.blogAddedShort);
                $scope.newItem = {};
                $scope.load();
                spinOff();
                $("#modal-add").modal('hide');
                $scope.focusInput = false;
            }, function (response) {
                toastr.error(response.data);
                spinOff();
                $("#modal-add").modal('hide');
                $scope.focusInput = false;
            });
    }

    $scope.processChecked = function (action, itemsChecked) {
        if (itemsChecked) {
            processChecked("/api/blogs/processchecked/", action, $scope, dataService);
        }
    }

    $scope.load();

    $(document).ready(function () {
        $('#form').validate({
            rules: {
                txtBlogName: { required: true },
                txtUserName: { required: true },
                txtEmail: { required: true, email: true },
                txtPassword: { required: true },
                txtConfirmPassword: { required: true, equalTo: '#txtPassword' }
            }
        });
    });


    $(document).ready(function () {
        bindCommon();
    });
}]);
