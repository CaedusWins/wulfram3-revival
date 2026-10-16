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
            // Let the tank settle on its hover height and every Start() run.
            yield return StartCoroutine(Wait(3f));

            Com.Wulfram3.Unit myUnit = me.GetComponent<Com.Wulfram3.Unit>();
            Debug.Log("GAMEPLAY: player tank '" + me.name + "' team=" + (myUnit == null ? "?" : myUnit.team) +
                " at " + me.transform.position + " cursorVisible=" + Cursor.visible + " lockState=" + Cursor.lockState);
            yield return StartCoroutine(CannonFuelDestroy(me, myUnit));
            Finish();
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

            // A real enemy unit, on the ground 10 m in front of the tank (the tank's rotation is driven
            // by mouse look every frame, so the target comes to the tank rather than the other way).
            string prefab = myUnit.team.ToLower() == "red" ? "BlueFlakTurret" : "RedFlakTurret";
            Vector3 spot = me.transform.position + Vector3.ProjectOnPlane(me.transform.forward, Vector3.up).normalized * 10f;
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                spot.y = terrain.SampleHeight(spot) + terrain.transform.position.y;
            }
            GameObject target = PhotonNetwork.Instantiate(prefab, spot, Quaternion.LookRotation(-me.transform.forward), 0);
            Com.Wulfram3.FlakTurretManager ai = target.GetComponent<Com.Wulfram3.FlakTurretManager>();
            if (ai != null)
            {
                ai.enabled = false;
            }
            Com.Wulfram3.HitPointsManager targetHp = target.GetComponent<Com.Wulfram3.HitPointsManager>();
            Com.Wulfram3.Unit targetUnit = target.GetComponent<Com.Wulfram3.Unit>();
            yield return StartCoroutine(Wait(1f));

            // What the cannon's ray hits from where the tank is now (the same origin TryFire uses).
            Vector3 origin = me.transform.position + (me.transform.forward * 1.0f + me.transform.up * 0.2f);
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

            // cannon: hold the trigger for 2 s.
            int healthBefore = targetHp.health;
            int fuelBefore = fuel.fuel;
            int lowestFuel = fuel.fuel;
            float hold = Time.realtimeSinceStartup + 2f;
            while (Time.realtimeSinceStartup < hold)
            {
                cannon.TryFire();
                lowestFuel = Mathf.Min(lowestFuel, fuel.fuel);
                yield return null;
            }
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
