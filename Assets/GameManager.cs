using Assets.InternalApis;
using Assets.InternalApis.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;


using UnityEngine;
using UnityEngine.SceneManagement;

namespace Com.Wulfram3
{
    public class GameManager : Photon.PunBehaviour
    {
        public GameObject pulseShellPrefab;

        public GameObject flakShellPrefab;

        public GameObject explosionPrefab;

        public GameObject hullBar;

        public GameObject fuelBar;

        public GameObject cargoPrefab;

        public Material redcolor;
        public Material bluecolor;
        public Material graycolor;

        public GameObject playerInfoPanelPrefab;
        public Transform[] spawnPointsBlue;
        public Transform[] spawnPointsRed;
     


        [HideInInspector]
        public Camera normalCamera;

        //[HideInInspector]
        public Camera overheadCamera;

        private TargetInfoController targetChangeListener;


        #region Photon Messages




        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public void OnLeftRoom()
        {
            // The local tank is DontDestroyOnLoad. Online, PUN destroys it when the room is
            // left (NetworkingPeer.LeftRoomCleanup); in offline mode PUN skips that cleanup,
            // and the tank kept running in the launcher (NullReferenceException in
            // PlayerMovementManager.FixedUpdate every physics step - no TerrainCollider there).
            if (PlayerMovementManager.LocalPlayerInstance != null)
            {
                Destroy(PlayerMovementManager.LocalPlayerInstance);
            }
            SceneManager.LoadScene(0);
        }

        public override void OnPhotonPlayerConnected(PhotonPlayer other)
        {
            Debug.Log("OnPhotonPlayerConnected() " + other.NickName); // not seen if you're the player connecting


            if (PhotonNetwork.isMasterClient)
            {
                Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected


                //LoadArena();
            }
        }


        public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
        {
            Debug.Log("OnPhotonPlayerDisconnected() " + other.NickName); // seen when other disconnects


            if (PhotonNetwork.isMasterClient)
            {
                Debug.Log("OnPhotonPlayerDisonnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected


                //LoadArena();
            }
        }


        #endregion


        #region Public Methods


        // The HUD's clickable panel (the Leave Game button) is saved inactive in Playground, and the
        // GUIManager that shows it while the cursor is visible sits on that panel, so it never ran and
        // a player could not leave (design/gdd/hud.md). Activating the panel lets GUIManager take over.
        private void ShowClickableUI() {
            foreach (GUIManager gui in Resources.FindObjectsOfTypeAll<GUIManager>()) {
                if (gui.gameObject.scene.IsValid() && !gui.gameObject.activeSelf) {
                    gui.gameObject.SetActive(true);
                }
            }
        }

        public void LeaveRoom()
        {
            var userControler = DepenencyInjector.Resolve<IUserController>();
            var discordApi = DepenencyInjector.Resolve<IDiscordApi>();
            PhotonNetwork.LeaveRoom();
            StartCoroutine(discordApi.PlayerLeft(userControler.GetWulframPlayerData().userName));
        }

        public void Start()
        {
            Debug.Log("We are Instantiating LocalPlayer from " + Application.loadedLevelName);

            if (PlayerMovementManager.LocalPlayerInstance == null) {
                Debug.Log("We are Instantiating LocalPlayer from " + Application.loadedLevelName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate


                //team start
                PunTeams.UpdateTeamsNow();

                int redPlayers = PunTeams.PlayersPerTeam[PunTeams.Team.red].Count;
                int bluePlayers = PunTeams.PlayersPerTeam[PunTeams.Team.blue].Count;

                Debug.Log("Number of Red players: " + redPlayers);
                Debug.Log("Number of Blue players: " + bluePlayers);
                // A player who already has a team (the arena was reset after a match,
                // MatchManager.ResetArena) keeps it; the counts include that player.
                PunTeams.Team myTeam = PhotonNetwork.player.GetTeam();
                if (myTeam == PunTeams.Team.red || (myTeam != PunTeams.Team.blue && bluePlayers > redPlayers)) {
                    Debug.Log("Spawn red tank");
                    Transform selectedSpawnPoint = spawnPointsRed[0];
                    GameObject player = PhotonNetwork.Instantiate("RedTank", selectedSpawnPoint.position, selectedSpawnPoint.rotation, 0);
                    PhotonNetwork.player.SetTeam(PunTeams.Team.red);
                    
                   
                } else {
                    Debug.Log("Spawn blue tank");
                    Transform selectedSpawnPoint = spawnPointsBlue[0];
                    GameObject player = PhotonNetwork.Instantiate("PlayerTank", selectedSpawnPoint.position, selectedSpawnPoint.rotation, 0);
                    PhotonNetwork.player.SetTeam(PunTeams.Team.blue);
                    
                }
            } else {
                Debug.Log("Ignoring scene load for " + Application.loadedLevelName);
            }
            normalCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            overheadCamera = GameObject.FindGameObjectWithTag("OverheadCamera").GetComponent<Camera>();
            overheadCamera.enabled = false; //set disabled so that it does't render in the background

            // The win condition (design/gdd/win-condition.md). Added here, not in the binary scene;
            // its RPCs go through this object's PhotonView.
            if (GetComponent<MatchManager>() == null) {
                gameObject.AddComponent<MatchManager>();
                photonView.RefreshRpcMonoBehaviourCache();
            }
            ShowClickableUI();
        }

        [PunRPC]
        public void SpawnPulseShell(Vector3 pos, Quaternion rotation)
        {
            if (PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.Instantiate(pulseShellPrefab.name, pos, rotation, 0);
            }
        }
        public void SpawnFlakShell(Vector3 pos, Quaternion rotation)
        {
            if (PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.Instantiate(flakShellPrefab.name, pos, rotation, 0);
            }
        }

        public void SpawnExplosion(Vector3 pos)
        {
            if (PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.Instantiate(explosionPrefab.name, pos, Quaternion.identity, 0);
            }
        }

        //laser stuff autocannon
        /*public void DrawLine(Vector3 startPos, Vector3 endPos)
		{ 
			if (PhotonNetwork.isMasterClient) {
				//PhotonNetwork.Instantiate(lineRender, startPos, Quaternion.identity, 0);

			}

		}*/


        public void UnitsHealthUpdated(HitPointsManager hitpointsManager)
        {
            if (hitpointsManager.tag.Equals("Player") && hitpointsManager.photonView.isMine)
            {
                SetHullBar((float)hitpointsManager.health / (float)hitpointsManager.maxHealth);
            }
            if (PhotonNetwork.isMasterClient && hitpointsManager.health <= 0 && !hitpointsManager.tag.Equals("Player"))
            {
                PhotonNetwork.Destroy(hitpointsManager.gameObject);
                SpawnExplosion(hitpointsManager.transform.position);
            }
        }

        public void SetHullBar(float level)
        {
            hullBar.GetComponent<LevelController>().SetLevel(level);
        }

        public void FuelLevelUpdated(FuelManager fuelManager) {
            SetFuelBar((float) fuelManager.fuel / (float) fuelManager.maxFuel);
        }

        public void SetFuelBar(float level) {
            fuelBar.GetComponent<LevelController>().SetLevel(level);
        }

        public void SetCurrentTarget(GameObject go)
        {
            if (targetChangeListener != null)
            {
                targetChangeListener.TargetChanged(go);
            }
        }

        public void AddTargetChangeListener(TargetInfoController tic)
        {
            targetChangeListener = tic;
        }

        public void DestroyNow(GameObject go)
        {
            if (PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.Destroy(go);
                SpawnExplosion(go.transform.position);
            }
        }

        public string GetColoredPlayerName(string name, bool isMaster, bool colorFull = false, PunTeams.Team team = PunTeams.Team.none)
        {
            string masterClient = "";
            string modTag = "";
            string username = "";
            string teamColor = "";
            switch (team)
            {
                case PunTeams.Team.none:
                    teamColor = this.graycolor.color.ToHex();
                    break;
                case PunTeams.Team.red:
                    teamColor = this.redcolor.color.ToHex();
                    break;
                case PunTeams.Team.blue:
                    teamColor = this.bluecolor.color.ToHex();
                    break;
                default:
                    teamColor = this.graycolor.color.ToHex();
                    break;
            }


            if (isMaster)
            {
                masterClient = "<color=magenta>*</color>";
            }

            if (name.Contains("[MOD]"))
            {
                var names = name.Split(' ');

                modTag = "<color=yellow>" + names[0] + "</color>"; 
                if (colorFull == true)
                {
                    username = modTag + " " + "<color=" + teamColor + ">" + names[1] + "</color>";
                }
                else
                {
                    username = modTag + " " + "<color=white>" + names[1] + "</color>";
                }
            }
            else if (name.Contains("[DEV]"))
            {
                var names = name.Split(' ');

                modTag = "<color=#e0950b>" + names[0] + "</color>";
                if (colorFull == true)
                {
                    username = modTag + " " + "<color=" + teamColor + ">" + names[1] + "</color>";
                }
                else
                {
                    username = modTag + " " + "<color=white>" + names[1] + "</color>";
                }
            }
            else
            {
                if (colorFull == true)
                {
                    username = "<color=" + teamColor + ">" + name + "</color>";
                }
                else
                {
                    username = "<color=white>" + name + "</color>";
                }
            }

            return masterClient + username;
        }

        // Set by Respawn when the dead local player's team had no repair pad: Update watches for one
        // (a teammate deploying it) and builds the tank there.
        private bool waitingForPad;
        private float nextPadCheck;

        public bool WaitingForPad
        {
            get { return waitingForPad; }
        }

        // Called every destroyDelayWhenDead while the local player is dead. Only your own team's
        // repair pads build you a new tank (the manual: "as long as there is a RepairPad to build it
        // for you"); you pick one on the overhead map. With none left you wait there until a
        // teammate deploys one. This used to respawn you at the arena centre, which made a team
        // impossible to eliminate (design/gdd/win-condition.md).
        public void Respawn(PlayerMovementManager player)
        {
            overheadCamera.enabled = true;
            normalCamera.enabled = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            waitingForPad = RepairPad.FindNearestOwn(player) == null;
        }

        public void Update()
        {
            if (!waitingForPad || Time.time < nextPadCheck)
            {
                return;
            }
            nextPadCheck = Time.time + 0.25f;
            GameObject me = PlayerMovementManager.LocalPlayerInstance;
            PlayerMovementManager player = me == null ? null : me.GetComponent<PlayerMovementManager>();
            if (player == null || !player.isDead)
            {
                waitingForPad = false;
                return;
            }
            RepairPad pad = RepairPad.FindNearestOwn(player);
            if (pad != null)
            {
                waitingForPad = false;
                RepairPad.Spawn(this, player, pad.transform.position);
            }
        }

        // Cargo RPCs carry the tank's PhotonView ID, not the CargoManager itself: PUN can't send a
        // component over the network. It only seemed to work because offline mode doesn't serialize
        // RPC arguments (Tools/check-rpc.sh found it, 2026-10-16).
        private static CargoManager FindCargoManager(int cargoManagerViewId) {
            PhotonView view = PhotonView.Find(cargoManagerViewId);
            return view == null ? null : view.GetComponent<CargoManager>();
        }

        [PunRPC]
        public void RequestPickUpCargo(int cargoManagerViewId) {
            CargoManager cargoManager = FindCargoManager(cargoManagerViewId);
            if (cargoManager == null) {
                return;
            }
            if (PhotonNetwork.isMasterClient) {
                if (cargoManager.pickedUpCargo != "") {
                    return;
                }
                Cargo cargo = FindCargoInRange(cargoManager.transform.position, 5f);
                if (cargo != null) {
                    cargoManager.photonView.RPC("SetPickedUpCargo", PhotonTargets.All, cargo.content);
                    if (cargo.GetComponentInParent<PlayerMovementManager>() != null) {
                    }
                    PhotonNetwork.Destroy(cargo.gameObject);
                }
            }
        }

        private Cargo FindCargoInRange(Vector3 position, float scanRadius) {
            Transform closestTarget = null;
            float minDistance = scanRadius + 10f;
            var cols = Physics.OverlapSphere(position, scanRadius);
            var rigidbodies = new List<Rigidbody>();
            foreach (var col in cols) {
                if (col.attachedRigidbody != null && !rigidbodies.Contains(col.attachedRigidbody) && col.attachedRigidbody.GetComponentInParent<Cargo>() != null) {
                    rigidbodies.Add(col.attachedRigidbody);
                }
            }

            foreach (Rigidbody rb in rigidbodies) {
                Transform target = rb.transform;

                float distance = Vector3.Distance(position, target.transform.position);
                if (distance < minDistance) {
                    minDistance = distance;
                    closestTarget = target;
                }
            }

            if (closestTarget == null) {
                return null;
            }
            return closestTarget.GetComponentInParent<Cargo>();
        }

        public void PickUpCargo(CargoManager cargoManager) {
            photonView.RPC("RequestPickUpCargo", PhotonTargets.MasterClient, cargoManager.photonView.viewID);
        }

        [PunRPC]
        public void RequestDropCargo(int cargoManagerViewId) {
            CargoManager cargoManager = FindCargoManager(cargoManagerViewId);
            if (cargoManager == null) {
                return;
            }
            if (PhotonNetwork.isMasterClient) {
                string pickedUpCargo = cargoManager.pickedUpCargo;
                if (pickedUpCargo == "") {
                    return;
                }
                cargoManager.photonView.RPC("SetPickedUpCargo", PhotonTargets.All, "");
                // A room object, like a deployed unit (RequestDeploy): it stays when the master leaves,
                // and the arena reset removes it (instantiation data marks it Unit.deployed).
                GameObject go = PhotonNetwork.InstantiateSceneObject(cargoPrefab.name, cargoManager.transform.position, Quaternion.identity, 0, new object[] { "" });
                go.GetComponent<Cargo>().photonView.RPC("SetContent", PhotonTargets.All, pickedUpCargo);
            }
        }

        public void DropCargo(CargoManager cargoManager) {
            photonView.RPC("RequestDropCargo", PhotonTargets.MasterClient, cargoManager.photonView.viewID);
        }

        // Deployed units belong to the room, not to the player who deployed them (ADR-0003): the master
        // creates them as scene objects, so the master can destroy them at 0 health (PUN refuses to
        // network-destroy another active player's object) and they stay when their builder leaves (PUN
        // removes a leaving player's own objects). The team travels as instantiation data (Unit.Awake).
        public void DeployUnit(CargoManager cargoManager, Vector3 position, Quaternion rotation) {
            photonView.RPC("RequestDeploy", PhotonTargets.MasterClient, cargoManager.photonView.viewID, position, rotation);
        }

        // The request names only the tank; the master takes the unit from the cargo actually on board
        // and the team from the tank, and refuses a tank that isn't the sender's, an empty hold or a
        // unit with no prefab, and anything between a match's end and the reset (a request sent just
        // before the reset finds its old tank gone). It used to create whatever a client named
        // (architecture review 2, N1/N4).
        [PunRPC]
        public void RequestDeploy(int cargoManagerViewId, Vector3 position, Quaternion rotation, PhotonMessageInfo info) {
            if (!PhotonNetwork.isMasterClient || MatchManager.DamageBlocked) {
                return;
            }
            CargoManager cargoManager = FindCargoManager(cargoManagerViewId);
            if (cargoManager == null || info.sender == null || cargoManager.photonView.ownerId != info.sender.ID) {
                return;
            }
            string unitName = cargoManager.pickedUpCargo;
            Unit tank = cargoManager.GetComponent<Unit>();
            if (unitName == "" || tank == null || Resources.Load<GameObject>(unitName) == null) {
                return;
            }
            cargoManager.photonView.RPC("SetPickedUpCargo", PhotonTargets.All, "");
            PhotonNetwork.InstantiateSceneObject(unitName, position, rotation, 0, new object[] { tank.team });
        }

        #endregion

        #region Private Methods

        public override void OnJoinedRoom()
        {


            Debug.Log("Sent Post!' ");
            // #Critical: We only load if we are the first player, else we rely on  PhotonNetwork.automaticallySyncScene to sync our instance scene.
            if (PhotonNetwork.room.PlayerCount == 1)
            {
                Debug.Log("We load the 'Playground' ");

                // #Critical
                // Load the Room Level. 
                PhotonNetwork.LoadLevel("Playground");

            }
        }

        void LoadArena()
        {
            if (!PhotonNetwork.isMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
                return;
            }
            Debug.Log("PhotonNetwork : Loading Level : " + PhotonNetwork.room.PlayerCount);
            PhotonNetwork.LoadLevel("Playground");
        }


        #endregion


    }
}