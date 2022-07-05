using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class World : MonoBehaviour {
    const int MOUSE_BUTTON_LEFT   = 0;
    const int MOUSE_BUTTON_RIGHT  = 1;
    const int MOUSE_BUTTON_MIDDLE = 2;

///
/// AI
///

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
    uint number_of_enemy_groups = 0;
    uint number_of_friendly_groups = 0;

///
/// INPUT
///

    [SerializeField] Player_Hand player_hand;
    [SerializeField] InputActionReference input_action_select;
    [SerializeField] InputActionReference input_action_clear_console;
    [SerializeField] InputActionReference input_action_new_wave;

///
/// DEBUG CONSOLE
///

    [SerializeField] TextMeshProUGUI debug_console;

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
        Debug.Assert(debug_console != null);

            //- Input Action
        input_action_select.action.performed += on_input_select;
        input_action_select.action.canceled  += on_input_unselect;
        input_action_clear_console.action.performed += on_input_clear_console;
        input_action_new_wave.action.performed += on_input_new_wave;

        //- Generate the Entities
        enemy_entities = new List<AI_Actor>(GROUP_MAX_CAPACITY);
        friendly_entities = new List<AI_Actor>(GROUP_MAX_CAPACITY);
        holographic_objects = new List<HolographicObject>();

            //- Generate Enemies
        event_add_enemy_group();

            //- Generate Friendly Ships
        event_add_friendly_group();

            //- Setup Teams
        event_update_teams();

            //- Reset alls enemies and prepares everything for a new wave
        event_start_new_wave_immediately();

        //@temp test debug console
        event_log_clear();
        event_log("Things get logged here");
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
            //- Update Enemies
        bool are_all_enemies_dead = true;
        foreach (AI_Actor entity in enemy_entities) {
            entity.update();
            if (!entity.is_dead) are_all_enemies_dead = false;
        }

        if (are_all_enemies_dead) {
            event_start_new_wave_with_delay();
        }

            //- Update Friendlies
        foreach (AI_Actor entity in friendly_entities) {
            entity.update();
        }

        foreach (HolographicObject holographic_object in holographic_objects) {
            holographic_object.update(enemy_spawn_point.position, spawn_radius);
        }

            //@debug
        Keyboard keyboard = Keyboard.current;
        if (keyboard.spaceKey.wasPressedThisFrame) {
            event_start_new_wave_immediately();
        }
    }

    Vector3 get_random_position_in_worldspace(Vector3 origin) {
        Vector3 result;
        result.y = (floor.offset * floor.normal).y + Random.Range(-2.0f, 2.0f);
        result.x = origin.x + Random.Range(-spawn_radius, spawn_radius);
        result.z = origin.z + Random.Range(-spawn_radius, spawn_radius);

        return result;
    }

///
///                      INPUT CONTROLLER
///

    void on_input_select(InputAction.CallbackContext ctx) {
        HolographicObject obj = player_hand.select();
        if (obj) {
            event_select_holographic_object(obj);
            AI_Actor entity = obj.attachedObject.GetComponent<AI_Actor>();
            if (entity) {
                    //- This is where we select an entity through a holographic object
                event_select_ship(entity);
            }
        }
    }

    void on_input_clear_console(InputAction.CallbackContext ctx) {
        event_log_clear();
    }

    void on_input_new_wave(InputAction.CallbackContext ctx) {
        event_start_new_wave_immediately();
    }

    void on_input_unselect(InputAction.CallbackContext ctx) {
        Vector3 target_pos = player_hand.transform.position;
        event_log("unselecting at " + target_pos);
        for (int i = 0; i < enemy_entities.Count; i++) {
            if (enemy_entities[i].is_selected) {
                enemy_entities[i].move(target_pos);
            }
        }
    }

///
///     GAME EVENTS THAT CAN BE CALLED FROM OUTSIDE OF THIS SCRIPT
///
    bool wave_timer_has_started = false;
    [SerializeField] float wave_timer_init = 10;
    [SerializeField] float wave_timer = 10;
        /// Calls event_start_new_wave_immediately after "wave_timer_init" time
    public void event_start_new_wave_with_delay() {
        if (wave_timer_has_started) {
            if (wave_timer > 0) {
                wave_timer -= Time.deltaTime;
            } else {
                // - TIMER REACHED ITS END
                wave_timer_has_started = false;
                event_start_new_wave_immediately();
            }
        } else {
            wave_timer_has_started = true;
            wave_timer = wave_timer_init;
        }
    }

        /// Spawn new enemies and regenerate friendlies.
        /// Does not get rid of left over enemies.
    [SerializeField] float player_score = 0;
    [SerializeField] float cost_of_new_friendly_group = 10;
    public void event_start_new_wave_immediately() {
            //- Reset Enemies
        event_reset_enemy_groups();

            //- Reset Friendlies
            // @temp do we want to reset friendlies?
        event_reset_friendly_groups();

            //- Add a new group of friendlies if we have enough score
        if (player_score > cost_of_new_friendly_group) {
            cost_of_new_friendly_group += player_score;
            event_add_friendly_group();
        }
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

    public void event_damage_enemies_in_radius(Vector3 origin, float radius, float amount) {
        foreach (AI_Actor entity in enemy_entities) {
            if (!entity.is_dead) {
                if (Vector3.Distance(entity.transform.position, origin) <= radius) {
                    entity.take_damage(amount);
                }
            }
        }
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
        if (obj == null) return;

        event_log("selecting a hologram");
        Transform obj_attachment = obj.select();
        if (obj_attachment != null)
        {
            event_log("hologram has an attachment");
            AI_Actor entity = obj_attachment.GetComponent<AI_Actor>();
            if (entity != null)
                event_log("attachment has an AI Actor");
            else
                event_log("attachment DID NOT have an AI Actor");

            if (entity != null) {
                // if the selected holographic object is an ai actor select the actor
                event_select_ship(entity);
            }
        }
        else
        {
            event_log("hologram DID NOT have an attachment");
        }
    }

        /// only allows the selection of friendly entities
    public void event_select_ship(AI_Actor actor) {
            event_log("selecting a ship");
        if (!actor.is_enemy) {
            event_log("selecting a friendlyship");

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

        /// Remember to call event_update_teams() afterwards
        /// Adds another group of friendlies to the world
    public void event_add_friendly_group() {
        number_of_friendly_groups++;
        AI_Actor lead = null;
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

            if (i == 0) lead = entity;
            entity.init(this, friendly_blackboard, lead, pos, false);
            friendly_entities.Add(entity);
        }
    }

        /// Remember to call event_update_teams() afterwards
        /// Adds another group of enemies to the world
    public void event_add_enemy_group() {
        number_of_enemy_groups++;
        AI_Actor lead = null;
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
            if (i == 0) lead = entity;
            entity.init(this, enemy_blackboard, lead, pos, true);
            enemy_entities.Add(entity);
        }
    }

    public void event_update_teams() {
        foreach (AI_Actor entity in enemy_entities) {
            friendly_blackboard.enemies.Add(entity);
        }
        foreach(AI_Actor entity in friendly_entities) {
            enemy_blackboard.enemies.Add(entity);
        }
    }

    public void event_reset_enemy_groups() {
        // make sure that the number of spawned enemies (dead or alive) is equal to the number of groups * GROUP_MAX_CAPACITY
        Debug.Assert(number_of_enemy_groups * GROUP_MAX_CAPACITY == enemy_entities.Count());

        for (int group = 0; group < number_of_enemy_groups; group++) {
            AI_Actor lead = null;
            for (int i = group; i < (GROUP_MAX_CAPACITY + group); i++) {
                AI_Actor entity = enemy_entities[i];
                if (i == 0) {
                    lead = entity;
                }

                Vector3 pos = get_random_position_in_worldspace(enemy_spawn_point.position);
                    // Makes enemies undead
                entity.init(this, enemy_blackboard, lead, pos, true);

                if (lead) {
                    lead.move(friendly_spawn_point.position);
                }
            }
        }
    }

    public void event_reset_friendly_groups() {
        // make sure that the number of spawned friendlies (dead or alive) is equal to the number of groups * GROUP_MAX_CAPACITY
        Debug.Assert(number_of_friendly_groups * GROUP_MAX_CAPACITY == friendly_entities.Count());

        for (int group = 0; group < number_of_friendly_groups; group++) {
            AI_Actor lead = null;
            for (int i = group; i < (GROUP_MAX_CAPACITY + group); i++) {
                AI_Actor entity = friendly_entities[i];
                if (i == 0) lead = entity;

                Vector3 pos = get_random_position_in_worldspace(friendly_spawn_point.position);
                    // Makes enemies undead
                entity.init(this, friendly_blackboard, lead, pos, false);
            }
        }
    }

    public void event_log(string message) {
        debug_console.text += ">>" + message + '\n';
    }

    public void event_log_clear() {
        debug_console.text = "";
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
