using Assets.InternalApis.Classes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3 {
    public class Unit : MonoBehaviour {

        public string team;

        // Team names in the 2018 prefabs and scene are spelled inconsistently ("blue", "Blue",
        // "BlueTeam"), and every team check compared the strings exactly - so flak turrets that
        // shoot 'BlueTeam' never matched a 'blue' tank (found 2026-10-20 by the gameplay test).
        // Compare teams only through here: case, spaces and a trailing "team" don't matter.
        public static string NormalizeTeam(string team) {
            if (team == null) {
                return "";
            }
            string t = team.Trim().ToLowerInvariant();
            if (t.EndsWith("team")) {
                t = t.Substring(0, t.Length - 4).Trim();
            }
            return t;
        }

        public static bool SameTeam(string a, string b) {
            return NormalizeTeam(a) == NormalizeTeam(b);
        }

        public string name;

        public PunTeams.Team unitTeam;

        public UnitType unitType;

        

        // A unit deployed from cargo is created with its deployer's team as instantiation data
        // (GroundPlacementController.Release). Only a prefab with no team of its own takes it: the
        // RepairPad prefab has none, so a deployed pad used to belong to no one.
        void Awake() {
            PhotonView view = GetComponent<PhotonView>();
            if (Unit.NormalizeTeam(team) == "" && view != null && view.instantiationData != null &&
                view.instantiationData.Length > 0 && view.instantiationData[0] is string) {
                team = (string)view.instantiationData[0];
            }
        }

        // Use this for initialization
        void Start() {
            switch (team.ToLower())
            {
                case "blue":
                    unitTeam = PunTeams.Team.blue;
                    break;
                case "red":
                    unitTeam = PunTeams.Team.red;
                    break;
                default:
                    unitTeam = PunTeams.Team.none;
                    break;
            }

            switch (name.ToLower())
            {
                case "tank":
                    unitType = UnitType.Tank;
                    break;
                case "scout":
                    unitType = UnitType.Scout;
                    break;
                default:
                    unitType = UnitType.None;
                    break;
            }
        }

        // Update is called once per frame
        void Update() {

        }
    }
}
