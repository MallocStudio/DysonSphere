using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Author: Blake, Matin
 *
 * manages the Hologram ship scale, position and roation to match the ship on the grid
 */


public class HolographicObject : MonoBehaviour
{
    public Transform attachedObject;

    public float differnceInScale = 0.01f;
    public float differenceInVisualScale = 0.01f;

    MeshRenderer mesh_renderer;
    Vector3 offSet;

    public void init(Transform _attachedObject)
    {
        this.attachedObject = _attachedObject;
        
            // If this attachment is with an AI Actor, set its attachment obj to this
        AI_Actor entity = _attachedObject.GetComponent<AI_Actor>();
        if (entity != null)
        {
            entity.attached_holographic_obj = this;
        }

        Debug.Assert(attachedObject != null);

        transform.localScale = new Vector3(differenceInVisualScale, differenceInVisualScale, differenceInVisualScale);
        offSet = new Vector3(0.0f, 0.3f, 0.0f);
    }

    public void update(Vector3 spawn_point, float world_radius)
    {
        //Update my position to match the ship's position
        transform.rotation = attachedObject.rotation;
        transform.localPosition = (attachedObject.position - spawn_point) * differnceInScale + (offSet);
    }

    public Transform select() {
        //mesh_renderer.material.color = Color.white; // @debug // @nocheckin
        return attachedObject;
    }
}