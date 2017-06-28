using System;

namespace uMirror.core.Bll
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