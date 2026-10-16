using System.Collections;
using UnityEngine;

// C# port of Unity's Procedural Examples DragTransform.js (UnityScript), behavior unchanged:
// tint the object on mouse-over, drag it with the left mouse button. It keeps the .js file's
// .meta GUID (02b033a7...), so existing references - BlueScout.prefab, RedScout.prefab and the
// "Lightning bolt" demo scene - resolve to this class, and the serialized mouseOverColor is kept.
// Ported so the project no longer needs the UnityScript compiler, which crashed or failed with
// internal type-load errors on this machine and left the editor with stale assemblies (corrupted
// player builds). See CLAUDE.md.
public class DragTransform : MonoBehaviour
{
    public Color mouseOverColor = Color.blue;
    private Color originalColor;

    void Start()
    {
        originalColor = GetComponent<Renderer>().sharedMaterial.color;
    }

    void OnMouseEnter()
    {
        GetComponent<Renderer>().material.color = mouseOverColor;
    }

    void OnMouseExit()
    {
        GetComponent<Renderer>().material.color = originalColor;
    }

    IEnumerator OnMouseDown()
    {
        Vector3 screenSpace = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));
        while (Input.GetMouseButton(0))
        {
            Vector3 curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace) + offset;
            transform.position = curPosition;
            yield return null;
        }
    }
}
