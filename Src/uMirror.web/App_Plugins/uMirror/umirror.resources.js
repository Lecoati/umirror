angular.module("umbraco.resources")
.factory("umirrorResources", function ($http) {
    return {
        getProjectById: function (id) {
            return $http.get("backoffice/uMirror/uMirrorApi/GetProjectById?id=" + id);
        },
        saveProject: function (project) {
            return $http.post("backoffice/uMirror/uMirrorApi/PostSaveProject", angular.toJson(project));
        },
        deleteProjectById: function (id) {
            return $http.delete("backoffice/uMirror/uMirrorApi/DeleteProjectById?id=" + id);
        },
        getNodeById: function (id) {
            return $http.get("backoffice/uMirror/uMirrorApi/GetNodeById?id=" + id);
        },
        saveNode: function (node) {
            return $http.post("backoffice/uMirror/uMirrorApi/PostSaveNode", angular.toJson(node));
        },
        deleteNodeById: function (id) {
            return $http.delete("backoffice/uMirror/uMirrorApi/DeleteNodeById?id=" + id);
        },
        getProxyMethods: function () {
            return $http.get("backoffice/uMirror/uMirrorApi/GetProxyMethods");
        },
        testProxyMethod: function (name) {
            return $http.get("backoffice/uMirror/uMirrorApi/GetTestProxyMethod?proxyMethodName=" + name);
        },
        getDocumentTypes: function () {
            return $http.get("backoffice/uMirror/uMirrorApi/GetDocumentTypes");
        },
        getElements: function (projectId, withPrefix) {
            return $http.get("backoffice/uMirror/uMirrorApi/GetElements?projectId=" + projectId + "&withPrefix=" + !withPrefix);
        },
        getProperties: function (alias) {
            return $http.get("backoffice/uMirror/uMirrorApi/GetProperties?docTypeAlias=" + alias);
        },
        start: function (id) {
            return $http.put("backoffice/uMirror/uMirrorApi/PutStart?id=" + id);
        },
        stop: function () {
            return $http.put("backoffice/uMirror/uMirrorApi/PutStop");
        },
        startMethod: function (assemblyRef) {
            return $http.get("backoffice/uMirror/uMirrorApi/StartMethod?assemblyRef=" + assemblyRef);
        },
        getAppNum: function () {
            return $http.get("backoffice/uMirror/uMirrorApi/GetAppNum");
        },
        getapplock: function () {
            return $http.get("backoffice/uMirror/uMirrorApi/Getapplock");
        }
    };
 });