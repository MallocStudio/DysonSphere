using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class TeleportActiviate : MonoBehaviour
{
    
    //Access the GO containing the teleport con script
    [SerializeField] public GameObject teleCon;

    // Ref to input action with the button map
    [SerializeField] public InputActionReference teleActRef;

    [Header("Teleport Events")]
    [SerializeField] public UnityEvent onTeleportActivate;
    [SerializeField] public UnityEvent onTeleportCancel;

    void Start()
    {
        // An Interaction with the teleportActivationReference has been completed and performs a callback to the TeleportModeActivate.
        teleActRef.action.performed += TeleportModeActivate;
        // An Interaction with the teleportActivationReference has been cancelled and performs a callback to the TeleportModeCancel.
        teleActRef.action.canceled += TeleportModeCancel;
    }

    // This will let us call a series of events created in the onTeleportActivate events in the inspector
    private void TeleportModeActivate(InputAction.CallbackContext obj) => onTeleportActivate.Invoke();
    
    // This will delay the call of the DelayTeleportation function for 0.1 of a second
    private void TeleportModeCancel(InputAction.CallbackContext obj) => Invoke("DelayTeleportation ", .1f);

}
