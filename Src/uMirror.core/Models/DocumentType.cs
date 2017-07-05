using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace uMirror.core.Models
{

    public class DocumentType
    {

        public string Alias { get; set; }

        public int Id { get; set; }

        public string Icon { get; set; }
        public string Name { get; internal set; }
    }
}