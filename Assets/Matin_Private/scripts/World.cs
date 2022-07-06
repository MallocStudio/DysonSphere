using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using UnityEngine.SceneManagement;

public class World : MonoBehaviour {
    const int MOUSE_BUTTON_LEFT   = 0;
    const int MOUSE_BUTTON_RIGHT  = 1;
    const int MOUSE_BUTTON_MIDDLE = 2;

///
/// AI
///

    AI_Blackboard enemy_blackboard = new AI_Blackboard();
    AI_Blackboard friendly_blackboard = new AI_Blackboard();
    List<AI_Actor> enemy_entities;
    List<AI_Actor> friendly_entities;

    List<HolographicObject> holographic_objects;
    [SerializeField] GameObject enemy_spaceship_prefab;
    [SerializeField] GameObject enemy_capitalship_prefab;
    [SerializeField] GameObject friendly_spaceship_prefab;
    [SerializeField] Light alarm;
    [SerializeField] Camera main_camera;
    [SerializeField] Transform enemy_spawn_point;
    [SerializeField] public Transform friendly_spawn_point;
    [SerializeField] public Transform player_ship_point;
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

///
/// PLAYER STATS
///
    //@temp serialized to debug
    [SerializeField] float player_health = 10;
    bool is_player_dead = false;
    float spawn_radius = 30.0f;
    const int GROUP_MAX_CAPACITY = 6;

        //- Floor
    Plane floor = new Plane();

    void Start() {
        Debug.Assert(enemy_blackboard != null);
        Debug.Assert(main_camera != null);
        Debug.Assert(enemy_spaceship_prefab != null);
        Debug.Assert(enemy_capitalship_prefab != null);
        Debug.Assert(friendly_spaceship_prefab != null);
        Debug.Assert(alarm != null);
        Debug.Assert(enemy_spawn_point != null);
        Debug.Assert(friendly_spawn_point != null);
        Debug.Assert(player_hand != null);
        Debug.Assert(debug_console != null);
        Debug.Assert(player_ship_point != null);

            //- Input Action
        input_action_select.action.performed += on_input_select;
        input_action_select.action.canceled  += on_input_unselect;
        input_action_clear_console.action.performed += on_input_clear_console;
        //input_action_new_wave.action.performed += on_input_new_wave;

        //- Generate the Entities
        enemy_entities = new List<AI_Actor>(GROUP_MAX_CAPACITY);
        friendly_entities = new List<AI_Actor>(GROUP_MAX_CAPACITY);
        holographic_objects = new List<HolographicObject>();

            //- Generate Enemies
        // event_add_enemy_group();

            //- Generate Friendly Ships
        event_add_friendly_group();

            //- Setup Teams
        event_update_teams();

        //     //- Reset alls enemies and prepares everything for a new wave
        // event_start_new_wave_immediately();

        //@temp test debug console
        event_log_clear();
        event_log("Things get logged here");
    }

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

        bool are_all_friendlies_dead = true;
            //- Update Friendlies
        foreach (AI_Actor entity in friendly_entities) {
            entity.update();
            if (!entity.is_dead) are_all_friendlies_dead = false;
        }

            //- All friendlies are dead. So enemies should attack the player
        if (are_all_friendlies_dead && !are_all_enemies_dead) {
            event_attack_player_ship();
        }

        foreach (HolographicObject holographic_object in holographic_objects) {
            holographic_object.update(enemy_spawn_point.position, spawn_radius);
        }

            //- Alarm
        if (alarm_has_gone_off) {
            event_alarm_activate();
        }

            //- Update death sequence
        update_player_death_sequence();

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
        }
    }

    void on_input_clear_console(InputAction.CallbackContext ctx) {
        event_log_clear();
    }

    void on_input_new_wave(InputAction.CallbackContext ctx) {
        event_start_new_wave_immediately();
    }

    void on_input_unselect(InputAction.CallbackContext ctx) {
        Vector3 target_pos = player_hand.transform.position - hologram_panel.transform.transform.transform.transform.position;
        for (int i = 0; i < friendly_entities.Count; i++) {
            if (friendly_entities[i].is_selected) {
                float scale_delta = 1 - friendly_entities[i].attached_holographic_obj.differnceInScale;
                event_log("unselecting at " + target_pos * scale_delta);
                friendly_entities[i].move(target_pos * scale_delta);
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
    [SerializeField] uint wave_count = 0;
    public void event_start_new_wave_immediately() {
        wave_count++;
        if (wave_count == 1) {
            // add enemies for the first time
            event_add_enemy_group();
        } else
        if (wave_count % 5 == 0) {
            event_add_enemy_group();
            event_add_friendly_group();
        }

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

            //- Activate the Alarm
        event_alarm_activate();
        event_update_teams();
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

        /// The enemies now target the player
    public void event_attack_player_ship() {
        // make sure that the number of spawned enemies (dead or alive) is equal to the number of groups * GROUP_MAX_CAPACITY
        Debug.Assert(number_of_enemy_groups * GROUP_MAX_CAPACITY == enemy_entities.Count());

        for (int group = 0; group < number_of_enemy_groups; group++) {
            AI_Actor lead = null;
            for (int i = group; i < (GROUP_MAX_CAPACITY * (group+1)); i++) {
                AI_Actor entity = enemy_entities[i];
                entity.attacking_the_player = true;
                if (i == 0) {
                    lead = entity;
                }
                if (lead) {
                    lead.move(player_ship_point.position);
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

            if (entity != null) {
                // if the selected holographic object is an ai actor select the actor
                event_log("attachment has an AI Actor");
                event_select_ship(entity);
            }
        }
    }

        /// only allows the selection of friendly entities
    public void event_select_ship(AI_Actor actor) {
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
            Hologram_Type type = Hologram_Type.FRIENDLY;
            if (hologram_panel != null) {
                holographic_objects.Add(hologram_panel.LinkNewEntity(gameobject.transform, type));
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
            GameObject gameobject = null;
            if (i == 0) {
                gameobject = Instantiate(enemy_capitalship_prefab, pos, Quaternion.identity);
            } else {
                gameobject = Instantiate(enemy_spaceship_prefab, pos, Quaternion.identity);
            }

                //- Link the created entity to the hologram tabel
            Hologram_Type type = Hologram_Type.ENEMY;
            if (i == 0) type = Hologram_Type.LEADERSHIP;
            if (hologram_panel != null) {
                holographic_objects.Add(hologram_panel.LinkNewEntity(gameobject.transform, type));
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
        // // make sure that the number of spawned enemies (dead or alive) is equal to the number of groups * GROUP_MAX_CAPACITY
        // Debug.Assert(number_of_enemy_groups * GROUP_MAX_CAPACITY == enemy_entities.Count());

        // for (int group = 0; group < number_of_enemy_groups; group++) {
        //     AI_Actor lead = null;
        //     for (int i = group; i < (GROUP_MAX_CAPACITY * (group+1)); i++) {
        //         AI_Actor entity = enemy_entities[i];
        //         if (i == 0) {
        //             lead = entity;
        //         }

        //         Vector3 pos = get_random_position_in_worldspace(enemy_spawn_point.position);
        //             // Makes enemies undead
        //         entity.init(this, enemy_blackboard, lead, pos, true);

        //         if (lead) {
        //             lead.move(friendly_spawn_point.position);
        //         }
        //     }
        // }
        foreach (AI_Actor entity in enemy_entities) {
            Vector3 pos = get_random_position_in_worldspace(enemy_spawn_point.position);
            entity.init(this, enemy_blackboard, entity.lead, pos, true);
            entity.move(friendly_spawn_point.position);
        }
    }

    public void event_reset_friendly_groups() {
        // // make sure that the number of spawned friendlies (dead or alive) is equal to the number of groups * GROUP_MAX_CAPACITY
        // Debug.Assert(number_of_friendly_groups * GROUP_MAX_CAPACITY == friendly_entities.Count());

        // for (int group = 0; group < number_of_friendly_groups; group++) {
        //     AI_Actor lead = null;
        //     for (int i = group; i < (GROUP_MAX_CAPACITY * (group+1)); i++) {
        //         AI_Actor entity = friendly_entities[i];
        //         if (i == 0) lead = entity;

        //         Vector3 pos = get_random_position_in_worldspace(friendly_spawn_point.position);
        //             // Makes enemies undead
        //         entity.init(this, friendly_blackboard, lead, pos, false);
        //     }
        // }
        foreach (AI_Actor entity in friendly_entities) {
            Vector3 pos = get_random_position_in_worldspace(friendly_spawn_point.position);
            entity.init(this, friendly_blackboard, entity.lead, pos, false);
            entity.move(friendly_spawn_point.position);
        }
    }

    public void event_log(string message) {
        if (debug_console.isTextOverflowing) {
            event_log_clear();
        }
        debug_console.text += ">>" + message + '\n';
        Debug.Log(message);
    }

    public void event_log_clear() {
        debug_console.text = "";
    }

    float alarm_timer = 3;
    float alarm_timer_init = 3;
    bool alarm_has_gone_off = false;
    public void event_alarm_activate() {
            //- Timer aspect
        alarm_has_gone_off = true;
        if (alarm_timer > 0) {
            alarm_timer -= Time.deltaTime;
        } else {
            alarm_timer = alarm_timer_init;
            alarm_has_gone_off = false;
        }

            //- Visual aspect
        if (alarm_has_gone_off) {
            alarm.enabled = true;
            alarm.transform.RotateAround(alarm.transform.position, Vector3.up, Time.deltaTime * 360);
        } else {
            alarm.enabled = false;
        }
    }

    public void event_play_sound(AudioSource audio, AudioClip clip) {
        audio.PlayOneShot(clip);
    }

    public void event_damage_player(float damage) {
        player_health -= damage;
        if (player_health <= 0) {
            event_start_player_death_sequence();
        } else {
            if ((int)player_health % 1 == 0) {
                event_log("WARNING: Under Attack. Stability: " + (int)player_health);
            }
        }
    }

    bool has_player_death_started = false;
    public void event_start_player_death_sequence() {
        if (!has_player_death_started) {
            has_player_death_started = true;
            is_player_dead = true;
            event_log("System Failure. Eject Now.");
        }
    }

    float player_death_sequence_timer = 10;
    void update_player_death_sequence() {
        if (has_player_death_started) {
            if (!alarm_has_gone_off) {
                event_alarm_activate();
            }
                // @temp load scene when the player dies for now
            if (player_death_sequence_timer > 0) {
                player_death_sequence_timer -= Time.deltaTime;
                if (((int)player_death_sequence_timer) % 1 == 0) event_log("EJECTING IN " + (int)player_death_sequence_timer + " SECONDS.");
            } else {
                event_log("reloaded scene");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
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
