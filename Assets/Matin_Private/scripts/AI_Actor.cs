using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class Boid {
    public float separation_radius = 3;
    public float separation = 0.5f;
    public float cohesion = 1;
    public float target_radius = 10; // the radius around the target
    public float alignment_radius = 2;
    public float alignment = 0.01f;
    public Transform transform; // the parent transform

    protected Vector3 final_velocity = Vector3.zero;

    public Vector3 get_velocity(Vector3 target_pos, List<Boid> boids) {
        Vector3 separation_v = get_separation_velocity(boids);
        Vector3 cohesion_v = get_cohesion_velocity(target_pos);
        Vector3 alignment_v = get_alignment_velocity(boids);

        this.final_velocity = separation_v + alignment_v + cohesion_v; // store for internal use
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

public class AI_Actor : MonoBehaviour {
    //-      AI BOIDS
        // The data shared between AI_Actors
    public AI_Blackboard blackboard;
        // The leader of this crowd
        // ! Lead can be null. In that case this AI_Actor is the lead
    public AI_Actor lead;
        // The position we're asked to move towards
        // ! If we have a "lead" we don't move towards target_pos
    Vector3 nav_target_pos = Vector3.zero;
    public bool is_selected = false;
    Boid boid = new Boid();
    protected float speed = 10;
    [SerializeField] float attack_radius = 15;
    Vector3 velocity = Vector3.zero;
    float starting_y_pos = 0;
    float floatiness_offset = 0;

    //-     COMBAT
    float health = 1.0f; // 1 is max, 0 is min
    float damage = 0.4f; // the amount of damage this entity applies to others

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
    public void init(AI_Blackboard blackboard, AI_Actor lead, float starting_y_pos) {
        this.blackboard = blackboard;
        this.lead = lead;
        this.starting_y_pos = starting_y_pos;
        this.floatiness_offset = Random.Range(-1.0f, 1.0f);
        nav_target_pos = transform.position;

            //- Add this AI_Actor's boid to blackboard
        boid.transform = transform;
        blackboard.boids.Add(boid);

        line_renderer = GetComponent<LineRenderer>();
        line_renderer_visibility_material_amount = line_renderer_visibility_material_min;
        line_renderer.material.SetFloat(line_renderer_visibility_material_name, line_renderer_visibility_material_amount);
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
    }

    /// <summary>
    /// Update the state of this ai actor.
    /// Any movement calculated by the ai gets updated after this procedure is called.
    /// </summary>
    public void update() {
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

        //- Go towards nav_target_pos
        if (lead) {
            nav_target_pos = lead.transform.position;
            velocity = boid.get_velocity(nav_target_pos, blackboard.boids);
        } else {
            velocity = (nav_target_pos - transform.position).normalized;
        }

        Vector3 final_velocity = velocity;

        transform.position += final_velocity * speed * Time.deltaTime;

        Vector3 final_pos = transform.position;
        final_pos.y = Mathf.Sin(Time.fixedTime + floatiness_offset) + starting_y_pos;
        transform.position = final_pos;

        {   //- Look At Where You're Going
            if (lead) {
                if (Vector3.Distance(transform.position, lead.transform.position) < boid.target_radius) {
                    // transform.rotation = lead.transform.rotation;
                    // look_at(lead.transform.position + lead.transform.forward);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lead.transform.rotation, Time.deltaTime);
                }
            }
            Vector3 lookat_pos = final_velocity + transform.position;
            lookat_pos.y = transform.position.y;
            look_at(lookat_pos);
        }

        {   //- Attack enemies
            foreach (AI_Actor enemy in blackboard.enemies) {
                if (Vector3.Distance(transform.position, enemy.transform.position) < attack_radius) {
                    // look_at(enemy.transform.position); //@incomplete instead shoot at the enemy. Transform should be replaced with SpaceShip
                    if (line_renderer_visibility_material_amount <= line_renderer_visibility_material_min) {
                        shoot_at(enemy);
                    }
                }
            }

            if (line_renderer_visibility_material_amount > line_renderer_visibility_material_min) {
                const float shoot_speed = 2;
                line_renderer_visibility_material_amount -= Time.deltaTime * shoot_speed;
                line_renderer.material.SetFloat(line_renderer_visibility_material_name, line_renderer_visibility_material_amount);
            } else {
                line_renderer_visibility_material_amount = line_renderer_visibility_material_min;
            }
        }
    }

    public void shoot_at(AI_Actor entity) {
            //- Take Damage
        entity.health -= damage;
        if (entity.health <= 0) entity.kill();

            //- Setup Visuals
        line_renderer.SetPosition(0, transform.position);
        line_renderer.SetPosition(1, entity.transform.position);
        line_renderer_visibility_material_amount = line_renderer_visibility_material_max;
        line_renderer.material.SetFloat(line_renderer_visibility_material_name, line_renderer_visibility_material_amount);
    }

    private void look_at(Vector3 pos) {
        // Quaternion rot = transform.rotation;
        // Quaternion lookat_rot = Quaternion.LookRotation((pos - transform.position).normalized, Vector3.up);

        // rot = Quaternion.Slerp(rot, lookat_rot, Time.deltaTime * 10);

        // transform.rotation = rot;
        transform.LookAt(pos, Vector3.up);
    }

        /// This is called from shoot_at() when health is less than or equal to zero
    public void kill() {
        health = 0; // for sanity's sake for when we kill this thing outside of shoot_at()
        // ...
    }
}
