using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Linq;
using System.Xml.XPath;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

using umbraco;
using umbraco.DataLayer;
using System.Reflection;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models;
using uMirror.core.DataStore;
using uMirror.core.Models;

namespace uMirror.core.Bll
{

    public delegate void before_sync_delegate(string projectName, int projectId, string xmlFileName);
    public delegate void after_sync_delegate(string projectName, int projectId, string xmlFileName);
    public delegate void change_sync_delegate(Synchronizer.operationType operation, Object item);

    public class Synchronizer
    {

        public enum operationType
        {
            update,
            delete,
            add,
            test
        }

        #region Public properties

        public HttpContext context
        {
            set
            {
               _context = value;
            }
            get
            {
                return _context;
            }
        }
        public int projectid
        {
            set
            {
                _projectid = value;
            }
            get
            {
                return _projectid;
            }
        }
        public User currentUser
        {
            set
            {
                _currentUser = value;
            }
            get
            {
                return _currentUser;
            }
        }
        public static event before_sync_delegate before_sync = delegate { };
        public static event after_sync_delegate after_sync = delegate { };
        public static event change_sync_delegate change_sync = delegate { };

        #endregion

        #region Public static roperties

        public static bool appCancel
        {
            set
            {
                _context.Application["Synchronizer_cancel"] = value;
            }
            get
            {
                try { return (bool)_context.Application["Synchronizer_cancel"]; }
                catch { return false; }
            }
        }
        public static bool appTesting
        {
            set
            {
                _context.Application["Synchronizer_testing"] = value;
            }
            get
            {
                try { return (bool)_context.Application["Synchronizer_testing"]; }
                catch { return false; }
            }
        }

        public static bool appLock
        {
            set 
            {
                _context.Application["Synchronizer_lock"] = value;
            }
            get 
            {
                try { return (bool)_context.Application["Synchronizer_lock"]; }
                catch { return false; }
            }
        }
        public static int appPro
        {
            set
            {
                _context.Application["Synchronizer_appPro"] = value;
            }
            get
            {
                try { return (int)_context.Application["Synchronizer_appPro"]; }
                catch { return -1; }
            }
        }
        public static string appState
        {
            set
            {
                _context.Application["Synchronizer_state"] = value;
            }
            get
            {
                try { return (string)_context.Application["Synchronizer_state"]; }
                catch { return ""; }
            }
        }
        public static int appNumDel
        {
            set
            {
                _context.Application["Synchronizer_num_del"] = value;
            }
            get
            {
                try { return (int)_context.Application["Synchronizer_num_del"]; }
                catch { return 0; }
            }
        }
        public static int appNumAdd
        {
            set
            {
                _context.Application["Synchronizer_num_add"] = value;
            }
            get
            {
                try { return (int)_context.Application["Synchronizer_num_add"]; }
                catch { return 0; }
            }
        }
        public static int appNumUpd
        {
            set
            {
                _context.Application["Synchronizer_num_upd"] = value;
            }
            get
            {
                try { return (int)_context.Application["Synchronizer_num_upd"]; }
                catch { return 0; }
            }
        }
        public static int appNumSki
        {
            set
            {
                _context.Application["Synchronizer_num_ski"] = value;
            }
            get
            {
                try { return (int)_context.Application["Synchronizer_num_ski"]; }
                catch { return 0; }
            }
        }
        public static int appNumErr
        {
            set
            {
                _context.Application["Synchronizer_num_err"] = value;
            }
            get
            {
                try { return (int)_context.Application["Synchronizer_num_err"]; }
                catch { return 0; }
            }
        }

        #endregion

        #region Private static properties

        private static HttpContext  _context;
        private static int _projectid;
        private static User _currentUser;
        private static string _projectName;
        private static XmlNamespaceManager xnm;

        IContentTypeService cts = ApplicationContext.Current.Services.ContentTypeService;
        IContentService cs = ApplicationContext.Current.Services.ContentService;
        IMediaService ms = ApplicationContext.Current.Services.MediaService;
        IDataTypeService ds = ApplicationContext.Current.Services.DataTypeService;

        private static bool preview = false;
        private static bool changeSpy = false;
        private static ISqlHelper SqlHelper
        {
            get
            {
                return umbraco.BusinessLogic.Application.SqlHelper;
            }
        }

        private const int numTry = 1;

        #endregion

        #region Public method

        /// <summary>
        /// Stop the current process
        /// </summary>
        public void stop()
        {
            appCancel = true;
        }

        /// <summary>
        /// Start uMirror extension method
        /// </summary>
        public void startMethod(String assemblyRef)
        {
            MethodInfo method = Store.GetProjectMethod(assemblyRef);
            if (method != null) method.Invoke(null, null);
            else throw new Exception("uMirror extension method not found");
        }

        /// <summary>
        /// Prepare test of extension method
        /// </summary>
        public void startTestMethod(string proxyMethodName) {
            if (HttpContext.Current == null) HttpContext.Current = _context;

            try {
                appTesting = true;
                appLock = true;
                appState = string.Format("Testing proxy method: {0}", proxyMethodName);

                var methodInfos = Store.GetMethods();
                var result = new List<ProxyMethod>();

                var methodInfo = methodInfos.FirstOrDefault(m => m.Name.Equals(proxyMethodName));
                if (methodInfo != null) methodInfo.Invoke(null, null);
                else throw new Exception("uMirror extension method not found");
            }
            catch (Exception ex)
            {
                Util.UpdateStateAndLogs(_projectName, Util.LogType.error, "Method test was stopped by an error", true, ex, true);
            }
            finally
            {
                appTesting = false;
                appLock = false;
            }
        }

        /// <summary>
        /// Start synchronization
        /// </summary>
        /// 

        public void start()
        {

            // Only one synchronization in time
            if (!appLock)
            {

                try
                {

                    // If current context is null
                    if (HttpContext.Current == null) HttpContext.Current = _context;

                    // Update scriptTimeout
                    _context.Server.ScriptTimeout = 36000;

                    // Init aplication data
                    appNumAdd = 0;
                    appNumDel = 0;
                    appNumUpd = 0;
                    appNumSki = 0;
                    appNumErr = 0;
                    appState = "";
                    appPro = -1;
                    appCancel = false;

                    // Init project
                    Project project = Store.GetProject(_projectid, true);
                    if (project != null)
                        _projectName = project.Name;

                    // Check if the project's configuration is correct 
                    if (project != null
                        && ( !string.IsNullOrEmpty( project.XmlFileName ) || !string.IsNullOrEmpty( project.ExtensionMethod ) )
                        && project.Nodes.Count() > 0
                        && project.Nodes.Where(r => r.ParentId == -1).Count() > 0)
                    {

                        // Init properties
                        XElement xElSource = null;
                        int rootParentId = -1;

                        // Lock process
                        appLock = true;
                        appPro = project.id;
                        Util.UpdateStateAndLogs( _projectName, Util.LogType.info, "Process start", true); 

                        // Init value
                        IList<Node> currentFirstNodes = project.Nodes.Where(r => r.ParentId == -1).ToList(); // First nodes
                        preview = (bool)project.Preview; // Preview
                        rootParentId = project.UmbRootId != null ? project.UmbRootId.Value : rootParentId; // root parent Id

                        // Raising before_sync event
                        // Util.UpdateStateAndLogs( _projectName, Util.LogType.info, "Raising before_sync event", true);
                        before_sync(project.Name, project.id, project.XmlFileName);

                        // Raising uMirrorExtencion method
                        if ( !string.IsNullOrEmpty(project.ExtensionMethod) && project.ExtensionMethod.Split(';').Count() == 2)
                        {
                            Util.UpdateStateAndLogs( _projectName, Util.LogType.info, "Raising " + project.ExtensionMethod.Split(';')[1], true);
                            startMethod(project.ExtensionMethod);
                        }

                        // Loading XML source file
                        Util.UpdateStateAndLogs( _projectName, uMirror.core.Bll.Util.LogType.info, "Loading " + project.XmlFileName, true);
                        loadSourceXML(project, ref xElSource);

                        if (xElSource != null)
                        {

                            Node parent = null;
                            IEnumerable<XElement> xIElFrom = null;

                            string xpath = "";
                            string xpathUmbraco = "//* [@id=" + rootParentId.ToString() + "]";


                            // Start synchronization
                            sincNavigator(currentFirstNodes, parent, xpathUmbraco, xpath, rootParentId, project, xIElFrom, xElSource);

                            // Raising after_sync event
                            //Util.UpdateStateAndLogs( _projectName, Util.LogType.info, "Raising after_sync event", true);
                            after_sync(project.Name, project.id, project.XmlFileName);
                            System.Threading.Thread.Sleep(1000);

                            // Endind synchronization
                            if (!appCancel)
                                Util.UpdateStateAndLogs(_projectName, Util.LogType.info, "Process completed", true, null, true);
                            else
                                Util.UpdateStateAndLogs(_projectName, Util.LogType.error, "Process manually stopped", true, null, true);

                        }
                        else
                        {
                            Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "XML file couldn't be loaded", true, null, true);
                        }

                        // Trigger next process
                        if (project.TriggerProyect > -1)
                        {
                            int projectIdToTrigger = Store.GetProject(project.TriggerProyect).id;
                            if (projectIdToTrigger != -1)
                            {
                                Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "Trigger next process", true, null, true);
                                _projectid = projectIdToTrigger;
                                appPro = -1;
                                appLock = false;
                                start();
                            }
                        }

                    }
                    else
                    {
                        Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "No documents were found to synchronize", true, null, true);
                    }

                }
                catch (Exception ex)
                {
                    Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "Synchronizer was stopped by an error", true, ex, true);
                }
                finally
                {
                    appPro = -1;
                    appLock = false;
                }
            }
            else
            {
                Util.UpdateStateAndLogs(_projectName, Util.LogType.error, "Another process is running", true, null, true);
            }

        }

        #endregion

        #region Private method

        /// <summary>
        /// 
        /// </summary>
        private void sincNavigator(IList<Node> currentNodes, Node parent, string xpathBaseUmbraco, string xpath, int rootParentId, Project project, IEnumerable<XElement> xIElFrom, XElement xElSource)
        {
            
            foreach (Node currentNode in currentNodes  /*.Where(r => r.OnlyAdd == false )*/ )
            {

                // Syncronization Canceled
                if (appCancel) return;

                // Load Document type and Loading nodes
                ContentType doc = (ContentType)cts.GetContentType(currentNode.UmbDocumentTypeAlias);
                Util.UpdateStateAndLogs( project.Name, Util.LogType.info, "Loading " + doc.Alias, false);
                IEnumerator ienum = getNodeToSync(doc.Alias, xpathBaseUmbraco, rootParentId).GetEnumerator();

                // relative xpath to current doctype
                string relXpathUmbraco = "/";
                relXpathUmbraco = "/" + doc.Alias;

                // absoltute xpath to current doctype
                string absXpathUmbraco = xpathBaseUmbraco;
                for (int i = 1; i <= currentNode.LevelNumber; i++) relXpathUmbraco = "/*" + relXpathUmbraco;
                absXpathUmbraco += relXpathUmbraco;

                IEnumerable<XElement> xIElFromForChild = null;
                IEnumerable<XElement> xIELTo = null;

                changeSpy = false;
                while (ienum.MoveNext())
                {

                    // Syncronization Canceled
                    if (appCancel) return;

                    IEnumerable<XElement> xIElFromSelect = null;
                    XElement xIndex = GetXElement((XmlNode)ienum.Current);

                    // Select data from to filter by xpath
                    xpath = "/*";
                    if (currentNode.XmlDocumentXPath != null) xpath = currentNode.XmlDocumentXPath;
                    if (parent != null)
                    {
                        IEnumerable<XElement> e;
                        e = xIElFrom.Where(p => p.CreateNavigator().SelectSingleNode(parent.XmlIdentifierXPath).Value == (string)xIndex.Element(parent.UmbIdentifierProperty));
                        if (e.Count() > 0) {
                            var selectedElement = xIElFrom.Where(p => p.CreateNavigator().SelectSingleNode(parent.XmlIdentifierXPath).Value == (string)xIndex.Element(parent.UmbIdentifierProperty)).First();

                            XElement xIElFromSelectTest = XElement.Parse(selectedElement.ToString());

                            xIElFromSelect = xIElFromSelectTest.XPathSelectElements(xpath);
//                            xIElFromSelect = innerXmlDoc.SelectNodes(xpath).Cast<XmlElement>().Select(x => XElement.Parse(x.OuterXml));
                        }
                    }
                    else
                        xIElFromSelect = xElSource.XPathSelectElements(xpath);

                    //Select data from FROM filter by xpath
                    xIELTo = xIndex.XPathSelectElements("." + relXpathUmbraco);

                    // Start sync
                    if (xIElFromSelect != null)
                    {
                        syn(xIElFromSelect, xIELTo, (ContentType)cts.GetContentType(doc.Alias), currentNode, xIndex, (bool)project.LogAllAction);

                        // Concat all child
                        if (xIElFromForChild == null)
                            xIElFromForChild = xIElFromSelect;
                        else
                            xIElFromForChild = xIElFromForChild.Concat(xIElFromSelect);
                    }

                }

                // Navigate in children nodes
                IList<Node> NodeChildren = Store.GetNodes(currentNode.id);
                if (NodeChildren.Any() && !appCancel)
                {
                    sincNavigator(NodeChildren, currentNode, absXpathUmbraco, xpath, rootParentId, project, xIElFromForChild, xElSource);
                }

                // If content was changed, refresh cache content
                if (changeSpy)
                {
                    umbraco.library.RefreshContent();
                    System.Threading.Thread.Sleep(1000);
                }

            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void syn(IEnumerable<XElement> xFrom, IEnumerable<XElement> xUmbraco, ContentType dt, Node node, XElement fatherNode, bool addToLog)
        {

            // Syncronization Canceled
            if (appCancel) return;

            // synchronize 
            IEnumerable<String> itemToUpdate = new List<String>();
            IEnumerable<String> itemToDelete = new List<String>();
            IEnumerable<String> itemToAdd = new List<String>();

            var fatherNodeId = (int)fatherNode.Attribute("id");

            // get pending operation
            ContentType DocType = (ContentType)cts.GetContentType(node.UmbDocumentTypeAlias);
            String fatherName = fatherNodeId > 1 ? ((Content)cs.GetById(fatherNodeId)).Name : "Root";

            Util.UpdateStateAndLogs(_projectName, Util.LogType.info, "Comparing " + DocType.Name + " under " + fatherName, false);
            getPendingOperation(xFrom, xUmbraco, node, fatherNode, ref itemToUpdate, ref itemToDelete, ref itemToAdd);

            //Update in Umbraco
            if (itemToDelete.Count() + itemToUpdate.Count() + itemToAdd.Count() > 0)
            {
                // Delete items
                Util.UpdateStateAndLogs(_projectName, Util.LogType.info, "Delete " + DocType.Name + " under " + fatherName, false);
                foreach (String indexDelete in itemToDelete)
                {
                    // Syncronization Canceled
                    if (appCancel) break;

                    // Delete items
                    deleteNode(node, indexDelete, false, addToLog);
                }

                // Update items
                Util.UpdateStateAndLogs(_projectName, Util.LogType.info, "Update " + DocType.Name + " under " + fatherName, false);
                foreach (String indexUpdate in itemToUpdate)
                {
                    // Syncronization Canceled
                    if (appCancel) break;

                    // Update items
                    updateNode(xFrom, xUmbraco, node, indexUpdate, addToLog);
                }
                // Add new items
                Util.UpdateStateAndLogs(_projectName, Util.LogType.info, "Adding " + DocType.Name + " under " + fatherName, false);
                foreach (String indexAdd in itemToAdd)
                {
                    // Syncronization Canceled
                    if (appCancel) break;

                    // Add new items
                    saveNode(xFrom, fatherNodeId, node, indexAdd, addToLog);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void getPendingOperation(IEnumerable<XElement> xFrom,
                                    IEnumerable<XElement> xTo,
                                    Node node,
                                    XElement fatherNode,
                                    ref IEnumerable<String> itemToUpdate,
                                    ref IEnumerable<String> itemToDelete,
                                    ref IEnumerable<String> itemToAdd)
        {

            // Get an identifiers list from xfrom
            var xFormId = xFrom.Select(p => p.CreateNavigator().SelectSingleNode(node.XmlIdentifierXPath, xnm)?.Value);

            // Get an identifiers list from xto
            IEnumerable<XElement> umbracoId;
            ContentType DocType = (ContentType)cts.GetContentType(node.UmbDocumentTypeAlias);
            string nodeTypeAlias = DocType.Alias;
            umbracoId = xTo.Where(p => p.Name == nodeTypeAlias && (string)p.Element(node.UmbIdentifierProperty) != "").Select(r => r);

            //TODO add this as an option. existingUmbracoId vs umbracoId. All types or only the ones as a destination.
            var xIELTo = fatherNode.XPathSelectElements(".//*[@nodeTypeAlias]");
            var existingUmbracoId = xIELTo.Where(p => (string)p.Element(node.UmbIdentifierProperty) != "").Select(r => r);

            if (!(bool)node.OnlyAdd)
            {
                if (xFormId != null && xFormId.Any()) {
                    itemToAdd = xFormId.Except(existingUmbracoId.Select(r => (string)r.Element(node.UmbIdentifierProperty)));

                    // Get items to delete, duplicate and empty node
                    itemToDelete = umbracoId.Where(r => !xFormId.Contains((string)r.Element(node.UmbIdentifierProperty))).Select(r => r.Attribute("id").Value);
                    itemToDelete = itemToDelete.Concat(xTo.GroupBy(i => (string)i.Element(node.UmbIdentifierProperty)).Where(g => g.Count() > 1).SelectMany(g => g.Skip(1)).Select(r => r.Attribute("id").Value));
                    itemToDelete = itemToDelete.Concat(xTo.Where(p => p.Name == nodeTypeAlias && string.IsNullOrEmpty((string)p.Element(node.UmbIdentifierProperty))).Select(r => r.Attribute("id").Value));
                    itemToDelete = itemToDelete.Distinct();

                    itemToUpdate = xFormId.Intersect(umbracoId.Select(r => (string)r.Element(node.UmbIdentifierProperty)));
                }
            }
            else
            {
                itemToDelete = new string[0];
                itemToUpdate = new string[0];
                itemToAdd = xFormId;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void deleteNode(Node node, string indexDelete, bool force, bool addToLog)
        {

            if (!(bool)node.NeverDelete || force)
            {
                int documentId = 0;
                for (int otry = 0; otry < numTry; otry++)
                {

                    // Syncronization Canceled
                    if (appCancel) return;

                    try
                    {
                        documentId = int.Parse(indexDelete);
                        if (!preview)
                        {
                            changeSpy = true;
                            Content doc = (Content)cs.GetById(documentId);

                            cs.UnPublish(doc);
                            cs.Delete(doc);

                            // Log all operation
                            if (addToLog) Util.UpdateStateAndLogs( _projectName, Util.LogType.info, "Delete node " + documentId.ToString(), false);

                            // Raising before_sync event
                            change_sync(operationType.delete, documentId);

                        }

                        //Update application info
                        appNumDel++;
                        break;

                    }
                    catch (Exception ex)
                    {
                        //if (getExMessage(ex) == "No node exists with id '" + indexDelete + "'")
                        //{
                        //    appNumSki++;
                        //    break;
                        //}
                        //else if (getExMessage(ex) == "No Document exists with Version '00000000-0000-0000-0000-000000000000'")
                        //{
                        //    deleteCorruptNode(int.Parse(indexDelete));
                        //    appNumDel++;
                        //    Log.Add(Util.LogType.error, _currentUser, int.Parse(indexDelete), "[uMirror][" + synName + "][warning] delete corrupt node.");
                        //    break;
                        //}
                        if (otry >= numTry - 1)
                        {
                            appNumErr++;
                            Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "Delete node " + indexDelete, false, ex, true);
                        }
                        else
                        {
                            Util.UpdateStateAndLogs( _projectName, Util.LogType.info, "Error to delete node " + indexDelete + ", try:" + (otry + 1).ToString(), false, null, true);
                            System.Threading.Thread.Sleep(5000);
                        }
                    }
                }
            }
            else
            {
                appNumSki++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void updateNode(IEnumerable<XElement> xFrom, IEnumerable<XElement> xTo, Node node, string indexUpdate, bool addToLog)
        {

            int documentId = 0;
            for (int otry = 0; otry < numTry; otry++)
            {

                // Syncronization Canceled
                if (appCancel) return;

                try
                {
                    ContentType DocType = (ContentType)cts.GetContentType(node.UmbDocumentTypeAlias);
                    string nodeTypeAlias = DocType.Alias;
                    XElement toItem = xTo.Where(p => p.Name == nodeTypeAlias && ((string)p.Element(node.UmbIdentifierProperty) == indexUpdate)).First();

                    XElement fromItem = xFrom.Where(p => (string)p.CreateNavigator().SelectSingleNode(node.XmlIdentifierXPath, xnm).Value == indexUpdate).First();
                    documentId = (int)toItem.Attribute("id");
                    if (toItem != null && fromItem != null && comparValues(toItem, fromItem, node) == false)
                    {

                        Content doc = (Content)cs.GetById(documentId);

                        if (!(bool)node.IgnoreNodeName)
                            doc.Name = Util.truncStr(fromItem.CreateNavigator().SelectSingleNode(node.XmlNodeNameXPath).Value, node.TruncateNodeName == null ? 0 : (int)node.TruncateNodeName);

                        if (initProperties(doc, fromItem, node, false))
                        {
                            if (!preview)
                            {
                                changeSpy = true;
                                if (doc.Published) 
                                    cs.SaveAndPublish(doc);
                                else
                                    cs.Save(doc);

                                // Log all operation
                                if (addToLog) Util.UpdateStateAndLogs( _projectName, Util.LogType.info, "Update node " + documentId.ToString(), false);

                                // Raising before_sync event
                                change_sync(operationType.update, doc);

                            }
                            
                            //Update application info
                            appNumUpd++;
                            break;
                        }
                        else
                        {
                            appNumErr++;
                            break;
                        }
                    }
                    else
                    {
                        appNumSki++;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (otry >= numTry - 1)
                    {
                        appNumErr++;
                        Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "Update node " + documentId.ToString(), false, ex, true);
                    }
                    else
                    {
                        Util.UpdateStateAndLogs( _projectName, Util.LogType.info, "Error to update node " + documentId.ToString() + ", try:" + (otry + 1).ToString(), false, null, true);
                        System.Threading.Thread.Sleep(5000);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void saveNode(IEnumerable<XElement> xFrom, int fatherNodeId, Node node, string indexAdd, bool addToLog)
        {
            for (int otry = 0; otry < numTry; otry++)
            {

                // Syncronization Canceled
                if (appCancel)
                    return;

                Content doc = null;
                string nodeName = "";
                int newNodeId = -1;
                try
                {
                    XElement fromItem = xFrom.Where(p => (string)p.CreateNavigator().SelectSingleNode(node.XmlIdentifierXPath, xnm).Value == indexAdd).First();
                    nodeName = Util.truncStr((string)fromItem.CreateNavigator().SelectSingleNode(node.XmlNodeNameXPath).Value, node.TruncateNodeName == null ? 0 : (int)node.TruncateNodeName);
                    if (!preview)
                    {
                        changeSpy = true;

                        //TODO: Hack because uDateFoldersy doesn't work with services
                        doc = (Content)cs.CreateContentWithIdentity(nodeName, fatherNodeId, node.UmbDocumentTypeAlias); 
                        newNodeId = doc.Id;
                        doc.Name = nodeName;
                        
                        doc = (Content)cs.GetById(doc.Id);
                        if (initProperties(doc, fromItem, node, false))
                        {

                            cs.Save(doc);
                            doc = (Content)cs.GetById(doc.Id);

                            cs.Publish(doc);

                            // Log all operation
                            if (addToLog) Util.UpdateStateAndLogs( _projectName, Util.LogType.info, "New node " + doc.Id.ToString(), false);

                            // Raising before_sync event
                            change_sync(operationType.add, doc);

                        }
                        else
                        {
                            appNumErr++;
                            break;
                        }
                    }
                    //Update application info
                    appNumAdd++;
                    break;
                }
                catch (Exception ex)
                {
                    if (otry >= numTry - 1)
                    {
                        appNumErr++;
                        Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "New node (" + nodeName + ") id:" + newNodeId, false, ex, true);
                    }
                    else
                    {
                        Util.UpdateStateAndLogs( _projectName, Util.LogType.info, "Error to add new node " + nodeName + ", try:" + (otry + 1).ToString(), false, null, true);
                        if (newNodeId > -1) deleteNode(node, newNodeId.ToString(), true, addToLog);
                        System.Threading.Thread.Sleep(5000);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool initProperties(Content doc, XElement xFrom, Node node, Boolean force)
        {

            if (!doc.HasProperty(node.UmbIdentifierProperty)) // If old identity property not found in DocumentType
            {
                Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "Init values, " + doc.ContentType.Alias + ", "
                                                                                                    + node.UmbIdentifierProperty
                                                                                                    + " property not found in DocumentType", true);
                return false;
            }

            if (xFrom.CreateNavigator().SelectSingleNode(node.XmlIdentifierXPath, xnm) == null) // If old identity property not found in DocumentType
            {
                Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "Init values, " + node.XmlDocumentXPath + ", "
                                                                                                    + node.XmlIdentifierXPath
                                                                                                    + " property not found in XML source file", true);
                return false;
            }

            // Init old identity property
            doc.SetPropertyValue(node.UmbIdentifierProperty, xFrom.CreateNavigator().SelectSingleNode(node.XmlIdentifierXPath, xnm).Value);

            // Init anothers properties
            foreach (uMirror.core.DataStore.Property prop in node.Properties)
            {
                if (!(bool)prop.Ignore || force)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(prop.XmlPropertyXPath)) // if property do not exist in XML source file
                        {
                            doc.SetPropertyValue(prop.UmbPropertyAlias, string.Empty);
                        }
                        else if (xFrom.CreateNavigator().SelectSingleNode(prop.XmlPropertyXPath, xnm) == null) // if property not found in XML source file
                        {
                            Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "Init values, " + node.XmlDocumentXPath + ", "
                                                                                                                + prop.XmlPropertyXPath
                                                                                                                + " property not found XML source file", true);
                        }
                        else if (!doc.HasProperty(prop.UmbPropertyAlias)) // if property not found in DocumentType
                        {
                            Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "Init values, " + doc.ContentType.Alias + ", "
                                                                                                                + prop.UmbPropertyAlias
                                                                                                                + " property not found in DocumentType", true);
                        }
                        else if (prop.InitLikeXml)
                        {
                            doc.SetPropertyValue(prop.UmbPropertyAlias, xFrom.CreateNavigator().SelectSingleNode(prop.XmlPropertyXPath, xnm).InnerXml);
                        }
                        else
                        {
                            switch (doc.PropertyTypes.FirstOrDefault(r => r.Alias == prop.UmbPropertyAlias).DataTypeId.ToString())
                            {
                                case "4023e540-92f5-11dd-ad8b-0800200c9a66":
                                    umbraco.cms.businesslogic.Tags.Tag.RemoveTagsFromNode(doc.Id);
                                    foreach (string tag in xFrom.CreateNavigator().SelectSingleNode(prop.XmlPropertyXPath, xnm).Value.Split(','))
                                        umbraco.cms.businesslogic.Tags.Tag.AddTagsToNode(doc.Id, tag.Trim(), "default");
                                    doc.SetPropertyValue(prop.UmbPropertyAlias, xFrom.CreateNavigator().SelectSingleNode(prop.XmlPropertyXPath, xnm).Value);
                                    break;
                                case "454545ab-1234-4321-abcd-1234567890ab": //Embedded Content
                                    doc.SetPropertyValue(prop.UmbPropertyAlias, xFrom.CreateNavigator().SelectSingleNode(prop.XmlPropertyXPath, xnm).InnerXml);
                                    break;
                                case "ead69342-f06d-4253-83ac-28000225583b": // Media Picker
                                    String[] mediaInfos = xFrom
                                                        .CreateNavigator()
                                                        .SelectSingleNode(prop.XmlPropertyXPath, xnm)
                                                        .Value
                                                        .Split(new String[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);

                                    String mediaFilePath = mediaInfos != null && mediaInfos.Count() >= 1 ? mediaInfos[0] : string.Empty;
                                    String mediaFolderPath = mediaInfos != null && mediaInfos.Count() >= 2 ? mediaInfos[1] : string.Empty;

                                    if (System.IO.File.Exists(mediaFilePath))
                                    {
                                        Umbraco.Core.Models.Media media = null;
                                        int mediaid = -1;
                                        if (doc.GetValue(prop.UmbPropertyAlias) != null && int.TryParse(doc.GetValue(prop.UmbPropertyAlias).ToString(), out mediaid))
                                        {
                                            // Check if the media is Trashed and compare with the new file
                                            var m = (Umbraco.Core.Models.Media)ms.GetById(mediaid);
                                            if ( m != null && !m.Path.Contains("-21") && !m.Trashed )
                                            {
                                                if (Util.FileCompare(mediaFilePath, context.Server.MapPath(m.GetValue("umbracoFile").ToString()))) break;
                                                media = m;
                                            }
                                        }

                                        int parentId = prop.MediaParent > 0 ? prop.MediaParent : Util.GetMediaParentId(mediaFolderPath);
                                        int newMediaId = Util.SaveMedia(mediaFilePath, parentId, media);
                                        doc.SetPropertyValue(prop.UmbPropertyAlias, newMediaId);
                                    }
                                    else
                                        doc.SetPropertyValue(prop.UmbPropertyAlias, xFrom.CreateNavigator().SelectSingleNode(prop.XmlPropertyXPath, xnm).Value);
                                    break;

                                case "a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6": // Dropdown List
                                    var value = xFrom.CreateNavigator().SelectSingleNode(prop.XmlPropertyXPath, xnm).Value;
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        var valueId= Util.GetDtIdByValue(doc.PropertyTypes.FirstOrDefault(r => r.Alias == prop.UmbPropertyAlias).DataTypeDefinitionId, value);
                                        if (value !=  "-1") value = valueId.ToString();
                                    }
                                    doc.SetPropertyValue(prop.UmbPropertyAlias, value);
                                    break;

                                case "a52c7c1c-c330-476e-8605-d63d3b84b6a6": // RadioButton List
                                case "23e93522-3200-44e2-9f29-e61a6fcbb79a": // Date
                                case "60b7dabf-99cd-41eb-b8e9-4d2e669bbde9": // Simple Editor
                                case "67db8357-ef57-493e-91ac-936d305e0f2a": // Textbox multiple
                                case "5e9b75ae-face-41c8-b47e-5f4b0fd82f83": // Richtext editor
                                case "38b352c1-e9f8-4fd8-9324-9a2eab06d97a": // True/False
                                case "1413afcb-d19a-4173-8e9a-68288d2a73b8": // Numeric
                                default:
                                    doc.SetPropertyValue(prop.UmbPropertyAlias, xFrom.CreateNavigator().SelectSingleNode(prop.XmlPropertyXPath, xnm).Value);
                                    break;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "initValues " + node.UmbDocumentTypeAlias + " " + prop.UmbPropertyAlias, false, ex, true);
                        return false;
                    }
                }
            }
            return true;

        }

        /// <summary>
        /// 
        /// </summary>
        private bool comparValues(XElement index, XElement xFrom, Node node)
        {
            // Check nodeName
            try
            {
                if (!string.IsNullOrEmpty(node.XmlNodeNameXPath)) // if property exist in XML source file
                {
                    if (xFrom.CreateNavigator().SelectSingleNode(node.XmlNodeNameXPath, xnm) == null) // if property not found in XML source file
                    {
                        Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "Compare values, " + node.XmlDocumentXPath + ", "
                                                                                                               + node.XmlNodeNameXPath
                                                                                                               + " property not found in XML source file", true);
                    }
                    else
                    {
                        // TODO compare?
                        if (!(bool)node.IgnoreNodeName &&
                            !(Util.truncStr(xFrom.CreateNavigator().SelectSingleNode(node.XmlNodeNameXPath, xnm).Value, node.TruncateNodeName == null ? 0 : (int)node.TruncateNodeName).Trim().ToLower() == ((string)index.Attribute("nodeName")).Trim().ToLower()))
                            return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "Compare values " + node.XmlNodeNameXPath + "/nodeName", false, ex, true);
                return false;
            }

            // Check properties
            ContentType ct = (ContentType)cts.GetContentType(node.UmbDocumentTypeAlias);
            foreach (uMirror.core.DataStore.Property prop in node.Properties)
            {
                try
                {
                    if (!string.IsNullOrEmpty(prop.XmlPropertyXPath)) // if property do not exist in XML source file
                    {

                        if (xFrom.CreateNavigator().SelectSingleNode(prop.XmlPropertyXPath, xnm) == null) // if property not found in XML source file
                        {
                            Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "Compare values, " + node.XmlDocumentXPath + ", "
                                                                                                                  + prop.XmlPropertyXPath
                                                                                                                  + " property not found in XML source file", true);
                        }
                        else
                        {

                            // Get value from XML umbraco
                            string valueUmbraco = string.Empty;
                            if (index.Element(prop.UmbPropertyAlias) != null)
                            {
                                if (prop.InitLikeXml)
                                    valueUmbraco = index.Element(prop.UmbPropertyAlias).FirstNode.ToString().Trim().ToLower();
                                else
                                    valueUmbraco = index.Element(prop.UmbPropertyAlias).Value.ToString().Trim().ToLower().Replace(System.Environment.NewLine, string.Empty).Replace("\n", string.Empty);
                            }

                            // Get value from XML source
                            string value = string.Empty;
                            if (prop.InitLikeXml)
                            {
                                if (xFrom.Element(prop.XmlPropertyXPath) != null)
                                {
                                    value = xFrom.Element(prop.XmlPropertyXPath).FirstNode.ToString().Trim().ToLower();
                                }
                            }
                            else
                            {
                                XPathNavigator xpathProp = xFrom.CreateNavigator().SelectSingleNode(prop.XmlPropertyXPath, xnm);
                                if (xpathProp != null)
                                {
                                    value = xpathProp.Value.Trim().ToLower().Replace(System.Environment.NewLine, string.Empty).Replace("\n", string.Empty);
                                }
                            }

                            // Compare
                            if (!(bool)prop.Ignore)
                            {
                                switch (ct.CompositionPropertyTypes.FirstOrDefault(r => r.Alias == prop.UmbPropertyAlias).DataTypeId.ToString())
                                {
                                    case "ead69342-f06d-4253-83ac-28000225583b": // Media Picker

                                        String[] mediaInfos = xFrom
                                                            .CreateNavigator()
                                                            .SelectSingleNode(prop.XmlPropertyXPath, xnm)
                                                            .Value
                                                            .Split(new String[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);

                                        String mediaFilePath = mediaInfos != null && mediaInfos.Count() >= 1 ? mediaInfos[0] : string.Empty;

                                        if (System.IO.File.Exists(mediaFilePath))
                                        {
                                            int mediaId = (int.TryParse(valueUmbraco, out mediaId) ? mediaId : -1);
                                            if (mediaId > 0)
                                            {
                                                var m = ms.GetById(mediaId);
                                                // Check if the media is Trashed and compare with the new file
                                                if (m != null 
                                                    && !m.Path.Contains("-21")
                                                    && !m.Trashed
                                                    && Util.FileCompare(mediaFilePath, context.Server.MapPath(m.GetValue("umbracoFile").ToString())))
                                                    break;
                                            }
                                        }
                                        if (value == valueUmbraco) break;
                                        return false;
                                    case "23e93522-3200-44e2-9f29-e61a6fcbb79a": // Date
                                        DateTime dateValue = DateTime.TryParse(value, out dateValue) ? dateValue : DateTime.MinValue;
                                        DateTime dateValueUmbraco = DateTime.TryParse(valueUmbraco, out dateValueUmbraco) ? dateValueUmbraco : DateTime.MinValue;
                                        if (dateValue == dateValueUmbraco) break;
                                        return false;
                                    case "a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6": // Dropdown List
                                    case "4023e540-92f5-11dd-ad8b-0800200c9a66": // Tags
                                    case "454545ab-1234-4321-abcd-1234567890ab": // Embedded Content
                                    case "a52c7c1c-c330-476e-8605-d63d3b84b6a6": // RadioButton List
                                    case "60b7dabf-99cd-41eb-b8e9-4d2e669bbde9": // Simple Editor
                                    case "67db8357-ef57-493e-91ac-936d305e0f2a": // Textbox multiple
                                    case "5e9b75ae-face-41c8-b47e-5f4b0fd82f83": // Richtext editor
                                    case "38b352c1-e9f8-4fd8-9324-9a2eab06d97a": // True/False
                                    case "1413afcb-d19a-4173-8e9a-68288d2a73b8": // Numeric
                                    default:
                                        if (value == valueUmbraco) break;
                                        return false;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.UpdateStateAndLogs( _projectName, Util.LogType.error, "Compare values " + prop.XmlPropertyXPath + "/" + prop.UmbPropertyAlias, false, ex, true);
                    return false;
                }
            }

            return true;

        }

        #endregion

        #region Private static method

        /// <summary>
        /// get the xml of the data from umbraco
        /// </summary>
        private static XmlNodeList getNodeToSync(string nodeTypeAlias, string xpathUmbraco, int rootParentId)
        {
            
            XmlDocument XmlContent = ((XmlDocument)content.Instance.XmlContent.Clone());
            
            // TODO: unpublish nodes
            //if (false)
            //{
            //    IContentService cs = ApplicationContext.Current.Services.ContentService;
            //    Content documentObject = (Content)cs.GetById(rootParentId);
            //    PopulateXMLPreview(rootParentId, nodeTypeAlias, XmlContent);
            //    if (documentObject == null) throw new ArgumentNullException("documentObject");
            //    ConstructAndAppendPreviewXml(documentObject, true, XmlContent);
            //}

            return XmlContent.SelectNodes(xpathUmbraco);

        }

        /// <summary>
        /// Load source data to sync from xml
        /// </summary>
        private static void loadSourceXML(Project project, ref XElement xElSource)
        {
            string path = Store.GetProjectFilePath(project);
            if (System.IO.File.Exists(_context.Server.MapPath(path)))
            {
                XDocument result = XDocument.Load(_context.Server.MapPath(path));
                
                XPathNavigator foo = result.CreateNavigator();
                foo.MoveToFollowing(XPathNodeType.Element);
                IDictionary<string, string> whatever = foo.GetNamespacesInScope(XmlNamespaceScope.All);
                
                xnm = new XmlNamespaceManager(new NameTable());
                foreach (KeyValuePair<string, string> index in whatever)
                    xnm.AddNamespace(index.Key, index.Value);

                xElSource = result.Root;
            }
            else
            {
                throw new Exception("file not found");
            }
        }

        /// <summary>
        /// XmlNode to XElement
        /// </summary>
        private static XElement GetXElement(XmlNode node)
        {
            XDocument xDoc = new XDocument();
            using (XmlWriter xmlWriter = xDoc.CreateWriter())
                node.WriteTo(xmlWriter);
            return xDoc.Root;
        }

        /// <summary>
        /// Delete corrupt node
        /// </summary>
        /// <param name="nodeId"></param>
        //private static void deleteCorruptNode(int nodeId)
        //{
        //    string delete = "delete from cmsPreviewXml where versionID in (select versionid from cmsContentVersion where ContentId in (select nodeId from cmsContent where nodeId = @nodeid)) " +
        //        "delete from cmsContentVersion where ContentId = @nodeid " +
        //        "delete from cmsContentXML where nodeId = @nodeid " +
        //        "delete from cmsDocument where nodeId = @nodeid " +
        //        "delete from cmsPropertyData where contentNodeId = @nodeid " +
        //        "delete from cmsContent where nodeId = @nodeid " +
        //        "delete from umbracoUser2NodePermission where nodeId = @nodeid " +
        //        "delete from umbracoNode where id = @nodeid";
        //    try
        //    {
        //        SqlHelper.ExecuteNonQuery(delete, SqlHelper.CreateParameter("@nodeid", nodeId));
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Add(Util.LogType.error, _currentUser, nodeId, "[uMirror][" + synName + "] delete corrupt node: " + getExMessage(ex));
        //    }
        //}

        /// <summary>
        /// Update and add unPublished nodes in xmlContent
        /// </summary>
        //private static void ConstructAndAppendPreviewXml(Content documentObject, bool includeSubs, XmlDocument XmlContent)
        //{
        //    content.AppendDocumentXml(documentObject.Id, documentObject.Level, documentObject.ParentId, documentObject.ToPreviewXml(XmlContent), XmlContent);
        //    foreach (CMSPreviewNode prevNode in documentObject.GetNodesForPreview(true))
        //    {
        //        content.AppendDocumentXml(prevNode.NodeId, prevNode.Level, prevNode.ParentId, XmlContent.ReadNode(XmlReader.Create(new StringReader(prevNode.Xml))), XmlContent);
        //    }

        //    foreach (Document index in Document.GetChildrenForTree(documentObject.Id))
        //    {
        //        ConstructAndAppendPreviewXml(index, true, XmlContent);
        //    }
        //}

        /// <summary>
        /// Populate XMLPreview for unPublished nodes 
        /// </summary>
        //private static void PopulateXMLPreview(int fatherId, string contentTypeAlias, XmlDocument XmlContent)
        //{
        //    Document[] children = Document.GetChildrenForTree(fatherId);
        //    foreach (Document index in children)
        //    {
        //        if (!index.Published /*&& index.ContentType.Alias == contentTypeAlias */)
        //            ((CMSNode)index).ToPreviewXml(XmlContent);
        //        if (index.HasChildren)
        //            PopulateXMLPreview(index.Id, contentTypeAlias, XmlContent);
        //    }
        //}

        #endregion

        #region Private class

        /// <summary>
        /// Compare XElement value
        /// </summary>
        private class idComparer : IEqualityComparer<XElement>
        {
            public bool Equals(XElement x, XElement y)
            {
                try
                {
                    if ((string)x == (string)y)
                    {
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }

            public int GetHashCode(XElement obj)
            {
                return ((int)obj).GetHashCode();
            }
        }

        #endregion

    }

}