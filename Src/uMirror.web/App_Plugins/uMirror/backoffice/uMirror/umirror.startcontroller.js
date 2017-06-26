angular.module("umbraco").controller("uMirror.startController",
    function ($scope, $location, $window, umirrorResources, navigationService) {
        $scope.start = function (id) {
            $location.path("/developer/uMirror/edit/" + id);
        umirrorResources.start(id.replace("project_", ""))
        navigationService.syncTree({ tree: 'uMirror', path: [-1], forceReload: true });
        navigationService.hideNavigation();
        $location.path("/developer/uMirror/edit/" + id);
        $window.location.reload();
    };

    $scope.cancelDelete = function () {
        navigationService.hideNavigation();
    };

});