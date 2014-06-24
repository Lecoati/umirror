using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

//These are the main namespaces we need to use
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Lecoati.uMirror.Pocos
{
    [Serializable()]
    [TableName("uMirrorNode")]
    [PrimaryKey("id", autoIncrement = true)]
    [ExplicitColumns]
    public class Node
    {
        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int id { get; set; }

        [Column("UmbIdentifierProperty")]
        public string UmbIdentifierProperty { get; set; }

        [Column("XmlIdentifierXPath")]
        public string XmlIdentifierXPath { get; set; }

        [Column("XmlNodeNameXPath")]
        public string XmlNodeNameXPath { get; set; }

        [Column("LevelNumber")]
        public int? LevelNumber { get; set; }

        [Column("TruncateNodeName")]
        public int? TruncateNodeName { get; set; }

        [Column("IgnoreNodeName")]
        public bool IgnoreNodeName { get; set; }

        [Column("NeverDelete")]
        public bool NeverDelete { get; set; }

        [Column("OnlyAdd")]
        public bool OnlyAdd { get; set; }

        [Column("XmlDocumentXPath")]
        public string XmlDocumentXPath { get; set; }

        [Column("ParentId")]
        public int? ParentId { get; set; }

        [Column("ProjectId")]
        public int? ProjectId { get; set; }

        [Column("UmbDocumentTypeAlias")]
        public string UmbDocumentTypeAlias { get; set; }

        [Column("Enable")]
        public bool Enable { get; set; }

        [Ignore]
        public Property[] Properties { get; set; }

        [Ignore]
        public Node[] Nodes { get; set; }
    }

    //public class NodePropertyRelator
    //{
    //    public Node current;
    //    public Node MapIt(Node a, Property p)
    //    {
    //        try { 

    //        if (a == null) return current;

    //        if (current != null && current.id == a.id)
    //        {
    //            current.Properties = current.Properties.Concat(new Property[] { p }).ToArray();
    //            return null;
    //        }

    //        var prev = current;
    //        current = a;
    //        current.Properties = new Property[] { };
    //        if (p.id > 0) current.Properties = current.Properties.Concat(new Property[] { p }).ToArray();
    //        return prev;

    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }

    //    }
    //}

}