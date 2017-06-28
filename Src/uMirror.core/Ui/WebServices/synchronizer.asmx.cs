using System.ComponentModel;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.Threading;
using System;
using Lecoati.uMirror.Core;

namespace synchronizer
{

    /// <summary>
    /// Summary description for syndataext_syn
    /// </summary>
    [WebService(Namespace = "http://www.lecoati.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class services : System.Web.Services.WebService
    {

        [WebMethod]
        public string stop()
        {
            new Synchronizer().stop();
            return "";
        }

        [WebMethod]
        public string start(int synchronizerId)
        {
            Synchronizer sync = new Synchronizer();
            //Thread standardTCPServerThread = new Thread(sync.start);
            sync.context = HttpContext.Current;
            sync.projectid = synchronizerId;
            sync.currentUser = umbraco.BusinessLogic.User.GetCurrent();
            sync.start();
            //standardTCPServerThread.Start();
            return "start";
        }

        [WebMethod]
        public string startMethod(String assemblyRef)
        {
            try
            {
                Synchronizer sync = new Synchronizer();
                sync.startMethod(assemblyRef);
                return "done";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [WebMethod]
        public string getappnum()
        {

            string state = Synchronizer.appState.ToString();
            if (Synchronizer.appCancel) return "Canceling process, please wait ... ";

            if (Synchronizer.appLock)
                return /*state + " ( " + */ "skipped: <b><span style=\"color:green\">" + Synchronizer.appNumSki.ToString() +
                        " </span></b>updated: <b><span style=\"color:green\">" + Synchronizer.appNumUpd.ToString() +
                        " </span></b>added: <b><span style=\"color:green\">" + Synchronizer.appNumAdd.ToString() +
                        " </span></b>deleted: <b><span style=\"color:green\">" + Synchronizer.appNumDel.ToString() +
                        " </span></b>error: <b><span style=\"color:red\">" + Synchronizer.appNumErr.ToString() + "</span></b>" /*+ " )"*/;
            else
                return "";
        }

        [WebMethod]
        public bool getapplock()
        {
            return Synchronizer.appLock;
        }
     
    }

}
