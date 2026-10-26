using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wulfram.SmokeTest
{
    /// <summary>
    /// L1 gameplay smoke test (docs/L1-LOCAL-PLAYABILITY.md): plays the core loop offline, through the
    /// game's own code, and checks the outcome of each step. Does nothing without -gameplaySmokeTest.
    ///
    ///   Wulfram3.exe -batchmode -offlinePlay -gameplaySmokeTest
    ///
    /// Unity 2017's Input class can't be fed fake keys or mouse buttons, so each phase calls the method
    /// the control handler calls (e.g. AutoCannon.TryFire for a held left mouse button), the way the M2
    /// test calls TellServerTakeDamage. Phases:
    ///   cannon   (row 5)  hold the trigger at a real enemy unit: its health drops, fuel is spent
    ///   fuel     (row 7)  let go: fuel regenerates
    ///   destroy  (row 15) the cannon finishes the unit: the master removes it and spawns an explosion
    ///   pulse    (row 6)  the right mouse button's handler (TryFirePulse): fuelPerPulse spent, reload
    ///                     enforced, the shell hits a real enemy unit (damage), explodes and is removed
    ///   death    (row 10) lethal damage: the tank is dead and its cannon refuses to fire
    ///   respawn  (row 10) the overhead map + cursor after destroyDelayWhenDead; a click on an enemy
    ///                     pad does nothing, a click on an own-team pad (its own OnMouseDown) brings
    ///                     the tank back with full health and fuel
    ///   cargo    (rows 12/13) what Q, Z and ',' + click do: pick up a box, drop it, pick it up
    ///                     again and deploy it as a unit (preview, then the unit, hold emptied); a
    ///                     deployed repair pad takes the deployer's team
    ///   self-destruct (row 11) the V key's handler: the blast damages a nearby enemy and kills the
    ///                     tank; with no own-team pad left the tank stays dead until a teammate's new
    ///                     pad appears, then it is rebuilt there
    ///   win      (design/gdd/win-condition.md) victory, no damage after it, the arena reset, a draw
    /// The target is a real enemy prefab (a flak turret from Resources) spawned in front of the tank,
    /// with its turret AI switched off so nothing else changes health or fuel during the checks.
    /// Each phase prints "GAMEPLAY: <phase> PASS|FAIL - details"; the last line is "GAMEPLAY: PASS" or
    /// "GAMEPLAY: FAIL" (also FAIL on any exception or error in the log).
    /// </summary>
    public class GameplaySmokeTest : MonoBehaviour
    {
        private int exceptions;
        private int errors;
        private bool allOk = true;
        private readonly List<string> firstProblems = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-gameplaySmokeTest") < 0)
            {
                return;
            }
            GameObject go = new GameObject("GameplaySmokeTest");
            DontDestroyOnLoad(go);
            go.AddComponent<GameplaySmokeTest>();
        }

        private void Awake()
        {
            Application.logMessageReceived += OnLog;
        }

        // Never hold the tester's mouse (see OfflineSmokeTest.lockCursor): the game locks the cursor
        // to its window; a test run releases it every frame. Cursor.visible, which the game's checks
        // use, is left alone. The gate runs this test headless, where there is no mouse to hold.
        private void Update()
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
            }
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

        private void Check(string phase, bool ok, string details)
        {
            allOk = allOk && ok;
            Debug.Log("GAMEPLAY: " + phase + " " + (ok ? "PASS" : "FAIL") + " - " + details);
        }

        private static IEnumerator Wait(float seconds)
        {
            float until = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < until)
            {
                yield return null;
            }
        }

        private IEnumerator Start()
        {
            Debug.Log("GAMEPLAY: starting in scene '" + SceneManager.GetActiveScene().name + "'");
            if (!Wulfram.OfflinePlay.OfflinePlay.Active)
            {
                Check("setup", false, "needs -offlinePlay (the test plays offline, through the launcher's Play button)");
                Finish();
                yield break;
            }

            // Enter exactly the way a player does: the launcher's Play button handler.
            Com.Wulfram3.LauncherWithLogin launcher = FindObjectOfType<Com.Wulfram3.LauncherWithLogin>();
            if (launcher == null)
            {
                Check("setup", false, "no launcher in scene '" + SceneManager.GetActiveScene().name + "'");
                Finish();
                yield break;
            }
            launcher.Login();
            float deadline = Time.realtimeSinceStartup + 30f;
            while ((SceneManager.GetActiveScene().name != "Playground" || Com.Wulfram3.PlayerMovementManager.LocalPlayerInstance == null) &&
                Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
            GameObject me = Com.Wulfram3.PlayerMovementManager.LocalPlayerInstance;
            if (me == null)
            {
                Check("setup", false, "no player tank in Playground after 30s (scene '" + SceneManager.GetActiveScene().name + "')");
                Finish();
                yield break;
            }
            // Let every Start() run, then wait until the tank has come down from its spawn point to
            // its hover height (third run: it was still sinking from 7.7 to about 3 while the cannon
            // phase ran). Settled = height changed less than 5 cm over the last second.
            yield return StartCoroutine(Wait(1f));
            float settleStart = Time.realtimeSinceStartup;
            float lastY = me.transform.position.y;
            float settleDeadline = settleStart + 20f;
            bool settled = false;
            while (!settled && Time.realtimeSinceStartup < settleDeadline)
            {
                yield return StartCoroutine(Wait(1f));
                float y = me.transform.position.y;
                settled = Mathf.Abs(y - lastY) < 0.05f;
                lastY = y;
            }
            Debug.Log("GAMEPLAY: tank " + (settled ? "settled" : "STILL MOVING") + " at height " + lastY.ToString("0.00") +
                " after " + (Time.realtimeSinceStartup - settleStart).ToString("0.0") + " s");

            // Turn to open ground the way a player does with the mouse: PlayerMovementManager keeps the
            // mouse-look yaw in rotationX and applies it every Update. The settled tank faces a hill
            // (fourth run: terrain 2 m in front of the barrel), so turn in 30 degree steps until the
            // line of fire is clear for 12 m.
            bool clear = false;
            System.Reflection.FieldInfo yaw = typeof(Com.Wulfram3.PlayerMovementManager).GetField("rotationX",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Com.Wulfram3.PlayerMovementManager movement = me.GetComponent<Com.Wulfram3.PlayerMovementManager>();
            float heading = 0f;
            if (yaw == null || movement == null || yaw.FieldType != typeof(float))
            {
                Check("aim", false, "can't turn the tank: PlayerMovementManager.rotationX (mouse-look yaw) not found");
            }
            else
            {
                for (int step = 0; step < 12 && !clear; step++)
                {
                    heading = step * 30f;
                    yaw.SetValue(movement, heading);
                    yield return null;
                    yield return null;
                    Vector3 from = me.transform.position + (me.transform.forward * 1.0f + me.transform.up * 0.2f);
                    clear = !Physics.Raycast(from, me.transform.forward, 12f);
                }
                Check("aim", clear, clear ? "turned to mouse-look yaw " + heading + " deg, line of fire clear for 12 m" :
                    "no heading with a clear 12 m line of fire");
            }
            if (!clear)
            {
                Finish();
                yield break;
            }

            Com.Wulfram3.Unit myUnit = me.GetComponent<Com.Wulfram3.Unit>();
            Debug.Log("GAMEPLAY: player tank '" + me.name + "' team=" + (myUnit == null ? "?" : myUnit.team) +
                " at " + me.transform.position + " cursorVisible=" + Cursor.visible + " lockState=" + Cursor.lockState);
            yield return StartCoroutine(CannonFuelDestroy(me, myUnit));
            yield return StartCoroutine(Pulse(me, myUnit));
            yield return StartCoroutine(CargoPickupDropDeploy(me));
            TeamAudit();
            yield return StartCoroutine(TurretAttacks(me, myUnit));
            yield return StartCoroutine(DeathAndRespawn(me));
            yield return StartCoroutine(SelfDestructAndRespawn(me, myUnit));
            yield return StartCoroutine(WinCondition());
            Finish();
        }

        // pulse (row 6): the right mouse button's handler (PlayerMovementManager.TryFirePulse) fires a
        // slow, heavy shell at a real enemy unit on the line of fire. It must spend exactly
        // fuelPerPulse, refuse an immediate second shot (reload), spawn a shell that hits, damages the
        // target by at least the shell's directHitpointsDamage (the impact explosion's splash can add
        // more), leaves an explosion and is removed.
        private IEnumerator Pulse(GameObject me, Com.Wulfram3.Unit myUnit)
        {
            Com.Wulfram3.PlayerMovementManager movement = me.GetComponent<Com.Wulfram3.PlayerMovementManager>();
            Com.Wulfram3.FuelManager fuel = me.GetComponent<Com.Wulfram3.FuelManager>();
            Com.Wulfram3.HitPointsManager myHp = me.GetComponent<Com.Wulfram3.HitPointsManager>();
            Com.Wulfram3.GameManager gameManager = FindObjectOfType<Com.Wulfram3.GameManager>();
            GameObject target = SpawnEnemyOnLine(me, myUnit, 10f);
            Com.Wulfram3.HitPointsManager targetHp = target.GetComponent<Com.Wulfram3.HitPointsManager>();
            yield return StartCoroutine(Wait(1f));
            if (targetHp == null || movement == null || fuel == null)
            {
                Check("pulse", false, "missing " + (targetHp == null ? "target HitPointsManager " : "") +
                    (movement == null ? "PlayerMovementManager " : "") + (fuel == null ? "FuelManager" : ""));
                yield break;
            }
            string explosionName = gameManager == null || gameManager.explosionPrefab == null ? "?" : gameManager.explosionPrefab.name;
            HashSet<int> oldExplosions = FindExplosions(explosionName);
            int healthBefore = targetHp.health;
            int myHealthBefore = myHp == null ? -1 : myHp.health;
            int fuelBefore = fuel.fuel;
            bool fired = movement.TryFirePulse();
            int fuelAfterShot = fuel.fuel;
            bool secondShot = movement.TryFirePulse();

            int shellDamage = -1;
            HashSet<int> shellsSeen = new HashSet<int>();
            List<Com.Wulfram3.PulseShellManager> shells = new List<Com.Wulfram3.PulseShellManager>();
            int newExplosions = 0;
            float until = Time.realtimeSinceStartup + 4f;
            while (Time.realtimeSinceStartup < until)
            {
                Com.Wulfram3.PulseShellManager[] live = FindObjectsOfType<Com.Wulfram3.PulseShellManager>();
                for (int i = 0; i < live.Length; i++)
                {
                    if (shellsSeen.Add(live[i].GetInstanceID()))
                    {
                        shells.Add(live[i]);
                        shellDamage = live[i].directHitpointsDamage;
                    }
                }
                foreach (int id in FindExplosions(explosionName))
                {
                    if (oldExplosions.Add(id))
                    {
                        newExplosions++;
                    }
                }
                bool shellsGone = true;
                for (int i = 0; i < shells.Count; i++)
                {
                    shellsGone = shellsGone && shells[i] == null;
                }
                if (shells.Count > 0 && shellsGone && newExplosions > 0 && targetHp.health < healthBefore)
                {
                    break;
                }
                yield return null;
            }
            bool allGone = true;
            for (int i = 0; i < shells.Count; i++)
            {
                allGone = allGone && shells[i] == null;
            }
            int lost = healthBefore - targetHp.health;
            Check("pulse", fired && !secondShot && fuelBefore - fuelAfterShot == movement.fuelPerPulse && shells.Count == 1 &&
                allGone && newExplosions > 0 && shellDamage > 0 && lost >= shellDamage,
                "fired=" + fired + " fuel " + fuelBefore + " -> " + fuelAfterShot + " (fuelPerPulse " + movement.fuelPerPulse +
                "), immediate second shot refused=" + !secondShot + ", shells seen " + shells.Count + " removed=" + allGone +
                ", target health " + healthBefore + " -> " + targetHp.health + " (shell damage " + shellDamage + "), explosions " +
                newExplosions + "; own tank health " + myHealthBefore + " -> " + (myHp == null ? "?" : myHp.health.ToString()));
            if (target != null)
            {
                PhotonNetwork.Destroy(target);
            }
            yield return StartCoroutine(Wait(0.5f));
        }

        // cargo (rows 12/13): drive onto the nearest cargo box (the tank is put just above it and
        // settles), then what Q, Z and ',' + click do: GameManager.PickUpCargo (the box goes, its
        // content is on board), GameManager.DropCargo (a box with that content appears at the tank,
        // the hold is empty), pick it up again, GroundPlacementController.TogglePlacement shows the
        // placement preview and Release deploys the carried unit (preview gone, one more of that unit,
        // hold empty).
        private IEnumerator CargoPickupDropDeploy(GameObject me)
        {
            Com.Wulfram3.CargoManager hold = me.GetComponent<Com.Wulfram3.CargoManager>();
            Com.Wulfram3.GameManager gameManager = FindObjectOfType<Com.Wulfram3.GameManager>();
            GroundPlacementController placement = me.GetComponentInChildren<GroundPlacementController>(true);
            Com.Wulfram3.Cargo[] boxes = FindObjectsOfType<Com.Wulfram3.Cargo>();
            Dictionary<string, int> contents = new Dictionary<string, int>();
            for (int i = 0; i < boxes.Length; i++)
            {
                string c = boxes[i].content == null ? "(null)" : "'" + boxes[i].content + "'";
                int n;
                contents.TryGetValue(c, out n);
                contents[c] = n + 1;
            }
            string summary = "";
            foreach (KeyValuePair<string, int> pair in contents)
            {
                summary += (summary.Length > 0 ? ", " : "") + pair.Key + " x" + pair.Value;
            }
            Debug.Log("GAMEPLAY: cargo boxes in the arena: " + boxes.Length + " (" + summary + ")");
            // Deploying creates the unit from Resources by the box's content name, so a content that
            // names no prefab there can be carried but never deployed. Reported, not failed: it is
            // scene data, fixed on its own branch (docs/L1-LOCAL-PLAYABILITY.md, row 13).
            string undeployable = "";
            foreach (KeyValuePair<string, int> pair in contents)
            {
                string name = pair.Key.Trim('\'');
                if (pair.Key == "(null)" || Resources.Load<GameObject>(name) == null)
                {
                    undeployable += (undeployable.Length > 0 ? ", " : "") + pair.Key + " x" + pair.Value;
                }
            }
            if (undeployable.Length > 0)
            {
                Debug.Log("GAMEPLAY: NOTE cargo contents with no prefab in Resources (can't be deployed): " + undeployable);
            }
            if (hold == null || gameManager == null || placement == null)
            {
                Check("cargo-pickup", false, "missing " + (hold == null ? "CargoManager " : "") + (gameManager == null ? "GameManager " : "") +
                    (placement == null ? "GroundPlacementController" : ""));
                yield break;
            }
            Com.Wulfram3.Cargo box = null;
            for (int i = 0; i < boxes.Length; i++)
            {
                if (!string.IsNullOrEmpty(boxes[i].content) &&
                    (box == null || Vector3.Distance(boxes[i].transform.position, me.transform.position) < Vector3.Distance(box.transform.position, me.transform.position)))
                {
                    box = boxes[i];
                }
            }
            if (box == null)
            {
                Check("cargo-pickup", false, "no cargo box with content in the arena");
                yield break;
            }

            // Drive onto it.
            Rigidbody body = me.GetComponent<Rigidbody>();
            Vector3 onTop = box.transform.position + Vector3.up * 2f;
            if (body != null)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.position = onTop;
            }
            me.transform.position = onTop;
            yield return StartCoroutine(Wait(2f));

            // Q
            string content = box.content;
            int boxesBefore = FindObjectsOfType<Com.Wulfram3.Cargo>().Length;
            float boxDistance = Vector3.Distance(box.transform.position, me.transform.position);
            gameManager.PickUpCargo(hold);
            float until = Time.realtimeSinceStartup + 3f;
            while (hold.pickedUpCargo == "" && Time.realtimeSinceStartup < until)
            {
                yield return null;
            }
            yield return StartCoroutine(Wait(0.5f));
            int boxesAfter = FindObjectsOfType<Com.Wulfram3.Cargo>().Length;
            Check("cargo-pickup", hold.pickedUpCargo == content && box == null && boxesAfter == boxesBefore - 1,
                "box '" + content + "' " + boxDistance.ToString("0.0") + " m from the tank: on board '" + hold.pickedUpCargo +
                "', box " + (box == null ? "removed" : "STILL THERE") + ", boxes " + boxesBefore + " -> " + boxesAfter);

            // Z
            HashSet<int> oldBoxes = new HashSet<int>();
            Com.Wulfram3.Cargo[] now = FindObjectsOfType<Com.Wulfram3.Cargo>();
            for (int i = 0; i < now.Length; i++)
            {
                oldBoxes.Add(now[i].GetInstanceID());
            }
            gameManager.DropCargo(hold);
            until = Time.realtimeSinceStartup + 3f;
            while (hold.pickedUpCargo != "" && Time.realtimeSinceStartup < until)
            {
                yield return null;
            }
            yield return StartCoroutine(Wait(0.5f));
            Com.Wulfram3.Cargo dropped = null;
            now = FindObjectsOfType<Com.Wulfram3.Cargo>();
            for (int i = 0; i < now.Length; i++)
            {
                if (!oldBoxes.Contains(now[i].GetInstanceID()))
                {
                    dropped = now[i];
                }
            }
            Check("cargo-drop", hold.pickedUpCargo == "" && dropped != null && dropped.content == content &&
                Vector3.Distance(dropped.transform.position, me.transform.position) < 5f,
                "hold now '" + hold.pickedUpCargo + "', new box " + (dropped == null ? "NONE" : "'" + dropped.content + "' " +
                Vector3.Distance(dropped.transform.position, me.transform.position).ToString("0.0") + " m from the tank"));

            // Q again, then ',' and a click.
            gameManager.PickUpCargo(hold);
            until = Time.realtimeSinceStartup + 3f;
            while (hold.pickedUpCargo == "" && Time.realtimeSinceStartup < until)
            {
                yield return null;
            }
            yield return StartCoroutine(Wait(0.5f));
            Debug.Log("GAMEPLAY: GroundPlacementController is on '" + placement.gameObject.name + "'" +
                (placement.gameObject == me ? " (the tank itself)" : " (a child of the tank)") +
                ", own PhotonView there: " + (placement.GetComponent<PhotonView>() != null));
            string unitName = hold.pickedUpCargo + "(Clone)";
            int unitsBefore = CountNamed(unitName);
            placement.TogglePlacement();
            yield return StartCoroutine(Wait(0.5f));
            bool previewShown = placement.currentPlaceableObject != null;
            Vector3 deployAt = placement.placeObject == null ? Vector3.zero : placement.placeObject.transform.position;
            placement.Release();
            yield return StartCoroutine(Wait(1f));
            int unitsAfter = CountNamed(unitName);
            Check("deploy", previewShown && placement.currentPlaceableObject == null && unitsAfter == unitsBefore + 1 && hold.pickedUpCargo == "",
                "carried '" + content + "': preview shown=" + previewShown + " gone after release=" + (placement.currentPlaceableObject == null) +
                ", '" + unitName + "' count " + unitsBefore + " -> " + unitsAfter + " (deployed at " + deployAt + "), hold now '" + hold.pickedUpCargo + "'");

            // deploy-team (design/gdd/win-condition.md): a deployed unit whose prefab has no team takes
            // the deployer's - the RepairPad prefab has none. Carry a pad, deploy it, check its team,
            // then remove it so the later phases see the arena's own pads only.
            Com.Wulfram3.Unit myUnit = me.GetComponent<Com.Wulfram3.Unit>();
            hold.photonView.RPC("SetPickedUpCargo", PhotonTargets.All, "RepairPad");
            // GroundPlacementController.Update reads the hold into baseUnit.
            yield return StartCoroutine(Wait(0.5f));
            int ownPadsBefore = RepairPad.Count(myUnit.team);
            placement.TogglePlacement();
            yield return StartCoroutine(Wait(0.5f));
            int ownPadsWithPreview = RepairPad.Count(myUnit.team);
            placement.Release();
            yield return StartCoroutine(Wait(1f));
            RepairPad deployedPad = null;
            foreach (RepairPad pad in FindObjectsOfType<RepairPad>())
            {
                if (pad.name == "RepairPad(Clone)")
                {
                    deployedPad = pad;
                }
            }
            Check("deploy-team", deployedPad != null && Com.Wulfram3.Unit.SameTeam(deployedPad.Team, myUnit.team) &&
                ownPadsWithPreview == ownPadsBefore && RepairPad.Count(myUnit.team) == ownPadsBefore + 1 && hold.pickedUpCargo == "",
                "deployed 'RepairPad': " + (deployedPad == null ? "NONE" : "team '" + deployedPad.Team + "'") + " (tank team '" + myUnit.team +
                "'), own-team pads " + ownPadsBefore + " -> " + ownPadsWithPreview + " with the preview (must not count) -> " +
                RepairPad.Count(myUnit.team) + ", hold now '" + hold.pickedUpCargo + "'");
            if (deployedPad != null)
            {
                PhotonNetwork.Destroy(deployedPad.gameObject);
                yield return StartCoroutine(Wait(0.5f));
            }
        }

        // teams (row 14): every unit prefab named Red* must be on team "red" and every Blue* on "blue";
        // a turret's teamDetect (who it shoots) must be the other team. At runtime only a prefab with
        // no team gets one (the deployer's, Unit.Awake), so for these the prefab is the truth. Other
        // unit prefabs are listed for the record.
        private void TeamAudit()
        {
            string[] prefabs = { "RedTank", "PlayerTank", "RedScout", "BlueScout", "RedFlakTurret", "BlueFlakTurret",
                "BlueGunTurret", "RedPowerCell", "RepairPad" };
            string wrong = "";
            string listed = "";
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject prefab = Resources.Load<GameObject>(prefabs[i]);
                Com.Wulfram3.Unit unit = prefab == null ? null : prefab.GetComponent<Com.Wulfram3.Unit>();
                Com.Wulfram3.FlakTurretManager flak = prefab == null ? null : prefab.GetComponent<Com.Wulfram3.FlakTurretManager>();
                Com.Wulfram3.GTManager gun = prefab == null ? null : prefab.GetComponent<Com.Wulfram3.GTManager>();
                string detect = flak != null ? flak.teamDetect : (gun != null ? gun.teamDetect : null);
                string team = unit == null ? null : unit.team;
                listed += (listed.Length > 0 ? ", " : "") + prefabs[i] + "(team=" + (team == null ? "-" : "'" + team + "'") +
                    (detect == null ? "" : " shoots='" + detect + "'") + ")";
                string expected = prefabs[i].StartsWith("Red") ? "red" : (prefabs[i].StartsWith("Blue") ? "blue" : null);
                if (prefab == null || unit == null)
                {
                    wrong += (wrong.Length > 0 ? "; " : "") + prefabs[i] + ": " + (prefab == null ? "no prefab in Resources" : "no Unit");
                    continue;
                }
                if (expected != null && team != expected)
                {
                    wrong += (wrong.Length > 0 ? "; " : "") + prefabs[i] + " team '" + team + "' (expected '" + expected + "')";
                }
                if (expected != null && detect != null && detect != (expected == "red" ? "blue" : "red"))
                {
                    wrong += (wrong.Length > 0 ? "; " : "") + prefabs[i] + " shoots '" + detect + "' (expected '" + (expected == "red" ? "blue" : "red") + "')";
                }
            }
            Debug.Log("GAMEPLAY: unit prefabs: " + listed);
            Check("teams", wrong.Length == 0, wrong.Length == 0 ? "every Red*/Blue* unit prefab is on its team (canonical 'red'/'blue') and turrets shoot the other team" : wrong);

            // The arena's own units (placed in Playground, possibly with their own values), grouped.
            // Reported, not failed: scene data is fixed on its own branch; the game compares teams
            // through Unit.SameTeam, so spelling variants ('BlueTeam') already work.
            Dictionary<string, int> sceneUnits = new Dictionary<string, int>();
            int hostileToOwnTeam = 0;
            Com.Wulfram3.Unit[] units = FindObjectsOfType<Com.Wulfram3.Unit>();
            for (int i = 0; i < units.Length; i++)
            {
                if (units[i].GetComponentInParent<Com.Wulfram3.PlayerMovementManager>() != null)
                {
                    continue;
                }
                Com.Wulfram3.FlakTurretManager flak = units[i].GetComponent<Com.Wulfram3.FlakTurretManager>();
                Com.Wulfram3.GTManager gun = units[i].GetComponent<Com.Wulfram3.GTManager>();
                string detect = flak != null ? flak.teamDetect : (gun != null ? gun.teamDetect : null);
                if (detect != null && Com.Wulfram3.Unit.SameTeam(detect, units[i].team) && Com.Wulfram3.Unit.NormalizeTeam(detect) != "")
                {
                    hostileToOwnTeam++;
                }
                string key = units[i].gameObject.name + "(team='" + units[i].team + "'" + (detect == null ? "" : " shoots='" + detect + "'") + ")";
                int n;
                sceneUnits.TryGetValue(key, out n);
                sceneUnits[key] = n + 1;
            }
            string sceneList = "";
            foreach (KeyValuePair<string, int> pair in sceneUnits)
            {
                sceneList += (sceneList.Length > 0 ? ", " : "") + pair.Key + (pair.Value > 1 ? " x" + pair.Value : "");
            }
            Debug.Log("GAMEPLAY: arena units: " + sceneList);
            if (hostileToOwnTeam > 0)
            {
                Debug.Log("GAMEPLAY: NOTE " + hostileToOwnTeam + " arena turret(s) shoot their own team");
            }
        }

        // turret (row 14): a live enemy flak turret (AI on) 25 m away on open ground must find the
        // tank, turn to it, fire flak shells and damage it. Runs on the master (offline: this client).
        private IEnumerator TurretAttacks(GameObject me, Com.Wulfram3.Unit myUnit)
        {
            Com.Wulfram3.HitPointsManager myHp = me.GetComponent<Com.Wulfram3.HitPointsManager>();
            Com.Wulfram3.GameManager gameManager = FindObjectOfType<Com.Wulfram3.GameManager>();
            string prefab = myUnit.team.ToLower() == "red" ? "BlueFlakTurret" : "RedFlakTurret";
            GameObject shellPrefab = gameManager == null ? null : gameManager.flakShellPrefab;
            string shellParts = "";
            if (shellPrefab != null)
            {
                Component[] parts = shellPrefab.GetComponents<Component>();
                for (int i = 0; i < parts.Length; i++)
                {
                    shellParts += (shellParts.Length > 0 ? "," : "") + (parts[i] == null ? "<missing>" : parts[i].GetType().Name);
                }
            }
            Vector3 flat = Vector3.ProjectOnPlane(me.transform.forward, Vector3.up).normalized;
            Vector3 spot = me.transform.position + flat * 25f;
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                spot.y = terrain.SampleHeight(spot) + terrain.transform.position.y;
            }
            GameObject turret = PhotonNetwork.Instantiate(prefab, spot, Quaternion.LookRotation(flat), 0);
            Com.Wulfram3.FlakTurretManager ai = turret.GetComponent<Com.Wulfram3.FlakTurretManager>();
            Debug.Log("GAMEPLAY: turret " + prefab + " at " + turret.transform.position + " shoots='" + (ai == null ? "?" : ai.teamDetect) +
                "' reload=" + (ai == null ? 0f : ai.reloadTime) + "s scan=" + (ai == null ? 0f : ai.scanInterval) + "s; flak shell '" +
                (shellPrefab == null ? "?" : shellPrefab.name) + "' = [" + shellParts + "]" +
                (shellPrefab != null && shellPrefab.GetComponent<Com.Wulfram3.PulseShellManager>() != null ?
                    " direct damage " + shellPrefab.GetComponent<Com.Wulfram3.PulseShellManager>().directHitpointsDamage : ""));
            int healthBefore = myHp.health;
            string shellName = shellPrefab == null ? "?" : shellPrefab.name;
            HashSet<int> oldShells = FindExplosions(shellName);
            int shellsFired = 0;
            float bestAim = 180f;
            float until = Time.realtimeSinceStartup + 25f;
            while (Time.realtimeSinceStartup < until && turret != null && myHp.health >= healthBefore)
            {
                yield return null;
                Vector3 toMe = me.transform.position - turret.transform.position;
                bestAim = Mathf.Min(bestAim, Vector3.Angle(turret.transform.forward, toMe));
                foreach (int id in FindExplosions(shellName))
                {
                    if (oldShells.Add(id))
                    {
                        shellsFired++;
                    }
                }
            }
            yield return StartCoroutine(Wait(1f));
            int healthAfter = myHp.health;
            Check("turret", ai != null && shellsFired > 0 && bestAim < 10f && healthAfter < healthBefore,
                "enemy " + prefab + " 25 m away: best aim " + bestAim.ToString("0.0") + " deg off the tank, flak shells fired " + shellsFired +
                ", tank health " + healthBefore + " -> " + healthAfter);
            if (turret != null)
            {
                PhotonNetwork.Destroy(turret);
            }
            // Heal the tank back up for the phases after this one (as a repair pad would).
            myHp.TellServerHealth(myHp.maxHealth);
            yield return StartCoroutine(Wait(0.5f));
        }

        private static int CountNamed(string objectName)
        {
            int count = 0;
            GameObject[] all = FindObjectsOfType<GameObject>();
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == objectName)
                {
                    count++;
                }
            }
            return count;
        }

        // A real enemy flak turret (AI off, held in place) whose collider centre is on the cannon's
        // line of fire, `distance` out (or in front of anything closer). Same placement as the cannon
        // phase.
        private static GameObject SpawnEnemyOnLine(GameObject me, Com.Wulfram3.Unit myUnit, float distance)
        {
            Vector3 origin = me.transform.position + (me.transform.forward * 1.0f + me.transform.up * 0.2f);
            Vector3 aim = me.transform.forward;
            RaycastHit ground;
            if (Physics.Raycast(origin, aim, out ground, distance + 2f))
            {
                distance = Mathf.Max(2.5f, ground.distance - 1.5f);
            }
            string prefab = myUnit.team.ToLower() == "red" ? "BlueFlakTurret" : "RedFlakTurret";
            Vector3 spot = origin + aim * distance;
            GameObject target = PhotonNetwork.Instantiate(prefab, spot, Quaternion.LookRotation(-Vector3.ProjectOnPlane(aim, Vector3.up)), 0);
            Com.Wulfram3.FlakTurretManager ai = target.GetComponent<Com.Wulfram3.FlakTurretManager>();
            if (ai != null)
            {
                ai.enabled = false;
            }
            Rigidbody body = target.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.isKinematic = true;
            }
            Collider[] colliders = target.GetComponentsInChildren<Collider>();
            if (colliders.Length > 0)
            {
                Bounds bounds = colliders[0].bounds;
                for (int i = 1; i < colliders.Length; i++)
                {
                    bounds.Encapsulate(colliders[i].bounds);
                }
                target.transform.position += spot - bounds.center;
            }
            return target;
        }

        // death (row 10): lethal damage through the network damage path (the call self-destruct
        // makes). The tank must count as dead and its cannon must refuse to fire; then the respawn
        // flow must bring it back (see Respawn).
        private IEnumerator DeathAndRespawn(GameObject me)
        {
            Com.Wulfram3.HitPointsManager hp = me.GetComponent<Com.Wulfram3.HitPointsManager>();
            Com.Wulfram3.PlayerMovementManager movement = me.GetComponent<Com.Wulfram3.PlayerMovementManager>();
            Com.Wulfram3.AutoCannon cannon = me.GetComponent<Com.Wulfram3.AutoCannon>();
            Com.Wulfram3.FuelManager fuel = me.GetComponent<Com.Wulfram3.FuelManager>();
            if (hp == null || movement == null)
            {
                Check("death", false, "player tank is missing " + (hp == null ? "HitPointsManager" : "PlayerMovementManager"));
                yield break;
            }
            hp.TellServerTakeDamage(hp.maxHealth);
            float until = Time.realtimeSinceStartup + 3f;
            while (!movement.isDead && Time.realtimeSinceStartup < until)
            {
                yield return null;
            }
            // Dead tanks can't shoot: hold the trigger for half a second, no fuel may be spent.
            int fuelDead = fuel == null ? 0 : fuel.fuel;
            float hold = Time.realtimeSinceStartup + 0.5f;
            while (cannon != null && Time.realtimeSinceStartup < hold)
            {
                cannon.TryFire();
                yield return null;
            }
            int fuelAfter = fuel == null ? 0 : fuel.fuel;
            Check("death", movement.isDead && hp.health == 0 && fuelAfter == fuelDead,
                "after lethal damage: isDead=" + movement.isDead + " health=" + hp.health +
                ", trigger held while dead: fuel " + fuelDead + " -> " + fuelAfter + " (must not change)");
            yield return StartCoroutine(Respawn("respawn", me));
        }

        // Respawn (row 10, design/gdd/win-condition.md): destroyDelayWhenDead after death the game shows
        // the overhead map with the cursor (GameManager.Respawn). With an own-team repair pad the
        // player clicks it - RepairPad's own OnMouseDown handler, sent as a message (Unity 2017 can't
        // fake a mouse click) - and a click on an enemy pad first must do nothing. With no own-team pad
        // the player must stay dead on the map (it used to respawn at the arena centre), the rule must
        // not end the match while it isn't armed (a lone player), and when a teammate deploys a pad -
        // created here the way GroundPlacementController.Release does - the game must rebuild the tank
        // there by itself within 2 s. Either way the tank must end up alive at the pad, with full
        // health and fuel, back in the normal camera with the cursor hidden.
        private IEnumerator Respawn(string phase, GameObject me)
        {
            Com.Wulfram3.HitPointsManager hp = me.GetComponent<Com.Wulfram3.HitPointsManager>();
            Com.Wulfram3.PlayerMovementManager movement = me.GetComponent<Com.Wulfram3.PlayerMovementManager>();
            Com.Wulfram3.FuelManager fuel = me.GetComponent<Com.Wulfram3.FuelManager>();
            Com.Wulfram3.GameManager gameManager = FindObjectOfType<Com.Wulfram3.GameManager>();
            string myTeam = me.GetComponent<Com.Wulfram3.Unit>().team;
            RepairPad own = RepairPad.FindNearestOwn(movement);
            RepairPad enemy = null;
            foreach (RepairPad pad in FindObjectsOfType<RepairPad>())
            {
                if (!pad.BuildsFor(movement) && Com.Wulfram3.Unit.NormalizeTeam(pad.Team) != "")
                {
                    enemy = pad;
                }
            }
            float deadSince = Time.realtimeSinceStartup;
            float until = deadSince + movement.destroyDelayWhenDead + 5f;
            while (!(gameManager.overheadCamera.enabled && !gameManager.normalCamera.enabled) && Time.realtimeSinceStartup < until)
            {
                yield return null;
            }
            bool view = gameManager.overheadCamera.enabled && !gameManager.normalCamera.enabled && Cursor.visible;
            Check(phase + "-view", view, "after " + (Time.realtimeSinceStartup - deadSince).ToString("0.0") + " s dead: overhead camera=" +
                gameManager.overheadCamera.enabled + " normal camera=" + gameManager.normalCamera.enabled +
                " cursorVisible=" + Cursor.visible + ", own-team ('" + myTeam + "') pads: " + RepairPad.Count(myTeam) +
                ", enemy pad: " + (enemy == null ? "none" : "'" + enemy.name + "' team '" + enemy.Team + "'"));
            Vector3 expected;
            if (own != null)
            {
                if (enemy != null)
                {
                    enemy.SendMessage("OnMouseDown");
                    yield return StartCoroutine(Wait(1f));
                    Check(phase + "-enemy-pad", movement.isDead, "clicked the enemy pad '" + enemy.name + "': isDead=" + movement.isDead + " (must stay dead)");
                }
                expected = own.transform.position + new Vector3(0f, 5f, 0f);
                own.SendMessage("OnMouseDown");
            }
            else
            {
                // Past at least one more GameManager.Respawn call.
                float watchUntil = Time.realtimeSinceStartup + movement.destroyDelayWhenDead + 2f;
                bool stayedDead = true;
                while (Time.realtimeSinceStartup < watchUntil)
                {
                    stayedDead = stayedDead && movement.isDead;
                    yield return null;
                }
                Check(phase + "-no-pad", stayedDead && gameManager.WaitingForPad && gameManager.overheadCamera.enabled,
                    "no own-team pad: stayed dead for " + (Time.realtimeSinceStartup - deadSince).ToString("0.0") + " s=" + stayedDead +
                    ", waiting for a pad=" + gameManager.WaitingForPad + ", overhead camera=" + gameManager.overheadCamera.enabled);
                Com.Wulfram3.MatchManager match = Com.Wulfram3.MatchManager.Instance;
                Check(phase + "-lone-player", match != null && match.state == Com.Wulfram3.MatchManager.State.Waiting &&
                    match.result == Com.Wulfram3.MatchManager.Result.None,
                    "one team only, own team with no pads and no living tank: match " + (match == null ? "MISSING" : "state=" + match.state + " result=" + match.result) +
                    " (must be Waiting, no result)");

                Vector3 spot = me.transform.position + Vector3.ProjectOnPlane(me.transform.forward, Vector3.up).normalized * 8f;
                if (Terrain.activeTerrain != null)
                {
                    spot.y = Terrain.activeTerrain.SampleHeight(spot) + Terrain.activeTerrain.transform.position.y;
                }
                GameObject newPad = PhotonNetwork.Instantiate("RepairPad", spot, Quaternion.identity, 0, new object[] { myTeam });
                float padAt = Time.realtimeSinceStartup;
                while (movement.isDead && Time.realtimeSinceStartup < padAt + 5f)
                {
                    yield return null;
                }
                float took = Time.realtimeSinceStartup - padAt;
                Check(phase + "-new-pad", !movement.isDead && took <= 2f,
                    "a teammate's new pad (team '" + newPad.GetComponent<RepairPad>().Team + "'): rebuilt after " + took.ToString("0.0") + " s (at most 2 s)");
                expected = newPad.transform.position + new Vector3(0f, 5f, 0f);
            }
            yield return StartCoroutine(Wait(1f));
            float offset = Vector3.Distance(new Vector3(me.transform.position.x, 0f, me.transform.position.z), new Vector3(expected.x, 0f, expected.z));
            bool alive = !movement.isDead && hp.health == hp.maxHealth && (fuel == null || fuel.fuel == fuel.maxFuel);
            bool views = gameManager.normalCamera.enabled && !gameManager.overheadCamera.enabled && !Cursor.visible;
            Check(phase, alive && views && offset < 5f,
                "isDead=" + movement.isDead + " health=" + hp.health + "/" + hp.maxHealth +
                " fuel=" + (fuel == null ? "?" : fuel.fuel + "/" + fuel.maxFuel) + ", " + offset.ToString("0.0") +
                " m from the respawn point, normal camera=" + gameManager.normalCamera.enabled +
                " overhead=" + gameManager.overheadCamera.enabled + " cursorVisible=" + Cursor.visible);
            // Let the tank come down from the respawn height before anything else happens.
            yield return StartCoroutine(Wait(3f));
        }

        // self-destruct (row 11): the V key's handler (selfDestruct.Trigger) plays the effect; when it
        // ends the blast damages every unit with hit points within sphereRadius (more the closer it
        // is) and kills the tank. An enemy turret 4 m away must be damaged or destroyed, the tank must
        // die, and the respawn flow must bring it back again.
        private IEnumerator SelfDestructAndRespawn(GameObject me, Com.Wulfram3.Unit myUnit)
        {
            selfDestruct destruct = me.GetComponent<selfDestruct>();
            Com.Wulfram3.PlayerMovementManager movement = me.GetComponent<Com.Wulfram3.PlayerMovementManager>();
            if (destruct == null)
            {
                Check("self-destruct", false, "player tank has no selfDestruct component");
                yield break;
            }
            string prefab = myUnit.team.ToLower() == "red" ? "BlueFlakTurret" : "RedFlakTurret";
            Vector3 spot = me.transform.position + Vector3.ProjectOnPlane(me.transform.forward, Vector3.up).normalized * 4f;
            GameObject victim = PhotonNetwork.Instantiate(prefab, spot, Quaternion.identity, 0);
            Com.Wulfram3.FlakTurretManager ai = victim.GetComponent<Com.Wulfram3.FlakTurretManager>();
            if (ai != null)
            {
                ai.enabled = false;
            }
            Rigidbody victimBody = victim.GetComponent<Rigidbody>();
            if (victimBody != null)
            {
                victimBody.isKinematic = true;
            }
            Com.Wulfram3.HitPointsManager victimHp = victim.GetComponent<Com.Wulfram3.HitPointsManager>();
            yield return StartCoroutine(Wait(0.5f));
            int victimBefore = victimHp == null ? -1 : victimHp.health;
            float distance = Vector3.Distance(victim.transform.position, me.transform.position);

            // Friendly units in range too: the blast damages every HitPointsManager in its sphere,
            // whatever its team (the first run lost one of the two repair pads - the tank had just
            // respawned on it). Recorded, not judged: whether that is intended is a design question.
            List<Com.Wulfram3.HitPointsManager> nearby = new List<Com.Wulfram3.HitPointsManager>();
            List<int> nearbyBefore = new List<int>();
            List<string> nearbyNames = new List<string>();
            Com.Wulfram3.HitPointsManager[] all = FindObjectsOfType<Com.Wulfram3.HitPointsManager>();
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].gameObject != me && all[i] != victimHp &&
                    Vector3.Distance(all[i].transform.position, me.transform.position) <= destruct.sphereRadius + 1f)
                {
                    nearby.Add(all[i]);
                    nearbyBefore.Add(all[i].health);
                    Com.Wulfram3.Unit unit = all[i].GetComponent<Com.Wulfram3.Unit>();
                    nearbyNames.Add("'" + all[i].name + "' team=" + (unit == null ? "?" : unit.team) + " " +
                        Vector3.Distance(all[i].transform.position, me.transform.position).ToString("0.0") + " m away");
                }
            }

            destruct.Trigger();
            float armed = Time.realtimeSinceStartup;
            float until = armed + 20f;
            while (!movement.isDead && Time.realtimeSinceStartup < until)
            {
                yield return null;
            }
            float blastAfter = Time.realtimeSinceStartup - armed;
            yield return StartCoroutine(Wait(0.5f));
            bool victimHit = victim == null || (victimHp != null && victimHp.health < victimBefore);
            Check("self-destruct", movement.isDead && victimHit,
                "blast " + blastAfter.ToString("0.0") + " s after arming, tank isDead=" + movement.isDead +
                "; enemy turret " + distance.ToString("0.0") + " m away (radius " + destruct.sphereRadius + "): " +
                (victim == null ? "destroyed" : "health " + victimBefore + " -> " + (victimHp == null ? "?" : victimHp.health.ToString())));
            for (int i = 0; i < nearby.Count; i++)
            {
                Debug.Log("GAMEPLAY: blast also hit " + nearbyNames[i] + ": health " + nearbyBefore[i] + " -> " +
                    (nearby[i] == null ? "destroyed" : nearby[i].health.ToString()));
            }
            if (movement.isDead)
            {
                // The enemy takes out whatever own-team pads the blast left, so the respawn below runs
                // with none (design/gdd/win-condition.md: no pad, no respawn).
                int finished = DestroyPads(myUnit.team);
                Debug.Log("GAMEPLAY: own-team pads left after the blast, destroyed by the test: " + finished);
                yield return StartCoroutine(Wait(0.5f));
                yield return StartCoroutine(Respawn("respawn-after-self-destruct", me));
            }
        }

        // Lethal damage, through the master, to every repair pad of the team; returns how many.
        private static int DestroyPads(string team)
        {
            int count = 0;
            foreach (RepairPad pad in FindObjectsOfType<RepairPad>())
            {
                Com.Wulfram3.HitPointsManager hp = pad.GetComponent<Com.Wulfram3.HitPointsManager>();
                if (hp != null && Com.Wulfram3.Unit.NormalizeTeam(pad.Team) != "" && Com.Wulfram3.Unit.SameTeam(pad.Team, team))
                {
                    hp.TellServerTakeDamage(hp.maxHealth);
                    count++;
                }
            }
            return count;
        }

        // win condition (design/gdd/win-condition.md). Offline there is no second team, so the test
        // arms the rule (MatchManager.ArmWithoutOpponents) and plays the enemy.
        //   victory  the other team's last pad goes (it has no tanks): within 2 s the master announces
        //            this tank's team as the winner (VICTORY here), and damage is ignored until the reset
        //   reset    announceSeconds later the arena is reloaded: a new tank, alive with full health
        //            and fuel, one starting pad per team again, units deployed this match gone, the
        //            rule armed again
        //   draw     every pad of both teams and this tank go at the same moment: DRAW
        private IEnumerator WinCondition()
        {
            Com.Wulfram3.MatchManager match = Com.Wulfram3.MatchManager.Instance;
            GameObject me = Com.Wulfram3.PlayerMovementManager.LocalPlayerInstance;
            if (match == null || me == null)
            {
                Check("match-armed", false, (match == null ? "no MatchManager in the arena (GameManager.Start adds it) " : "") + (me == null ? "no player tank" : ""));
                yield break;
            }
            string myTeam = Com.Wulfram3.Unit.NormalizeTeam(me.GetComponent<Com.Wulfram3.Unit>().team);
            string enemyTeam = myTeam == "red" ? "blue" : "red";
            Com.Wulfram3.MatchManager.Result myWin = myTeam == "red" ? Com.Wulfram3.MatchManager.Result.RedWins : Com.Wulfram3.MatchManager.Result.BlueWins;
            Com.Wulfram3.MatchManager.ArmWithoutOpponents = true;
            match.announceSeconds = 6f;
            float until = Time.realtimeSinceStartup + 3f;
            while (match.state != Com.Wulfram3.MatchManager.State.Playing && Time.realtimeSinceStartup < until)
            {
                yield return null;
            }
            Check("match-armed", match.state == Com.Wulfram3.MatchManager.State.Playing && match.result == Com.Wulfram3.MatchManager.Result.None,
                "state=" + match.state + " result=" + match.result + "; " + MatchCounts(match));

            // victory
            string deployed = "BlueFlakTurret(Clone)";
            int deployedBefore = CountNamed(deployed);
            int enemyPads = DestroyPads(enemyTeam);
            float destroyedAt = Time.realtimeSinceStartup;
            while (match.state != Com.Wulfram3.MatchManager.State.Ended && Time.realtimeSinceStartup < destroyedAt + 5f)
            {
                yield return null;
            }
            float took = Time.realtimeSinceStartup - destroyedAt;
            Check("victory", match.state == Com.Wulfram3.MatchManager.State.Ended && match.result == myWin &&
                match.ResultFor(myTeam) == "VICTORY" && took <= 2f,
                "destroyed the " + enemyTeam + " team's " + enemyPads + " pad(s): state=" + match.state + " result=" + match.result +
                " ('" + match.ResultFor(myTeam) + "' for this " + myTeam + " tank) after " + took.ToString("0.0") + " s (at most 2 s); " + MatchCounts(match));
            Com.Wulfram3.HitPointsManager myHp = me.GetComponent<Com.Wulfram3.HitPointsManager>();
            int healthBefore = myHp.health;
            myHp.TellServerTakeDamage(30);
            yield return StartCoroutine(Wait(0.5f));
            Check("match-over-no-damage", myHp.health == healthBefore, "30 damage after the result: health " + healthBefore + " -> " + myHp.health + " (must not change)");

            // reset
            float endedAt = Time.realtimeSinceStartup;
            float announce = match.announceSeconds;
            until = endedAt + announce + 20f;
            while (Time.realtimeSinceStartup < until && (Com.Wulfram3.PlayerMovementManager.LocalPlayerInstance == null ||
                Com.Wulfram3.PlayerMovementManager.LocalPlayerInstance == me || Com.Wulfram3.MatchManager.Instance == null ||
                Com.Wulfram3.MatchManager.Instance == match))
            {
                yield return null;
            }
            float resetAfter = Time.realtimeSinceStartup - endedAt;
            // Let the new tank's Start() run and the rule re-arm.
            yield return StartCoroutine(Wait(3f));
            GameObject fresh = Com.Wulfram3.PlayerMovementManager.LocalPlayerInstance;
            Com.Wulfram3.MatchManager next = Com.Wulfram3.MatchManager.Instance;
            Com.Wulfram3.HitPointsManager freshHp = fresh == null ? null : fresh.GetComponent<Com.Wulfram3.HitPointsManager>();
            Com.Wulfram3.FuelManager freshFuel = fresh == null ? null : fresh.GetComponent<Com.Wulfram3.FuelManager>();
            Com.Wulfram3.Unit freshUnit = fresh == null ? null : fresh.GetComponent<Com.Wulfram3.Unit>();
            bool tankOk = fresh != null && freshHp != null && freshHp.health == freshHp.maxHealth && (freshFuel == null || freshFuel.fuel == freshFuel.maxFuel) &&
                freshUnit != null && Com.Wulfram3.Unit.SameTeam(freshUnit.team, myTeam);
            bool arenaOk = RepairPad.Count("red") == 1 && RepairPad.Count("blue") == 1 && CountNamed("RepairPad(Clone)") == 0 && CountNamed(deployed) == 0;
            bool matchOk = next != null && next.state == Com.Wulfram3.MatchManager.State.Playing && next.result == Com.Wulfram3.MatchManager.Result.None;
            Check("reset", tankOk && arenaOk && matchOk && resetAfter >= announce - 0.5f,
                "after " + resetAfter.ToString("0.0") + " s (announceSeconds " + announce + "): new tank " + (fresh == null ? "NONE" :
                "team '" + (freshUnit == null ? "?" : freshUnit.team) + "' health " + (freshHp == null ? "?" : freshHp.health + "/" + freshHp.maxHealth) +
                " fuel " + (freshFuel == null ? "?" : freshFuel.fuel + "/" + freshFuel.maxFuel)) + "; pads red " + RepairPad.Count("red") + " blue " + RepairPad.Count("blue") +
                ", deployed pads " + CountNamed("RepairPad(Clone)") + ", '" + deployed + "' " + deployedBefore + " -> " + CountNamed(deployed) +
                "; match " + (next == null ? "MISSING" : "state=" + next.state + " result=" + next.result));
            if (fresh == null || next == null || freshHp == null)
            {
                Com.Wulfram3.MatchManager.ArmWithoutOpponents = false;
                yield break;
            }

            // draw
            int redPads = DestroyPads("red");
            int bluePads = DestroyPads("blue");
            freshHp.TellServerTakeDamage(freshHp.maxHealth);
            float wipedAt = Time.realtimeSinceStartup;
            while (next.state != Com.Wulfram3.MatchManager.State.Ended && Time.realtimeSinceStartup < wipedAt + 5f)
            {
                yield return null;
            }
            Check("draw", next.state == Com.Wulfram3.MatchManager.State.Ended && next.result == Com.Wulfram3.MatchManager.Result.Draw &&
                next.ResultFor(myTeam) == "DRAW",
                "destroyed " + redPads + " red and " + bluePads + " blue pad(s) and this tank together: state=" + next.state + " result=" + next.result +
                " ('" + next.ResultFor(myTeam) + "') after " + (Time.realtimeSinceStartup - wipedAt).ToString("0.0") + " s");
            Com.Wulfram3.MatchManager.ArmWithoutOpponents = false;
        }

        private static string MatchCounts(Com.Wulfram3.MatchManager match)
        {
            return "red pads " + match.redPads + " tanks " + match.redTanks + " (alive " + match.redTanksAlive + "), blue pads " +
                match.bluePads + " tanks " + match.blueTanks + " (alive " + match.blueTanksAlive + ")";
        }

        private IEnumerator CannonFuelDestroy(GameObject me, Com.Wulfram3.Unit myUnit)
        {
            Com.Wulfram3.AutoCannon cannon = me.GetComponent<Com.Wulfram3.AutoCannon>();
            Com.Wulfram3.FuelManager fuel = me.GetComponent<Com.Wulfram3.FuelManager>();
            if (cannon == null || fuel == null || myUnit == null)
            {
                Check("cannon", false, "player tank is missing " + (cannon == null ? "AutoCannon " : "") +
                    (fuel == null ? "FuelManager " : "") + (myUnit == null ? "Unit" : ""));
                yield break;
            }
            Debug.Log("GAMEPLAY: cannon range=" + cannon.range + " bulletsPerSecond=" + cannon.bulletsPerSecond +
                " damage=" + cannon.bulletDamageinHitpoints + " fuelPerBullet=" + cannon.fuelPerBullet +
                " fuel=" + fuel.fuel + "/" + fuel.maxFuel + " fuelRegen=" + fuel.fuelRegenerationPerSecond + "/s");

            // A real enemy unit, centred on the cannon's line of fire. The tank's rotation is driven by
            // mouse look every frame (and it can sit nose-down: on the first run the straight shot hit
            // the terrain 4.5 m ahead), so the target is put where the barrel points - in front of any
            // terrain in the way - instead of the tank being turned towards a target.
            Vector3 origin = me.transform.position + (me.transform.forward * 1.0f + me.transform.up * 0.2f);
            Vector3 aim = me.transform.forward;
            float distance = 10f;
            RaycastHit ground;
            if (Physics.Raycast(origin, aim, out ground, distance + 2f))
            {
                distance = Mathf.Max(2.5f, ground.distance - 1.5f);
            }
            Debug.Log("GAMEPLAY: aim pitch=" + (-Mathf.Asin(Mathf.Clamp(aim.y, -1f, 1f)) * Mathf.Rad2Deg).ToString("0.0") +
                " deg (positive = nose down); first thing on the line: " +
                (ground.collider == null ? "nothing within 12 m" : "'" + ground.transform.name + "' at " + ground.distance.ToString("0.0") + " m") +
                "; target goes " + distance.ToString("0.0") + " m out");

            string prefab = myUnit.team.ToLower() == "red" ? "BlueFlakTurret" : "RedFlakTurret";
            Vector3 spot = origin + aim * distance;
            GameObject target = PhotonNetwork.Instantiate(prefab, spot, Quaternion.LookRotation(-Vector3.ProjectOnPlane(aim, Vector3.up)), 0);
            Com.Wulfram3.FlakTurretManager ai = target.GetComponent<Com.Wulfram3.FlakTurretManager>();
            if (ai != null)
            {
                ai.enabled = false;
            }
            // Hold it where it was put, and move it so the middle of its colliders is on the line.
            Rigidbody targetBody = target.GetComponent<Rigidbody>();
            if (targetBody != null)
            {
                targetBody.isKinematic = true;
            }
            Collider[] targetColliders = target.GetComponentsInChildren<Collider>();
            Vector3 centreOffset = Vector3.zero;
            if (targetColliders.Length > 0)
            {
                Bounds bounds = targetColliders[0].bounds;
                for (int i = 1; i < targetColliders.Length; i++)
                {
                    bounds.Encapsulate(targetColliders[i].bounds);
                }
                centreOffset = bounds.center - target.transform.position;
                target.transform.position = spot - centreOffset;
            }
            Com.Wulfram3.HitPointsManager targetHp = target.GetComponent<Com.Wulfram3.HitPointsManager>();
            Com.Wulfram3.Unit targetUnit = target.GetComponent<Com.Wulfram3.Unit>();
            yield return StartCoroutine(Wait(1f));

            // What the cannon's ray hits now (the same origin and direction TryFire uses, without spread).
            origin = me.transform.position + (me.transform.forward * 1.0f + me.transform.up * 0.2f);
            RaycastHit probe;
            bool rayHits = Physics.Raycast(origin, me.transform.forward, out probe, cannon.range);
            Debug.Log("GAMEPLAY: target " + prefab + " at " + target.transform.position + " team=" +
                (targetUnit == null ? "?" : targetUnit.team) + " health=" + (targetHp == null ? "?" : targetHp.health.ToString()) +
                " turretAiOff=" + (ai != null) + "; straight ray hits " +
                (rayHits ? "'" + probe.transform.name + "' unit=" + (probe.transform.GetComponent<Com.Wulfram3.Unit>() != null) +
                " at " + probe.distance.ToString("0.0") + "m" : "nothing"));
            if (targetHp == null || targetUnit == null)
            {
                Check("cannon", false, "spawned " + prefab + " has no " + (targetHp == null ? "HitPointsManager" : "Unit"));
                yield break;
            }

            // cannon: hold the trigger for 2 s. The hovering tank bobs and pitches (second run: level
            // when the target was placed, the ray in the terrain 4.5 m out a second later), so every
            // frame the target is put back on the barrel's line, in front of any terrain - this checks
            // that a bullet reaching a unit damages it; how the tank moves is measured and reported.
            int healthBefore = targetHp.health;
            int fuelBefore = fuel.fuel;
            int lowestFuel = fuel.fuel;
            float minPitch = 999f, maxPitch = -999f, minY = 99999f, maxY = -99999f;
            float hold = Time.realtimeSinceStartup + 2f;
            while (Time.realtimeSinceStartup < hold)
            {
                KeepOnLine(me, target, centreOffset);
                cannon.TryFire();
                lowestFuel = Mathf.Min(lowestFuel, fuel.fuel);
                float pitch = -Mathf.Asin(Mathf.Clamp(me.transform.forward.y, -1f, 1f)) * Mathf.Rad2Deg;
                minPitch = Mathf.Min(minPitch, pitch);
                maxPitch = Mathf.Max(maxPitch, pitch);
                minY = Mathf.Min(minY, me.transform.position.y);
                maxY = Mathf.Max(maxY, me.transform.position.y);
                yield return null;
            }
            Debug.Log("GAMEPLAY: while firing the tank's pitch ranged " + minPitch.ToString("0.0") + ".." + maxPitch.ToString("0.0") +
                " deg (positive = nose down), height " + minY.ToString("0.00") + ".." + maxY.ToString("0.00"));
            yield return StartCoroutine(Wait(0.5f));
            int healthAfter = targetHp.health;
            Check("cannon", healthAfter < healthBefore && lowestFuel < fuelBefore,
                "target health " + healthBefore + " -> " + healthAfter + ", fuel " + fuelBefore + " -> lowest " + lowestFuel);

            // fuel: let go of the trigger; fuel must come back.
            int fuelReleased = fuel.fuel;
            float regenWait = Time.realtimeSinceStartup + Mathf.Max(3f, 3f / Mathf.Max(fuel.fuelRegenerationPerSecond, 0.01f));
            while (fuel.fuel <= fuelReleased && Time.realtimeSinceStartup < regenWait)
            {
                yield return null;
            }
            Check("fuel", fuel.fuel > fuelReleased, "fuel " + fuelReleased + " -> " + fuel.fuel + " after letting go");

            // destroy: bring the target down to a few points, then the cannon finishes it. The master
            // (this client, offline) removes a non-player unit at 0 health and spawns an explosion.
            // Explosions remove themselves after a few seconds (AutoDestroy), so new ones are spotted
            // frame by frame, by the prefab name GameManager spawns.
            Com.Wulfram3.GameManager gameManager = FindObjectOfType<Com.Wulfram3.GameManager>();
            string explosionName = gameManager == null || gameManager.explosionPrefab == null ? "?" : gameManager.explosionPrefab.name;
            HashSet<int> oldExplosions = FindExplosions(explosionName);
            int newExplosions = 0;
            int left = Mathf.Max(3, cannon.bulletDamageinHitpoints * 3);
            if (targetHp.health > left)
            {
                targetHp.TellServerTakeDamage(targetHp.health - left);
            }
            yield return StartCoroutine(Wait(0.5f));
            int healthBeforeFinish = targetHp.health;
            float finish = Time.realtimeSinceStartup + 10f;
            float afterGone = -1f;
            while (Time.realtimeSinceStartup < finish && (afterGone < 0f || Time.realtimeSinceStartup < afterGone))
            {
                if (target != null)
                {
                    KeepOnLine(me, target, centreOffset);
                    cannon.TryFire();
                }
                else if (afterGone < 0f)
                {
                    afterGone = Time.realtimeSinceStartup + 0.5f;
                }
                yield return null;
                foreach (int id in FindExplosions(explosionName))
                {
                    if (oldExplosions.Add(id))
                    {
                        newExplosions++;
                    }
                }
            }
            Check("destroy", target == null && newExplosions > 0,
                "target at " + healthBeforeFinish + " health " + (target == null ? "was removed" : "STILL THERE (health " + targetHp.health + ")") +
                ", new '" + explosionName + "' explosions: " + newExplosions);
        }

        // Puts the target's collider centre on the cannon's line of fire (TryFire's origin and
        // direction), up to 10 m out but in front of anything else on the line.
        private static void KeepOnLine(GameObject me, GameObject target, Vector3 centreOffset)
        {
            Vector3 origin = me.transform.position + (me.transform.forward * 1.0f + me.transform.up * 0.2f);
            Vector3 aim = me.transform.forward;
            float distance = 10f;
            RaycastHit[] hits = Physics.RaycastAll(origin, aim, distance + 2f);
            for (int i = 0; i < hits.Length; i++)
            {
                if (!hits[i].transform.IsChildOf(target.transform) && !hits[i].transform.IsChildOf(me.transform))
                {
                    distance = Mathf.Min(distance, Mathf.Max(2.5f, hits[i].distance - 1.5f));
                }
            }
            target.transform.position = origin + aim * distance - centreOffset;
        }

        private static HashSet<int> FindExplosions(string prefabName)
        {
            HashSet<int> ids = new HashSet<int>();
            GameObject[] all = FindObjectsOfType<GameObject>();
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name.StartsWith(prefabName))
                {
                    ids.Add(all[i].GetInstanceID());
                }
            }
            return ids;
        }

        private void Finish()
        {
            Debug.Log("GAMEPLAY: exceptions=" + exceptions + " errors=" + errors);
            for (int i = 0; i < firstProblems.Count; i++)
            {
                Debug.Log("GAMEPLAY: problem " + (i + 1) + " - " + firstProblems[i]);
            }
            bool pass = allOk && exceptions == 0 && errors == 0;
            Debug.Log("GAMEPLAY: " + (pass ? "PASS" : "FAIL"));
            Application.Quit();
        }
    }
}
