using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ssMissileTube : MonoBehaviour
{
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
