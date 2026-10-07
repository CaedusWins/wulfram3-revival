using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wulfram.SmokeTest
{
    /// <summary>
    /// End-to-end boot check for a built player, with no Photon server needed.
    /// Does nothing unless the player is started with -offlineSmokeTest.
    ///
    /// With the flag: switches PUN to offline mode and creates a local room, which
    /// sends the real launcher (LauncherWithLogin.OnJoinedRoom) down its normal
    /// path into Playground. Then waits, reports what loaded, and quits - exit
    /// code 0 if Playground loaded, the local player spawned and no exceptions
    /// were logged, 1 otherwise.
    ///
    ///   Wulfram3.exe -batchmode -offlineSmokeTest [-smokeSeconds 15]
    /// </summary>
    public class OfflineSmokeTest : MonoBehaviour
    {
        private int exceptions;
        private int errors;
        private float seconds = 15f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            bool enabled = false;
            float seconds = 15f;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-offlineSmokeTest")
                {
                    enabled = true;
                }
                if (args[i] == "-smokeSeconds" && i + 1 < args.Length)
                {
                    float.TryParse(args[i + 1], out seconds);
                }
            }

            if (!enabled)
            {
                return;
            }

            GameObject go = new GameObject("OfflineSmokeTest");
            DontDestroyOnLoad(go);
            go.AddComponent<OfflineSmokeTest>().seconds = seconds;
        }

        private void Awake()
        {
            Application.logMessageReceived += OnLog;
        }

        private void OnLog(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                exceptions++;
            }
            else if (type == LogType.Error)
            {
                errors++;
            }
        }

        private IEnumerator Start()
        {
            Debug.Log("SMOKE: starting in scene '" + SceneManager.GetActiveScene().name + "'");
            PhotonNetwork.offlineMode = true;
            PhotonNetwork.CreateRoom("smoke");

            float deadline = Time.realtimeSinceStartup + seconds;
            while (SceneManager.GetActiveScene().name != "Playground" && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            string scene = SceneManager.GetActiveScene().name;
            Debug.Log("SMOKE: active scene '" + scene + "', inRoom=" + PhotonNetwork.inRoom);

            // Let Playground run for a while so per-frame errors show up.
            float settle = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < settle)
            {
                yield return null;
            }

            int missing = 0;
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                Component[] all = roots[r].GetComponentsInChildren<Component>(true);
                for (int c = 0; c < all.Length; c++)
                {
                    if (all[c] == null)
                    {
                        missing++;
                    }
                }
            }

            bool playerSpawned = Com.Wulfram3.PlayerMovementManager.LocalPlayerInstance != null;
            int cargo = FindObjectsOfType<Com.Wulfram3.Cargo>().Length;
            Debug.Log("SMOKE: scene=" + scene + " playerSpawned=" + playerSpawned + " cargo=" + cargo +
                " missingScripts=" + missing + " exceptions=" + exceptions + " errors=" + errors);

            bool pass = scene == "Playground" && playerSpawned && missing == 0 && exceptions == 0;
            Debug.Log("SMOKE: " + (pass ? "PASS" : "FAIL"));
            Application.Quit();
            // Application.Quit has no exit-code overload in 2017.3; the PASS/FAIL line is the result.
        }
    }
}
