using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{
    public Transform lookAt;
    public float smoothSpeed = 0.125f; // Adjust this value
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Camera offset (z is important!)
    public float boundX = 0.3f;
    public float boundY = 0.15f;


    private Vector3 velocity = Vector3.zero;

    // Late update is being called after Update and FixedUpdate, so the camera is moved the user actions
    //private void LateUpdate()
    //{
    //    Vector3 delta = Vector3.zero;

    //    float deltaX = lookAt.position.x - transform.position.x; // get distance between center of camera and player
    //    if(deltaX > boundX || deltaX < -boundX)
    //    {
    //        // Check if we're inside the bounds of X axis
    //        if(transform.position.x < lookAt.position.x)
    //        {
    //            delta.x = deltaX - boundX;
    //        } 
    //        else
    //        {
    //            delta.x = deltaX + boundX;
    //        }
    //    }

    //    // Check if we're inside the bounds of Y axis
    //    float deltaY = lookAt.position.y - transform.position.y; // get distance between center of camera and player
    //    if (transform.position.y < lookAt.position.y)
    //    {
    //        delta.y = deltaY - boundY;
    //    }
    //    else
    //    {
    //        delta.y = deltaY + boundY;
    //    }

    //    transform.position += new Vector3(delta.x, delta.y, 0);
    //}

    private void FixedUpdate() // Use FixedUpdate
    {
        if (lookAt == null) return; // Safety check

        Vector3 desiredPosition = lookAt.position + offset;

        // Apply bounds logic *before* smoothing
        float deltaX = lookAt.position.x - transform.position.x;
        if (Mathf.Abs(deltaX) > boundX)  // Use Mathf.Abs for cleaner comparison
        {
            desiredPosition.x = transform.position.x + (deltaX > 0 ? deltaX - boundX : deltaX + boundX);
        }

        float deltaY = lookAt.position.y - transform.position.y;
        if (Mathf.Abs(deltaY) > boundY)
        {
            desiredPosition.y = transform.position.y + (deltaY > 0 ? deltaY - boundY : deltaY + boundY);
        }

        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed * Time.fixedDeltaTime);
        transform.position = smoothedPosition;
    }
}
