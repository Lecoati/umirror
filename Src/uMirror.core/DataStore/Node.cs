using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace uMirror.core.DataStore
{
    [XmlRoot(ElementName = "Nodes")]
    public class NodesContainer
    {
        [XmlElement(ElementName = "Node")]
        public Node[] Nodes { get; set; }
    }
    public class Node
    {
        
        [XmlElement]
        public int id { get; set; }

        [XmlElement]
        public string UmbIdentifierProperty { get; set; }

        [XmlElement]
        public string XmlIdentifierXPath { get; set; }

        [XmlElement]
        public string XmlNodeNameXPath { get; set; }

        [XmlElement]
        public int? LevelNumber { get; set; }

        [XmlElement]
        public int? TruncateNodeName { get; set; }

        [XmlElement]
        public bool IgnoreNodeName { get; set; }

        [XmlElement]
        public bool NeverDelete { get; set; }

        [XmlElement]
        public bool OnlyAdd { get; set; }

        [XmlElement]
        public string XmlDocumentXPath { get; set; }

        [XmlElement]
        public int? ParentId { get; set; }

        [XmlElement]
        public int? ProjectId { get; set; }

        [XmlElement]
        public string UmbDocumentTypeAlias { get; set; }

        [XmlElement]
        public string UmbDocumentTypeIcon { get; set; }

        [XmlElement]
        public bool Enable { get; set; }

        [XmlIgnore]
        public Property[] Properties { get; set; }

        [XmlIgnore]
        public Node[] Nodes { get; set; }
    }
}