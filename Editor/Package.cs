using System;

namespace PackageToSource
{
    [Serializable]
    public class Package
    {
        public string packageId = "";
        public string name = "";
        public string displayName = "";
        public string url = "";
        public string hash = "";
        public string tag = "";
        public string version = "";
        public string branch = "";
        [NonSerialized] public string resolvedPath = "";
        [NonSerialized] public int filesChanged = 0;
    }
}