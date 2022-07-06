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
    [SerializeField] private ssWeaponBay actWeaponBay;
    [SerializeField] private List<ssWeaponBay> weaponBays = new List<ssWeaponBay>();

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
    [SerializeField] private bool isRotating;
    [SerializeField] public float rotateDegrees = 120.0f;
    [SerializeField] public float rotateSpeed = 1.0f;

    [Header("Station Status GUI")]
    [SerializeField] private GameObject cannonRet;
    [SerializeField] private GameObject cannonRet2;
    [SerializeField] private GameObject mgunRet;
    [SerializeField] private GameObject mgunRet2;
    [SerializeField] private GameObject missileRet;
    [SerializeField] private GameObject missileRet2;
    [SerializeField] private TMP_Text TMPcurWeapon;
    [SerializeField] private TMP_Text TMPweaponsHot;

    [Header("Station Console Inserts")]
    [SerializeField] public GameObject gModeLight;

    [Header("Input Events")]
    [SerializeField] public UnityEvent trigStartEv;
    [SerializeField] public UnityEvent trigEndEv;


    private void Awake()
    {
        /* CACHE SHIT */

        /* Input Action SetUp */
        trigActRef.action.performed += TrigStartAction;
        trigActRef.action.canceled += TrigEndAction;
    }

    private void Start()
    {
        /* Set Default */
        combatRing.Rotate(0, 0, 0);

        isRotating = false;
        curBay = weaponBay.Cannon;
        actWeaponBay = weaponBays[0].GetComponent<ssWeaponBay>();
        actWeaponBay.activeBay = true;
        TMPcurWeapon.SetText("CURRENT WEAPON: " + curBay.ToString());

        gunMode = false;
        GunneryCall();
        TMPweaponsHot.SetText("Weapons Off");
        gModeLight.SetActive(false);
    }
    private void TrigStartAction(InputAction.CallbackContext obj)
    {
        trigStartEv.Invoke();
    }
    private void TrigEndAction(InputAction.CallbackContext obj)
    {
        trigEndEv.Invoke();
    }
    private void LateUpdate()
    {
        GunneryCall();    
    }
    public void GunneryToggle()
    {
        gunMode = !gunMode;

        if (gunMode)
        {
            TMPweaponsHot.SetText("Weapons Active");
            gModeLight.SetActive(false);
        }
        else if (!gunMode)
        {
            TMPweaponsHot.SetText("Weapons Off");
            gModeLight.SetActive(false);
        }
    }
    private void GunneryCall()
    {
        if (gunMode == false)
        {
            cannonRet.SetActive(false);
            cannonRet2.SetActive(false);
            mgunRet.SetActive(false);
            mgunRet2.SetActive(false);
            missileRet.SetActive(false);
            missileRet2.SetActive(false);
            gModeLight.SetActive(false);
        }
        if (gunMode)
        {
            if (curBay == weaponBay.Cannon)
            {
                cannonRet.SetActive(true);
                cannonRet2.SetActive(true);
                mgunRet.SetActive(false);
                mgunRet2.SetActive(false);
                missileRet.SetActive(false);
                missileRet2.SetActive(false);
            }
            else if (curBay == weaponBay.MachineGun)
            {
                cannonRet.SetActive(false);
                cannonRet2.SetActive(false);
                mgunRet.SetActive(true);
                mgunRet2.SetActive(true);
                missileRet.SetActive(false);
                missileRet2.SetActive(false);
            }
            else if (curBay == weaponBay.Missile)
            {
                cannonRet.SetActive(false);
                cannonRet2.SetActive(false);
                mgunRet.SetActive(false);
                mgunRet2.SetActive(false);
                missileRet.SetActive(true);
                missileRet2.SetActive(true);
            }
        }   
    }
    public void RotateBayUp()
    {
        actWeaponBay.activeBay = false;
        curBay = (weaponBay)((int)curBay + 1);

        // wrap around if needed
        if (curBay == weaponBay.COUNT)
        {
            curBay = (weaponBay)(0);
            actWeaponBay = weaponBays[(int)curBay].GetComponent<ssWeaponBay>();
        }

        // activate the new bay
        actWeaponBay.activeBay = true;

        GunneryCall();
        TMPcurWeapon.SetText("Current Weapon: " + curBay.ToString());

        if (isRotating == false)
        {
            StartCoroutine(Rotate(Vector3.down, rotateDegrees, rotateSpeed));
        }
        else return;
    }
    public void RotateBayDown()
    {
        actWeaponBay.activeBay = false;
        curBay = (weaponBay)((int)curBay - 1);
     
        // wrap around if needed
        if (curBay < (weaponBay)(0))
        {
            curBay = weaponBay.COUNT - 1;
            actWeaponBay = weaponBays[(int)curBay].GetComponent<ssWeaponBay>();
        }
        
        // activate the new bay
        actWeaponBay.activeBay = true;
        
        GunneryCall();
        TMPcurWeapon.SetText("Current Weapon: " + curBay.ToString());
        if (isRotating == false)
        {
            StartCoroutine(Rotate(Vector3.up, rotateDegrees, rotateSpeed));
        }
        else return;
    }

    IEnumerator Rotate(Vector3 axis, float angle, float duration)
    {
        Quaternion start = transform.rotation;
        Quaternion end = transform.rotation;
        end *= Quaternion.Euler(axis * angle);

        float clock = 0.0f;
        while (clock < duration)
        {
            transform.rotation = Quaternion.Slerp(start, end, clock / duration);
            isRotating = true;
            clock += Time.deltaTime;
            if (clock >= duration)
            {
                isRotating = false; ;
            }
            yield return null;
        }
        transform.rotation = end;
    }
}

public enum weaponBay
{
    Cannon, // 0
    MachineGun,   // 1
    Missile, // 2

    COUNT, // 3 the maximum number of stuff above
}

//Use events in buttons to call fire and detonate functions