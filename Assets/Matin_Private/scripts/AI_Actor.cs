using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AI_Actor : MonoBehaviour {
    //// AI BOIDS
        // The data shared between AI_Actors
    AI_Blackboard blackboard;
        // The leader of this crowd
        // ! Lead can be null. In that case this AI_Actor is the lead
    AI_Actor lead;

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
    }

    /// <summary>
    /// Moves this AI_Actor towards the given point.
    /// This point can be anywhere in space, and the AI will navigate its way towards that point.
    /// NOTE that if this ai actor has a lead (some other actor that it must follow)
    /// this procedure will do nothing.
    /// </summary>
    public void move(Vector3 position) {
            // If we have someone else to follow, follow him during update()
        if (lead != null) return;
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
            //-- Follow the lead using BOIDS
        } else {
            //-- Move towards the given point
            if (nav_completion < 0.9999f) {
                nav_completion += Time.deltaTime;
                transform.position = Vector3.Lerp(nav_start_pos, nav_target_pos, nav_completion);
            }
        }
    }
}
