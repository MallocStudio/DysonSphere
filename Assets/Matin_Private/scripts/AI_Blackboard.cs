using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Blackboard {
        /// The list of boids found in friendlies of this group
    public List<Boid> boids = new List<Boid>();
        /// The list of enemies of this group
    public List<AI_Actor> enemies = new List<AI_Actor>();
        /// The list of friendlies of this group
    public List<AI_Actor> group = new List<AI_Actor>();
}
