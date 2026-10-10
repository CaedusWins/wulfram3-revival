using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wulfram.OfflinePlay
{
    /// <summary>
    /// Offline practice play: the whole game on one machine with no Photon server, using PUN's
    /// offline mode. Off unless requested:
    ///   - built player:  Wulfram3.exe -offlinePlay
    ///   - Unity editor:  menu Wulfram > Offline Play (no server), then press Play
    ///
    /// From the launcher, press Play as usual: the guest login runs, LauncherWithLogin.Connect
    /// sees PUN as connected (always true offline) and calls JoinRandomRoom, which offline
    /// creates a local room, and OnJoinedRoom loads Playground. Leaving the room returns to the
    /// launcher, and Play works again.
    ///
    /// The game must start in the launcher: it sets up dependency injection, the player name and the
    /// room before Playground loads, and Playground's scripts assume that. In the editor, turning
    /// offline play on makes Play always start from the launcher (EditorSceneManager.playModeStartScene,
    /// see WulframOfflinePlayMenu), whatever scene is open; stopping returns to that scene.
    /// </summary>
    public static class OfflinePlay
    {
        public const string EditorPrefKey = "Wulfram.OfflinePlay";

        public static bool Active { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            Active = Requested();
            if (!Active)
            {
                return;
            }

            // Setting offlineMode while it is already on logs "Can't start OFFLINE mode while
            // connected!" (connected is always true offline), so only switch it once.
            if (!PhotonNetwork.offlineMode)
            {
                PhotonNetwork.offlineMode = true;
            }
            Debug.Log("OfflinePlay: PUN offline mode on - no server needed");

            if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                // Creating the room here instead was tried: Playground's scripts then get
                // OnJoinedRoom before their Start (PunTeams/TeamCounter NullReferenceException) and
                // leaving fails because the launcher never set up dependency injection.
                Debug.LogWarning("OfflinePlay: started in '" + SceneManager.GetActiveScene().name +
                    "', not the launcher - start from the launcher (Wulfram > Play Offline From Launcher)");
            }
        }

        private static bool Requested()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-offlinePlay")
                {
                    return true;
                }
            }
#if UNITY_EDITOR
            return UnityEditor.EditorPrefs.GetBool(EditorPrefKey, false);
#else
            return false;
#endif
        }
    }
}
