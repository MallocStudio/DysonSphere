using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class Boid {
    public float separation_radius = 3;
    public float separation = 5;
    public float cohesion = 5;
    public float target_radius = 3;
    public Transform transform; // the parent transform

    public Vector3 get_velocity(Vector3 target_pos, List<Boid> boids) {
        Vector3 separation_v = get_separation_velocity(boids);
        Vector3 cohesion_v = get_cohesion_velocity(target_pos);

        return separation_v + cohesion_v;
    }

    private Vector3 get_separation_velocity(List<Boid> boids) {
        Vector3 velocity = Vector3.zero;
        foreach (Boid boid in boids) {
            if (boid.Equals(this)) continue;

            // float separation_force = 100;
            // float distance = (transform.position - boid.transform.position).magnitude;
            // if (distance != 0) {
            //     separation_force = separation_radius / distance;
            //     if (separation_force > 1) separation_force = 1;
            // }
            // velocity += (transform.position - boid.transform.position) * separation * separation_force;

            float separation_force = 0;
            float distance = (transform.position - boid.transform.position).magnitude;
            if (distance < separation_radius) {
                separation_force = separation;
            }
            velocity += (transform.position - boid.transform.position) * separation_force;
        }
        return velocity;
    }

    private Vector3 get_cohesion_velocity(Vector3 target_pos) {
        Vector3 velocity = Vector3.zero;
        if ((target_pos - transform.position).magnitude > target_radius) {
            velocity = (target_pos - transform.position).normalized * cohesion;
        }
        return velocity;
    }
}

public class AI_Actor : MonoBehaviour {
    //// AI BOIDS
        // The data shared between AI_Actors
    AI_Blackboard blackboard;
        // The leader of this crowd
        // ! Lead can be null. In that case this AI_Actor is the lead
    public AI_Actor lead;

    //// NAVIGATION
        // The position we're asked to move towards
        // ! If we have a "lead" we don't move towards target_pos
    Vector3 nav_target_pos = Vector3.zero;
    public bool is_selected = false;

    Boid boid = new Boid();

    [SerializeField] protected float speed = 3;
    Vector3 velocity = Vector3.zero;
    float starting_y_pos = 0;
    float floatiness_offset = 0;

    /// <summary>
    /// Initialise a new ai actor. Each AI actor must share the same blackboard as others.
    /// the "lead" parameter can be null. If it is not null, this ai actor will follow the "lead" around.
    /// </summary>
    public void init(AI_Blackboard blackboard, AI_Actor lead, float starting_y_pos) {
        this.blackboard = blackboard;
        this.lead = lead;
        this.starting_y_pos = starting_y_pos;
        this.floatiness_offset = Random.Range(0.0f, 1.0f);
        nav_target_pos = transform.position;

            //- Add this AI_Actor's boid to blackboard
        boid.transform = transform;
        blackboard.boids.Add(boid);
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
        float acceptable_destination_radius = 0.1f;

        if (lead) {
                //- Follow the lead using BOIDS
            speed = lead.speed;
            nav_target_pos = lead.transform.position;
            acceptable_destination_radius = boid.separation_radius;

                // Mix velocity with the velocity of previous frame to make it smoother
            velocity = boid.get_velocity(nav_target_pos, blackboard.boids);
            nav_target_pos = transform.position + velocity;
            // if (Vector3.Distance(transform.position, nav_target_pos) > acceptable_destination_radius) {
                // transform.position += velocity * speed * Time.deltaTime;
            // }

        } else {
                // We don't use boids. Go towards nav_target_pos
        }

        velocity = (nav_target_pos - transform.position).normalized;
        if (Vector3.Distance(transform.position, nav_target_pos) > acceptable_destination_radius) {
            transform.position += velocity * speed * Time.deltaTime;
        }

        Vector3 final_position = transform.position;
        final_position.y = Mathf.Sin(Time.fixedTime + floatiness_offset) + starting_y_pos;
        transform.position = final_position;
    }
}
