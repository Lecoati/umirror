angular.module("umbraco").controller("uMirror.startController",
function ($scope, $location, umirrorResources, navigationService) {

    $scope.start = function (id) {
        umirrorResources.start(id.replace("project_", ""))
        navigationService.syncTree({ tree: 'uMirror', path: [-1], forceReload: true });
        navigationService.hideNavigation();
        $location.path("/developer/uMirror/edit/" + id);
    };

    $scope.cancelDelete = function () {
        navigationService.hideNavigation();
    };

});