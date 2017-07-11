angular.module("umbraco").controller("umirror.projecteditcontroller",
    function ($scope, $routeParams, umirrorResources, notificationsService, navigationService, dialogService) {

        $scope.loaded = false;
        $scope.isAppLock = false;
        $scope.testing = false;
        $scope.canceled = false;
        $scope.log = [];

        var refreshAppState = function () {
            $scope.$apply(function () {
                umirrorResources.getapplock().then(function (response) {
                    $scope.isAppLock = JSON.parse(response.data);

                    if (!$scope.isAppLock) {
                        $scope.testing = false;
                        clearInterval($scope.refreshAppStateInterval);
                    }
                });

                umirrorResources.getAppNum().then(function (response) {
                    var currentAppNum = JSON.parse(response.data);

                    if ($scope.currentAppNum !== currentAppNum) {
                        $scope.log.push(currentAppNum);
                    }

                    $scope.currentAppNum = currentAppNum;
                });
            });
        }

        var init = function () {
            umirrorResources.getProxyMethods().then(function (response) {
                $scope.proxyMethods = response.data;
            });

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

        $scope.removeParent = function () {
            $scope.project.UmbRootId = -1;
            $scope.project.UmbRootNode = undefined;
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

        $scope.testProxyMethod = function (proxyMethodAssemblyRef) {
            var i = 0, len = $scope.proxyMethods.length, proxyMethodAssembly;
            for (i; i < len; i++) {
                if ($scope.proxyMethods[i].AssemblyRef === proxyMethodAssemblyRef) {
                    proxyMethodAssembly = $scope.proxyMethods[i].Name;
                }
            }

            $scope.testing = true;
            umirrorResources.testProxyMethod(proxyMethodAssembly);
            $scope.refreshAppStateInterval = setInterval(refreshAppState, 500);
        }

        $scope.save = function (project) {
            umirrorResources.saveProject(project).then(function (response) {
                $scope.project = response.data;
                $scope.projectForm.$dirty = false;
                navigationService.syncTree({ tree: 'uMirror', path: [-1, -1], forceReload: true });
                notificationsService.success("Success", $scope.project.Name + " has been saved");
            });
        };

        $scope.cancel = function () {
            umirrorResources.stop();
            $scope.canceled = true;
            notificationsService.error("Cancel", "The synchronization process for " + $scope.project.Name + " has been stopped");
        };

        umirrorResources.getapplock().then(function (response) {
            $scope.isAppLock = JSON.parse(response.data);
            if ($scope.isAppLock) {
                $scope.refreshAppStateInterval = setInterval(refreshAppState, 500);
                $scope.proxyMethods = umirrorResources.getProxyMethods();
                umirrorResources.getProjectById($routeParams.id.replace("project_", "")).then(function (response) {
                    $scope.project = response.data;
                    $scope.loaded = true;
                });
            }
            else {
                init();
            }
        });

    });