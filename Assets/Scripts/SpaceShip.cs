using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShip : MonoBehaviour
{
    public Hologram hologram;

    // Start is called before the first frame update
    void Start()
    {
        hologram.LinkNewEntity(transform);
    }
}