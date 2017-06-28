using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using Umbraco.Core.Persistence;

namespace uMirror.core.DataStore
{
    [XmlRoot(ElementName = "Projects")]
    public class ProjectContainer
    {
        [XmlElement(ElementName = "Project")]
        public Project[] Projects { get; set; }
    }

    //[TableName("Projects")]
    //[DataContract(Name = "project")]
    public class Project
    {
        [XmlElement]
        public int id { get; set; }

        [XmlElement]
        public string Name { get; set; }

        [XmlElement]
        public string XmlFileName { get; set; }

        [XmlElement]
        public int? UmbRootId { get; set; }

        [XmlElement]
        public UmbracoNode UmbRootNode { get; set; }

        [XmlElement]
        public bool Preview { get; set; }

        [XmlElement]
        public bool LogAllAction { get; set; }

        [XmlElement]
        public bool OldSchema { get; set; }

        [XmlElement]
        public int? Period { get; set; }

        [XmlElement]
        public string Dayofweek { get; set; }

        [XmlElement]
        public string Dayofmonth { get; set; }

        [XmlElement]
        public int? TriggerProyect { get; set; }

        [XmlElement]
        public int? StartHour { get; set; }

        [XmlElement]
        public int? StartMinute { get; set; }

        [XmlElement]
        public int? RootNodeID { get; set; }

        [XmlElement]
        public string ExtensionMethod { get; set; }

        [XmlIgnore]
        public Node[] Nodes { get; set; }
    }

    public class UmbracoNode
    {

        [XmlElement]
        public int Id { get; set; }

        [XmlElement]
        public string Icon { get; set; }

        [XmlElement]
        public string Name { get; set; }

    }

}