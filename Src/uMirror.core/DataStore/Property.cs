using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace uMirror.core.DataStore
{
    [XmlRoot(ElementName = "Properties")]
    public class PropertiesContainer
    {
        [XmlElement(ElementName = "Property")]
        public Property[] Properties { get; set; }
    }
    public class Property
    {
        [XmlElement]
        public int id { get; set; }

        [XmlElement]
        public string UmbPropertyAlias { get; set; }

        [XmlElement]
        public string XmlPropertyXPath { get; set; }

        [XmlElement]
        public bool Ignore { get; set; }

        [XmlElement]
        public int MediaParent { get; set; }

        [XmlElement]
        public bool InitLikeXml { get; set; }

        [XmlElement]
        public int NodeID { get; set; }
    }
}