using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ssWeaponBay : MonoBehaviour
{
    [Header("Plugins")]
    [SerializeField] private bool curBay;
    [SerializeField] private bool triggerDown;
    [SerializeField] private weaponBay weapon;
    [SerializeField] private ssManager ss;
    [SerializeField] private List<Transform> weaponList = new List<Transform>();
    [SerializeField] private List<ParticleSystem> particleList;

    [Header("Combat Stats")]
    [SerializeField] private float projkDmg;
    [SerializeField] private float projkSpd;
    [SerializeField] private float shootTimer;
    [SerializeField] private float shootTimerMax;
    [SerializeField] private int weaponIndex;

    [Header("Projk Pool")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize;
    [SerializeField] private bool expandable;

    [SerializeField] private List<GameObject> freeList;
    [SerializeField] private List<GameObject> usedList;

    private void Awake()
    {
        /* OBJECT POOL */
        freeList = new List<GameObject>();
        usedList = new List<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GenerateNewObject();
        }

        /* DEFAULTS */
        curBay = false;
        triggerDown = false;
        weaponIndex = 0;
        ss = GetComponentInParent<ssManager>();
        foreach (Transform weapon in weaponList)
        {
            particleList.Add(weapon.GetComponent<ParticleSystem>());
        }
    }

    private void Update()
    {
        if (ss.gunMode == true)
        {
            Aim();
        }
        if (triggerDown)
        {
            if (shootTimer <= 0)
            {
                shootTimer = shootTimerMax;
                Fire();
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
        if (ss.gunMode)
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
        if (curBay)
        {
            particleList[weaponIndex].Play();
            weaponIndex++;
            if (weaponIndex >= weaponList.Count)
                weaponIndex = 0;
            /* INSTANTIATE PROJK */
            GameObject g = GetObject();
            g.transform.position = weaponList[weaponIndex].position;
            g.transform.rotation = weaponList[weaponIndex].rotation;
            g.SetActive(true);
            /* END INSTANTIATE */

        }
        else return;
    }

    /* OBJECT POOLING */
    public GameObject GetObject()
    {
        int totalFree = freeList.Count;

        if (totalFree == 0 && !expandable)
            return null;
        else if (totalFree == 0)
            GenerateNewObject();

        //Grabs bottom object in list
        GameObject g = freeList[totalFree - 1];
        freeList.RemoveAt(totalFree - 1);
        usedList.Add(g);
        return g;
    }

    public void ReturnObject(GameObject obj)
    {
        Debug.Assert(usedList.Contains(obj));
        obj.SetActive(false);
        usedList.Remove(obj);
        freeList.Add(obj);
    }

    private void GenerateNewObject()
    {
        GameObject g = Instantiate(prefab);
        g.transform.parent = transform;
        g.SetActive(false);
        freeList.Add(g);
    }
}