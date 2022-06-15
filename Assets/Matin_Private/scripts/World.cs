using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.InputSystem;

[System.Serializable]
public class PrimaryButtonEvent : UnityEvent<bool> {}

public class World : MonoBehaviour {
    const int MOUSE_BUTTON_LEFT   = 0;
    const int MOUSE_BUTTON_RIGHT  = 1;
    const int MOUSE_BUTTON_MIDDLE = 2;

    AI_Blackboard blackboard;
    [SerializeField] List<AI_Actor> entities = new List<AI_Actor>();
    [SerializeField] Camera main_camera;

    //// INPUT


    void Start() {
        // Debug.Assert(blackboard != null);
        Debug.Assert(main_camera != null);
        foreach (AI_Actor entity in entities) {
            entity.init(blackboard, null);
        }
    }

    void Update() {
        foreach (AI_Actor entity in entities) {
            entity.update();
        }

        Mouse mouse = Mouse.current;
        Debug.Assert(mouse != null);

        if (mouse.leftButton.wasPressedThisFrame) {
            Ray ray = main_camera.ScreenPointToRay(mouse.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)){
                AI_Actor actor = hit.transform.GetComponent<AI_Actor>();
                if (actor) {
                        // unselect other actors
                    foreach (AI_Actor entity in entities) {
                        entity.is_selected = false;
                    }
                    actor.is_selected = true;
                }
            }
        }
    }
}