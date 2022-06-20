using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;

public class ssManager : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private Transform rightHandCon;
    [SerializeField] private InputActionReference trigActRef;

    [Header ("Plugins")]
    [SerializeField] private Transform combatRing;
    [SerializeField] private Transform cannonAimRet;
    [SerializeField] private Transform mgunAimRet;
    [SerializeField] private Transform missileAimRet;
    [SerializeField] private List<Transform> weaponList = new List<Transform>();

    [Header("Combat Stats")]
    [SerializeField] private float rotateSpd;
    [SerializeField] private float cannonFireRate;
    [SerializeField] private float mgunFireRate;
    [SerializeField] private float cannonReload;
    [SerializeField] private float mgunReload;
    [SerializeField] private float missileReload;

    [Header("Station Status")]
    [SerializeField] private weaponBay curBay;
    [SerializeField] public bool gunneryMode;
    [SerializeField] private float cannonTimer;
    [SerializeField] private float mgunTimer;
    [SerializeField] private float missileTimer;
    
    [Header("Station Status GUI")]
    [SerializeField] private GameObject cannonRet;
    [SerializeField] private GameObject mgunRet;
    [SerializeField] private GameObject missileRet;
    [SerializeField] private TMP_Text TMPcannonTimer;
    [SerializeField] private TMP_Text TMPmgunTimer;
    [SerializeField] private TMP_Text TMPmissileTimer;


    [Header("Input Events")]
    [SerializeField] public UnityEvent trigEv;

    private void Awake()
    {
        /* CACHE SHIT */

        /* Set Default */
        curBay = weaponBay.cannon;
        gunneryMode = false;
        cannonTimer = 0f;
        mgunTimer = 0f;
        missileTimer = 0f;
        cannonRet.SetActive(false);
        mgunRet.SetActive(false);
        missileRet.SetActive(false);

        /* Input Action SetUp */
        trigActRef.action.performed += TrigAction;
    }


    private void LateUpdate()
    {
        /* Update Console UI elements */
        if(cannonTimer >= 0)
            TMPcannonTimer.SetText("Cannon: READY FIRE");
        else
            TMPcannonTimer.SetText("Cannon: RELOADING: " + cannonTimer);

        if (mgunTimer >= 0)
            TMPmgunTimer.SetText("Machine Gun: READY FIRE");
        else
            TMPmgunTimer.SetText("Machine Gun: RELOADING " + mgunTimer);

        if (missileTimer >= 0)
            TMPmissileTimer.SetText("Missile: READY FIRE");
        else
            TMPmissileTimer.SetText("Missile: RELOADING " + missileTimer);
    }
    private void TrigAction(InputAction.CallbackContext obj)
    {
        trigEv.Invoke();
    }

    private void RotateBay(int bayNo)
    {
        combatRing.Rotate(0, 5 * rotateSpd, 0);
    }

    public void GunnaryActive()
    {
        if (curBay == weaponBay.cannon)
        {
            cannonRet.SetActive(true);
            mgunRet.SetActive(false);
            missileRet.SetActive(false);
        }
        else if (curBay == weaponBay.mgun)
        {
            cannonRet.SetActive(false);
            mgunRet.SetActive(true);
            missileRet.SetActive(false);
        }
        else if (curBay == weaponBay.cannon)
        {
            cannonRet.SetActive(false);
            mgunRet.SetActive(false);
            missileRet.SetActive(true);
        }
        else return;
    }

    private void Aim()
    {
        if (gunneryMode)
        {
            if (curBay == weaponBay.cannon)
            {

            }
            else if (curBay == weaponBay.mgun)
            {

            }
            else if (curBay == weaponBay.cannon)
            {

            }
            else return;
        }
        else return;
    }

    private void Fire()
    {
        //for loop to fire through current list of weapons

    }
}

public enum weaponBay
{
    cannon,
    mgun,
    missile
}
