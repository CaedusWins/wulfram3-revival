using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Wulfram3;

public class GroundPlacementController : Photon.PunBehaviour
{
    [SerializeField]
    public GameObject placeableObjectPrefab;
    
    [SerializeField]
    public KeyCode newObjectHotkey = KeyCode.Comma;

    public GameObject currentPlaceableObject;

    public float mouseWheelRotation;

    public GameObject placeObject;

    public Material PreviewMaterial;

    public GameManager gameManager;

    public string baseUnit;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        
    }

    // Every client runs this on every tank, so only the tank's owner may act on its keys, or pressing
    // ',' would place other players' cargo too. The tank's PhotonView is looked up in the parents:
    // PUN's photonView property only checks this GameObject.
    private PhotonView ownerView;

    private bool IsMine()
    {
        if (ownerView == null)
        {
            ownerView = GetComponentInParent<PhotonView>();
        }
        return ownerView != null && ownerView.isMine;
    }

    public void Update()
    {
        if (!IsMine())
        {
            return;
        }
        HandleNewObjectHotkey();
        baseUnit = this.GetComponentInParent<CargoManager>().pickedUpCargo;
        if (currentPlaceableObject != null)
        {
            MoveCurrentObjectToMouse();
            RotateFromMouseWheel();
            ReleaseIfClicked();
        }
    }

    public void HandleNewObjectHotkey()
    {
        if (Input.GetKeyDown(newObjectHotkey))
        {
            TogglePlacement();
        }
    }

    // The deploy hotkey (','): with cargo on board, shows a placement preview of the carried unit,
    // or removes the preview if one is showing. Public so the L1 gameplay smoke test can press it
    // (Unity 2017 can't fake key presses); players reach it only through HandleNewObjectHotkey.
    public void TogglePlacement()
    {
        if (!(baseUnit == "") && IsMine())
        {
            if (currentPlaceableObject != null)
            {
                // The preview is a network object (PhotonNetwork.Instantiate): a plain Destroy left
                // it standing on every other client.
                PhotonNetwork.Destroy(currentPlaceableObject);
            }
            else
            {
                currentPlaceableObject = PhotonNetwork.Instantiate(baseUnit, placeObject.transform.position, placeObject.transform.rotation, 0);
            }
        }
    }

    public void MoveCurrentObjectToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
        currentPlaceableObject.transform.position = placeObject.transform.position;
            //currentPlaceableObject.transform.position = hitInfo.point;
  }


        //foreach (var renderer in currentPlaceableObject.GetComponentsInChildren<MeshRenderer>(true))
            //renderer.sharedMaterial = PreviewMaterial;
        ChangeMaterial(PreviewMaterial);


        foreach (var script in currentPlaceableObject.GetComponentsInChildren<MonoBehaviour>(true))
            script.enabled = true;
    }

    public void RotateFromMouseWheel()
    {
        Debug.Log(Input.mouseScrollDelta);
        mouseWheelRotation += Input.mouseScrollDelta.y;
        currentPlaceableObject.transform.Rotate(Vector3.up, mouseWheelRotation * 10f);
    }

    public void ChangeMaterial(Material newMat)
    {
        Renderer[] children;
        children = currentPlaceableObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in children)
        {
            var mats = new Material[rend.materials.Length];
            for (var j = 0; j < rend.materials.Length; j++)
            {
                mats[j] = newMat;
            }
            rend.materials = mats;
        }
    }

    public void ReleaseIfClicked()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Release();
        }
    }

    // A left click while the preview shows: deploys the carried unit where the preview is and
    // empties the cargo hold. Public so the L1 gameplay smoke test can click (Unity 2017 can't fake
    // input); players reach it only through ReleaseIfClicked, which Update calls while a preview shows.
    public void Release()
    {
        if (currentPlaceableObject == null || !IsMine())
        {
            return;
        }
            //get baseunit to deploy based on its name in the resources folder (Must match)

            Debug.Log(baseUnit);

            // A network object: PhotonNetwork.Destroy, not Destroy (that left a ghost preview on other clients).
            PhotonNetwork.Destroy(currentPlaceableObject);
            PhotonNetwork.Instantiate(baseUnit, placeObject.transform.position, placeObject.transform.rotation, 0);
            // Empty the hold for everyone, as GameManager.RequestDropCargo does; calling SetPickedUpCargo
            // directly emptied it on this client only.
            this.GetComponentInParent<CargoManager>().photonView.RPC("SetPickedUpCargo", PhotonTargets.All, "");
    }
}


