using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Author: Blake
 *
 * manages the Hologram ship scale, position and roation to match the ship on the grid
 */


public class HolographicObject : MonoBehaviour
{
    public Transform attachedObject;

    float differnceInScale = 0.005f;

    Vector3 offSet;

    public void init(Transform _attachedObject)
    {
        this.attachedObject = _attachedObject;

        Debug.Assert(attachedObject != null);

        // float changeInScale = 0.005f;
        float changeInScale = differnceInScale;
        transform.localScale = new Vector3(changeInScale, changeInScale, changeInScale);
        offSet = new Vector3(0.0f, 0.3f, 0.0f);
    }

    public void update(Vector3 spawn_point, float world_radius)
    {
        Debug.Log("being called");
        //Update my position to match the ship's position
        transform.rotation = attachedObject.rotation;
        transform.localPosition = (attachedObject.position - spawn_point) * differnceInScale + (offSet);
    }
}