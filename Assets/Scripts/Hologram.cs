using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Author: Blake
 * 
 * 
 */

/// THE GRID
public class Hologram : MonoBehaviour
{
    public GameObject holographPrefab;

    public void LinkNewEntity(Transform transform)
    {
        GameObject theHolograph = Instantiate(holographPrefab);
        theHolograph.GetComponent<HolographicObject>().attachedObject = transform;
        theHolograph.GetComponent<HolographicObject>().panel = this.transform;
    }
}