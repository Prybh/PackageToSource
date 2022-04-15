using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace PackageToSource
{
    public class PackageListRequest : IRequest
    {
        private ListRequest _listRequest = null;
        private List<Package> _distantPackages = new List<Package>();
        private List<Package> _localPackages = new List<Package>();

        public List<Package> distantPackages
        {
            get { return _distantPackages; }
        }
        public List<Package> localPackages
        {
            get { return _localPackages; }
        }

        public PackageListRequest()
        {
            _listRequest = Client.List();
        }

        public override bool Update()
        {
            if (_listRequest != null && _listRequest.IsCompleted)
            {
                if (_listRequest.Status == StatusCode.Success)
                {
                    _distantPackages.Clear();
                    _localPackages.Clear();

                    foreach (PackageInfo packageInfo in _listRequest.Result)
                    {
                        if (packageInfo.source == PackageSource.Git)
                        {
                            _distantPackages.Add(ConvertUnityGitPackageToPackage(packageInfo));
                        }
                        else if (packageInfo.source != PackageSource.BuiltIn)
                        {
                            UnityEngine.Debug.Log(packageInfo.displayName + " " + packageInfo.source.ToString());
                        }
                        // TODO : Maybe we can support Tarball/Registry ?
                    }
                }
                else
                {
                    Logger.Log(_listRequest.Error.message, Log.Error);
                }

                _listRequest = null;
                return true;
            }
            return false;
        }

        private static Package ConvertUnityGitPackageToPackage(PackageInfo packageInfo)
        {
            Package package = new Package();
            package.packageId = packageInfo.packageId;
            package.name = packageInfo.name;
            package.displayName = packageInfo.displayName;

            string gitUrl = packageInfo.packageId.Substring(packageInfo.packageId.LastIndexOf('@') + 1);
            string gitTag = "";

            int diesePos = gitUrl.LastIndexOf('#');
            if (diesePos >= 0)
            {
                gitTag = gitUrl.Substring(diesePos + 1);
                gitUrl = gitUrl.Substring(0, diesePos);
            }

            package.url = gitUrl;
            package.hash = packageInfo.git.hash;
            package.tag = gitTag;
            package.version = packageInfo.version;

            return package;
        }
    }
}
