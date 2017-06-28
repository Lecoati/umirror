
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

using Umbraco.Core;
using Umbraco.Core.Models;
using System.Xml.Linq;
using uMirror.core.DataStore;
using uMirror.core.Models;
using uMirror.core.Bll;

namespace uMirror.core.Controllers
{
    [PluginController("uMirror")]
    public class uMirrorApiController : UmbracoAuthorizedJsonController
    {

        #region "backend"

        public Project GetProjectById(int id)
        {
            return Store.GetProject(id);
        }

        public Project PostSaveProject(Project project)
        {
            if (project.id <= 0) {
                project.id = Store.GetAllProjects().Count() + 1;
            }

            Store.UpdateProject(project);
            return project;
        }

        public void DeleteProjectById(int id)
        {
            Store.DeleteProject(id);
        }

        public Node GetNodeById(int id)
        {
            return Store.GetNode(id);
        }

        public Node PostSaveNode(Node node)
        {
            if (node.id <= 0)
            {
                node.id = Store.GetAllNodes().Count() + 1;   
            }
            Store.UpdateNode(node);
            return node;
        }

        public void DeleteNodeById(int id)
        {
            Store.DeleteNode(id);
        }

        public IEnumerable<ProxyMethod> GetProxyMethods()
        {

            var methodInfos = Store.GetMethods();
            var result = new List<ProxyMethod>();

            foreach (var method in methodInfos) {
                String assemblyRef = method.ReflectedType.Assembly.FullName + ";" + method.Name;
                if (assemblyRef.Contains("App_Code")) assemblyRef = "App_Code" + assemblyRef.Substring(assemblyRef.IndexOf(","));
                String filePath = Store.GetProjectFilePath(assemblyRef);
                if (!string.IsNullOrEmpty(filePath))
                {
                    result.Add(new ProxyMethod()
                    {
                        Name = method.Name,
                        AssemblyRef = assemblyRef,
                        FilePath = filePath
                    });
                }
            }

            return result;

        }

        public IEnumerable<DocumentType> GetDocumentTypes()
        {
            var methodInfos = Store.GetMethods();
            return ApplicationContext.Current.Services.ContentTypeService
            .GetAllContentTypes()
            .Select(dt => new DocumentType()
            {
                Alias = dt.Name,
                Id = dt.Id,
                Icon = dt.Icon
            });
        }

        public IEnumerable<Element> GetElements(int projectId, bool withPrefix)
        {

            var projectFilePath = Store.GetProjectFilePath(Store.GetProject(projectId));
            var suggestedElement = SuggestElementAndAttribute(HttpContext.Current.Server.MapPath(projectFilePath));

            string xpathPrefix = "//";
            if (!string.IsNullOrEmpty(HttpContext.Current.Request["id"]) && HttpContext.Current.Request["id"].StartsWith("node_")) xpathPrefix = "./";

            return suggestedElement.Where(r => r.Value.Substring(0, 1) != "@" || !withPrefix).Select(r => new Element()
            {
                Value = withPrefix ? xpathPrefix + r.Value : r.Value,
                Name = withPrefix ? xpathPrefix + r.Value : r.Value
            });

        }

        public IEnumerable<Models.Property> GetProperties(string docTypeAlias)
        {
            return LoadDdlUmbIdentifierProperty(docTypeAlias).Select(r => new core.Models.Property() {
                Alias = r.Alias,
                Id = r.Id
            } );
        }

        private IDictionary<string, string> SuggestElementAndAttribute(string xmlFilePath)
        {

            IList<string> attributes = new List<string>();
            IList<string> elements = new List<string>();

            if (System.IO.File.Exists(xmlFilePath))
            {
                XDocument currentDoc = XDocument.Load(xmlFilePath);
                foreach (var name in currentDoc.Root.DescendantNodes().OfType<XElement>().Select(

                    x => !string.IsNullOrEmpty(x.GetPrefixOfNamespace(x.Name.Namespace)) ? x.GetPrefixOfNamespace(x.Name.Namespace) + ":" + x.Name.LocalName : x.Name.LocalName).Distinct())
                {
                    elements.Add(name);
                    string cleanName = name.Substring(name.LastIndexOf(":") + 1);
                    if (currentDoc.Descendants(cleanName).Any())
                    {
                        attributes = attributes.Concat(currentDoc.Descendants(cleanName).ToArray()[0].Attributes().Select(r => r.Name.LocalName).ToList()).ToList();
                    }
                }

                attributes = attributes.Distinct().ToList().OrderBy(r => r).ToList();
                elements = elements.Distinct().ToList().OrderBy(r => r).ToList();
            }

            IDictionary<string, string> results = new Dictionary<string, string>();
            foreach (string index in elements) results.Add(index, index);
            foreach (string index in attributes) results.Add("@" + index, "@" + index);

            return results;

        }

        private IList<PropertyType> LoadDdlUmbIdentifierProperty(string docTypeAlias)
        {
            ContentType docType = (ContentType)ApplicationContext.Current.Services.ContentTypeService.GetContentType(docTypeAlias);
            return docType.CompositionPropertyTypes.ToList();
        }

        #endregion

        #region "sync method"

        public void PutStart(int id)
        {
            var sync = new Synchronizer()
            {
                context = HttpContext.Current,
                projectid = id,
                currentUser = umbraco.BusinessLogic.User.GetCurrent()
            };
            sync.start();
        }

        public void PutStop()
        {
            new Synchronizer().stop();  
        }

        public string StartMethod(String assemblyRef)
        {
            try
            {
                var sync = new Synchronizer();
                sync.startMethod(assemblyRef);
                return "done";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetAppNum()
        {

            string state = Synchronizer.appState.ToString();
            if (Synchronizer.appCancel) return "Canceling process, please wait ... ";

            if (Synchronizer.appLock)
                return  "skipped: <b><span style=\"color:green\">" + Synchronizer.appNumSki.ToString() +
                        " </span></b>updated: <b><span style=\"color:green\">" + Synchronizer.appNumUpd.ToString() +
                        " </span></b>added: <b><span style=\"color:green\">" + Synchronizer.appNumAdd.ToString() +
                        " </span></b>deleted: <b><span style=\"color:green\">" + Synchronizer.appNumDel.ToString() +
                        " </span></b>error: <b><span style=\"color:red\">" + Synchronizer.appNumErr.ToString() + "</span></b>" /*+ " )"*/;
            else
                return "";
        }

        public bool Getapplock()
        {
            return Synchronizer.appLock;
        }

        #endregion

    }

}