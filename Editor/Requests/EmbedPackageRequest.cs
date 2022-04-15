using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace PackageToSource
{
    public class EmbedPackageRequest : IRequest
    {
        private EmbedRequest _embedRequest = null;

        public EmbedPackageRequest(Package package)
        {
            _embedRequest = Client.Embed("file:" + package.displayName); // TODO : Test without "file:" ?
        }

        public override bool Update()
        {
            if (_embedRequest != null && _embedRequest.IsCompleted)
            {
                if (_embedRequest.Status == StatusCode.Success)
                {
                    Logger.Log("Embedded: " + _embedRequest.Result.packageId);
                }
                else
                {
                    Logger.Log(_embedRequest.Error.message, Log.Error);
                }

                _embedRequest = null;
                return true;
            }
            return false;
        }
    }
}
