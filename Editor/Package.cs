using System;

namespace PackageToSource
{
    [System.Serializable]
    public class Package
    {
        public string packageId = "";
        public string name = "";
        public string displayName = "";
        public string url = "";
        public string hash = "";
        public string tag = "";
        public string version = "";
    }
}