using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ssProjk : MonoBehaviour
{
    [SerializeField] private bool detonate = false;
    [SerializeField] private float damage;
    [SerializeField] private float spd;
    [SerializeField] private float lifeTime;
    [SerializeField] private float timer;
    [SerializeField] private ssWeaponBay pool;

    private void Start()
    {
        pool = transform.parent.GetComponent<ssWeaponBay>();

        damage = transform.gameObject.GetComponentInParent<ssWeaponBay>().projkDmg;
    }

    private void Update()
    {
        if (detonate == true)
        {
            Explosion();
        }

        transform.Translate(Vector3.forward * spd * Time.deltaTime);
        timer += Time.deltaTime;
        if(timer >= lifeTime)
        {
            detonate = true;
            timer = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            detonate = true;
            Debug.Log("object collid");
        }
    }

    public void Explosion()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        pool.ReturnObject(gameObject);

        Debug.Log(damage);

        ShipHealth shipHealth = gameObject.GetComponent<ShipHealth>();
        shipHealth.Health(damage);
    }
}
