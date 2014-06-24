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
    [TableName("uMirrorProperty")]
    [PrimaryKey("id", autoIncrement = true)]
    [ExplicitColumns]
    public class Property
    {
        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int id { get; set; }

        [Column("UmbPropertyAlias")]
        public string UmbPropertyAlias { get; set; }

        [Column("XmlPropertyXPath")]
        public string XmlPropertyXPath { get; set; }

        [Column("Ignore")]
        public bool Ignore { get; set; }

        [Column("MediaParent")]
        public int MediaParent { get; set; }

        [Column("InitLikeXml")]
        public bool InitLikeXml { get; set; }

        [Column("NodeID")]
        public int NodeID { get; set; }
    }
}