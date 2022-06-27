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

    public HolographicObject LinkNewEntity(Transform actual_entity)
    {
        GameObject theHolograph = Instantiate(holographPrefab);
        theHolograph.transform.SetParent(transform, false);
        HolographicObject hologram = theHolograph.GetComponent<HolographicObject>();
        Debug.Assert(hologram != null);
        hologram.init(actual_entity);
        return hologram;
    }
}