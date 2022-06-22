using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ssProjk : MonoBehaviour
{
    [SerializeField] private float spd;
    [SerializeField] private float lifeTime;
    [SerializeField] private float timer;
    [SerializeField] private ssWeaponBay pool;

    private void Start()
    {
        pool = transform.parent.GetComponent<ssWeaponBay>();
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * spd * Time.deltaTime);
        timer += Time.deltaTime;
        if(timer >= lifeTime)
        {
            pool.ReturnObject(gameObject);
            timer = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        pool.ReturnObject(gameObject);
    }
}
