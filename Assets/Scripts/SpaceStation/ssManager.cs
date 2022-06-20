using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ssManager : MonoBehaviour
{
    InputActionReference actBut;

    [Header ("Plugins")]
    [SerializeField] private Transform combatRing;
    [SerializeField] private int curBay;

    
    private void Awake()
    {
        curBay = 0;    
    }
    private void RotateBay()
    {
        combatRing.Rotate(0, 30, 0); 
    }

    private void Fire()
    {
        
    }
}
