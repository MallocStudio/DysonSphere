using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Author: Blake
 *
 *
 */


public enum Hologram_Type {
    ENEMY, LEADERSHIP, FRIENDLY
}
/// THE GRID
public class Hologram : MonoBehaviour
{
    public GameObject enemy;
    public GameObject leadership;
    public GameObject friendly;

    public HolographicObject LinkNewEntity(Transform actual_entity, Hologram_Type type)
    {
        GameObject prefab = null;
        switch (type) {
            case Hologram_Type.ENEMY: {
                prefab = enemy;
            } break;
            case Hologram_Type.LEADERSHIP: {
                prefab = leadership;
            } break;
            case Hologram_Type.FRIENDLY: {
                prefab = friendly;
            } break;
        }
        GameObject theHolograph = Instantiate(prefab);
        theHolograph.transform.SetParent(transform, false);
        HolographicObject hologram = theHolograph.GetComponent<HolographicObject>();
        Debug.Assert(hologram != null);
        hologram.init(actual_entity);
        return hologram;
    }
}