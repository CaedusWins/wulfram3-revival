using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3{
public class RayViewer : Photon.PunBehaviour {
	public float weaponRange = 50f;
	private Camera fpsCam;



	void Start () {
		fpsCam = GetComponentInParent<Camera> ();
		fpsCam = Camera.main;
	}







	void Update () {



		// Tanks survive scene loads: after an arena reset the cached camera is destroyed, and a
		// remote player's old tank runs here until its owner's destroy arrives.
		if (fpsCam == null) {
			fpsCam = Camera.main;
			if (fpsCam == null) {
				return;
			}
		}
		Vector3 lineOrigin = fpsCam.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 0));
		Debug.DrawRay (lineOrigin, fpsCam.transform.forward * weaponRange, Color.green);


		
	}
	
	

}
}