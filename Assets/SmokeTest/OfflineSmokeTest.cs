using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wulfram.SmokeTest
{
    /// <summary>
    /// End-to-end check for a built player, with no Photon server needed.
    /// Does nothing unless the player is started with -offlineSmokeTest.
    ///
    /// With the flag it switches PUN to offline mode and creates a local room, which
    /// sends the real launcher (LauncherWithLogin.OnJoinedRoom) down its normal path
    /// into Playground, then:
    ///   1. lets Playground run, and scans it for missing scripts;
    ///   2. drives targeting: cycles Tab targets through TargetController.CycleTarget
    ///      (each pick must be damageable and not the player's own tank), then selects
    ///      a probe Unit with no HitPointsManager in front of the camera - what pressing
    ///      T on a power cell or shell does - so TargetInfoController has to cope;
    ///   3. leaves the room and expects GameManager to return to the launcher at
    ///      build index 0.
    /// It prints one "SMOKE: PASS" or "SMOKE: FAIL" line and quits (2017.3 has no
    /// Application.Quit(exitCode), so that line is the result).
    ///
    ///   Wulfram3.exe -batchmode -offlineSmokeTest [-smokeSeconds 15]
    ///
    /// Add -offlinePlay to enter through the launcher's real Play button instead of creating the
    /// room directly (tests the offline play mode, Wulfram.OfflinePlay.OfflinePlay).
    ///
    /// Run it once without -batchmode as well: only a rendered frame makes the probe
    /// visible, which is what reaches the body of TargetInfoController.LateUpdate
    /// (reported as probePanelShown=True).
    ///
    /// Optional, windowed runs only: -smokeScreenshots <dir> saves the game's own rendered
    /// frames there (launcher, arena, a 360-degree pan of the player's tank as pan-NN.png,
    /// the target panel, the probe, back at the launcher). Without the flag nothing is saved
    /// and the tank is never turned.
    /// </summary>
    public class OfflineSmokeTest : MonoBehaviour
    {
        private int exceptions;
        private int errors;
        private float seconds = 15f;
        private string shotsDir;
        private bool online;
        private int expectPlayers = 1;
        private readonly System.Collections.Generic.List<string> firstProblems = new System.Collections.Generic.List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            bool enabled = false;
            float seconds = 15f;
            string shotsDir = null;
            bool online = false;
            int expectPlayers = 1;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-offlineSmokeTest")
                {
                    enabled = true;
                }
                if (args[i] == "-smokeOnline")
                {
                    online = true;
                }
                if (args[i] == "-smokeExpectPlayers" && i + 1 < args.Length)
                {
                    int.TryParse(args[i + 1], out expectPlayers);
                }
                if (args[i] == "-smokeSeconds" && i + 1 < args.Length)
                {
                    float.TryParse(args[i + 1], out seconds);
                }
                if (args[i] == "-smokeScreenshots" && i + 1 < args.Length)
                {
                    shotsDir = args[i + 1];
                }
            }

            if (!enabled)
            {
                return;
            }

            GameObject go = new GameObject("OfflineSmokeTest");
            DontDestroyOnLoad(go);
            OfflineSmokeTest test = go.AddComponent<OfflineSmokeTest>();
            test.seconds = seconds;
            test.shotsDir = shotsDir;
            test.online = online;
            test.expectPlayers = Mathf.Max(1, expectPlayers);
        }

        // Saves the next rendered frame as <shotsDir>/<name>.png (no-op without -smokeScreenshots).
        private IEnumerator Shot(string name)
        {
            if (shotsDir == null)
            {
                yield break;
            }
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(shotsDir, name + ".png"));
            // The capture is written at the end of the frame.
            yield return null;
            yield return null;
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
            else
            {
                return;
            }

            // Kept for the report; logging from inside this handler would recurse.
            if (firstProblems.Count < 5)
            {
                string where = stackTrace == null ? "" : stackTrace.Split('\n')[0];
                firstProblems.Add(type + ": " + message + " @ " + where);
            }
        }

        private IEnumerator Start()
        {
            Debug.Log("SMOKE: starting in scene '" + SceneManager.GetActiveScene().name + "'");
            if (shotsDir != null)
            {
                // Give the launcher UI a moment to draw.
                float uiReady = Time.realtimeSinceStartup + 1.5f;
                while (Time.realtimeSinceStartup < uiReady)
                {
                    yield return null;
                }
                yield return StartCoroutine(Shot("1-launcher"));
            }
            if (Wulfram.OfflinePlay.OfflinePlay.Active || online)
            {
                // Go in exactly the way a player does: the launcher's Play button handler (guest
                // login -> Connect -> ConnectUsingSettings / JoinRandomRoom). With -offlinePlay that
                // is PUN offline mode; with -smokeOnline it is the real Photon Cloud.
                Com.Wulfram3.LauncherWithLogin launcher = FindObjectOfType<Com.Wulfram3.LauncherWithLogin>();
                Debug.Log("SMOKE: entering via the launcher's Play button (" + (online ? "online, Photon Cloud" : "offline play") + ")" +
                    (launcher == null ? " - NO LAUNCHER FOUND" : ""));
                if (launcher != null)
                {
                    launcher.Login();
                }
            }
            else
            {
                if (!PhotonNetwork.offlineMode)
                {
                    PhotonNetwork.offlineMode = true;
                }
                PhotonNetwork.CreateRoom("smoke");
            }

            // Connecting to the cloud takes longer than an offline room.
            float deadline = Time.realtimeSinceStartup + (online ? Mathf.Max(seconds, 45f) : seconds);
            while (SceneManager.GetActiveScene().name != "Playground" && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            string scene = SceneManager.GetActiveScene().name;
            Debug.Log("SMOKE: active scene '" + scene + "', inRoom=" + PhotonNetwork.inRoom);

            // 1. Let Playground run for a while so per-frame errors show up.
            float settle = Time.realtimeSinceStartup + seconds;
            bool arenaShot = false;
            while (Time.realtimeSinceStartup < settle)
            {
                if (!arenaShot && shotsDir != null && Time.realtimeSinceStartup > settle - seconds + 3f)
                {
                    arenaShot = true;
                    yield return StartCoroutine(Shot("2-arena"));
                }
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

            // With -smokeExpectPlayers N (online), wait until N players are in the room and N tanks
            // exist here - the M2 proof that clients see each other - then hold a moment so the other
            // clients can see this one too before it leaves.
            int playersSeen = PhotonNetwork.playerList.Length;
            int tanksSeen = FindObjectsOfType<Com.Wulfram3.PlayerMovementManager>().Length;
            if (expectPlayers > 1)
            {
                float waitPlayers = Time.realtimeSinceStartup + Mathf.Max(seconds, 90f);
                while ((playersSeen < expectPlayers || tanksSeen < expectPlayers) && Time.realtimeSinceStartup < waitPlayers)
                {
                    yield return null;
                    playersSeen = PhotonNetwork.playerList.Length;
                    tanksSeen = FindObjectsOfType<Com.Wulfram3.PlayerMovementManager>().Length;
                }
                Debug.Log("SMOKE: sees " + playersSeen + " player(s) and " + tanksSeen + " tank(s), expected " + expectPlayers);
                float hold = Time.realtimeSinceStartup + 10f;
                while (Time.realtimeSinceStartup < hold)
                {
                    yield return null;
                }
            }
            // M2: damage both ways, through the game's real network path - the call AutoCannon makes on
            // a hit (HitPointsManager.TellServerTakeDamage -> RPC to the master client, which applies
            // it and broadcasts UpdateHealth to all). Each client hits the other tank for 10 once;
            // both clients must then see both tanks at 90. Only aiming/raycasting is skipped.
            string damage = "";
            bool damageOk = true;
            if (online && expectPlayers > 1)
            {
                GameObject me = Com.Wulfram3.PlayerMovementManager.LocalPlayerInstance;
                Com.Wulfram3.HitPointsManager mine = me == null ? null : me.GetComponent<Com.Wulfram3.HitPointsManager>();
                Com.Wulfram3.HitPointsManager other = null;
                Com.Wulfram3.PlayerMovementManager[] tanks = FindObjectsOfType<Com.Wulfram3.PlayerMovementManager>();
                for (int i = 0; i < tanks.Length; i++)
                {
                    if (tanks[i].gameObject != me)
                    {
                        other = tanks[i].GetComponent<Com.Wulfram3.HitPointsManager>();
                    }
                }

                if (mine == null || other == null)
                {
                    damageOk = false;
                    damage = " damage: no " + (mine == null ? "own" : "other") + " tank";
                }
                else
                {
                    int mineBefore = mine.health;
                    int otherBefore = other.health;
                    other.TellServerTakeDamage(10);
                    float waitDamage = Time.realtimeSinceStartup + 30f;
                    while ((mine.health != 90 || other.health != 90) && Time.realtimeSinceStartup < waitDamage)
                    {
                        yield return null;
                    }
                    damageOk = mine.health == 90 && other.health == 90;
                    damage = " damage: master=" + PhotonNetwork.isMasterClient + " own " + mineBefore + "->" + mine.health +
                        " other " + otherBefore + "->" + other.health;
                    Debug.Log("SMOKE:" + damage);

                    // Let the other client finish seeing its result before this one leaves.
                    float settleDamage = Time.realtimeSinceStartup + 5f;
                    while (Time.realtimeSinceStartup < settleDamage)
                    {
                        yield return null;
                    }
                }
            }

            bool cloudRoom = PhotonNetwork.inRoom && !PhotonNetwork.offlineMode;
            string net = !online ? "" : " net: cloudRoom=" + cloudRoom + " region=" + PhotonNetwork.CloudRegion +
                " ping=" + PhotonNetwork.GetPing() + "ms room=" + (PhotonNetwork.room == null ? "none" : PhotonNetwork.room.Name) +
                " players=" + playersSeen + " tanks=" + tanksSeen + damage;
            bool netOk = !online || (cloudRoom && playersSeen >= expectPlayers && tanksSeen >= expectPlayers && damageOk);

            GameObject player = Com.Wulfram3.PlayerMovementManager.LocalPlayerInstance;
            bool playerSpawned = player != null;
            int cargo = FindObjectsOfType<Com.Wulfram3.Cargo>().Length;

            // Screenshots only: turn the tank a full circle, a frame every 15 degrees.
            if (shotsDir != null && player != null)
            {
                Rigidbody body = player.GetComponent<Rigidbody>();
                Quaternion heading = player.transform.rotation;
                for (int i = 0; i < 24; i++)
                {
                    Quaternion turned = Quaternion.Euler(0f, 15f * (i + 1), 0f) * heading;
                    if (body != null)
                    {
                        body.angularVelocity = Vector3.zero;
                        body.MoveRotation(turned);
                    }
                    else
                    {
                        player.transform.rotation = turned;
                    }
                    for (int f = 0; f < 4; f++)
                    {
                        yield return null;
                    }
                    yield return StartCoroutine(Shot("pan-" + i.ToString("00")));
                }
            }

            // Include inactive objects: the panel deactivates its own GameObject whenever its target
            // is off screen (e.g. a Tab pick), and FindObjectOfType skips inactive objects.
            Com.Wulfram3.TargetInfoController info = null;
            Com.Wulfram3.TargetInfoController[] infos = Resources.FindObjectsOfTypeAll<Com.Wulfram3.TargetInfoController>();
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i].gameObject.scene.IsValid())
                {
                    info = infos[i];
                }
            }

            // 2a. Tab targeting through the real TargetController.
            int tabPicks = 0;
            int tabInvalid = 0;
            bool targetShot = false;
            Com.Wulfram3.TargetController targeting = player == null ? null : player.GetComponent<Com.Wulfram3.TargetController>();
            if (targeting != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    GameObject picked = targeting.CycleTarget();
                    if (picked != null)
                    {
                        tabPicks++;
                        if (picked.transform.IsChildOf(player.transform) || picked.GetComponent<Com.Wulfram3.HitPointsManager>() == null)
                        {
                            tabInvalid++;
                            Debug.Log("SMOKE: invalid Tab target " + picked.name);
                        }
                    }
                    for (int f = 0; f < 5; f++)
                    {
                        yield return null;
                    }
                    if (!targetShot && shotsDir != null && info != null && info.targetInfoPanel != null && info.targetInfoPanel.activeSelf)
                    {
                        targetShot = true;
                        yield return StartCoroutine(Shot("3-target-info"));
                    }
                }
            }

            // 2b. A selected Unit with no HitPointsManager (T on a power cell or shell).
            bool probePanelShown = false;
            int probeVisibleFrames = 0;
            Com.Wulfram3.GameManager gameManager = FindObjectOfType<Com.Wulfram3.GameManager>();
            Debug.Log("SMOKE: probe setup - Camera.main=" + (Camera.main == null ? "none" : Camera.main.name) +
                " cameras=" + Camera.allCamerasCount + " targetInfoController=" + (info != null));
            if (gameManager != null && Camera.main != null)
            {
                GameObject probe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                probe.name = "SmokeProbe";
                Destroy(probe.GetComponent<Collider>());
                Com.Wulfram3.Unit unit = probe.AddComponent<Com.Wulfram3.Unit>();
                unit.team = "";
                unit.name = "Smoke Probe";
                MeshRenderer probeRenderer = probe.GetComponent<MeshRenderer>();

                // Render it first: TargetInfoController's panel is its own GameObject, and if the
                // target isn't visible on the first LateUpdate after selection it deactivates
                // itself (and with it the controller) until the next target change. A real
                // T-press targets something already on screen.
                for (int f = 0; f < 5; f++)
                {
                    if (Camera.main != null)
                    {
                        probe.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 8f;
                    }
                    yield return null;
                }

                gameManager.SetCurrentTarget(probe);
                for (int f = 0; f < 30; f++)
                {
                    if (Camera.main != null)
                    {
                        probe.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 8f;
                    }
                    yield return null;
                    if (probeRenderer.isVisible)
                    {
                        probeVisibleFrames++;
                    }
                    // TargetChanged() shows the panel immediately; after a couple of frames it
                    // is only still shown if LateUpdate's visible branch ran.
                    if (f >= 2 && info != null && info.targetInfoPanel != null && info.targetInfoPanel.activeSelf)
                    {
                        probePanelShown = true;
                    }
                    if (f == 10 && probePanelShown)
                    {
                        yield return StartCoroutine(Shot("4-probe-panel"));
                    }
                }

                gameManager.SetCurrentTarget(null);
                Destroy(probe);
                yield return null;
            }

            // 3. Leave the room: GameManager.OnLeftRoom loads build index 0, the launcher.
            bool returnedToLauncher = false;
            if (gameManager != null)
            {
                gameManager.LeaveRoom();
                float back = Time.realtimeSinceStartup + seconds;
                while (SceneManager.GetActiveScene().buildIndex != 0 && Time.realtimeSinceStartup < back)
                {
                    yield return null;
                }
                returnedToLauncher = SceneManager.GetActiveScene().buildIndex == 0;

                // Let the launcher run its Start again.
                float launcherSettle = Time.realtimeSinceStartup + 3f;
                while (Time.realtimeSinceStartup < launcherSettle)
                {
                    yield return null;
                }
                if (returnedToLauncher)
                {
                    yield return StartCoroutine(Shot("5-back-at-launcher"));
                }
            }

            Debug.Log("SMOKE: scene=" + scene + " playerSpawned=" + playerSpawned + " cargo=" + cargo +
                " missingScripts=" + missing + " tabPicks=" + tabPicks + " tabInvalid=" + tabInvalid +
                " probeVisibleFrames=" + probeVisibleFrames + " probePanelShown=" + probePanelShown +
                " returnedToLauncher=" + returnedToLauncher + " (" + SceneManager.GetActiveScene().name + ")" +
                " exceptions=" + exceptions + " errors=" + errors + net);
            for (int i = 0; i < firstProblems.Count; i++)
            {
                Debug.Log("SMOKE: problem " + (i + 1) + " - " + firstProblems[i]);
            }

            bool pass = scene == "Playground" && playerSpawned && missing == 0 && tabPicks > 0 &&
                tabInvalid == 0 && returnedToLauncher && exceptions == 0 && errors == 0 && netOk;
            Debug.Log("SMOKE: " + (pass ? "PASS" : "FAIL"));
            Application.Quit();
        }
    }
}
