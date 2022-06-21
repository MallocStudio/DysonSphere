using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ssWeaponBay : MonoBehaviour
{
    [Header("Plugins")]
    [SerializeField] private bool curBay;
    [SerializeField] private bool triggerDown;
    [SerializeField] private Transform projk;
    [SerializeField] private weaponBay weapon;
    [SerializeField] private ssManager ss;
    [SerializeField] private List<Transform> weaponList = new List<Transform>();

    [Header("Combat Stats")]
    [SerializeField] private float projkDmg;
    [SerializeField] private float projkSpd;
    [SerializeField] private float shootTimer;
    [SerializeField] private float shootTimerMax;

    private void Awake()
    {
        curBay = false;
        triggerDown = false;
        ss = GetComponentInParent<ssManager>();

    }

    private void Update()
    {
        if (ss.gunneryMode == true)
        {
            Aim();
        }

        if (triggerDown)
        {
            if (shootTimer <= 0)
            {
                Fire();
                shootTimer = shootTimerMax;
            }
            else
            {
                shootTimer -= Time.deltaTime;
            }
        }
    }

    public void TriggerOn()
    {
        triggerDown = true;
    }

    public void TriggerOff()
    {
        shootTimer = 0;
        triggerDown = false;
    }

    private void Aim()
    {
        if (ss.gunneryMode)
        {
            if (weapon == weaponBay.cannon)
            {
                foreach (Transform battery in weaponList)
                {
                    battery.LookAt(ss.cannonAimRet);
                }
            }
            else if (weapon == weaponBay.mgun)
            {
                foreach (Transform battery in weaponList)
                {
                    battery.LookAt(ss.mgunAimRet);
                }
            }
            else if (weapon == weaponBay.missile)
            {
                foreach (Transform battery in weaponList)
                {
                    battery.LookAt(ss.missileAimRet);
                }
            }
            else return;
        }
        else return;
    }

    public void Fire()
    {
        if (curBay == true)
        {
            for (int i = 0; i < weaponList.Count; i++)
            {
                Instantiate(projk, weaponList[i].position, weaponList[i].rotation);
            }
        }
        else return;
    }

}