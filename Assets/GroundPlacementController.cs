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
                Destroy(currentPlaceableObject);
            }
            else
            {
                currentPlaceableObject = MakePreview(baseUnit);
            }
        }
    }

    // The placement preview is this client's own picture of the unit: its meshes in the preview
    // material and nothing else. It used to be a networked copy of the real unit with every script
    // switched on, so a previewed turret could fire, and the master could not remove it (it belonged
    // to this player; architecture review 2, N2).
    private GameObject MakePreview(string unitName)
    {
        GameObject prefab = Resources.Load<GameObject>(unitName);
        if (prefab == null)
        {
            return null;
        }
        GameObject preview = new GameObject(unitName + " (preview)");
        preview.transform.position = placeObject.transform.position;
        preview.transform.rotation = placeObject.transform.rotation;
        Transform root = prefab.transform;
        foreach (MeshFilter filter in prefab.GetComponentsInChildren<MeshFilter>(true))
        {
            MeshRenderer source = filter.GetComponent<MeshRenderer>();
            if (source == null || filter.sharedMesh == null)
            {
                continue;
            }
            GameObject part = new GameObject(filter.name);
            part.transform.SetParent(preview.transform, false);
            part.transform.localPosition = root.InverseTransformPoint(filter.transform.position);
            part.transform.localRotation = Quaternion.Inverse(root.rotation) * filter.transform.rotation;
            part.transform.localScale = filter.transform.lossyScale;
            part.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
            Material[] materials = new Material[source.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = PreviewMaterial;
            }
            part.AddComponent<MeshRenderer>().sharedMaterials = materials;
        }
        return preview;
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

            // Where the preview is, turned as the mouse wheel turned it.
            Vector3 position = currentPlaceableObject.transform.position;
            Quaternion rotation = currentPlaceableObject.transform.rotation;
            Destroy(currentPlaceableObject);
            // The master checks the request, creates the carried unit for the room with this tank's
            // team, and empties the hold for everyone (GameManager.RequestDeploy).
            gameManager.DeployUnit(this.GetComponentInParent<CargoManager>(), position, rotation);
    }
}


