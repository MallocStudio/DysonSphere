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

    [Header("Plugins")]
    [SerializeField] private Transform combatRing;
    [SerializeField] public Transform cannonAimRet;
    [SerializeField] public Transform mgunAimRet;
    [SerializeField] public Transform missileAimRet;
    [SerializeField] private List<ssWeaponBay> weaponBays = new List<ssWeaponBay>();
    [SerializeField] private List<float> weaponBayReloads = new List<float>();

    [Header("Combat Stats")]
    [SerializeField] private float rotateSpd;
    [SerializeField] private float cannonFireRate;
    [SerializeField] private float mgunFireRate;
    [SerializeField] private float cannonReload;
    [SerializeField] private float mgunReload;
    [SerializeField] private float missileReload;

    [Header("Station Status")]
    [SerializeField] private weaponBay curBay;
    [SerializeField] public bool gunMode;
    [SerializeField] public bool fireSafety;

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

    [Header("Station Console Inserts")]
    [SerializeField] public GameObject gModeLight;
    [SerializeField] public GameObject ringSel;

    [Header("Input Events")]
    [SerializeField] public UnityEvent trigStartEv;
    [SerializeField] public UnityEvent trigEndEv;

    private void Awake()
    {
        /* CACHE SHIT */

        /* Set Default */
        curBay = weaponBay.cannon;
        combatRing.Rotate(0, 0, 0);
        cannonTimer = 0f;
        mgunTimer = 0f;
        missileTimer = 0f;

        gunMode = false;
        gModeLight.SetActive(false);
        cannonRet.SetActive(false);
        mgunRet.SetActive(false);
        missileRet.SetActive(false);

        /* Input Action SetUp */
        trigActRef.action.performed += TrigStartAction;
        trigActRef.action.canceled += TrigEndAction;
    }

    private void LateUpdate()
    {
        /* Update Console UI elements */
        if (cannonTimer >= 0)
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
    private void TrigStartAction(InputAction.CallbackContext obj)
    {
        trigStartEv.Invoke();
    }
    private void TrigEndAction(InputAction.CallbackContext obj)
    {
        trigEndEv.Invoke();
    }

    public void GunneryToggle()
    {
        gunMode = !gunMode;

        if (gunMode)
        {
            GunnaryActive();
            gModeLight.SetActive(true);
        }

        if (!gunMode)
        {
            cannonRet.SetActive(false);
            mgunRet.SetActive(false);
            missileRet.SetActive(false);
            gModeLight.SetActive(false);
        }
    }
    private void GunnaryActive()
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
    public void RotateBayUp()
    {
        //Rotate 120* for each weapon bay
        combatRing.Rotate(0, 120, 0);
        curBay = (weaponBay)((int)curBay + 1);
        if(curBay == weaponBay.COUNT)
        {
            curBay = weaponBay.cannon;
        }
        /*
        for (int i = 0; i < (int)weaponBay.COUNT; i++)
        {
            weaponBay bay = (weaponBay)i;
        }
        */
    }
    public void RotateBayDown()
    {
        curBay = (weaponBay)((int)curBay - 1);
        if (curBay == weaponBay.cannon)
        {
            curBay = weaponBay.missile;
        }
    }
}

public enum weaponBay
{
    cannon, // 0
    mgun,   // 1
    missile, // 2



    COUNT, // 3 the maximum number of stuff above
}

//Use events in buttons to call fire and detonate functions