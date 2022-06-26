using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using System.Linq;

[System.Serializable]
public class PrimaryButtonEvent : UnityEvent<bool> {}

public class Plane {
    public Vector3 normal;
    public float offset;

    public Plane() {
        normal = Vector3.up;
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
    List<HolographicObject> holographic_objects;
    const int ENTITY_MAX = 6;
    [SerializeField] GameObject spaceship_prefab;
    [SerializeField] Camera main_camera;
    [SerializeField] float world_radius = 30.0f;
    [SerializeField] Transform spawn_point;
    [SerializeField] Hologram hologram_panel;

        //- Floor
    Plane floor = new Plane();

    void Start() {
        Debug.Assert(blackboard != null);
        Debug.Assert(main_camera != null);
        Debug.Assert(spaceship_prefab != null);
        Debug.Assert(spawn_point != null);

            //- Generate the Entities
        entities = new List<AI_Actor>(ENTITY_MAX);
        holographic_objects = new List<HolographicObject>(ENTITY_MAX);

        for (int i = 0; i < ENTITY_MAX; i++) {
            Vector3 pos = get_random_position_in_worldspace();
            GameObject gameobject = Instantiate(spaceship_prefab, pos, Quaternion.identity);

                //- Link the created entity to the hologram tabel
            if (hologram_panel != null) {
                holographic_objects.Add(hologram_panel.LinkNewEntity(gameobject.transform));
            } else {
                Debug.LogWarning("hologram_panel on world component is null");
            }

            AI_Actor entity = gameobject.GetComponent<AI_Actor>();

            AI_Actor lead = null;
            if (i > 0) lead = entities[0];
            entity.init(blackboard, lead, pos.y);

            entities.Add(entity);
        }
    }

    void Update() {
        foreach (AI_Actor entity in entities) {
            entity.update();
            Vector3 clamped_pos = entity.transform.position;
            if (clamped_pos.x > spawn_point.position.x + world_radius) clamped_pos.x = spawn_point.position.x + world_radius;
            if (clamped_pos.x < spawn_point.position.x - world_radius) clamped_pos.x = spawn_point.position.x - world_radius;
            if (clamped_pos.y > spawn_point.position.y + world_radius) clamped_pos.y = spawn_point.position.y + world_radius;
            if (clamped_pos.y < spawn_point.position.y - world_radius) clamped_pos.y = spawn_point.position.y - world_radius;
            if (clamped_pos.z > spawn_point.position.z + world_radius) clamped_pos.z = spawn_point.position.z + world_radius;
            if (clamped_pos.z < spawn_point.position.z - world_radius) clamped_pos.z = spawn_point.position.z - world_radius;
            entity.transform.position = clamped_pos;
        }

        foreach (HolographicObject holographic_object in holographic_objects) {
            holographic_object.update(spawn_point.position, world_radius);
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
        result.y = (floor.offset * floor.normal).y + Random.Range(-2.0f, 2.0f);
        result.x = spawn_point.position.x + Random.Range(-world_radius, world_radius);
        result.z = spawn_point.position.z + Random.Range(-world_radius, world_radius);

        return result;
    }
}