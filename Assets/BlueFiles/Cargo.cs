
using UnityEngine;
using System.Collections;


namespace Com.Wulfram3
{
    public class Cargo : Photon.PunBehaviour
    {
        public string content;

        // Two contents saved in the 2018 Playground scene name no prefab, so those boxes could never be
        // deployed (design/gdd/cargo.md). Corrected when the box loads, in code rather than in the
        // binary scene.
        public static string CorrectedContent(string content)
        {
            if (content == "Repair Pad")
            {
                return "RepairPad";
            }
            if (content == "BlueCargo")
            {
                return "BlueGunTurret";
            }
            return content;
        }

        void Awake()
        {
            content = CorrectedContent(content);
        }

        [PunRPC]
        public void SetContent(string content)
        {
            this.content = content;
        }
    }
}
