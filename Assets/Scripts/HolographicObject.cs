using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolographicObject : MonoBehaviour
{
    public Transform attachedObject;

    Vector3 scaleChange;
    Vector3 offSet;
    float differnceInScale = 0.05f;

    private void Start()
    {
        scaleChange = new Vector3(0.01f, 0.01f, 0.01f);
        offSet = new Vector3(0.0f, 0.5f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        //Update my position to match the ship's position
        //transform.SetPositionAndRotation(attachedObject.position, attachedObject.rotation);
        transform.position = attachedObject.position * differnceInScale + offSet;
        transform.localScale = scaleChange;

        //Hologram.position = Enitty.Position * differenceInScale ;
    }
}
