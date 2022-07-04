using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    public Transform cameraTransform;
    private Vector3 orignalCameraPos;

    public float shakeDuration;
    public float shakeAmount;

    //private bool canShake = false;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    public void ShakeCamera(float duration)
    {
        //canShake = true;
        shakeDuration = duration;
    }

    // Start is called before the first frame update
    void Start()
    {
        orignalCameraPos = cameraTransform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(shakeDuration > 0)
        {
            cameraTransform.localPosition = orignalCameraPos + Random.insideUnitSphere * shakeAmount;
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            shakeDuration = 0.0f;
            cameraTransform.localPosition = orignalCameraPos;
        }
    }
}
