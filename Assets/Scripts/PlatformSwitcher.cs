using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSwitcher : MonoBehaviour
{
    public UnityTemplateProjects.SimpleCameraController cameraController;
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        cameraController.enabled = false;
#else
        gameObject.SetActive(false);
#endif
    }
}
