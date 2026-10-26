using Com.Wulfram3;
using UnityEditor;
using UnityEngine;

namespace Wulfram.EditorTools
{
    /// <summary>
    /// One-off data fix (2026-10-20, L1 row 14): the 2018 unit prefabs had inconsistent or wrong teams.
    /// The gameplay test's team audit found: RedFlakTurret/BlueFlakTurret with an empty team and
    /// shooting 'BlueTeam'/'RedTeam' (which never matched a 'blue'/'red' tank, so flak turrets never
    /// fired), BlueGunTurret on team 'Blue' and shooting 'blue' (its own team), BlueScout with no team.
    /// This sets them to the canonical "red"/"blue" and prints every value before and after. It is
    /// idempotent: running it again changes nothing.
    ///
    ///   Unity.exe -batchmode -quit -projectPath <path> -executeMethod Wulfram.EditorTools.WulframFixUnitTeams.Run
    /// </summary>
    public static class WulframFixUnitTeams
    {
        public static void Run()
        {
            int changed = 0;
            changed += Fix("Assets/Resources/RedFlakTurret.prefab", "red", "blue");
            changed += Fix("Assets/Resources/BlueFlakTurret.prefab", "blue", "red");
            changed += Fix("Assets/Resources/BlueGunTurret.prefab", "blue", "red");
            changed += Fix("Assets/Resources/BlueScout.prefab", "blue", null);
            AssetDatabase.SaveAssets();
            Debug.Log("FIXTEAMS: done, " + changed + " value(s) changed");
        }

        // Sets Unit.team (and the turret's teamDetect, when given) on a prefab; returns how many values changed.
        private static int Fix(string path, string team, string shoots)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError("FIXTEAMS: no prefab at " + path);
                return 0;
            }
            int changed = 0;
            Unit unit = prefab.GetComponent<Unit>();
            if (unit == null)
            {
                Debug.LogError("FIXTEAMS: " + path + " has no Unit");
                return 0;
            }
            Debug.Log("FIXTEAMS: " + path + " team '" + unit.team + "' -> '" + team + "'");
            if (unit.team != team)
            {
                unit.team = team;
                EditorUtility.SetDirty(unit);
                changed++;
            }
            if (shoots != null)
            {
                FlakTurretManager flak = prefab.GetComponent<FlakTurretManager>();
                GTManager gun = prefab.GetComponent<GTManager>();
                if (flak == null && gun == null)
                {
                    Debug.LogError("FIXTEAMS: " + path + " has no turret component");
                    return changed;
                }
                string before = flak != null ? flak.teamDetect : gun.teamDetect;
                Debug.Log("FIXTEAMS: " + path + " shoots '" + before + "' -> '" + shoots + "'");
                if (before != shoots)
                {
                    if (flak != null)
                    {
                        flak.teamDetect = shoots;
                        EditorUtility.SetDirty(flak);
                    }
                    else
                    {
                        gun.teamDetect = shoots;
                        EditorUtility.SetDirty(gun);
                    }
                    changed++;
                }
            }
            return changed;
        }
    }
}
