angular.module("umbraco.directives").directive('selectWithOther', function () {
    return {
        restrict: 'EA',
        templateUrl: '/App_Plugins/uMirror/backoffice/uMirror/umirror.selectdirective.html',
        replace: true,
        scope: {
            items: '=',
            value: '='
        },
        link: function (scope, element, attrs, ctrl) {
            scope.elements = angular.copy(scope.items);
            scope.elements.push({ Name: "Other...", Value: scope.value });

            var i = 0, len = scope.elements.length;

            for (; i < len; i++) {
                scope.elements[i].Id = i;
            }

            scope.result = ctrl.result;
            scope.result.selectedElement = scope.elements[ctrl.getByProperty(scope.elements, "Value", scope.value)].Id;
            scope.result.otherValue = scope.value;
        },
        controller: function ($scope) {
            $scope.$watch(function ($scope) { return $scope.result.otherValue },
                function (newValue, oldValue) {
                    if (!!newValue && newValue != oldValue) {
                        $scope.value = newValue;
                    }
                }
            );

            $scope.$watch(function ($scope) { return $scope.result.selectedElement },
                function (newValue, oldValue) {
                    if (newValue !== oldValue) {
                        $scope.value = $scope.elements[newValue].Value;
                    }
                }
            );

            this.result = {};
            this.getByProperty = function (array, property, value) {
                if (array) {
                    var i = 0, len = array.length;
                    for (; i < len; i++) {
                        if (array[i][property] == value) {
                            return i;
                        }
                    }
                }
                return null;
            }
        }
    };
});
