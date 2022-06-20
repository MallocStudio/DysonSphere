using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.InputSystem;

[System.Serializable]
public class PrimaryButtonEvent : UnityEvent<bool> {}

public class Plane {
    public Vector3 normal;
    public Vector3 tangent;
    public Vector3 bitangent;
    public float offset;

    public Plane() {
        normal = Vector3.up;
        tangent = Vector3.left;
        bitangent = Vector3.forward;
        offset = 0;
    }

    public Vector3 get_pos_on_plane(Ray ray) {
        Vector3 plane_pos  = normal * offset;
        Vector3 ray_origin = ray.origin;
        Vector3 ray_dir    = ray.direction;

        float denom = Vector3.Dot(ray.direction, normal);
        // if (denom <= 1e-6) return plane_pos; // perpendicular

        float distance = Vector3.Dot((plane_pos - ray_origin), normal) / denom;

        Vector3 result = ray_origin + ray.direction * distance;

        return result;
    }
}

public class World : MonoBehaviour {
    const int MOUSE_BUTTON_LEFT   = 0;
    const int MOUSE_BUTTON_RIGHT  = 1;
    const int MOUSE_BUTTON_MIDDLE = 2;

    AI_Blackboard blackboard = new AI_Blackboard();
    List<AI_Actor> entities;
    const int ENTITY_MAX = 6;
    [SerializeField] GameObject spaceship_prefab;
    [SerializeField] Camera main_camera;

        //- Floor
    Plane floor = new Plane();

    void Start() {
        Debug.Assert(blackboard != null);
        Debug.Assert(main_camera != null);
        Debug.Assert(spaceship_prefab != null);

            //- Generate the Entities
        entities = new List<AI_Actor>(ENTITY_MAX);
        for (int i = 0; i < ENTITY_MAX; i++) {
            GameObject gameobject = Instantiate(spaceship_prefab, get_random_position_in_worldspace(), Quaternion.identity);
            AI_Actor entity = gameobject.GetComponent<AI_Actor>();

            AI_Actor lead = null;
            if (i > 0) lead = entities[0];
            entity.init(blackboard, lead);

            entities.Add(entity);
        }
    }

    void Update() {
        foreach (AI_Actor entity in entities) {
            entity.update();
        }

        Mouse mouse = Mouse.current;
        Debug.Assert(mouse != null);

        if (mouse.leftButton.wasPressedThisFrame) {
                //- Player giving Input
            Ray ray = main_camera.ScreenPointToRay(mouse.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)){
                AI_Actor actor = hit.transform.GetComponent<AI_Actor>();
                if (actor) {
                        // unselect other actors
                    foreach (AI_Actor entity in entities) {
                        entity.is_selected = false;
                    }

                        //- Select the Leader of this flock
                    if (actor.lead == null) {
                        actor.is_selected = true;
                    } else {
                        actor.lead.is_selected = true;
                    }
                }
            } else {
                    //- Move Actor
                    // The raycast did not hit any object so just move the actor there
                Vector3 target_pos = floor.get_pos_on_plane(ray);
                for (int i = 0; i < entities.Count; i++) {
                    if (entities[i].is_selected) {
                        entities[i].move(target_pos);
                    }
                }
            }
        }
    }

    Vector3 get_random_position_in_worldspace() {
        Vector3 result;
        result.y = (floor.offset * floor.normal).y;
        result.x = Random.Range(-10, 10);
        result.z = Random.Range(-10, 10);
        return result;
    }
}