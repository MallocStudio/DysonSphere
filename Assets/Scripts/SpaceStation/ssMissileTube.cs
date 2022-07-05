using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ssMissileTube : MonoBehaviour
{
    [Header("Projk Pool")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize;
    [SerializeField] private bool expandable;

    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioSource fireAS;

    [SerializeField] private List<ssProjk> projkList;

    [SerializeField] private List<GameObject> freeList;
    [SerializeField] private List<GameObject> usedList;

    private void Awake()
    {
        muzzleFlash = GetComponent<ParticleSystem>();
        fireAS = GetComponent<AudioSource>();
    }

    private void Start()
    {
        /* OBJECT POOL */
        freeList = new List<GameObject>();
        usedList = new List<GameObject>();
        projkList = new List<ssProjk>();

        for (int i = 0; i < poolSize; i++)
        {
            GenerateNewObject();
        }
    }

    public void DetonateActive()
    {
        foreach(ssProjk projk in projkList)
        {
            projk.evDetonate();
        }
    }

    public void MissileLoad()
    {
        /* INSTANTIATE PROJK */
        GameObject g = GetObject();
        g.transform.position = transform.position;
        g.transform.rotation = transform.rotation;
        /* END INSTANTIATE */
    }

    public void MissileFire()
    {
        muzzleFlash.Play();
        fireAS.Play();

        GameObject g = usedList[0];
        g.SetActive(true);
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
        projkList.Add(g.GetComponent<ssProjk>());
        return g;
    }

    public void ReturnObject(GameObject obj)
    {
        Debug.Assert(usedList.Contains(obj));
        obj.SetActive(false);
        usedList.Remove(obj);
        projkList.Add(obj.GetComponent<ssProjk>());
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
