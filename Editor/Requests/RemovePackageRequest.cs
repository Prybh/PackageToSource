using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace PackageToSource
{
    public class RemovePackageRequest : IRequest
    {
        private RemoveRequest _removeRequest = null;

        public RemovePackageRequest(Package package)
        {
            _removeRequest = Client.Remove(package.name);
        }

        public override bool Update()
        {
            if (_removeRequest != null && _removeRequest.IsCompleted)
            {
                if (_removeRequest.Status == StatusCode.Success)
                {
                    Logger.Log("Removed: " + _removeRequest.PackageIdOrName);
                }
                else
                {
                    Logger.Log(_removeRequest.Error.message, Log.Warning);
                }

                _removeRequest = null;
                return true;
            }
            return false;
        }
    }
}
