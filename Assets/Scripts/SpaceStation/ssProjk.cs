using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ssProjk : MonoBehaviour
{
    [SerializeField] private bool detonate;
    [SerializeField] private float projDamage;
    [SerializeField] private float spd;
    [SerializeField] private float lifeTime;
    [SerializeField] private float timer;
    [SerializeField] private ssWeaponBay pool;
    [SerializeField] private ShipHealth damageTarget;


    private void Start()
    {
        pool = transform.parent.GetComponent<ssWeaponBay>();
        
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
        damageTarget = other.transform.GetComponent<ShipHealth>();

        //Triggers when the object hit has the ShipHelth.cs
        if (other.gameObject == damageTarget.gameObject)
        {
            detonate = true;
        }
    }

    public void Explosion()
    {
        //Set the visual of the projectile visibility to false
        transform.GetChild(0).gameObject.SetActive(false);
        SphereCollider sphereCollider = gameObject.GetComponent<SphereCollider>();

        //Stores all objects caught in the the blast radius of the projectile
        Collider[] affectedColliders = Physics.OverlapSphere(transform.position, sphereCollider.radius);

        //Run a loop of all objects in the collision
        for(int i = 0; i < affectedColliders.Length - 1; i++)
        {
            //Gets the components of the object and passes the damage of the projectile
            //into the object health
            ShipHealth thisShip = affectedColliders[i].gameObject.GetComponent<ShipHealth>();
            thisShip.TakeDamage(projDamage);
            //Break loop after all affected objects have been called
            if (i == affectedColliders.Length - 1)
            {
                break;
            }
        }
        detonate = false;
        timer = 0;
        //Send the object back to the pool list
        pool.ReturnObject(gameObject);
        //Sets the visual back to true
        transform.GetChild(0).gameObject.SetActive(true);
    }
}