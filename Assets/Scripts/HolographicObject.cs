using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolographicObject : MonoBehaviour
{
    public Transform attachedObject;
    public Transform panel;

    Vector3 scaleChange;
    Vector3 offSet;
    float differnceInScale = 0.01f;

    private void Start()
    {
        scaleChange = new Vector3(0.01f, 0.01f, 0.01f);
        offSet = new Vector3(0.0f, 0.5f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        //Update my position to match the ship's position
        transform.localScale = scaleChange;
        transform.position = attachedObject.position * differnceInScale + (offSet + panel.position);
    }
}
