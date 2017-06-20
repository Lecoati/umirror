angular.module("umbraco").controller("umirror.nodeeditcontroller",
	function ($scope, $routeParams, umirrorResources, notificationsService, navigationService, dialogService) {

	    $scope.loaded = false;

	    if ($routeParams.create && $routeParams.id.indexOf("project_") !== -1) {
	        umirrorResources.getProjectById($routeParams.id.replace("project_", "")).then(function (response) {
	            $scope.parent = response.data;
	            $scope.node = {
	                id: -1,
	                ParentId: -1,
	                ProjectId: $routeParams.id.replace("project_", ""),
	                Properties: []
	            };
	            $scope.init();
	        });
	    }
	    else if ($routeParams.create && $routeParams.id.indexOf("node_") !== -1) {
	        umirrorResources.getNodeById($routeParams.id.replace("node_", "")).then(function (response) {
	            $scope.parent = response.data;
	            $scope.node = {
	                id : -1,
	                ParentId: $routeParams.id.replace("node_", ""),
	                ProjectId: $scope.parent.ProjectId,
	                Properties: []
	            };
	            $scope.init();
	        });
	    }
	    else if ($routeParams.id.indexOf("node_") !== -1) {
	        umirrorResources.getNodeById($routeParams.id.replace("node_", "")).then(function (response) {
	            $scope.node = response.data;
	            umirrorResources.getProperties($scope.node.UmbDocumentTypeAlias).then(function (response) {
	                $scope.Properties = response.data;
	            });
	            $scope.init();
	        });
	    }

	    $scope.init = function () {
	        umirrorResources.getDocumentTypes().then(function (response) {
	            $scope.documentTypes = response.data;
	            $scope.loaded = true;
	        });
	        umirrorResources.getElements($scope.node.ProjectId, true).then(function (response) {
	            $scope.elementsWithPrefix = response.data;
	        });
	        umirrorResources.getElements($scope.node.ProjectId, false).then(function (response) {
	            $scope.elements = response.data;
	        });
	    };

	    $scope.documentTypeChange = function () {
	        angular.forEach($scope.documentTypes, function (value, key) {
	            if (value.Alias == $scope.node.UmbDocumentTypeAlias) {
	                $scope.node.UmbDocumentTypeIcon = value.Icon
	            }
	        });
	        umirrorResources.getProperties($scope.node.UmbDocumentTypeAlias).then(function (response) {
	            $scope.Properties = response.data;
	            angular.forEach($scope.Properties, function (value, key) {
	                $scope.node.Properties.push({
	                    UmbPropertyAlias: value.Alias,
	                })
	            });
	        });
	    }

	    $scope.save = function (node) {
	        umirrorResources.saveNode(node).then(function (response) {
	            $scope.node = response.data;
	            $scope.nodeForm.$dirty = false;
	            navigationService.syncTree({ tree: 'uMirror', path: [-1, -1], forceReload: true });
	            notificationsService.success("Success", $scope.node.UmbDocumentTypeAlias + " has been saved");
	        });
	    };

	});