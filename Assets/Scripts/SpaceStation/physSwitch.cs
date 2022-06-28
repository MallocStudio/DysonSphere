using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class physSwitch : MonoBehaviour
{
    [SerializeField] public HingeJoint joint;
    [SerializeField] public JointSpring jSpring;
    [SerializeField] public JointLimits jLimits;

    [SerializeField] private bool modeOn;
    [SerializeField] private bool modeOff;
    [SerializeField] private float actAng;

    [SerializeField] public UnityEvent turnModeOnEv;
    [SerializeField] public UnityEvent turnModeOffEv;

    private void Awake()
    {
        joint = GetComponent<HingeJoint>();
        jSpring = joint.spring;
        jLimits = joint.limits;


        jSpring.targetPosition = -actAng;
        jLimits.max = actAng;
        jLimits.min = -actAng;

        modeOn = false;
        modeOff = true;
    }
    private void Update()
    {
        if (joint.angle >= 0)
        {
            jSpring.targetPosition = actAng;
            if (modeOn == false)
            {
                modeOn = true;
                modeOff = false;
                turnModeOnEv.Invoke();
            }
        }
        if (joint.angle < 0)
        {
            jSpring.targetPosition = -actAng;
            if (modeOff == false)
            {
                modeOn = false;
                modeOff = true;
                turnModeOffEv.Invoke();
            }
        }
        joint.spring = jSpring;
        joint.limits = jLimits;
    }
}
