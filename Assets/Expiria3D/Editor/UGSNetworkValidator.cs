namespace Expiria3DSpace
{
    using Expiria3DSpace.UGSService.Controller;
    using UnityEngine;
    using UnityEngine.Networking;

    public class UGSNetworkValidator : Expiria3DManager
    {
        public enum ConnectionState { NoInternetConnection, CannotConnectWithExpiria3D, Successful }

        public static ConnectionState ValidateConnectionToExpiria3D()
        {
            if (WorkerIsReachable())
            {
                return ConnectionState.Successful;
            }
            else
            {
                if (IsReachableURL("https://www.google.com"))
                {
                    return ConnectionState.CannotConnectWithExpiria3D;
                }
                else
                {
                    return ConnectionState.NoInternetConnection;
                }
            }
        }

        public static bool WorkerIsReachable()
        {
            WWWForm form = new WWWForm();
            form.AddField("ping", 0);

            UnityWebRequest www = UnityWebRequest.Post(User.ServiceURL, form);
            
            www.SendWebRequest();

            while (!www.isDone)
            {
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("<color=yellow>Expiria3D: </color>" + www.error);
                return false;
            }

            return true;
        }

        static bool IsReachableURL(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = 30; // Timeout after 30 seconds

                UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();

                // Wait until the request is done
                while (!asyncOperation.isDone) { }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return true;
                }
                else
                {
                    UnityEngine.Debug.LogWarning("<color=yellow>Expiria3D: </color>Error trying to connect to Expiria3D: " + request.error);
                    return false;
                }
            }
        }
    }
}
