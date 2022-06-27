using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class physSwitch : MonoBehaviour
{
    [SerializeField] private bool modeOn;
    [SerializeField] private UnityEvent turnModeOnEv;
    [SerializeField] private UnityEvent turnModeOffEv;

    public void InteractSwitch()
    {
        if(!modeOn)
        {
            modeOn = true;
            turnModeOnEv.Invoke();
        }
        else
        {
            modeOn = false;
            turnModeOffEv.Invoke();
        }
    }
}
