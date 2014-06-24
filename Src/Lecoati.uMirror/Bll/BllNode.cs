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
    public class BllNode
    {

        public int CreateEmptyNode(int idProject, int docTypeId)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                Node newNode = new Node();
                newNode.ProjectId = idProject;
                newNode.UmbDocumentTypeAlias = ApplicationContext.Current.Services.ContentTypeService.GetContentType(docTypeId).Alias;
                db.Insert(newNode);
                return newNode.id;
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                return -1;
            }
        }

        public int CreateChildNode(int parentId, int docTypeId)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                Node newNode = new Node();
                newNode.UmbDocumentTypeAlias = ApplicationContext.Current.Services.ContentTypeService.GetContentType(docTypeId).Alias;
                newNode.ParentId = parentId;
                db.Insert(newNode);
                return newNode.id;
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                return -1;
            }
        }

        public void ImportNode(Node node)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                db.Insert(node);
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

        public void DeleteNode(int id)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                IList<Node> nodes = GetNodes(id);

                foreach (Node child in nodes)
                    DeleteNode(child.id);

                Node myNode = GetNode(id);

                foreach (Property prop in myNode.Properties)
                    db.Delete(prop);

                db.Delete(myNode);
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
            }
        }

        public Node GetNode(int? id)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                Node myNode = db.Query<Node>("SELECT * FROM uMirrorNode WHERE uMirrorNode.id = @0", id).FirstOrDefault();
                myNode.Properties = db.Query<Property>("SELECT * FROM uMirrorProperty WHERE uMirrorProperty.NodeID = @0", myNode.id).ToArray();
                return myNode;
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                return null;
            }
        }

        public IList<Node> GetNodes(int ParentId)
        {
            return GetNodes(ParentId, false);
        }

        public IList<Node> GetNodes(int ParentId, Boolean withChildren)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                IList<Node> result = db.Query<Node>("SELECT * FROM uMirrorNode WHERE uMirrorNode.ParentId = @0", ParentId).ToList();

                foreach (Node node in result)
                {
                    node.Properties = db.Query<Property>("SELECT * FROM uMirrorProperty WHERE uMirrorProperty.NodeID = @0", node.id).ToArray();
                    node.Nodes = GetNodes(node.id, true).ToArray();
                }
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                return null;
            }
        }

        public IList<Node> GetNodesByProyect(int? proyectId)
        {
            return GetNodesByProyect(proyectId, false);
        }

        public IList<Node> GetNodesByProyect(int? proyectId, Boolean withChildren)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                IList<Node> result = db.Query<Node>("SELECT * FROM uMirrorNode WHERE uMirrorNode.ProjectId = @0 AND uMirrorNode.ParentId is null", proyectId).ToList();

                foreach (Node node in result)
                {
                    node.Properties = db.Query<Property>("SELECT * FROM uMirrorProperty WHERE uMirrorProperty.NodeID = @0", node.id).ToArray();
                    node.Nodes = GetNodes(node.id, true).ToArray();
                }

                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                return null;
            }
        }

        public void UpdateNode(Node node)
        {
            try
            {

                var db = ApplicationContext.Current.DatabaseContext.Database;
                Node oldNode = GetNode(node.id);
                oldNode.XmlDocumentXPath = node.XmlDocumentXPath;
                oldNode.UmbIdentifierProperty = node.UmbIdentifierProperty;
                oldNode.XmlIdentifierXPath = node.XmlIdentifierXPath;
                oldNode.XmlNodeNameXPath = node.XmlNodeNameXPath;
                oldNode.LevelNumber = node.LevelNumber;
                oldNode.TruncateNodeName = node.TruncateNodeName;
                oldNode.NeverDelete = node.NeverDelete;
                oldNode.OnlyAdd = node.OnlyAdd;
                oldNode.ProjectId = node.ProjectId;
                oldNode.IgnoreNodeName = node.IgnoreNodeName;
                oldNode.UmbDocumentTypeAlias = node.UmbDocumentTypeAlias;
                db.Save(oldNode);

                foreach (Property prop in oldNode.Properties)
                {
                    db.Delete(prop);
                }

                foreach (Property prop in node.Properties)
                {
                    Property propNew = new Property();
                    propNew.XmlPropertyXPath = prop.XmlPropertyXPath;
                    propNew.UmbPropertyAlias = prop.UmbPropertyAlias;
                    propNew.MediaParent = prop.MediaParent;
                    propNew.Ignore = prop.Ignore;
                    propNew.InitLikeXml = prop.InitLikeXml;
                    propNew.NodeID = node.id;
                    db.Insert(propNew);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, "[uMirror] db error: " + ex.Message, ex);
                throw;
            }

        }

    }
}