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

        /// <summary>
        /// The one room every player joins, by name, so players who press Play at the same moment
        /// meet (see LauncherWithLogin.JoinArena). -photonRoom <name> picks another room; the
        /// verification gate uses it to keep each test run apart from players and other runs.
        /// </summary>
        public const string DefaultRoomName = "Wulfram3";

        public static string RoomName
        {
            get
            {
                string room = Arg("-photonRoom");
                return string.IsNullOrEmpty(room) ? DefaultRoomName : room;
            }
        }

        /// <summary>
        /// Room size for rooms this client creates: -photonMaxPlayers <1-255> if given, else the
        /// launcher's serialized MaxPlayersPerRoom. Lets a test fill a room with two clients.
        /// </summary>
        public static byte MaxPlayers(byte fallback)
        {
            string arg = Arg("-photonMaxPlayers");
            byte value;
            if (!string.IsNullOrEmpty(arg) && byte.TryParse(arg, out value) && value > 0)
            {
                return value;
            }
            return fallback;
        }

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

        /// <summary>
        /// This session's Photon user ID. Photon refuses a second player with the same user ID in a
        /// room, and without one PUN uses the player's name, so two guests who drew the same name
        /// (two windows started in the same tick) could not share a room (2026-10-27 playtest).
        /// A fresh GUID per session makes it unique; -photonUserId sets it, for the gate's test of
        /// a refused join.
        /// </summary>
        public static string NewUserId()
        {
            string id = Arg("-photonUserId");
            return string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id;
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
