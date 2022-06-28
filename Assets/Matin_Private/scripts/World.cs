using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;

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

    AI_Blackboard enemy_blackboard = new AI_Blackboard();
    AI_Blackboard friendly_blackboard = new AI_Blackboard();
    const int GROUP_MAX_CAPACITY = 6;
    List<AI_Actor> enemy_entities;
    List<AI_Actor> friendly_entities;

    List<HolographicObject> holographic_objects;
    [SerializeField] GameObject enemy_spaceship_prefab;
    [SerializeField] GameObject friendly_spaceship_prefab;
    [SerializeField] Camera main_camera;
    [SerializeField] float world_radius = 30.0f;
    [SerializeField] Transform spawn_point;
    [SerializeField] Hologram hologram_panel;
    [SerializeField] Transform left_hand;

        //- Floor
    Plane floor = new Plane();

    void Start() {
        Debug.Assert(enemy_blackboard != null);
        Debug.Assert(main_camera != null);
        Debug.Assert(enemy_spaceship_prefab != null);
        Debug.Assert(friendly_spaceship_prefab != null);
        Debug.Assert(spawn_point != null);
        Debug.Assert(left_hand != null);

            //- Generate the Entities
        enemy_entities = new List<AI_Actor>(GROUP_MAX_CAPACITY);
        friendly_entities = new List<AI_Actor>(GROUP_MAX_CAPACITY);
        holographic_objects = new List<HolographicObject>();

            //- Generate Enemies
        for (int i = 0; i < GROUP_MAX_CAPACITY; i++) {
            Vector3 pos = get_random_position_in_worldspace();
            GameObject gameobject = Instantiate(enemy_spaceship_prefab, pos, Quaternion.identity);

                //- Link the created entity to the hologram tabel
            if (hologram_panel != null) {
                holographic_objects.Add(hologram_panel.LinkNewEntity(gameobject.transform));
            } else {
                Debug.LogWarning("hologram_panel on world component is null");
            }

            AI_Actor entity = gameobject.GetComponent<AI_Actor>();

            AI_Actor lead = null;
            if (i > 0) lead = enemy_entities[0];
            entity.init(enemy_blackboard, lead, pos.y);

            enemy_entities.Add(entity);
        }

            //- Generate Friendly Ships
        for (int i = 0; i < GROUP_MAX_CAPACITY; i++) {
            Vector3 pos = get_random_position_in_worldspace();
            GameObject gameobject = Instantiate(friendly_spaceship_prefab, pos, Quaternion.identity);

                //- Link the created entity to the hologram tabel
            if (hologram_panel != null) {
                holographic_objects.Add(hologram_panel.LinkNewEntity(gameobject.transform));
            } else {
                Debug.LogWarning("hologram_panel on world component is null");
            }

            AI_Actor entity = gameobject.GetComponent<AI_Actor>();

            AI_Actor lead = null;
            if (i > 0) lead = friendly_entities[0];
            entity.init(friendly_blackboard, lead, pos.y);

            friendly_entities.Add(entity);
        }

            //- Setup Teams
        foreach (AI_Actor entity_1 in enemy_entities) {
            foreach(AI_Actor entity_2 in friendly_entities) {
                entity_1.blackboard.enemies.Add(entity_2);
                entity_2.blackboard.enemies.Add(entity_1);
            }
        }
    }

    public void input_select() {
        if (Player_Raycaster.get_selection(left_hand, out RaycastHit hit)) {
            HolographicObject selection_holographic_obj = hit.transform.GetComponent<HolographicObject>();
            if (selection_holographic_obj != null) {
                AI_Actor actor = selection_holographic_obj.select().GetComponent<AI_Actor>();
                if (actor != null) {
                    // unselect other actors
                    foreach (AI_Actor entity in enemy_entities) {
                        entity.is_selected = false;
                    }

                        //- Select the Leader of this flock
                    if (actor.lead == null) {
                        actor.is_selected = true;
                    } else {
                        actor.lead.is_selected = true;
                    }
                }
            }
        } else {

        }
    }

    void Update() {
        foreach (AI_Actor entity in enemy_entities) {
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
        Keyboard keyboard = Keyboard.current;
        // XRController controller = XRController.current;

        Debug.Assert(mouse != null);

        if (mouse.leftButton.wasPressedThisFrame) {
                //- Player giving Input
            Ray ray = main_camera.ScreenPointToRay(mouse.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)){
                AI_Actor actor = hit.transform.GetComponent<AI_Actor>();
                if (actor) {
                        // unselect other actors
                    foreach (AI_Actor entity in enemy_entities) {
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
                for (int i = 0; i < enemy_entities.Count; i++) {
                    if (enemy_entities[i].is_selected) {
                        enemy_entities[i].move(target_pos);
                    }
                }
            }
        }

            //@debug shoot
        if (keyboard.spaceKey.wasPressedThisFrame) {
            foreach (AI_Actor entity in enemy_entities) {
                if (entity.lead != null) {
                    entity.shoot_at(entity.lead);
                }
            }
            // enemy_entities[1].shoot_at(enemy_entities[1].lead);
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