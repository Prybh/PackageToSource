using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace PackageToSource
{
    public class AddPackageRequest : IRequest
    {
        private AddRequest _addRequest = null;

        public AddPackageRequest(Package package)
        {
            _addRequest = Client.Add(package.url);
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
