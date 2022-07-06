using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingRotate : MonoBehaviour
{

    public float rotateDegrees = 120.0f;
    public float rotateSpeed = 1.0f;
    [SerializeField] private bool isRotating;

    private void Start()
    {
        isRotating = false;
    }

    public void LeftRotate()
    {
        if(isRotating == false)
        {
            StartCoroutine(Rotate(Vector3.down, rotateDegrees, rotateSpeed));
        }
        else return;
    }

    public void rightRotate()
    {
        if (isRotating == false)
        {
            StartCoroutine(Rotate(Vector3.up, rotateDegrees, rotateSpeed));
        }
        else return;
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
            isRotating = true;
            clock += Time.deltaTime;
            if (clock >= duration)
            {
                isRotating = false; ;
            }
            yield return null;
        }
        transform.rotation = end;
     }
}