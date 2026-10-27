using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3 {
    public class PulseShellManager : Photon.PunBehaviour {
        public float velocity = 30;
        public int directHitpointsDamage = 40;

        private GameManager gameManager;

        // Use this for initialization
        void Start() {
            
            if (photonView.owner.IsLocal) {
                Rigidbody rb = GetComponent<Rigidbody>();
                rb.velocity = transform.forward * velocity;
                gameManager = FindObjectOfType<GameManager>();
            }
        }

        // Update is called once per frame
        void Update() {

        }

        // A shell hits once. Unity can call OnCollisionEnter again (one call per collider of a tank)
        // before PhotonNetwork.Destroy takes effect at the end of the frame; each call did the direct
        // damage and spawned an explosion again, so one flak shell could do 140 (gate dc-gate).
        private bool hit;

        void OnCollisionEnter(Collision col) {
            if (photonView.owner.IsLocal && !hit) {
                hit = true;
                HitPointsManager hitpoints = col.gameObject.GetComponent<HitPointsManager>();
                if (hitpoints != null) {
                    // Through the master client, like the AutoCannon: only the shooter's client handles
                    // the collision, and TakeDamage only acts on the master, so calling it directly did
                    // nothing when a non-master fired (same bug class as HealthRegenerator, PR #11).
                    hitpoints.TellServerTakeDamage(directHitpointsDamage);
                }

                Vector3 pos = col.contacts[0].point;
                //gameManager.SpawnExplosion(pos);
                PhotonNetwork.Instantiate(gameManager.explosionPrefab.name, pos, Quaternion.identity, 0);

                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
