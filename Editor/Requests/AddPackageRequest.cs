using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace PackageToSource
{
    public class AddPackageRequest : IRequest
    {
        private AddRequest _addRequest = null;

        public static AddPackageRequest AddLocalPackage(Package package)
        {
            AddPackageRequest request = new AddPackageRequest();
            request._addRequest = Client.Add("file:" + FileIO.Combine(Settings.gitProjectsPath, package.displayName));
            return request;
        }

        public static AddPackageRequest AddDistantPackage(Package package)
        {
            AddPackageRequest request = new AddPackageRequest();

            string packageTag = Git.GetTagName(package.resolvedPath); // Recompute tag as if might have changed
            if (packageTag.Length > 0)
            {
                request._addRequest = Client.Add(package.url + "#" + packageTag);
            }
            else
            {
                request._addRequest = Client.Add(package.url);
            }

            return request;
        }

        public override bool Update()
        {
            if (_addRequest != null && _addRequest.IsCompleted)
            {
                if (_addRequest.Status == StatusCode.Success)
                {
                    Logger.Log("Added: " + _addRequest.Result.packageId);
                }
                else
                {
                    Logger.Log(_addRequest.Error.message, Log.Error);
                }

                _addRequest = null;
                return true;
            }
            return false;
        }
    }
}
