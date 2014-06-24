//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

////These are the main namespaces we need to use
//using Umbraco.Core.Persistence;
//using Umbraco.Core.Persistence.DatabaseAnnotations;

//namespace Lecoati.uMirror.Pocos
//{
//    public class umbracoLog
//    {
//        [Column("id")]
//        [PrimaryKeyColumn(AutoIncrement = true)]
//        public int id { get; set; }

//        [Column("userId")]
//        public int userId { get; set; }

//        [Column("NodeId")]
//        public int NodeId { get; set; }

//        [Column("Datestamp")]
//        public DateTime Datestamp { get; set; }

//        [Column("logHeader")]
//        public string logHeader { get; set; }

//        [Column("logComment")]
//        public string logComment { get; set; }
//    }
//}