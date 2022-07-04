using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player_Hand : MonoBehaviour {

        /// Run a physics sphere collision check at transform position and return the first HolographicObject
        /// we're colliding with.
    public HolographicObject select() {
        SphereCollider collider = gameObject.GetComponent<SphereCollider>();
        Collider[] collisions = Physics.OverlapSphere(transform.position, collider.radius);

        for (int i = 0; i < collisions.Count(); i++) {
            HolographicObject obj = collisions[i].gameObject.GetComponent<HolographicObject>();
            if (obj) {
                return obj;
            }
        }

        return null;
    }
}
