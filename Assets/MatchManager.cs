using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Com.Wulfram3
{
    /// <summary>
    /// The win condition (design/gdd/win-condition.md): a team is eliminated when it has no repair
    /// pads and no living tanks, and the other team wins (both at once: a draw). Every client counts
    /// pads and tanks for its HUD; only the master decides the result and tells everyone (MatchEnded),
    /// then resets the arena for a new match announceSeconds later (ResetArena). GameManager.Start
    /// adds this component; it lives and dies with the arena scene.
    /// </summary>
    public class MatchManager : Photon.PunBehaviour
    {
        public enum State { Waiting, Playing, Ended, Resetting }

        // Sent as an int in MatchEnded.
        public enum Result { None = 0, RedWins = 1, BlueWins = 2, Draw = 3 }

        public static MatchManager Instance;

        // The offline gameplay test has no second team; this arms the rule without one.
        public static bool ArmWithoutOpponents;

        public float announceSeconds = 15f;
        public float checkIntervalSeconds = 0.5f;
        public int minPlayersPerTeam = 1;

        public State state = State.Waiting;
        public Result result = Result.None;

        public int redPads;
        public int bluePads;
        public int redTanks;
        public int blueTanks;
        public int redTanksAlive;
        public int blueTanksAlive;

        private float nextCheck;
        private float endedAt;

        public static bool DamageBlocked
        {
            get { return Instance != null && (Instance.state == State.Ended || Instance.state == State.Resetting); }
        }

        public float SecondsToReset
        {
            get { return state == State.Ended ? Mathf.Max(0f, endedAt + announceSeconds - Time.time) : 0f; }
        }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (state == State.Ended)
            {
                // The master resets; a client that becomes master meanwhile does it on its own clock.
                if (PhotonNetwork.isMasterClient && Time.time >= endedAt + announceSeconds)
                {
                    // Destroy first, then the RPC: messages arrive in order, so every client removes
                    // the master's objects while it still has them. Destroy events that reach a client
                    // after its reload find nothing, and PUN logs "Ev Destroy Failed" as an error.
                    DestroyOwnMatchObjects();
                    photonView.RPC("ResetArena", PhotonTargets.All);
                }
                return;
            }
            if (state == State.Resetting || Time.time < nextCheck)
            {
                return;
            }
            nextCheck = Time.time + checkIntervalSeconds;
            Count();
            bool armed = ArmWithoutOpponents || (redTanks >= minPlayersPerTeam && blueTanks >= minPlayersPerTeam);
            state = armed ? State.Playing : State.Waiting;
            if (!armed || !PhotonNetwork.isMasterClient)
            {
                return;
            }
            bool redOut = redPads == 0 && redTanksAlive == 0;
            bool blueOut = bluePads == 0 && blueTanksAlive == 0;
            if (redOut || blueOut)
            {
                Result r = redOut && blueOut ? Result.Draw : (redOut ? Result.BlueWins : Result.RedWins);
                photonView.RPC("MatchEnded", PhotonTargets.All, (int)r);
            }
        }

        private void Count()
        {
            redPads = RepairPad.Count("red");
            bluePads = RepairPad.Count("blue");
            redTanks = blueTanks = redTanksAlive = blueTanksAlive = 0;
            // Alive = health above 0: what PlayerMovementManager.isDead is made from, read directly
            // because isDead is only refreshed in the tank's own Update.
            foreach (PlayerMovementManager tank in FindObjectsOfType<PlayerMovementManager>())
            {
                Unit unit = tank.GetComponent<Unit>();
                HitPointsManager hp = tank.GetComponent<HitPointsManager>();
                string team = unit == null ? "" : Unit.NormalizeTeam(unit.team);
                int alive = hp != null && hp.health > 0 ? 1 : 0;
                if (team == "red")
                {
                    redTanks++;
                    redTanksAlive += alive;
                }
                else if (team == "blue")
                {
                    blueTanks++;
                    blueTanksAlive += alive;
                }
            }
        }

        [PunRPC]
        public void MatchEnded(int r)
        {
            if (state == State.Ended || state == State.Resetting)
            {
                return;
            }
            state = State.Ended;
            result = (Result)r;
            endedAt = Time.time;
            Debug.Log("MATCH: ended - " + result + " (red pads " + redPads + " tanks alive " + redTanksAlive +
                ", blue pads " + bluePads + " tanks alive " + blueTanksAlive + "); new match in " + announceSeconds + " s");
        }

        // Every client removes the network objects it owns (its tank; the master also the units
        // deployed this match, which belong to the room, cargo it dropped, and the objects of players
        // who left) and reloads the arena, which brings back the starting pads, turrets and cargo.
        // GameManager.Start then builds a new tank on the same team.
        [PunRPC]
        public void ResetArena()
        {
            if (state == State.Resetting)
            {
                return;
            }
            state = State.Resetting;
            Debug.Log("MATCH: resetting the arena");
            DestroyOwnMatchObjects();
            // LoadLevel pauses sending until the load is done; send the destroys first, so other
            // clients drop this client's old tank (it survives their reload) as soon as possible.
            if (!PhotonNetwork.offlineMode)
            {
                PhotonNetwork.SendOutgoingCommands();
            }
            PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
        }

        // Once per reset (the master runs it before sending ResetArena, then again from the RPC).
        private bool destroyedOwn;

        private void DestroyOwnMatchObjects()
        {
            if (destroyedOwn)
            {
                return;
            }
            destroyedOwn = true;
            HashSet<GameObject> owned = new HashSet<GameObject>();
            foreach (PhotonView view in FindObjectsOfType<PhotonView>())
            {
                Unit unit = view.GetComponent<Unit>();
                bool matchObject = !view.isSceneView || (unit != null && unit.deployed);
                if (view.isMine && matchObject &&
                    (view.transform.parent == null || view.transform.parent.GetComponentInParent<PhotonView>() == null))
                {
                    owned.Add(view.gameObject);
                }
            }
            foreach (GameObject go in owned)
            {
                PhotonNetwork.Destroy(go);
            }
        }

        // The local player's side of a result: "VICTORY", "DEFEAT" or "DRAW".
        public string ResultFor(string team)
        {
            if (result == Result.Draw)
            {
                return "DRAW";
            }
            string winner = result == Result.RedWins ? "red" : (result == Result.BlueWins ? "blue" : "");
            return winner == "" ? "" : (Unit.SameTeam(team, winner) ? "VICTORY" : "DEFEAT");
        }

        private static string LocalTeam()
        {
            GameObject me = PlayerMovementManager.LocalPlayerInstance;
            Unit unit = me == null ? null : me.GetComponent<Unit>();
            return unit == null ? "" : Unit.NormalizeTeam(unit.team);
        }

        private void OnGUI()
        {
            string team = LocalTeam();
            GUIStyle small = new GUIStyle(GUI.skin.label);
            small.alignment = TextAnchor.UpperCenter;
            small.fontSize = 14;
            GUI.Label(new Rect(0f, 4f, Screen.width, 24f), "Red: " + redPads + " pads, " + redTanksAlive + " tanks   |   Blue: " +
                bluePads + " pads, " + blueTanksAlive + " tanks", small);

            GUIStyle big = new GUIStyle(GUI.skin.label);
            big.alignment = TextAnchor.MiddleCenter;
            big.fontStyle = FontStyle.Bold;
            if (state == State.Ended)
            {
                big.fontSize = 64;
                big.normal.textColor = result == Result.RedWins ? Color.red : (result == Result.BlueWins ? new Color(0.3f, 0.5f, 1f) : Color.white);
                GUI.Label(new Rect(0f, Screen.height * 0.3f, Screen.width, 80f), ResultFor(team), big);
                GUI.Label(new Rect(0f, Screen.height * 0.3f + 80f, Screen.width, 24f), "New match in " + Mathf.CeilToInt(SecondsToReset) + " s", small);
                return;
            }
            if (state != State.Playing)
            {
                return;
            }
            big.fontSize = 22;
            GameManager gameManager = GetComponent<GameManager>();
            string line = "";
            if (gameManager != null && gameManager.WaitingForPad)
            {
                line = "No repair pads - waiting for your team to deploy one";
            }
            else if (team != "" && (team == "red" ? bluePads : redPads) == 0)
            {
                line = "Enemy repair pads destroyed - finish them!";
            }
            else if (team != "" && (team == "red" ? redPads : bluePads) == 0)
            {
                line = "Your repair pads are gone - deploy a new one!";
            }
            if (line != "")
            {
                GUI.Label(new Rect(0f, Screen.height * 0.2f, Screen.width, 40f), line, big);
            }
        }
    }
}
