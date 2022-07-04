using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class jointPhysButton : MonoBehaviour
{
    [SerializeField] private float threshold = 0.1f;
    [SerializeField] private float deadZone = 0.025f;

    private bool isPressed;
    private Vector3 startPos; //check relative to cur v start
    private ConfigurableJoint cJoint; //linear limit from here

    public UnityEvent onPressed;
    public UnityEvent onReleased;
    public Transform DebugVisualCube = null; // @debug

    private void Awake()
    {
        /* CACHE SHIT */
        startPos = transform.localPosition;
        cJoint = GetComponent<ConfigurableJoint>();

    }

    private void Update()
    {
        if(!isPressed && GetValue() + threshold >= 1)
            Pressed();
        if (isPressed && GetValue() - threshold <= 0)
            Released();
    }

    private float GetValue()
    {
        var value = Vector3.Distance(startPos, transform.localPosition / cJoint.linearLimit.limit);
        if(Mathf.Abs(value) < deadZone)
        {
            value = 0;
        }
        return Mathf.Clamp(value, -1f, 1f);
    }

    private void Pressed()
    {
        isPressed = true;
        onPressed.Invoke();
    }

    private void Released()
    {
        isPressed = false;
        onReleased.Invoke();
    }

}
