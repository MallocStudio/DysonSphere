using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ssProjkAddIns : MonoBehaviour
{
    [SerializeField] private bool detonate;
    [SerializeField] private float projDamage;
    [SerializeField] private float spd;
    [SerializeField] private float lifeTime;
    [SerializeField] private float timer;
    [SerializeField] private ParticleSystem explPS;
    [SerializeField] private ssWeaponBay pool;
    [SerializeField] private AI_Actor damageTarget;
    World world;

    public void init(World world)
    {
        this.world = world;
    }

    private void Start()
    {
        pool = transform.parent.GetComponent<ssWeaponBay>();
        explPS = GetComponent<ParticleSystem>();

    }

    private void Update()
    {
        if (detonate == true)
        {
            Explosion();
        }

        transform.Translate(Vector3.forward * spd * Time.deltaTime);
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            detonate = true;
            timer = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        damageTarget = other.transform.GetComponent<AI_Actor>();

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
        world.event_damage_enemies_in_radius(transform.position, sphereCollider.radius, 0.5f);
        detonate = false;
        timer = 0;
        //Send the object back to the pool list
        pool.ReturnObject(gameObject);
        //Sets the visual back to true
        transform.GetChild(0).gameObject.SetActive(true);
    }
}