using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class Boid {
    public float separation_radius = 3;
    public float separation = 5;
    public float cohesion = 1;
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
        // The position we've started when we were commanded
        // to move to the target_pos
    Vector3 nav_start_pos = Vector3.zero;
        // How much along our destination are we from start pos
    float nav_completion = 0;
    public bool is_selected = false;
    public Boid boid = new Boid(); // @temp public for testing purposes
    public float speed = 1;

    /// <summary>
    /// Initialise a new ai actor. Each AI actor must share the same blackboard as others.
    /// the "lead" parameter can be null. If it is not null, this ai actor will follow the "lead" around.
    /// </summary>
    public void init(AI_Blackboard blackboard, AI_Actor lead) {
        this.blackboard = blackboard;
        this.lead = lead;
        nav_start_pos  = transform.position;
        nav_target_pos = nav_start_pos;
        nav_completion = 0.0f;

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
        nav_start_pos = transform.position;
        nav_target_pos = position;
        nav_completion = 0.0f;
    }

    /// <summary>
    /// Update the state of this ai actor.
    /// Any movement calculated by the ai gets updated after this procedure is called.
    /// </summary>
    public void update() {
        if (lead) {
            speed = lead.speed;
            //- Follow the lead using BOIDS
            transform.position += boid.get_velocity(lead.transform.position, blackboard.boids) * speed * Time.deltaTime;
        } else {
            //- moved by the player input
            if (nav_completion < 0.9999f) {
                nav_completion += Time.deltaTime;
                transform.position = Vector3.Lerp(nav_start_pos, nav_target_pos, nav_completion);
            }
        }

    }
}
