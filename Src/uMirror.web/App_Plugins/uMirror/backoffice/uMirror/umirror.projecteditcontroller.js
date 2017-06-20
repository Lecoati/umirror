﻿angular.module("umbraco").controller("umirror.projecteditcontroller",
	function ($scope, $routeParams, umirrorResources, notificationsService, navigationService, dialogService) {

	    $scope.loaded = false;  

	    $scope.proxyMethods = umirrorResources.getProxyMethods();

	    if ($routeParams.id == -1) {
	        $scope.project = {};
	        $scope.isProject = true;
	        $scope.loaded = true;
	    }
	    else if ($routeParams.id.indexOf("project_") !== -1 && !$routeParams.create) {
	        umirrorResources.getProjectById($routeParams.id.replace("project_","")).then(function (response) {
	            $scope.project = response.data;
	            $scope.loaded = true;
	        });
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


	});