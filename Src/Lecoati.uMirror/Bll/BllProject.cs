using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.presentation;
using Umbraco.Core;
using System.ComponentModel.DataAnnotations;
using Lecoati.uMirror.Pocos;
using umbraco.BusinessLogic;
using Umbraco.Core.Logging;
using System.Reflection;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Text;
using Lecoati.uMirror.Core;
using umbraco.cms.businesslogic.web;

namespace Lecoati.uMirror.Bll
{
    public class BllProject
    {

        public IList<Project> GetAllProjects()
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                IList<Project> result = db.Query<Project>("SELECT * FROM uMirrorProject").ToList();

                foreach (Project index in result)
                    index.Nodes = new BllNode().GetNodesByProyect(index.id).ToArray();

                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                return null;
            }
        }

        public IList<Project> GetOthersProjects(int id)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                IList<Project> result = db.Query<Project>("SELECT * FROM uMirrorProject WHERE uMirrorProject.id != @0", id).ToList();
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                return null;
            }
        }

        public void ImportProject(Project project)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                db.Insert(project);
                foreach(Node node in project.Nodes)
                {
                    node.ProjectId = project.id;
                    new BllNode().ImportNode(node);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
            }
        }

        public int CreateEmptyProject(string alias)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                Project newProject = new Project();
                newProject.Name = alias;
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
                db.Insert(newProject);
                return newProject.id;
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                return -1;
            }
        }

        public void DeleteProject(int id)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;

                Project myProject = GetProject(id);

                foreach (Node node in myProject.Nodes)
                    new BllNode().DeleteNode(node.id);

                db.Delete(myProject);
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                throw;
            }
        }

        public Project GetProject(int? id)
        {
            return GetProject(id, false);
        }

        public Project GetProject(int? id, Boolean withChildren)
        {

            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                Project myProject = db.Query<Project>("SELECT * FROM uMirrorProject WHERE uMirrorProject.id = @0", id).SingleOrDefault();
                myProject.Nodes = new BllNode().GetNodesByProyect(id, withChildren).ToArray();
                return myProject;
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                throw;
            }

        }

        public void UpdateProject(Project project)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                Project oldProject = GetProject(project.id);
                oldProject.Name = project.Name;
                oldProject.XmlFileName = project.XmlFileName;
                oldProject.Preview = project.Preview;
                oldProject.LogAllAction = project.LogAllAction;
                oldProject.OldSchema = project.OldSchema;
                oldProject.Period = project.Period;
                oldProject.Dayofmonth = project.Dayofmonth;
                oldProject.Dayofweek = project.Dayofweek;
                oldProject.StartHour = project.StartHour;
                oldProject.StartMinute = project.StartMinute;
                oldProject.TriggerProyect = project.TriggerProyect;
                oldProject.UmbRootId = project.UmbRootId;
                oldProject.ExtensionMethod = project.ExtensionMethod;
                db.Save(oldProject);
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                throw;
            }

        }

        public Project GetProjectByNode(Node node)
        {
            while (node.ProjectId == null)
                node = new BllNode().GetNode(node.ParentId);

            return GetProject(node.ProjectId);
        }

        public String GetProjectFilePath(Project project)
        {
            string result = result = GetProjectFilePath(project.ExtensionMethod);
            if (string.IsNullOrEmpty(result))
                return project.XmlFileName;
            else
                return result;
        }

        public String GetProjectFilePath(String assemblyRef)
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

        public MethodInfo GetProjectMethod(String assemblyRef)
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

        public IList<MethodInfo> GetMethods()
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
                catch {}

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

        public enum PeriodType
        {
            none = -1,
            hourly = 0,
            daily = 1,
            weekly = 2,
            monthly = 3,
            after = 4,
        }

    }
}