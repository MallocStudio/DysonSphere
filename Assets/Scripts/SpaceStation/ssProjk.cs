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
    [SerializeField] private ParticleSystem explPS;
    [SerializeField] private ssWeaponBay pool;
    [SerializeField] private AI_Actor damageTarget;
    [SerializeField] World world;

    public float radius = 3.0f;

    public void init(World world)
    {
        this.world = world;
    }

    private void Start()
    {
        if (world == null) {
            var objs = FindObjectsOfType<World>();
            this.world = objs[0];
        }
        Debug.Assert(world != null);
        pool = transform.parent.GetComponent<ssWeaponBay>();
        explPS = GetComponent<ParticleSystem>();
    }

    public void evDetonate()
    {
        detonate = true;
        explPS.Play();
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
        damageTarget = other.transform.GetComponent<AI_Actor>();

        //Triggers when the object hit has the ShipHelth.cs
        if (other.gameObject == damageTarget.gameObject)
        {
            detonate = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public void Explosion()
    {
        //Set the visual of the projectile visibility to false
        transform.GetChild(0).gameObject.SetActive(false);
        world.event_damage_enemies_in_radius(transform.position, radius, projDamage);

/*       //Stores all objects caught in the the blast radius of the projectile
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
*/
        detonate = false;
        timer = 0;
        //Send the object back to the pool list
        pool.ReturnObject(gameObject);
        //Sets the visual back to true
        transform.GetChild(0).gameObject.SetActive(true);
    }
}