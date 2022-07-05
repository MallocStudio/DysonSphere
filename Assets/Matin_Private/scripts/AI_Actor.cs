using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AI_Actor : MonoBehaviour {
    //-      AI BOIDS
        // The data shared between AI_Actors
    public AI_Blackboard blackboard;
    public World world;
    public bool is_dead = false;
        // The leader of this crowd
        // ! Lead can be null. In that case this AI_Actor is the lead
    public AI_Actor lead;
        // The position we're asked to move towards
        // ! If we have a "lead" we don't move towards target_pos
    Vector3 nav_target_pos = Vector3.zero;
    public bool is_selected = false;
    [System.NonSerialized] public Boid boid = new Boid();
    protected float speed = 10;
    [SerializeField] float attack_radius = 15;
    Vector3 velocity = Vector3.zero;
    float starting_y_pos = 0;
    float floatiness_offset = 0;
    public HolographicObject attached_holographic_obj;

    //-     COMBAT
        // ! ONLY MEANT TO BE SET TO SOMETHING THROUGH WORLD.CS
    public float health = 1.0f; // 1 is max, 0 is min // ! note that this gets set to 1 in init()
    public bool is_enemy = false;
    float damage = 0.02f; // the amount of damage this entity applies to others

        //-     LINE RENDERER
    private LineRenderer line_renderer;
    const string line_renderer_visibility_material_name = "Vector1_17cb148168fe458f8bcf66707d08fcfe";
    const float line_renderer_visibility_material_max = 1;
    const float line_renderer_visibility_material_min = 0;
    float line_renderer_visibility_material_amount = 0;

    /// <summary>
    /// Initialise a new ai actor. Each AI actor must share the same blackboard as others.
    /// the "lead" parameter can be null. If it is not null, this ai actor will follow the "lead" around.
    /// </summary>
    public void init(World world, AI_Blackboard blackboard, AI_Actor lead, Vector3 position, bool is_enemy) {
        this.is_dead = false; // ! reset to alive
        this.blackboard = blackboard;
        this.world = world;
        this.lead = lead;
        this.transform.position = position;
        this.starting_y_pos = position.y;
        this.floatiness_offset = Random.Range(-1.0f, 1.0f);
        this.is_enemy = is_enemy;
        this.health = 1; // reset to max
        nav_target_pos = transform.position;

            //- Add this AI_Actor's boid to blackboard
        boid.reset(transform);
        blackboard.boids.Add(boid);
        blackboard.group.Add(this);

        line_renderer = GetComponent<LineRenderer>();
        line_renderer_visibility_material_amount = line_renderer_visibility_material_min;
        line_renderer.material.SetFloat(line_renderer_visibility_material_name, line_renderer_visibility_material_amount);
        line_renderer.enabled = true;
    }

    /// <summary>
    /// Moves this AI_Actor towards the given point.
    /// This point can be anywhere in space, and the AI will navigate its way towards that point.
    /// NOTE that if this ai actor has a lead (some other actor that it must follow)
    /// this procedure will do nothing.
    /// </summary>
    public void move(Vector3 position) {
            // If we have someone else to follow, follow him during update()
        nav_target_pos = position;
        if (lead) lead.nav_target_pos = position;
    }

    /// <summary>
    /// Update the state of this ai actor.
    /// Any movement calculated by the ai gets updated after this procedure is called.
    /// </summary>
    public void update() {
        Vector3 position_previous_frame = transform.position;

        {   //- Check if we're dead or alive
            if (is_dead) {
                set_visibility(false);
                return;
            } else {
                set_visibility(true);
            }

            if (lead && lead.is_dead) {
                this.kill(); // kill yourself you can't even protect your lead.
            }
        }

        {   //- Sync settings with the lead
            if (lead) {
                speed                  = lead.speed;
                boid.separation_radius = lead.boid.separation_radius;
                boid.separation        = lead.boid.separation;
                boid.cohesion          = lead.boid.cohesion;
                boid.target_radius     = lead.boid.target_radius;
                boid.alignment_radius  = lead.boid.alignment_radius;
                boid.alignment         = lead.boid.alignment;
            }
        }

        Vector3 final_velocity = Vector3.zero;
        {   //- Go towards nav_target_pos
            if (lead) {
                nav_target_pos = lead.nav_target_pos;
                velocity = boid.get_velocity(nav_target_pos, blackboard.boids);
            } else {
                velocity = (nav_target_pos - transform.position).normalized;
            }

            final_velocity = velocity;
            transform.position += final_velocity * speed * Time.deltaTime;

            Vector3 final_pos = transform.position;
            final_pos.y = Mathf.Sin(Time.fixedTime + floatiness_offset) + starting_y_pos;
            transform.position = final_pos;
        }

        {   //- Look At Where You're Going
            if (final_velocity.sqrMagnitude < 1) {
            } else {
                Vector3 lookat_pos = transform.position + final_velocity;
                lookat_pos.y = transform.position.y;
                look_at(lookat_pos);
            }
        }

        {   //- Attack enemies
            foreach (AI_Actor enemy in blackboard.enemies) {
                if (enemy.is_dead) continue;
                if (Vector3.Distance(transform.position, enemy.transform.position) < attack_radius) {
                    if (line_renderer_visibility_material_amount <= line_renderer_visibility_material_min) {
                        shoot_at(enemy);
                    }
                }
            }

            if (line_renderer_visibility_material_amount > line_renderer_visibility_material_min) {
                float shoot_speed = Random.Range(1.0f, 3.0f);
                line_renderer_visibility_material_amount -= Time.deltaTime * shoot_speed;
                line_renderer.material.SetFloat(line_renderer_visibility_material_name, line_renderer_visibility_material_amount);
            } else {
                line_renderer_visibility_material_amount = line_renderer_visibility_material_min;
            }
        }
    }

    Vector3 get_random_pos(float amount) {
        Vector3 result;
        result.x = Random.Range(-amount, amount);
        result.y = Random.Range(-amount, amount);
        result.z = Random.Range(-amount, amount);
        return result;
    }

        /// This is called from update() depending on the status of "is_dead"
    void set_visibility(bool visible) {
        line_renderer.enabled = visible;
    }

    public void take_damage(float amount) {
        this.health -= amount;
        if (this.health <= 0) this.kill();
    }

    public void shoot_at(AI_Actor entity) {
            //- Take Damage
        entity.take_damage(this.damage);

            //- Setup Visuals
        line_renderer.SetPosition(0, transform.position);
        line_renderer.SetPosition(1, entity.transform.position + get_random_pos(1));
        line_renderer_visibility_material_amount = line_renderer_visibility_material_max;
        line_renderer.material.SetFloat(line_renderer_visibility_material_name, line_renderer_visibility_material_amount);
    }

    private void look_at(Vector3 pos) {
        // Quaternion rot = transform.rotation;
        // Vector3 look_dir = (transform.position - (pos * 1000)).normalized;
        // if (look_dir.sqrMagnitude < 1) {
        //     return; // don't do shit because what we looking at is outselves
        // }
        // Quaternion lookat_rot = Quaternion.LookRotation(look_dir, Vector3.up);

        // rot = Quaternion.Slerp(rot, lookat_rot, Time.deltaTime * 10);

        // transform.rotation = rot;
        transform.LookAt(pos, Vector3.up);
    }

        /// This is called from shoot_at() when health is less than or equal to zero
    public void kill() {
        health = 0; // for sanity's sake for when we kill this thing outside of shoot_at()
        is_dead = true;
    }
}

[System.Serializable]
public class Boid {
    public float separation_radius = 10;
    public float separation = 0.5f;
    public float cohesion = 1.2f;
    public float target_radius = 15; // the radius around the target
    public float alignment_radius = 3;
    public float alignment = 0.01f;
    public Transform transform; // the parent transform

    protected Vector3 final_velocity = Vector3.zero;
    private Vector3 previous_velocity = Vector3.zero; // used to dampen the fina velocity

    public void reset(Transform transform) {
        this.transform = transform;
        this.previous_velocity = Vector3.zero;
        this.final_velocity = Vector3.zero;
    }

    public Vector3 get_velocity(Vector3 target_pos, List<Boid> boids) {
        Vector3 separation_v = get_separation_velocity(boids);
        Vector3 cohesion_v = get_cohesion_velocity(target_pos);
        Vector3 alignment_v = get_alignment_velocity(boids);

        this.final_velocity = separation_v + alignment_v + cohesion_v; // store for internal use
        this.final_velocity = Vector3.Lerp(this.final_velocity, previous_velocity, 0.99f);
        previous_velocity = this.final_velocity; // update previous vel
        return this.final_velocity;
    }

    private Vector3 get_alignment_velocity(List<Boid> boids) {
        Vector3 velocity = Vector3.zero;
        foreach (Boid boid in boids) {
            if ((boid.transform.position - this.transform.position).sqrMagnitude < (alignment_radius * alignment_radius)) {
                velocity += boid.final_velocity * alignment;
            }
        }
        return velocity;
    }

    private Vector3 get_separation_velocity(List<Boid> boids) {
        Vector3 velocity = Vector3.zero;
        foreach (Boid boid in boids) {
            if (boid.Equals(this)) continue;

            float separation_force = 0;
            float distance_sqr = (transform.position - boid.transform.position).sqrMagnitude;
            if (distance_sqr < (separation_radius * separation_radius)) {
                separation_force = separation;
            }
            velocity += (transform.position - boid.transform.position) * separation_force;
        }
        return velocity;
    }

    private Vector3 get_cohesion_velocity(Vector3 target_pos) {
        Vector3 velocity = Vector3.zero;
        if (Vector3.Distance(target_pos, transform.position) > target_radius) {
            velocity = (target_pos - transform.position).normalized * cohesion;
        }
        return velocity;
    }
}
