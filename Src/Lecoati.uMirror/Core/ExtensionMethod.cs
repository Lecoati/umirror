using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Xml.Linq;

namespace Lecoati.uMirror.Core
{

    public class uMirrorExtension
    {
        public uMirrorExtension()
        {
        }
    }


    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public class UMirrorProxy : System.Attribute
    {

        readonly string _filePath;

        public UMirrorProxy(String filePath)
        {
            _filePath = filePath;
        }

        public string FilePath { get { return _filePath; } }

    }

}