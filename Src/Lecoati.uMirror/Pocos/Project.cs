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
    [TableName("uMirrorProject")]
    [PrimaryKey("id", autoIncrement = true)]
    [ExplicitColumns]
    public class Project
    {
        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("XmlFileName")]
        public string XmlFileName { get; set; }

        [Column("UmbRootId")]
        public int? UmbRootId { get; set; }

        [Column("Preview")]
        public bool Preview { get; set; }

        [Column("LogAllAction")]
        public bool LogAllAction { get; set; }

        [Column("OldSchema")]
        public bool OldSchema { get; set; }

        [Column("Period")]
        public int? Period { get; set; }

        [Column("Dayofweek")]
        public string Dayofweek { get; set; }

        [Column("Dayofmonth")]
        public string Dayofmonth { get; set; }

        [Column("TriggerProyect")]
        public int? TriggerProyect { get; set; }

        [Column("StartHour")]
        public int? StartHour { get; set; }

        [Column("StartMinute")]
        public int? StartMinute { get; set; }

        [Column("RootNodeID")]
        public int? RootNodeID { get; set; }

        [Column("ExtensionMethod")]
        public string ExtensionMethod { get; set; }

        [Ignore]
        public Node[] Nodes { get; set; }
    }

    //public class ProjectNodeRelator
    //{
    //    public Project current;
    //    public Project MapIt(Project a, Node p)
    //    {

    //        try {

    //        if (a == null) return current;

    //        if (current != null && current.id == a.id)
    //        {
    //            current.Nodes = current.Nodes.Concat(new Node[] { p }).ToArray();
    //            return null;
    //        }

    //        var prev = current;
    //        current = a;
    //        current.Nodes = new Node[]{};
    //        if (p.id > 0) current.Nodes = current.Nodes.Concat(new Node[] { p }).ToArray();
    //        return prev;

    //        }
    //        catch (Exception ex)
    //        {
    //            return null;
    //        }
    //    }
    //}

}