using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipHealth : MonoBehaviour
{
    public float health = 1.0f;
    public bool isDestroyed = false;

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            isDestroyed = true;
        }
    }
}
