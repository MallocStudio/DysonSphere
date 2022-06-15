using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {
    const int MOUSE_BUTTON_LEFT   = 0;
    const int MOUSE_BUTTON_RIGHT  = 1;
    const int MOUSE_BUTTON_MIDDLE = 2;

    AI_Blackboard blackboard;
    [SerializeField] List<AI_Actor> entities = new List<AI_Actor>();
    [SerializeField] Camera main_camera;

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
        if (Input.GetMouseButtonDown(MOUSE_BUTTON_LEFT)) {
            Ray ray = main_camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)){
                Debug.Log("pressed on something");
            }
        }
    }
}