using Lecoati.uMirror.Bll;
using Lecoati.uMirror.Core;
namespace Lecoati.uMirror
{
    public class loadNodeTasks : umbraco.interfaces.ITaskReturnUrl
    {

        #region ITaskReturnUrl Members

        public string ReturnUrl
        {
            get { return _returnUrl; }
        }

        #endregion

        #region ITask Members

        private string _alias;
        private int _parentID;
        private int _typeID;
        private int _userID;
        private string _returnUrl = "";

        public string Alias
        {
            get
            {
                return _alias;
            }
            set
            {
                _alias = value;
            }
        }

        public bool Delete()
        {
            new BllNode().DeleteNode(_parentID);
            _returnUrl = "umbraco/dashboard.aspx?app=uMirror";
            return true;
        }

        public int ParentID
        {
            get
            {
                return _parentID;
            }
            set
            {
                _parentID = value;
            }
        }

        public bool Save()
        {
            _returnUrl = "plugins/uMirror/Dialogs/editNode.aspx?parentID=" + _parentID + "&type=" + _typeID;
            return true;
        }

        public int TypeID
        {
            get
            {
                return _typeID;
            }
            set
            {
                _typeID = value;
            }
        }

        public int UserId
        {
            set { _userID = value; }
        }

        #endregion
    }
}