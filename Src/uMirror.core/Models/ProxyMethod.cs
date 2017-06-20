using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace uMirror.core.Models
{

    public class ProxyMethod
    {

        public string Name { get; set; }

        public string AssemblyRef { get; set; }

        public string FilePath { get; set; }

    }
}