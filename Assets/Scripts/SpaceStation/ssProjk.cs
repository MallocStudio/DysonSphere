using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ssProjk : MonoBehaviour
{
    [SerializeField] private bool detonate = false;
    [SerializeField] private float missileDamage;
    [SerializeField] private float spd;
    [SerializeField] private float lifeTime;
    [SerializeField] private float timer;
    [SerializeField] private ssWeaponBay pool;

    private ShipHealth damageTarget;

    private void Start()
    {
        pool = transform.parent.parent.GetComponent<ssWeaponBay>();

        //damage = transform.gameObject.GetComponentInParent<ssWeaponBay>().projkDmg;
        missileDamage = transform.parent.transform.gameObject.GetComponentInParent<ssWeaponBay>().projkDmg;
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
            //Debug.Log("object collid");
        }
    }

    public void Explosion()
    {
        transform.GetChild(0).gameObject.SetActive(false);

        Collider[] affectedColliders = Physics.OverlapSphere(transform.position, 3.5f);

        for(int i = 0; i < affectedColliders.Length - 1; i++)
        {
            ShipHealth thisShip = affectedColliders[i].gameObject.GetComponent<ShipHealth>();
            thisShip.TakeDamage(missileDamage);
            Debug.Log(missileDamage);

            if (i == affectedColliders.Length - 1)
            {
                break;
            }
        }

        //Debug.Log("break");

        //Debug.Log(missileDamage);
        //
        //ShipHealth shipHealth = this.gameObject.GetComponent<ShipHealth>();
        //shipHealth.TakeDamage(missileDamage);

        //pool.ReturnObject(gameObject);
    }
}
