using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Objects")]
    public GameObject target;

    [Header("Tweaks")]
    public float easing = 0.05f;
    public Vector2 shift = new Vector2(1f, 1f);

    private float cameraZ;
    private Vector3 modifiedTarget;

    private void Awake()
    {
        cameraZ = transform.position.z;
    }

    private void Update()
    {
        //raise up the camera a bit
        modifiedTarget = target.transform.position;
        modifiedTarget.x += shift.x;
        modifiedTarget.y += shift.y;

        Vector3 destination;

        //follow the target with easing to make the camera move smoothly
        if(easing != 0)
            destination = Vector3.Lerp(transform.position, modifiedTarget, easing);

        //if easing is off, give the camera the same coordinates as the player
        else
            destination = modifiedTarget;

        destination.z = cameraZ;
        transform.position = destination;
    }
}
