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

        [NonSerialized] public string version = "";
        [NonSerialized] public string branch = "";
        [NonSerialized] public string resolvedPath = "";
        [NonSerialized] public int filesChanged = 0;
        [NonSerialized] public bool isPackageToSourceProject = false;
    }
}