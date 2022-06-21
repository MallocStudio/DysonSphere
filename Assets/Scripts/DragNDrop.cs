using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragNDrop : MonoBehaviour
{
    public Transform pointer;

    [Header("Select To Include In Drag")]
    [SerializeField] private bool x;
    [SerializeField] private bool y;
    [SerializeField] private bool z;

    public void Drag()
    {
        float newX = x ? pointer.position.x : transform.position.x;
        float newY = y ? pointer.position.x : transform.position.y;
        float newZ = z ? pointer.position.x : transform.position.z;
        transform.position = new Vector3(newX, newY, newZ);
    }
}
