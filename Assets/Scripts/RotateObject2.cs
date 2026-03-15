using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject2 : MonoBehaviour
{
    public float speed = 10.0f;
    private float xDeg = 0.0f;
    private float yDeg = 0.0f;
    private Quaternion fromRotation;
    private Quaternion toRotation;

    void Start()
    {
        Init();
    }

    void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        fromRotation = transform.rotation;
        toRotation = transform.rotation;

        xDeg = Vector3.Angle(Vector3.right, transform.right);
        yDeg = Vector3.Angle(Vector3.up, transform.up);
    }

    void LateUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            xDeg -= Input.GetAxis("Mouse X") * speed;
            yDeg += Input.GetAxis("Mouse Y") * speed;

            // Omezíme rotaci na ose X, aby se objekt nepøetoèil
            xDeg = ClampAngle(xDeg, -90, 90);

            fromRotation = transform.rotation;
            toRotation = Quaternion.Euler(yDeg, xDeg, 0);
            transform.rotation = Quaternion.Lerp(fromRotation, toRotation, Time.deltaTime * speed);
        }
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
