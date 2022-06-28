using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingRotate : MonoBehaviour
{

    public float rotateDegrees = 120.0f;
    public float rotateSpeed = 1.0f;

    public void LeftRotate()
    {
        StartCoroutine(Rotate(Vector3.down, rotateDegrees, rotateSpeed));
    }

    public void rightRotate()
    {
        StartCoroutine(Rotate(Vector3.up, rotateDegrees, rotateSpeed));
    }

    IEnumerator Rotate(Vector3 axis, float angle, float duration)
    {
        Quaternion start = transform.rotation;
        Quaternion end = transform.rotation;
        end *= Quaternion.Euler(axis * angle);

        float clock = 0.0f;
        while (clock < duration)
        {
            transform.rotation = Quaternion.Slerp(start, end, clock / duration);
            clock += Time.deltaTime;
            yield return null;
        }
        transform.rotation = end;
     }
}