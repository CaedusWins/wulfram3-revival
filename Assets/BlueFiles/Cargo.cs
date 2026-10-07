
using UnityEngine;
using System.Collections;


namespace Com.Wulfram3
{
    public class Cargo : Photon.PunBehaviour
    {
        public string content;

        [PunRPC]
        public void SetContent(string content)
        {
            this.content = content;
        }
    }
}
