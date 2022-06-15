using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Actor : MonoBehaviour {
    //// AI BOIDS
        // The data shared between AI_Actors
    AI_Blackboard blackboard;
        // the leader of this crowd
        // ! Lead can be null. In that case this AI_Actor is the lead
    AI_Actor lead;

    //// NAVIGATION
        // the position we're asked to move towards
        // ! if we have a "lead" we don't move towards target_pos
    Vector3 target_pos;

    /// <summary>
    /// Initialise a new ai actor. Each AI actor must share the same blackboard as others.
    /// the "lead" parameter can be null. If it is not null, this ai actor will follow the "lead" around.
    /// </summary>
    public AI_Actor(AI_Blackboard blackboard, AI_Actor lead) {
        this.blackboard = blackboard;
        this.lead = lead;
        lead.move(new Vector3(0,0,0));
    }

    /// <summary>
    /// Moves this AI_Actor towards the given point.
    /// This point can be anywhere in space, and the AI will navigate its way towards that point.
    /// NOTE that if this ai actor has a lead (some other actor that it must follow)
    /// this procedure will do nothing.
    /// </summary>
    public void move(Vector3 position) {
            // if we have someone else to follow, follow him during update()
        if (lead != null) return;

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

        }
    }

}
