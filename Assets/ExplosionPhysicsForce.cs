using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Wulfram3;

namespace UnityStandardAssets.Effects
{
    public class ExplosionPhysicsForce : Photon.PunBehaviour {
        public float explosionForce = 4;
        // The most splash damage one explosion does to a unit (design/gdd/turrets.md): uncapped, a flak
        // or pulse shell bursting on a tank (distance near 0) killed it outright.
        public int splashCap = 30;


        private IEnumerator Start()
        {
            // wait one frame because some explosions instantiate debris which should then
            // be pushed by physics force
            yield return null;

            float multiplier = GetComponent<ParticleSystemMultiplier>().multiplier;

            float r = 10*multiplier;
            var cols = Physics.OverlapSphere(transform.position, r);
            var rigidbodies = new List<Rigidbody>();
            foreach (var col in cols)
            {
                if (col.attachedRigidbody != null && !rigidbodies.Contains(col.attachedRigidbody) && IsControlledByMe(col.gameObject))
                {
                    rigidbodies.Add(col.attachedRigidbody);
                }
            }
            foreach (var rb in rigidbodies)
            {
                rb.AddExplosionForce(explosionForce*multiplier, transform.position, r, 1*multiplier, ForceMode.Impulse);
                if (PhotonNetwork.isMasterClient) {
                    HitPointsManager hitpoints = rb.gameObject.GetComponent<HitPointsManager>();
                    if (hitpoints != null) {
                        float distance = Vector3.Distance(rb.transform.position, transform.position);
                        // Capped as a float first: at distance 0 the division is infinite.
                        hitpoints.TakeDamage((int)Mathf.Min(splashCap, explosionForce * 3f / Mathf.Max(distance, 0.001f)));
                    }
                }
            }
        }

        private bool IsControlledByMe(GameObject o) {
            if (o.tag.Equals("Player")) {
                return o.GetPhotonView().isMine;
            }
            return PhotonNetwork.isMasterClient;
        }
    }
}
