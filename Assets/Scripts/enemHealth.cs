using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemHealth : MonoBehaviour
{
    [SerializeField] private float MaxHp;
    [SerializeField] private float curHp;

    private void Awake()
    {
        curHp = MaxHp;        
    }

    public void Damage(float dmg)
    {
        curHp -= dmg;
    }
}
