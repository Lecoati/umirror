angular.module("umbraco").controller("umirror.projecteditcontroller",
	function ($scope, $routeParams, umirrorResources, notificationsService, navigationService, dialogService) {

        $scope.loaded = false;  
        $scope.appStart = false;
        
        var refreshAppState = function () {
            umirrorResources.getapplock().then(function (response) {
                if (response.data === "false") {
                    clearInterval($scope.refreshAppStateInterval);
                }
            });
            umirrorResources.getAppNum().then(function (response) {
                $scope.currentAppNum = response.data;
            });
        } 

        var init = function () {
            $scope.proxyMethods = umirrorResources.getProxyMethods();

            if ($routeParams.id == -1) {
                $scope.project = {};
                $scope.isProject = true;
                $scope.loaded = true;
            }
            else if ($routeParams.id.indexOf("project_") !== -1 && !$routeParams.create) {
                umirrorResources.getProjectById($routeParams.id.replace("project_", "")).then(function (response) {
                    $scope.project = response.data;
                    $scope.loaded = true;
                });
            }
        } 

	    $scope.setParent = function () {
	        dialogService.treePicker({
	            multiPicker: false,
	            section: "content",
	            treeAlias: "content",
	            startNodeId: -1,
	            callback: function (item) {
	                $scope.project.UmbRootId = item.id;
	                $scope.project.UmbRootNode = {
	                    Id: item.id,
	                    Icon: item.icon,
	                    Name: item.name
	                };
	            }
            });
	    };

	    $scope.save = function (project) {
	        umirrorResources.saveProject(project).then(function (response) {
	            $scope.project = response.data;
	            $scope.projectForm.$dirty = false;
	            navigationService.syncTree({ tree: 'uMirror', path: [-1, -1], forceReload: true });
	            notificationsService.success("Success", $scope.project.Name + " has been saved");
	        });
	    };

        umirrorResources.getapplock().then(function (response) {
            $scope.isAppLock = response.data;
            if (response.data === "true") {
                $scope.appStart = true;
                $scope.loaded = true;
                $scope.refreshAppStateInterval = setInterval(refreshAppState, 5000);
            }
            else {
                init();
            }
        });

	});