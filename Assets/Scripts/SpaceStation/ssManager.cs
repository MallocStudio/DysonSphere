using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ssManager : MonoBehaviour
{
    InputActionReference trigActRef;

    [Header ("Plugins")]
    [SerializeField] private Transform combatRing;
    [SerializeField] private int curBay;
    [SerializeField] private float rotateSpd;

    [SerializeField] public UnityEvent onTeleportActivate;


    private void Awake()
    {
        curBay = 0;
        trigActRef.action.performed += TrigAction;

    }


    private void RotateBay()
    {
        //change to auto stage rotation
        combatRing.Rotate(0, 5 * rotateSpd, 0); 
    }

    private void TrigAction(InputAction.CallbackContext obj)
    {
        onTeleportActivate.Invoke();
    }


    private void Fire()
    {
        
    }
}
