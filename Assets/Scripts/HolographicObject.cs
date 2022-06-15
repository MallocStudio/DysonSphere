using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolographicObject : MonoBehaviour
{
    public Transform attachedObject;
   
    // Update is called once per frame
    void Update()
    {
        //Update my position to match the ship's position
        transform.SetPositionAndRotation(attachedObject.position * 0.01f, attachedObject.rotation);
    }
}
