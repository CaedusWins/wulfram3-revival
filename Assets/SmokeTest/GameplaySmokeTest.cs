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
    ///   death    (row 10) lethal damage: the tank is dead and its cannon refuses to fire
    ///   respawn  (row 10) the overhead map + cursor after destroyDelayWhenDead, then a click on a repair
    ///                     pad (its own OnMouseDown) brings the tank back with full health and fuel
    ///   self-destruct (row 11) the V key's handler: the blast damages a nearby enemy and kills the
    ///                     tank, and respawn works again afterwards
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
            yield return StartCoroutine(DeathAndRespawn(me));
            yield return StartCoroutine(SelfDestructAndRespawn(me, myUnit));
            Finish();
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

        // Respawn (row 10): destroyDelayWhenDead after death the game shows the overhead map with the
        // cursor so the player can click a repair pad (GameManager.Respawn); with no pad in the arena it
        // respawns the tank at the arena centre itself. The click is RepairPad's own OnMouseDown
        // handler (sent as a message - Unity 2017 can't fake a mouse click). After it the tank must be
        // alive at the pad, with full health and fuel, back in the normal camera with the cursor hidden.
        private IEnumerator Respawn(string phase, GameObject me)
        {
            Com.Wulfram3.HitPointsManager hp = me.GetComponent<Com.Wulfram3.HitPointsManager>();
            Com.Wulfram3.PlayerMovementManager movement = me.GetComponent<Com.Wulfram3.PlayerMovementManager>();
            Com.Wulfram3.FuelManager fuel = me.GetComponent<Com.Wulfram3.FuelManager>();
            Com.Wulfram3.GameManager gameManager = FindObjectOfType<Com.Wulfram3.GameManager>();
            RepairPad[] pads = FindObjectsOfType<RepairPad>();
            float deadSince = Time.realtimeSinceStartup;
            float until = deadSince + movement.destroyDelayWhenDead + 5f;
            Vector3 expected;
            if (pads.Length > 0)
            {
                while (!(gameManager.overheadCamera.enabled && !gameManager.normalCamera.enabled) && Time.realtimeSinceStartup < until)
                {
                    yield return null;
                }
                bool view = gameManager.overheadCamera.enabled && !gameManager.normalCamera.enabled && Cursor.visible;
                Check(phase + "-view", view, "after " + (Time.realtimeSinceStartup - deadSince).ToString("0.0") + " s dead: overhead camera=" +
                    gameManager.overheadCamera.enabled + " normal camera=" + gameManager.normalCamera.enabled +
                    " cursorVisible=" + Cursor.visible + ", repair pads in arena: " + pads.Length);
                RepairPad pad = pads[0];
                for (int i = 1; i < pads.Length; i++)
                {
                    if (Vector3.Distance(pads[i].transform.position, me.transform.position) < Vector3.Distance(pad.transform.position, me.transform.position))
                    {
                        pad = pads[i];
                    }
                }
                expected = pad.transform.position + new Vector3(0f, 5f, 0f);
                pad.SendMessage("OnMouseDown");
            }
            else
            {
                expected = new Vector3(0f, 5f, 0f);
                while (movement.isDead && Time.realtimeSinceStartup < until)
                {
                    yield return null;
                }
                Debug.Log("GAMEPLAY: no repair pad in the arena - the game respawns the tank at the centre itself");
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
                yield return StartCoroutine(Respawn("respawn-after-self-destruct", me));
            }
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
