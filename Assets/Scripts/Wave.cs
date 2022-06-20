using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wave
{
    float duration = 60.0f;
    float currentFrame = 0.0f;

    public float UpdateAmount(float amount)
    {
        currentFrame += amount;
        if (currentFrame >= duration)
        {
            currentFrame = 0.0f;
            duration += 10.0f;
        }

        return currentFrame;
    }
}