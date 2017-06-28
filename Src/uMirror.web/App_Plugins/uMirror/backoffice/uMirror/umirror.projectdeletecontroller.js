angular.module("umbraco").controller("uMirror.projectDeleteController",
function ($scope, umirrorResources, navigationService) {

    $scope.delete = function (id) {
        if (id.indexOf("project_") !== -1) {
            umirrorResources.deleteProjectById(id.replace("project_", "")).then(function () {
                navigationService.syncTree({ tree: 'uMirror', path: [-1], forceReload: true });
                navigationService.hideNavigation();
            });
        }
        else {
            umirrorResources.deleteNodeById(id.replace("node_", "")).then(function () {
                navigationService.syncTree({ tree: 'uMirror', path: [-1, -1], forceReload: true });
                navigationService.hideNavigation();
            });
        }
    };

    $scope.cancelDelete = function () {
        navigationService.hideNavigation();
    };

});