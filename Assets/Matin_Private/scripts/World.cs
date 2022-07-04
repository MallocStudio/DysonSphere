using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;

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
    float spawn_radius = 10.0f;
    [SerializeField] Transform enemy_spawn_point;
    [SerializeField] public Transform friendly_spawn_point;
    [SerializeField] Hologram hologram_panel;

///
/// INPUT
///

    [SerializeField] Player_Hand player_hand;
    [SerializeField] InputActionReference input_action_select;

        //- Floor
    Plane floor = new Plane();

    void Start() {
        Debug.Assert(enemy_blackboard != null);
        Debug.Assert(main_camera != null);
        Debug.Assert(enemy_spaceship_prefab != null);
        Debug.Assert(friendly_spaceship_prefab != null);
        Debug.Assert(enemy_spawn_point != null);
        Debug.Assert(friendly_spawn_point != null);
        Debug.Assert(player_hand != null);

            //- Input Action
        input_action_select.action.performed += on_input_select;
        input_action_select.action.canceled  += on_input_unselect;

            //- Generate the Entities
        enemy_entities = new List<AI_Actor>(GROUP_MAX_CAPACITY);
        friendly_entities = new List<AI_Actor>(GROUP_MAX_CAPACITY);
        holographic_objects = new List<HolographicObject>();

            //- Generate Enemies
        for (int i = 0; i < GROUP_MAX_CAPACITY; i++) {
                // even though we set the position here, it'll get reset in entity.init()
            Vector3 pos = get_random_position_in_worldspace(enemy_spawn_point.position);
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
            // entity.is_dead = true; // start dead until we start a new wave

            // Vector3 pos = get_random_position_in_worldspace(enemy_spawn_point.position);
                // Makes enemies undead
            entity.init(this, enemy_blackboard, lead, pos, true);
            enemy_entities.Add(entity);
        }

            //- Generate Friendly Ships
        for (int i = 0; i < GROUP_MAX_CAPACITY; i++) {
                // even though we set the position here, it'll get reset in entity.init()
            Vector3 pos = get_random_position_in_worldspace(friendly_spawn_point.position);
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
            entity.init(this, friendly_blackboard, lead, pos, false);

            friendly_entities.Add(entity);
        }

            //- Setup Teams
        foreach (AI_Actor entity in enemy_entities) {
            friendly_blackboard.enemies.Add(entity);
        }
        foreach(AI_Actor entity in friendly_entities) {
            enemy_blackboard.enemies.Add(entity);
        }

            //- Reset alls enemies and prepares everything for a new wave
        event_start_new_wave();
    }

        //! We're no longer using this function
    // public void input_select() {
    //     if (Player_Raycaster.get_selection(player_hand.transform, out RaycastHit hit)) {
    //         HolographicObject selection_holographic_obj = hit.transform.GetComponent<HolographicObject>();
    //         if (selection_holographic_obj != null) {
    //             event_select_holographic_object(selection_holographic_obj);
    //         }
    //     } else {

    //     }
    // }

    void FixedUpdate() {
        // ! We used to clamp entities within the world_radius. But let's not do that and instead not
        // ! allow the player to move anything outside of the given coords of the hologram table.
        // ! Also, we're now using spawn_radius to determine how far from the spawn_point entities can spawn.
        // ! Note that enemy and friendly entities have separate spawn points - Matin
        // foreach (AI_Actor entity in enemy_entities) {
        //     entity.update();
        //     Vector3 clamped_pos = entity.transform.position;
        //     if (clamped_pos.x > spawn_point.position.x + spawn_radius) clamped_pos.x = spawn_point.position.x + spawn_radius;
        //     if (clamped_pos.x < spawn_point.position.x - spawn_radius) clamped_pos.x = spawn_point.position.x - spawn_radius;
        //     if (clamped_pos.y > spawn_point.position.y + spawn_radius) clamped_pos.y = spawn_point.position.y + spawn_radius;
        //     if (clamped_pos.y < spawn_point.position.y - spawn_radius) clamped_pos.y = spawn_point.position.y - spawn_radius;
        //     if (clamped_pos.z > spawn_point.position.z + spawn_radius) clamped_pos.z = spawn_point.position.z + spawn_radius;
        //     if (clamped_pos.z < spawn_point.position.z - spawn_radius) clamped_pos.z = spawn_point.position.z - spawn_radius;
        //     entity.transform.position = clamped_pos;
        // }

            //- Update Enemies
        bool are_all_enemies_dead = true;
        bool are_all_friendlies_dead = true;
        foreach (AI_Actor entity in enemy_entities) {
            entity.update();
            if (!entity.is_dead) are_all_enemies_dead = false;
        }

            //- All enemies are dead
        if (are_all_enemies_dead) {
            event_start_new_wave();
        }

            //- Update Friendlies
        foreach (AI_Actor entity in friendly_entities) {
            entity.update();
            if (!entity.is_dead) are_all_friendlies_dead = false;
        }

            //- All enemies are dead
        if (are_all_friendlies_dead) {
            event_start_new_wave();
        }

        foreach (HolographicObject holographic_object in holographic_objects) {
            holographic_object.update(enemy_spawn_point.position, spawn_radius);
        }

        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;

        Debug.Assert(mouse != null);

        if (mouse.leftButton.wasPressedThisFrame) {
                //- Player giving Input
            Ray ray = main_camera.ScreenPointToRay(mouse.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)){
                AI_Actor actor = hit.transform.GetComponent<AI_Actor>();
                select_entity(actor);
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
            event_start_new_wave();
            // foreach (AI_Actor entity in enemy_entities) {
            //     if (entity.lead != null) {
            //         entity.shoot_at(entity.lead);
            //     }
            // }
            // enemy_entities[1].shoot_at(enemy_entities[1].lead);
        }
    }

    Vector3 get_random_position_in_worldspace(Vector3 origin) {
        Vector3 result;
        result.y = (floor.offset * floor.normal).y + Random.Range(-2.0f, 2.0f);
        result.x = origin.x + Random.Range(-spawn_radius, spawn_radius);
        result.z = origin.z + Random.Range(-spawn_radius, spawn_radius);

        return result;
    }

        /// only allows the selection of friendly entities
    void select_entity(AI_Actor actor) {
        if (!actor.is_enemy) {
                //- unselect other actors
            foreach (AI_Actor entity in friendly_entities) {
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

///
///                      INPUT CONTROLLER
///

    void on_input_select(InputAction.CallbackContext ctx) {
        HolographicObject obj = player_hand.select();
        if (obj) {
            AI_Actor entity = obj.attachedObject.GetComponent<AI_Actor>();
            if (entity) {
                    //- This is where we select an entity through a holographic object
                select_entity(entity);
            }
        }
    }

    void on_input_unselect(InputAction.CallbackContext ctx) {
        Vector3 target_pos = player_hand.transform.position;
        for (int i = 0; i < enemy_entities.Count; i++) {
            if (enemy_entities[i].is_selected) {
                enemy_entities[i].move(target_pos);
            }
        }
    }

///
///     GAME EVENTS THAT CAN BE CALLED FROM OUTSIDE OF THIS SCRIPT
///

        /// Spawn new enemies and regenerate friendlies.
        /// Does not get rid of left over enemies.
    public void event_start_new_wave() {
        //- Reset Enemies
        for (int i = 0; i < enemy_entities.Count; i++) {
            AI_Actor entity = enemy_entities[i];
            AI_Actor lead = null;
            if (i > 0) lead = enemy_entities[0];

            Vector3 pos = get_random_position_in_worldspace(enemy_spawn_point.position);
                // Makes enemies undead
            entity.init(this, enemy_blackboard, lead, pos, true);
        }

        for (int i = 0; i < friendly_entities.Count; i++) {
            AI_Actor entity = friendly_entities[i];
            AI_Actor lead = null;
            if (i > 0) lead = friendly_entities[0];

            Vector3 pos = get_random_position_in_worldspace(friendly_spawn_point.position);
                // Makes enemies undead
            entity.init(this, friendly_blackboard, lead, pos, false);
        }
            // @temp tell each leader to go to center to fight each other
        Vector3 center = (enemy_spawn_point.position + friendly_spawn_point.position) / 2;
        friendly_entities[0].move(center);
        enemy_entities[0].move(center);
    }

        /// Pause the game and show the pause menu
    public void event_pause() {

    }

        /// Resume the game and hide the pause menu
    public void event_resume() {

    }

        /// Exit the game
    public void event_exit() {

    }

    public void event_kill_entity(AI_Actor entity) {
        entity.kill();
    }

        /// Kill all enemy ships that are found within the given radius
    public void event_kill_enemies_in_radius(Vector3 origin, float radius) {
        foreach (AI_Actor entity in enemy_entities) {
            if (Vector3.Distance(entity.transform.position, origin) <= radius) {
                event_kill_entity(entity);
            }
        }
    }

    public void event_damage_enemies_in_radius(Vector3 origin, float radius) {

    }

        /// Kill all friendly ships that are found within the given radius
    public void event_kill_friendly_in_radius(Vector3 origin, float radius) {
        foreach (AI_Actor entity in friendly_entities) {
            if (Vector3.Distance(entity.transform.position, origin) <= radius) {
                event_kill_entity(entity);
            }
        }
    }

        /// Resets the level as if a new game has started
    public void event_reset_level() {

    }

        /// Starts the sequences of the player's death
    public void event_kill_player() {

    }

    public void event_select_holographic_object(HolographicObject obj) {
        AI_Actor entity = obj.select().GetComponent<AI_Actor>();
        if (entity != null) {
            // if the selected holographic object is an ai actor select the actor
            event_select_ship(entity);
        }
    }

    public void event_select_ship(AI_Actor entity) {
        // unselect other actors
        foreach (AI_Actor other_entity in entity.blackboard.group) {
            other_entity.is_selected = false;
        }

            //- Select the Leader of this flock
        if (entity.lead == null) {
            entity.is_selected = true;
        } else {
            entity.lead.is_selected = true;
        }
    }
}


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


[System.Serializable]
public class PrimaryButtonEvent : UnityEvent<bool> {}
