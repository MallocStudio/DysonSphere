using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ssManager : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private Transform rightHandCon;
    [SerializeField] private InputActionReference trigActRef;

    [Header ("Plugins")]
    [SerializeField] private Transform combatRing;
    [SerializeField] private Transform aimRet;
    [SerializeField] private List<Transform> weaponList = new List<Transform>();

    [SerializeField] private int curBay;
    [SerializeField] private float rotateSpd;

    [Header("Station Status")]
    [SerializeField] private bool gunneryMode;

    [SerializeField] public UnityEvent trigEv;


    private void Awake()
    {
        /* CACHE SHIT */

        /* Default */
        curBay = 0;
        gunneryMode = false;

        /* Input Action SetUp */
        trigActRef.action.performed += TrigAction;
    }

    private void RotateBay()
    {
        //change to auto stage rotation
        combatRing.Rotate(0, 5 * rotateSpd, 0); 
    }

    private void TrigAction(InputAction.CallbackContext obj)
    {
        trigEv.Invoke();
    }

    private void Aim()
    {
        if (gunneryMode)
        {
            //Vector3 aimDir = aimRet.position - rightHandCon.position;
            //RAYCAST DIRECTION
            //Ray ray = Physics.Raycast(rightHandCon.position, )
        }
        else return;
    }

    private void Fire()
    {
        //for loop to fire through current list of weapons
    }
}
