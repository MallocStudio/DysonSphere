using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    /// RETURNS WHAT THE PLAYER IS POINTING AT.
    /// USED FOR SELECTING ENTITIES ON THE HOLOGRAM.
public static class Player_Raycaster {
        /// Returns the transform the player is pointing at
    public static bool get_selection(Transform from, out RaycastHit hit) {
        const float max = 100;
        return Physics.Raycast(from.position, from.forward, out hit, max);
    }
}
