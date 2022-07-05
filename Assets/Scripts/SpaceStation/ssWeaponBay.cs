using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ssWeaponBay : MonoBehaviour
{
    [Header("Plugins")]
    [SerializeField] public bool activeBay;
    [SerializeField] private bool triggerDown;
    [SerializeField] private weaponBay weapon;
    [SerializeField] private ssManager ss;
    [SerializeField] private TMP_Text TMPweaponTimer;
    [SerializeField] private List<Transform> weaponList = new List<Transform>();
    [SerializeField] private List<ParticleSystem> particleList;
    [SerializeField] private List<AudioSource> audioList;

    [Header("Combat Stats")]
    [SerializeField] public bool reloading;
    [SerializeField] public bool gunModeAct;
    [SerializeField] private bool isMissile;
    [SerializeField] private float shootTimer;
    [SerializeField] private float shootTimerMax;
    [SerializeField] private float fireReady;
    [SerializeField] private float fireReadyMax;
    [SerializeField] private float reloadTime;
    [SerializeField] private float reloadTimeMax;
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
        activeBay = false;
        triggerDown = false;
        weaponIndex = 0;
        ss = GetComponentInParent<ssManager>();
        foreach (Transform weapon in weaponList)
        {
            particleList.Add(weapon.GetComponent<ParticleSystem>());
            audioList.Add(weapon.GetComponent<AudioSource>());
        }
        TMPweaponTimer.SetText("CURRENT WEAPON: " + weapon.ToString());
    }

    private void Update()
    {
        gunModeAct = ss.gunMode;

        if (gunModeAct)
        {
            Aim();
        }
        if(fireReady >= fireReadyMax)
        {
            reloading = true;
            reloadTime -= Time.deltaTime;
            if(reloadTime <= 0)
            {
                reloading = false;
                fireReady = 0;
                reloadTime = reloadTimeMax;
            }
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

    private void LateUpdate()
    {
        if (!reloading)
            TMPweaponTimer.SetText(weapon.ToString() + ": READY FIRE");
        else
            TMPweaponTimer.SetText(weapon.ToString() + ": RELOADING: " + Mathf.Floor(reloadTime));
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
            if (weapon == weaponBay.Cannon)
            {
                foreach (Transform battery in weaponList)
                {
                    battery.LookAt(ss.cannonAimRet);
                }
            }
            else if (weapon == weaponBay.MachineGun)
            {
                foreach (Transform battery in weaponList)
                {
                    battery.LookAt(ss.mgunAimRet);
                }
            }
            else if (weapon == weaponBay.Missile)
            {
                foreach (Transform battery in weaponList)
                {
                    battery.LookAt(ss.missileAimRet);
                }
            }
            else return;
    }

    public void Fire()
    {
        
        if (activeBay && gunModeAct && !isMissile && !reloading)
        {
            fireReady += Time.deltaTime;
            particleList[weaponIndex].Play();
            audioList[weaponIndex].Play();
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
    public void MissileLoad(int tubeIndex)
    {
        /* INSTANTIATE PROJK */
        //  GameObject g = GetObject();
        GameObject g = weaponList[tubeIndex].gameObject;
        g.transform.position = weaponList[tubeIndex].position;
        g.transform.rotation = weaponList[tubeIndex].rotation;
        /* END INSTANTIATE */
    }

    public void MissileFire(int tubeIndex)
    {
        if (activeBay && isMissile )
        {

            particleList[tubeIndex].Play();
            audioList[tubeIndex].Play();

            GameObject g = weaponList[tubeIndex].gameObject;
            g.SetActive(true);

        }
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