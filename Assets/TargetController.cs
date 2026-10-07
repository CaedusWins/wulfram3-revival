using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3 {
    public class TargetController : Photon.PunBehaviour {

        private GameManager gameManager;
        public GameObject[] targets;

        private int currentTarget;
        private int totalTargets;

        public Transform target;
        public Texture2D image;

        Vector3 point;

        // Use this for initialization
        void Start() {
            gameManager = FindObjectOfType<GameManager>();
            // Targets are found by their Unit component (below), not a "Unit" tag -
            // that tag was never defined in TagManager, so FindGameObjectsWithTag threw.
        }

        // Update is called once per frame
        void Update() {
            if (!photonView.isMine)
                return;

            if (Input.GetKeyDown(KeyCode.T)) {
                Vector3 pos = transform.position + (transform.forward * 2.0f + transform.up * 0.2f);
                Quaternion rotation = transform.rotation;

                RaycastHit objectHit;
                bool targetFound = Physics.Raycast(pos, transform.forward, out objectHit, 300) && objectHit.transform.GetComponent<Unit>() != null;
                if (targetFound) {
                    gameManager.SetCurrentTarget(objectHit.transform.gameObject);
                }
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CycleTarget();
            }
        }

        /// <summary>
        /// Selects the next targetable unit and returns it (null if there is none).
        /// Targetable = has a HitPointsManager, which the target info panel displays -
        /// this skips shells, power cells and the model sub-unit inside each tank - and
        /// is not this player's own tank or one of its parts. Rebuilt on every call,
        /// since units spawn and die during play.
        /// </summary>
        public GameObject CycleTarget() {
            Unit[] units = FindObjectsOfType<Unit>();
            List<GameObject> found = new List<GameObject>();
            for (int i = 0; i < units.Length; i++) {
                if (units[i].transform.IsChildOf(transform) || units[i].GetComponent<HitPointsManager>() == null) {
                    continue;
                }
                found.Add(units[i].gameObject);
            }

            targets = found.ToArray();
            if (targets.Length == 0) {
                return null;
            }

            currentTarget = (currentTarget + 1) % targets.Length;
            gameManager.SetCurrentTarget(targets[currentTarget]);
            return targets[currentTarget];
        }
    }
}
