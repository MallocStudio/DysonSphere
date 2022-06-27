using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipHealth : MonoBehaviour
{
    public float health = 1.0f;
    public bool isDestroyed = true;

    public bool Health(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            isDestroyed = false;
            return isDestroyed;
        }

        return isDestroyed;
    }
}
