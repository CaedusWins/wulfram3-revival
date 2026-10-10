using System.Text.RegularExpressions;
using UnityEngine;

namespace Wulfram.Networking
{
    /// <summary>
    /// Points PUN at Photon Cloud with this project's App ID, which is kept OUT of git (the repo is
    /// public, and anyone holding the ID can spend its free 20-CCU allowance). First found wins:
    ///   1. command line   -photonAppId <id>
    ///   2. environment    WULFRAM_PHOTON_APPID
    ///   3. local file     Assets/Resources/PhotonAppId.local.txt   (gitignored; included in builds,
    ///                     so a built game connects when double-clicked)
    /// Region is fixed (every client must use the same one to see each other's rooms): "us" (US East),
    /// or -photonRegion <code>. Without an App ID it logs a warning and leaves the committed settings
    /// alone; offline play does not need one.
    ///
    /// The committed PhotonServerSettings.asset is never modified: an in-memory copy is changed instead.
    /// Changing the asset itself at runtime can be saved back to disk by the editor (AutoSaver calls
    /// AssetDatabase.SaveAssets), which would put the App ID into the public repo.
    /// </summary>
    public static class PhotonCloudSettings
    {
        public const string AppIdResource = "PhotonAppId.local";
        public const CloudRegionCode DefaultRegion = CloudRegionCode.us;

        private static readonly Regex AppIdFormat = new Regex("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$");

        public static bool Configured { get; private set; }

        /// <summary>
        /// Photon Chat needs a separate Photon app of type "Chat". The chat UI's serialized App ID is
        /// the original 2017 team's, which this project must not use, so chat stays off until a Chat
        /// App ID of our own is wired in (its own branch). Online play doesn't depend on chat.
        /// </summary>
        public static readonly bool ChatEnabled = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            string source = "-photonAppId";
            string appId = Arg("-photonAppId");
            if (string.IsNullOrEmpty(appId))
            {
                source = "WULFRAM_PHOTON_APPID";
                appId = System.Environment.GetEnvironmentVariable("WULFRAM_PHOTON_APPID");
            }
            if (string.IsNullOrEmpty(appId))
            {
                source = "Resources/" + AppIdResource + ".txt";
                TextAsset file = Resources.Load<TextAsset>(AppIdResource);
                appId = file == null ? null : file.text;
            }

            if (string.IsNullOrEmpty(appId))
            {
                Debug.LogWarning("PhotonCloud: no Photon App ID - online play will not connect (offline play still works). " +
                    "Put it in Assets/Resources/" + AppIdResource + ".txt, WULFRAM_PHOTON_APPID, or -photonAppId.");
                return;
            }

            appId = appId.Trim();
            if (!AppIdFormat.IsMatch(appId))
            {
                Debug.LogError("PhotonCloud: the App ID from " + source + " is not a valid Photon App ID - not using it");
                return;
            }

            CloudRegionCode region = DefaultRegion;
            string regionArg = Arg("-photonRegion");
            if (!string.IsNullOrEmpty(regionArg))
            {
                region = (CloudRegionCode)System.Enum.Parse(typeof(CloudRegionCode), regionArg, true);
            }

            ServerSettings copy = Object.Instantiate(PhotonNetwork.PhotonServerSettings);
            copy.UseCloud(appId, region);
            PhotonNetwork.PhotonServerSettings = copy;
            Configured = true;
            Debug.Log("PhotonCloud: Photon Cloud, region " + region + ", App ID " + appId.Substring(0, 8) + "... (from " + source + ")");
        }

        private static string Arg(string name)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
}
