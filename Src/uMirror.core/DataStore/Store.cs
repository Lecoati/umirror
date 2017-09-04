using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using uMirror.core.Bll;

namespace uMirror.core.DataStore
{
    
    public static class Store
    {
        private const int NumberOfRetries = 3;
        private const int DelayOnRetry = 1000;
        private static string _storeDir;

        static Store()
        {
            _storeDir = HttpContext.Current.Server.MapPath(SystemDirectories.Data);
            _storeDir += (!_storeDir.EndsWith("\\") ? "\\" : "") + "uMirror\\Config\\";
            if (!Directory.Exists(_storeDir))
            {
                Directory.CreateDirectory(_storeDir);
            }
        }

        // Inner Factory methods
        /// <summary>
        /// Initialize the XML serializer for T and sets de fileName where T is stored
        /// </summary>
        /// <typeparam name="T">Project, Node or Property type</typeparam>
        /// <param name="fileName">Stores the file name for T</param>
        /// <param name="serializer"></param>
        private static void GetFileSettings<T>(ref string fileName, ref XmlSerializer serializer)
        {
            if (typeof(T) == typeof(Project))
            {
                serializer = new XmlSerializer(typeof(ProjectContainer));
                fileName = "Projects.xml";
            }
            if (typeof(T) == typeof(Node))
            {
                serializer = new XmlSerializer(typeof(NodesContainer));
                fileName = "Nodes.xml";
            }
            if (typeof(T) == typeof(Property))
            {
                serializer = new XmlSerializer(typeof(PropertiesContainer));
                fileName = "Properties.xml";
            }
        }
            
        private static object GetFromFile<T>()
        {
            XmlSerializer xmlSer = null;
            string fileName = "";
            object data = null;
            GetFileSettings<T>(ref fileName, ref xmlSer);            
            if (File.Exists(_storeDir + fileName))
            {

                for (int i=1; i <= NumberOfRetries; ++i) {
                    try
                    {
                        using (FileStream fileReader = new FileStream(_storeDir + fileName, FileMode.Open))
                        {
                            data = xmlSer.Deserialize(fileReader);
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        // You may check error code to filter some exceptions, not every error
                        // can be recovered.
                        if (i == NumberOfRetries) // Last one, (re)throw exception and exit
                        {
                            LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                            throw;
                        }
                            

                        Thread.Sleep(DelayOnRetry);
                    }
                }
           
            }            
            return data;
        }

        private static void SaveToFile(Project data)
        {
            XmlSerializer xmlSer = null;
            string fileName = "";

            ProjectContainer Container = new ProjectContainer();            
            List<Project> projects = GetAllProjects().ToList();           

            GetFileSettings<Project>(ref fileName, ref xmlSer);
            Project pr = projects.FirstOrDefault(p => p.id == data.id);
            if (pr != null)
            {
                var index = projects.IndexOf(pr);
                projects.Remove(pr);
                projects.Insert(index, data);
            }
            else
            {
                projects.Add(data);
            }
            
            Container.Projects = projects.ToArray();            
            SaveToFile(xmlSer, Container, fileName); 
        }

        private static void SaveToFile(Node data)
        {
            XmlSerializer xmlSer = null;
            string fileName = "";

            NodesContainer Container = null;
            List<Node> nodes = null;

            GetFileSettings<Node>(ref fileName, ref xmlSer);
            Container = GetFromFile<Node>() as NodesContainer;

            if (Container == null)
                Container = new NodesContainer();

            if (Container.Nodes != null)
                nodes = Container.Nodes.ToList();
            else nodes = new List<Node>();

            Node n = nodes.FirstOrDefault(nd => nd.id == data.id);

            if (n != null)
            {
                var index = nodes.IndexOf(n);
                nodes.Remove(n);
                nodes.Insert(index, data);
            }
            else
            {
                nodes.Add(data);
            }

            Container.Nodes = nodes.ToArray();
            SaveToFile(xmlSer, Container, fileName);
        }

        private static void SaveToFile(Property data)
        {
            XmlSerializer xmlSer = null;
            string fileName = "";
            List<Property> properties = null;
            PropertiesContainer Container = GetFromFile<Property>() as PropertiesContainer;

            GetFileSettings<Property>(ref fileName, ref xmlSer);

            if (Container == null)
                Container = new PropertiesContainer();

            if (Container.Properties != null)
            {
                properties = Container.Properties.ToList();
                Property pr = properties.FirstOrDefault(p => p.id == data.id);
                if (pr != null)
                    properties.Remove(pr);                
            }               
        
            else
                properties = new List<Property>();

            properties.Add(data as Property);
            Container.Properties = properties.ToArray();
            SaveToFile(xmlSer, Container, fileName);
        }

        private static void SaveToFile(XmlSerializer xmlSer, object DataToFile, string fileName)
        {
            try
            {
                using (FileStream fs = new FileStream(_storeDir + fileName, FileMode.Create))
                {
                    xmlSer.Serialize(fs, DataToFile);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                throw ex;
            }
        }
        
        private static int GetNewId<T>()
        {
            int id = -1;
            string fileName = "", xmlPath = "";
            XmlSerializer xmlSer = null;
            XmlDocument xmlDoc = new XmlDocument();

            GetFileSettings<T>(ref fileName, ref xmlSer);

            if (!File.Exists(_storeDir + fileName))
                return 1;

            switch (fileName.ToLower())
            {
                case "projects.xml":
                    xmlPath = "/Projects/Project";
                    break;
                case "nodes.xml":
                    xmlPath = "/Nodes/Node";
                    break;
                case "properties.xml":
                    xmlPath = "/Properties/Property";
                    break;
            }

            xmlDoc.Load(_storeDir + fileName);
            if (xmlDoc.SelectNodes(xmlPath).Count == 0) return 1;

            var MaxId = xmlDoc.SelectNodes(xmlPath + "/id")
                .Cast<XmlNode>()
                .Max(n => int.TryParse(n.InnerText, out id));

            id++;

            return id;
        }

        private static void DeleteById<T>(int id)
        {
            string fileName = "", xmlPath = "", xmlRoot = "";
            XmlSerializer serializer = null;
            GetFileSettings<T>(ref fileName, ref serializer);
            XmlDocument xmlDoc = new XmlDocument();

            switch (fileName.ToLower())
            {
                case "projects.xml":
                    xmlPath = "/Projects/Project[id=" + id.ToString() + "]";
                    xmlRoot = "/Projects";
                    break;
                case "nodes.xml":
                    xmlPath = "/Nodes/Node[id=" + id.ToString() + "]"; ;
                    xmlRoot = "/Nodes";
                    break;
                case "properties.xml":
                    xmlPath = "/Properties/Property[id=" + id.ToString() + "]";
                    xmlRoot = "/Properties";
                    break;
            }
            if (File.Exists(_storeDir + fileName))
            {
                xmlDoc.Load(_storeDir + fileName);
                var MaxId = xmlDoc.SelectSingleNode(xmlPath);
                xmlDoc.SelectSingleNode(xmlRoot).RemoveChild(MaxId);                
                xmlDoc.Save(_storeDir + fileName);
            }

        }

        
        
        // Projects Factory Methods
        public static IList<Project> GetAllProjects()
        {
            IList<Project> Projects = null;
            try
            {
                ProjectContainer n = (GetFromFile<Project>() as ProjectContainer);
                if (n != null && n.Projects != null)
                    Projects = n.Projects;                 
                else               
                    Projects = new List<Project>();

            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                throw ex;
            }
            return Projects;
        }        

        public static int CreateEmptyProject(string name)
        {
            Project newProject = new Project();

            newProject.id = GetNewId<Project>();
            newProject.Name = name;
            newProject.Dayofmonth = string.Empty;
            newProject.Dayofweek = string.Empty;
            newProject.ExtensionMethod = string.Empty;
            newProject.LogAllAction = false;
            newProject.OldSchema = false;
            newProject.Period = null;
            newProject.Preview = false;
            newProject.RootNodeID = null;
            newProject.StartHour = null;
            newProject.StartMinute = null;
            newProject.TriggerProyect = null;
            newProject.UmbRootId = null;
            newProject.XmlFileName = string.Empty;

            SaveToFile(newProject);
            return newProject.id;
        }

        public static void DeleteProject(int id)
        {
            IList<Node> Nodes = GetNodesByProject(id);
            if (Nodes != null)
                foreach (Node n in Nodes)
                    DeleteNode(n.id);

            DeleteById<Project>(id);
        }

        public static IList<Project> GetOtherProjects(int id)
        {
            IList<Project> Projects = null;
            try
            {
                Projects = (GetFromFile<Project>() as ProjectContainer).Projects.Where(p => p.id != id).ToList();
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
            }
            return Projects;
        }

        public static Project GetProject (int ? id, bool withChildren = false)
        {
            IList<Project> lProjects = GetAllProjects();
            if (lProjects != null)
            {
                Project project = lProjects.FirstOrDefault(p => p.id == id);

                if (withChildren)
                    project.Nodes = GetNodesByProject(id).ToArray();

                if (project != null)                
                    return project;                
            }
            
            return null;
        }

        public static void ImportProject(Project project)
        {
            SaveToFile(project);
            foreach (Node item in project.Nodes)
            {
                item.ProjectId = project.id;
                SaveToFile(item);
            }
        }

        public static Project GetProjectByNode (Node node)
        {
            Node n = GetNodes(node.id, false).FirstOrDefault(p => p.ProjectId != null);
            if (n != null)
                return GetProject(n.ProjectId);
            else return null;
        }        

        public static void UpdateProject(Project project)
        {
            SaveToFile(project);
        }

        public static string GetProjectFilePath(Project project)
        {
            string result = string.Empty;
            if (project != null)
            {
                result = GetProjectFilePath(project.ExtensionMethod);

                if (string.IsNullOrWhiteSpace(result))
                    result = project.XmlFileName;                
            }            
            return result;

        }

        public static string GetProjectFilePath(string assemblyRef)
        {
            if (assemblyRef != null && assemblyRef != string.Empty &&
                assemblyRef.Split(';').Count() == 2)
            {

                Assembly assembly = Assembly.Load(assemblyRef.Split(';')[0].Trim());

                var methods = assembly.GetTypes()
                      .SelectMany(t => t.GetMethods())
                      .Where(m => m.Name == assemblyRef.Split(';')[1].Trim());

                if (methods.Any())
                {
                    var attributes = methods.First().GetCustomAttributes(typeof(UMirrorProxy), false).OfType<UMirrorProxy>();
                    if (attributes.Any())
                        return attributes.First().FilePath;
                }
            }

            return string.Empty;
        }

        public static MethodInfo GetProjectMethod(String assemblyRef)
        {
            if (assemblyRef != string.Empty &&
                assemblyRef.Split(';').Count() == 2)
            {
                Assembly assembly = Assembly.Load(assemblyRef.Split(';')[0].Trim());

                var methods = assembly.GetTypes()
                      .SelectMany(t => t.GetMethods())
                      .Where(m => m.Name == assemblyRef.Split(';')[1].Trim());

                if (methods.Any())
                {
                    return methods.First();
                }
            }

            return null;
        }

        public static IList<MethodInfo> GetMethods()
        {
            IList<MethodInfo> result = new List<MethodInfo>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            List<Type> types = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    foreach (Type t in assembly.GetTypes())
                    {
                        if (t.IsSubclassOf(typeof(uMirrorExtension)))
                        {
                            if (!types.Exists(type => type.Equals(t)))
                            {
                                result =
                                    result.Concat(
                                        t.GetMethods()
                                            .Where(m => m.GetCustomAttributes(typeof(UMirrorProxy), false).Length > 0)
                                            .ToList()).ToList();
                            }
                        }
                    }
                }
                catch { }

            }
            return result;
        }

        public static Project DeSerialize(string xml)
        {
            Project obj = null;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Project));
                StringReader stringReader = new StringReader(xml);
                XmlTextReader xmlReader;
                xmlReader = new XmlTextReader(stringReader);
                Project conf = (Project)ser.Deserialize(xmlReader);
                xmlReader.Close();
                stringReader.Close();
                return conf;
            }
            catch { }
            return obj;
        }

        public static string Serialize(Project e)
        {
            string xml = "";
            XmlSerializer Serializer = new XmlSerializer(typeof(Project));
            MemoryStream memStream = new MemoryStream();
            XmlTextWriter xmlWriter = new XmlTextWriter(memStream, Encoding.UTF8);
            Serializer.Serialize(xmlWriter, e);
            xmlWriter.Close();
            memStream.Close();
            xml = Encoding.UTF8.GetString(memStream.GetBuffer());
            xml = xml.Substring(xml.IndexOf(Convert.ToChar(60)));
            xml = xml.Substring(0, (xml.LastIndexOf(Convert.ToChar(62)) + 1));
            return xml;
        }

        // Projects options
        public enum PeriodType
        {
            none = -1,
            hourly = 0,
            daily = 1,
            weekly = 2,
            monthly = 3,
            after = 4,
        }


        // Nodes factory
        public static int CreateEmptyNode(int idProject, int docTypeId)
        {
            try
            {
                Node newNode = new Node();                
                newNode.ProjectId = idProject;
                newNode.UmbDocumentTypeAlias = ApplicationContext.Current.Services.ContentTypeService.GetContentType(docTypeId).Alias;
                newNode.id = GetNewId<Node>();
                SaveToFile(newNode);
                return newNode.id;
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                return -1;
            }
        }

        public static int CreateChildNode(int parentId, int docTypeId)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                Node newNode = new Node();
                newNode.UmbDocumentTypeAlias = ApplicationContext.Current.Services.ContentTypeService.GetContentType(docTypeId).Alias;
                newNode.ParentId = parentId;
                newNode.id = GetNewId<Node>();
                SaveToFile(newNode);
                return newNode.id;
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                return -1;
            }
        }

        public static void ImportNode(Node node)
        {
            try
            {                
                if (node.id == 0 || node.id == -1)                
                    node.id = GetNewId<Node>();
                
                UpdateNode(node);
                foreach (Node child in node.Nodes)
                {
                    child.ParentId = node.id;
                    ImportNode(child);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
            }
        }

        public static IList<Node>GetNodesByProject(int ? id, bool withChildren = false)
        {
            string fileName = "";
            XmlSerializer serializer = null;

            GetFileSettings<Node>(ref fileName, ref serializer);

            if (File.Exists(_storeDir + fileName))
            {
                NodesContainer nList = null;
                using (FileStream fs = new FileStream(_storeDir + fileName, FileMode.Open))
                {
                    nList = serializer.Deserialize(fs) as NodesContainer;
                }
                if (nList != null && nList.Nodes != null)
                {
                    IList<Node> prNodes = nList.Nodes.Where(p => p.ProjectId == id && p.ParentId == -1).ToList();
                    foreach (Node node in prNodes)
                    {
                        if (withChildren)
                            node.Nodes = GetNodes(node.id, true).ToArray();

                        node.Properties = GetProperties(node.id).ToArray();
                    }
                    return nList.Nodes.Where(p => p.ProjectId == id && p.ParentId == -1).ToList();
                }
                else return new List<Node>();
                    
            }
            return new List<Node>(); 
        }

        public static Node GetNode(int ? id)
        {
            string fileName = "";
            XmlSerializer serializer = null;

            GetFileSettings<Node>(ref fileName, ref serializer);

            if (File.Exists(_storeDir + fileName))
            {
                NodesContainer nList = null;
                using (FileStream fs = new FileStream(_storeDir + fileName, FileMode.Open))
                {
                    nList = serializer.Deserialize(fs) as NodesContainer;
                }
                if (nList != null && nList.Nodes != null)
                {
                    Node n = null;
                    n = nList.Nodes.FirstOrDefault(p => p.id == id);
                    if (n != null)
                        n.Properties = GetProperties(n.id).ToArray();
                    return n; 
                }
                    
            }
            return null;
        }

        public static IList<Node> GetAllNodes(bool withChildren = false)
        {
            string fileName = "";
            XmlSerializer serializer = null;

            GetFileSettings<Node>(ref fileName, ref serializer);

            if (File.Exists(_storeDir + fileName))
            {
                NodesContainer nList = null;
                using (FileStream fs = new FileStream(_storeDir + fileName, FileMode.Open))
                {
                    nList = serializer.Deserialize(fs) as NodesContainer;
                }
                if (nList != null && nList.Nodes != null)
                {
                    List<Node> result = nList.Nodes.ToList();

                    foreach (var item in result)
                    {
                        item.Properties = GetProperties(item.id).ToArray();
                        if (withChildren)
                            item.Nodes = GetNodes(item.id, true).ToArray();
                    }
                    return nList.Nodes.ToList();
                }
            }
            return new List<Node>();
        }

        public static IList<Node> GetNodes(int id, bool withChildren = false)
        {
            string fileName = "";
            XmlSerializer serializer = null;

            GetFileSettings<Node>(ref fileName, ref serializer);

            if (File.Exists(_storeDir + fileName))
            {
                NodesContainer nList = null;
                using (FileStream fs = new FileStream(_storeDir + fileName, FileMode.Open))
                {
                    nList = serializer.Deserialize(fs) as NodesContainer;
                }
                if (nList != null && nList.Nodes != null)
                {
                    List <Node> result = nList.Nodes.Where(p => p.ParentId == id).ToList();                    
                    
                    foreach (var item in result)
                    {
                        item.Properties = GetProperties(item.id).ToArray();
                        if (withChildren)
                            item.Nodes = GetNodes(item.id, true).ToArray();
                    }                                                                                   
                    return nList.Nodes.Where(p => p.ParentId == id).ToList();
                }                    
            }
            return new List<Node>();
        }

        public static void DeleteNode(int id)
        {
            IList<Node> Nodes = GetNodes(id, true);
            IList<Property> Properties = GetProperties(id);

            foreach (Node n in Nodes)            
                DeleteNode(n.id);

            foreach (Property p in Properties)
                DeleteById<Property>(p.id);
            
            DeleteById<Node>(id);            
        }

        public static void UpdateNode (Node node)
        {
            Node n = GetNode(node.id);
            if (n != null)
                foreach (Property item in n.Properties)
                    DeleteById<Property>(item.id);           

            SaveToFile(node);            
            foreach (Property p in node.Properties)
            {
                Property propNew = new Property();

                propNew.XmlPropertyXPath = p.XmlPropertyXPath;
                propNew.UmbPropertyAlias = p.UmbPropertyAlias;
                propNew.MediaParent = p.MediaParent;
                propNew.Ignore = p.Ignore;
                propNew.InitLikeXml = p.InitLikeXml;
                propNew.NodeID = node.id;                
                propNew.id = GetNewId<Property>();

                SaveToFile(propNew);
            }
        }


        /// Properties
        /// 
        private static IList<Property> GetProperties(int id)
        {
            string fileName = "";
            XmlSerializer serializer = null;

            GetFileSettings<Property>(ref fileName, ref serializer);

            if (File.Exists(_storeDir + fileName))
            {
                PropertiesContainer nList = null;
                using (FileStream fs = new FileStream(_storeDir + fileName, FileMode.Open))
                {
                    nList = serializer.Deserialize(fs) as PropertiesContainer;
                }
                if (nList != null && nList.Properties != null)
                    return nList.Properties.Where(r => r.NodeID == id).ToList();
                else
                    return new List<Property>();
            }
            else return new List<Property>(); 
        }
    }


}