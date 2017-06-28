using umbraco.interfaces;

namespace uMirror.core
{

    public class StartAction : IAction
    {

        public char Letter
        {
            get { return default(char); }
        }

        public bool ShowInNotifier
        {
            get { return false; }
        }

        public bool CanBePermissionAssigned
        {
            get { return false; }
        }

        public string Icon
        {
            get { return "next"; }
        }

        public string Alias
        {
            get { return "start"; }
        }

        public string JsFunctionName
        {
            get { return ""; }
        }

        public string JsSource
        {
            get { return ""; }
        }

    }

}