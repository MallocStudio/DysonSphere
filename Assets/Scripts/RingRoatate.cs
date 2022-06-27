using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingRoatate : MonoBehaviour
{
    float speed = 50.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LeftRotate()
    {
        transform.Rotate(Vector3.left * speed * Time.deltaTime);
    }

    public void rightRotate()
    {

    }
}
