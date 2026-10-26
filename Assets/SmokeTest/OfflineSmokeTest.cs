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
        // Test runs happen on someone's desktop: the game locks the cursor to its window
        // (CursorVisibilityManager), which pinned the owner's mouse to the screen centre during gate
        // runs (2026-10-26). Unless -smokeLockCursor, the test releases it every frame; visibility is
        // left to the game, so what the game checks (Cursor.visible) is unchanged.
        private bool lockCursor;
        private int cursorReleases;
        private readonly System.Collections.Generic.List<string> firstProblems = new System.Collections.Generic.List<string>();

        // Online M2: the lowest health seen on each tank while they are being watched (see Update).
        private Com.Wulfram3.HitPointsManager watchMine;
        private Com.Wulfram3.HitPointsManager watchOther;
        private int lowestMine;
        private int lowestOther;
        // Every distinct health value this client saw on the other tank, in order (e.g. "100,90,91"),
        // so an odd minimum can be read back from the log.
        private string otherSequence = "";
        private int otherLast = -1;

        // Online M2 lockstep: each client puts the last phase it finished in a player property.
        private const string PhaseKey = "smokePhase";
        private const float BarrierTimeout = 90f;
        private bool barriersOk = true;
        // Highest phase each player (by actor number) has reported - kept after a player leaves.
        private readonly System.Collections.Generic.Dictionary<int, int> phaseReached = new System.Collections.Generic.Dictionary<int, int>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            bool enabled = false;
            float seconds = 15f;
            string shotsDir = null;
            bool online = false;
            bool lockCursor = false;
            int expectPlayers = 1;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-offlineSmokeTest")
                {
                    enabled = true;
                }
                if (args[i] == "-smokeLockCursor")
                {
                    lockCursor = true;
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
            test.lockCursor = lockCursor;
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

        private void Update()
        {
            if (!lockCursor && Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                cursorReleases++;
            }
            if (watchMine != null)
            {
                lowestMine = Mathf.Min(lowestMine, watchMine.health);
            }
            if (watchOther != null)
            {
                lowestOther = Mathf.Min(lowestOther, watchOther.health);
                if (watchOther.health != otherLast && otherSequence.Length < 200)
                {
                    otherLast = watchOther.health;
                    otherSequence += (otherSequence.Length > 0 ? "," : "") + otherLast;
                }
            }
        }

        // Marks this client as done with <phase>, then waits until every expected player has done it
        // too. A local player's property changes apply immediately here and reach the others through
        // the server, so no one can pass a barrier before this client reaches it. A player counts
        // once it has reported the phase, even if it has left since: PUN can deliver another
        // client's "done" and its leaving in the same frame (seen 2026-10-16), so checking only the
        // players still in the room could wait forever.
        private IEnumerator Barrier(int phase, string what)
        {
            ExitGames.Client.Photon.Hashtable done = new ExitGames.Client.Photon.Hashtable();
            done[PhaseKey] = phase;
            PhotonNetwork.player.SetCustomProperties(done);
            float start = Time.realtimeSinceStartup;
            while (PlayersAtPhase(phase) < expectPlayers && Time.realtimeSinceStartup < start + BarrierTimeout)
            {
                yield return null;
            }
            int reached = PlayersAtPhase(phase);
            if (reached < expectPlayers)
            {
                barriersOk = false;
            }
            Debug.Log("SMOKE: barrier " + phase + " (" + what + "): " + reached + "/" + expectPlayers + " players after " +
                (Time.realtimeSinceStartup - start).ToString("0.0") + "s" + (reached < expectPlayers ? " - TIMED OUT" : ""));
        }

        private int PlayersAtPhase(int phase)
        {
            PhotonPlayer[] players = PhotonNetwork.playerList;
            for (int i = 0; i < players.Length; i++)
            {
                NotePhase(players[i], players[i].CustomProperties);
            }
            int count = 0;
            foreach (int reached in phaseReached.Values)
            {
                if (reached >= phase)
                {
                    count++;
                }
            }
            return count;
        }

        // PUN calls this (by SendMessage) on every MonoBehaviour when a player's properties change,
        // before it processes that player leaving.
        private void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
        {
            NotePhase(playerAndUpdatedProps[0] as PhotonPlayer, playerAndUpdatedProps[1] as ExitGames.Client.Photon.Hashtable);
        }

        private void NotePhase(PhotonPlayer player, ExitGames.Client.Photon.Hashtable props)
        {
            object value;
            if (player == null || props == null || !props.TryGetValue(PhaseKey, out value) || !(value is int))
            {
                return;
            }
            int known;
            if (!phaseReached.TryGetValue(player.ID, out known) || (int)value > known)
            {
                phaseReached[player.ID] = (int)value;
            }
        }

        // Online match (design/gdd/win-condition.md, ADR-0004; architecture review C1), two clients in
        // lockstep, one per team. The loser is the non-master's team, so the master decides a result
        // about a remote player.
        //   deploy   each client deploys a repair pad for its team through the real deploy path
        //            (GameManager.DeployUnit); both clients must see both pads, room-owned
        //   destroy  the non-master asks for lethal damage on its own team's deployed pad: the master
        //            must remove it (it couldn't when the deployer owned it - C1)
        //   result   the master destroys the loser team's last pad and tank: within 10 s the winner
        //            sees VICTORY and the loser DEFEAT
        //   reset    announceSeconds later both clients have a new tank, alive, on the same team; one
        //            starting pad per team; no deployed unit left; the rule armed again
        private bool matchOk = true;
        private string matchSummary = "";

        private IEnumerator OnlineMatch()
        {
            GameObject me = Com.Wulfram3.PlayerMovementManager.LocalPlayerInstance;
            Com.Wulfram3.GameManager gameManager = FindObjectOfType<Com.Wulfram3.GameManager>();
            Com.Wulfram3.MatchManager match = Com.Wulfram3.MatchManager.Instance;
            if (me == null || gameManager == null || match == null)
            {
                matchOk = false;
                matchSummary = "setup: tank=" + (me != null) + " gameManager=" + (gameManager != null) + " matchManager=" + (match != null);
                yield break;
            }
            bool master = PhotonNetwork.isMasterClient;
            string myTeam = Com.Wulfram3.Unit.NormalizeTeam(me.GetComponent<Com.Wulfram3.Unit>().team);
            string otherTeam = myTeam == "red" ? "blue" : "red";
            string loserTeam = master ? otherTeam : myTeam;

            float until = Time.realtimeSinceStartup + 10f;
            while (match.state != Com.Wulfram3.MatchManager.State.Playing && Time.realtimeSinceStartup < until)
            {
                yield return null;
            }
            bool armed = match.state == Com.Wulfram3.MatchManager.State.Playing;

            // deploy
            Vector3 spot = me.transform.position + Vector3.ProjectOnPlane(me.transform.forward, Vector3.up).normalized * 8f;
            if (Terrain.activeTerrain != null)
            {
                spot.y = Terrain.activeTerrain.SampleHeight(spot) + Terrain.activeTerrain.transform.position.y;
            }
            gameManager.DeployUnit("RepairPad", spot, Quaternion.identity, myTeam);
            until = Time.realtimeSinceStartup + 15f;
            while (DeployedPads("red") + DeployedPads("blue") < 2 && Time.realtimeSinceStartup < until)
            {
                yield return null;
            }
            bool deployOk = DeployedPads(myTeam) == 1 && DeployedPads(otherTeam) == 1 && AllDeployedRoomOwned();
            yield return StartCoroutine(Barrier(4, "both deployed pads seen"));

            // destroy (C1)
            if (!master)
            {
                foreach (RepairPad pad in FindObjectsOfType<RepairPad>())
                {
                    if (pad.GetComponent<Com.Wulfram3.Unit>().deployed && Com.Wulfram3.Unit.SameTeam(pad.Team, myTeam))
                    {
                        Com.Wulfram3.HitPointsManager hp = pad.GetComponent<Com.Wulfram3.HitPointsManager>();
                        hp.TellServerTakeDamage(hp.maxHealth);
                    }
                }
            }
            until = Time.realtimeSinceStartup + 15f;
            while (DeployedPads(loserTeam) > 0 && Time.realtimeSinceStartup < until)
            {
                yield return null;
            }
            bool destroyOk = DeployedPads(loserTeam) == 0;
            yield return StartCoroutine(Barrier(5, "the non-master's pad is gone"));

            // result
            if (master)
            {
                foreach (RepairPad pad in FindObjectsOfType<RepairPad>())
                {
                    Com.Wulfram3.HitPointsManager hp = pad.GetComponent<Com.Wulfram3.HitPointsManager>();
                    if (hp != null && Com.Wulfram3.Unit.SameTeam(pad.Team, loserTeam))
                    {
                        hp.TellServerTakeDamage(hp.maxHealth);
                    }
                }
                foreach (Com.Wulfram3.PlayerMovementManager tank in FindObjectsOfType<Com.Wulfram3.PlayerMovementManager>())
                {
                    if (Com.Wulfram3.Unit.SameTeam(tank.GetComponent<Com.Wulfram3.Unit>().team, loserTeam))
                    {
                        Com.Wulfram3.HitPointsManager hp = tank.GetComponent<Com.Wulfram3.HitPointsManager>();
                        hp.TellServerTakeDamage(hp.maxHealth);
                    }
                }
            }
            float resultFrom = Time.realtimeSinceStartup;
            while (match.state != Com.Wulfram3.MatchManager.State.Ended && Time.realtimeSinceStartup < resultFrom + 20f)
            {
                yield return null;
            }
            float resultAfter = Time.realtimeSinceStartup - resultFrom;
            string expected = myTeam == loserTeam ? "DEFEAT" : "VICTORY";
            string seen = match.ResultFor(myTeam);
            bool resultOk = match.state == Com.Wulfram3.MatchManager.State.Ended && seen == expected && resultAfter <= 10f;
            yield return StartCoroutine(Barrier(6, "everyone has seen the result"));

            // reset
            float announce = match.announceSeconds;
            until = Time.realtimeSinceStartup + announce + 45f;
            while (Time.realtimeSinceStartup < until && (Com.Wulfram3.MatchManager.Instance == null || Com.Wulfram3.MatchManager.Instance == match ||
                Com.Wulfram3.PlayerMovementManager.LocalPlayerInstance == null || Com.Wulfram3.PlayerMovementManager.LocalPlayerInstance == me))
            {
                yield return null;
            }
            GameObject fresh = Com.Wulfram3.PlayerMovementManager.LocalPlayerInstance;
            Com.Wulfram3.MatchManager next = Com.Wulfram3.MatchManager.Instance;
            // Both new tanks in, alive, and the rule armed again (old remote tanks linger until their
            // owner's destroy arrives).
            until = Time.realtimeSinceStartup + 30f;
            while (Time.realtimeSinceStartup < until && !(fresh != null && next != null && LiveTanks() == expectPlayers &&
                next.state == Com.Wulfram3.MatchManager.State.Playing))
            {
                yield return null;
            }
            Com.Wulfram3.HitPointsManager freshHp = fresh == null ? null : fresh.GetComponent<Com.Wulfram3.HitPointsManager>();
            Com.Wulfram3.Unit freshUnit = fresh == null ? null : fresh.GetComponent<Com.Wulfram3.Unit>();
            bool resetOk = fresh != null && next != null && freshHp != null && freshHp.health == freshHp.maxHealth &&
                freshUnit != null && Com.Wulfram3.Unit.SameTeam(freshUnit.team, myTeam) && LiveTanks() == expectPlayers &&
                RepairPad.Count("red") == 1 && RepairPad.Count("blue") == 1 && DeployedPads("red") + DeployedPads("blue") == 0 &&
                next.state == Com.Wulfram3.MatchManager.State.Playing && next.result == Com.Wulfram3.MatchManager.Result.None;
            yield return StartCoroutine(Barrier(7, "everyone has checked the reset"));

            matchOk = armed && deployOk && destroyOk && resultOk && resetOk;
            matchSummary = "armed=" + armed + " deploy=" + deployOk + " destroyNonMasterPad=" + destroyOk + " result=" + seen +
                " (expected " + expected + ", " + resultAfter.ToString("0.0") + " s) reset=" + resetOk +
                " [new tank " + (fresh == null ? "NONE" : "team '" + (freshUnit == null ? "?" : freshUnit.team) + "' health " +
                (freshHp == null ? "?" : freshHp.health + "/" + freshHp.maxHealth)) + ", live tanks " + LiveTanks() +
                ", pads red " + RepairPad.Count("red") + " blue " + RepairPad.Count("blue") + ", deployed " +
                (DeployedPads("red") + DeployedPads("blue")) + ", state " + (next == null ? "?" : next.state.ToString()) + "]";
            Debug.Log("SMOKE: match " + (matchOk ? "PASS" : "FAIL") + " - " + matchSummary);
        }

        private static int DeployedPads(string team)
        {
            int count = 0;
            foreach (RepairPad pad in FindObjectsOfType<RepairPad>())
            {
                if (pad.GetComponent<Com.Wulfram3.Unit>().deployed && Com.Wulfram3.Unit.SameTeam(pad.Team, team))
                {
                    count++;
                }
            }
            return count;
        }

        private static bool AllDeployedRoomOwned()
        {
            foreach (RepairPad pad in FindObjectsOfType<RepairPad>())
            {
                PhotonView view = pad.GetComponent<PhotonView>();
                if (pad.GetComponent<Com.Wulfram3.Unit>().deployed && (view == null || !view.isSceneView))
                {
                    return false;
                }
            }
            return true;
        }

        private static int LiveTanks()
        {
            int count = 0;
            foreach (Com.Wulfram3.PlayerMovementManager tank in FindObjectsOfType<Com.Wulfram3.PlayerMovementManager>())
            {
                Com.Wulfram3.HitPointsManager hp = tank.GetComponent<Com.Wulfram3.HitPointsManager>();
                if (hp != null && hp.health > 0)
                {
                    count++;
                }
            }
            return count;
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
            // exist here - the M2 proof that clients see each other. The barriers below keep every
            // client in the room until all of them have finished checking.
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
            }

            // M2: damage both ways, through the game's real network path - the call AutoCannon makes on
            // a hit (HitPointsManager.TellServerTakeDamage -> RPC to the master client, which applies
            // it and broadcasts UpdateHealth to all). Each client hits the other tank for 10 once. Both
            // tanks start at 100 and nothing else damages them, so each client must see BOTH tanks dip
            // to 90 or lower (91: see below). The LOWEST health seen counts: tanks regenerate, so "exactly 90 right
            // now" failed when one client was slower (seen 2026-10-10). Only aiming/raycasting is
            // skipped.
            //
            // The clients run in lockstep (Barrier): each starts watching before it says it is ready,
            // nobody shoots until everyone is watching, and nobody leaves until everyone has checked.
            // Without that, a client that entered later could be hit and heal before it started
            // watching, or find the other client already gone (seen 2026-10-16, gate run js-neg).
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
                    // Watch both tanks every frame from here on (Update), including while waiting at
                    // a barrier, then tell the others this client is watching.
                    watchMine = mine;
                    watchOther = other;
                    lowestMine = mine.health;
                    lowestOther = other.health;
                    yield return StartCoroutine(Barrier(1, "everyone is watching"));

                    // A tank counts as hit once it has been seen at 91 or lower, not only 90: the master
                    // can apply the hit and its own regeneration tick (+1, every 2 s at 0.5/s) moments
                    // apart, and when both UpdateHealth messages reach another client in the same
                    // frame, that client only ever sees 91 (seen 2026-10-20, gate run cg-gate: it waited
                    // 30 s for a 90 it would never see). Nothing but damage lowers health and two regen
                    // ticks can't share a frame, so 91 still proves the 10-point hit landed.
                    const int hitDamage = 10;
                    int hitSeen = Mathf.Min(mine.maxHealth, other.maxHealth) - hitDamage + 1;
                    other.TellServerTakeDamage(hitDamage);
                    float waitDamage = Time.realtimeSinceStartup + 30f;
                    while ((lowestMine > hitSeen || lowestOther > hitSeen) && Time.realtimeSinceStartup < waitDamage)
                    {
                        yield return null;
                    }
                    damageOk = lowestMine <= hitSeen && lowestOther <= hitSeen;
                    damage = " damage: master=" + PhotonNetwork.isMasterClient + " own lowest " + lowestMine + " now " + mine.health +
                        ", other lowest " + lowestOther + " now " + other.health + " (seen: " + otherSequence + ")";
                    Debug.Log("SMOKE:" + damage);
                    watchMine = null;
                    watchOther = null;
                    yield return StartCoroutine(Barrier(2, "everyone has checked damage"));

                    // Regeneration: each player's OWN tank must heal (HealthRegenerator) while every
                    // expected player is still in the room - healing only after the other player left
                    // (when this client may have become master) does not count. Wait long enough for
                    // a couple of points at the tank's healthPerSecond.
                    Com.Wulfram3.HealthRegenerator regen = me.GetComponent<Com.Wulfram3.HealthRegenerator>();
                    float perSecond = regen == null ? 0f : regen.healthPerSecond;
                    int healedFrom = mine.health;
                    bool healedWithAllPresent = false;
                    float waitRegen = Time.realtimeSinceStartup + (perSecond > 0f ? Mathf.Min(2.5f / perSecond, 60f) : 0f);
                    while (!healedWithAllPresent && Time.realtimeSinceStartup < waitRegen)
                    {
                        yield return null;
                        if (mine.health > healedFrom && PhotonNetwork.playerList.Length >= expectPlayers)
                        {
                            healedWithAllPresent = true;
                        }
                    }
                    damageOk = damageOk && healedWithAllPresent;
                    damage += " regen: perSecond=" + perSecond + " own " + healedFrom + "->" + mine.health +
                        " healedWithAllPresent=" + healedWithAllPresent;
                    Debug.Log("SMOKE: regen perSecond=" + perSecond + " own " + healedFrom + "->" + mine.health +
                        " healedWithAllPresent=" + healedWithAllPresent + " master=" + PhotonNetwork.isMasterClient);

                    yield return StartCoroutine(Barrier(3, "everyone has checked regeneration"));
                    if (expectPlayers == 2)
                    {
                        yield return StartCoroutine(OnlineMatch());
                        damageOk = damageOk && matchOk;
                        damage += " match: " + matchSummary;
                    }
                    damageOk = damageOk && barriersOk;
                    damage += " barriersOk=" + barriersOk;
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
                " exceptions=" + exceptions + " errors=" + errors + " cursorReleased=" + (!lockCursor) + "(" + cursorReleases + "x)" + net);
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
