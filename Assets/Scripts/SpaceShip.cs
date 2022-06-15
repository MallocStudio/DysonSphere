using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShip : MonoBehaviour
{

    //private GameObject attachedHolograph;

    public Hologram hologram;
    // Start is called before the first frame update
    void Start()
    {
        hologram.LinkNewEntity(transform);
    }

    //private void OnDestroy()
    //{
    //    Destroy(attachedHolograph);
    //}

    // Update is called once per frame
    void Update()
    {
        //transform.position += new Vector3(0.0f, 0.0f, 0.0f);
    }
}
