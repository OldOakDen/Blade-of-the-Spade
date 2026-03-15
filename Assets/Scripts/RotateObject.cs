using UnityEngine;

public class RotateWithMouse : MonoBehaviour
{
    public float speed = 10.0f;
    private Vector3 lastMousePosition;
    private float xDeg = 0.0f;
    private float yDeg = 0.0f;
    private Quaternion currentRotation;
    private Quaternion desiredRotation;
    private Quaternion rotation;

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
        rotation = transform.localRotation;
        currentRotation = transform.localRotation;
        desiredRotation = transform.localRotation;

        xDeg = Vector3.Angle(Vector3.right, transform.right);
        yDeg = Vector3.Angle(Vector3.up, transform.up);
    }

    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(1)) // 1 pro pravé tlaèítko myši
        {
            lastMousePosition = Input.mousePosition; // Pøidáno
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            if (delta.magnitude > 0)
            {
                xDeg -= Input.GetAxis("Mouse Y") * speed * 0.02f;
                yDeg += Input.GetAxis("Mouse X") * speed * 0.02f;

                // Omezíme rotaci na ose X, aby se objekt nepøetoèil
                xDeg = ClampAngle(xDeg, -90, 90);

                desiredRotation = Quaternion.Euler(xDeg, yDeg, 0);
                currentRotation = transform.localRotation;

                rotation = Quaternion.Slerp(currentRotation, desiredRotation, Time.deltaTime * speed);
                transform.localRotation = rotation;
            }
        }

        lastMousePosition = Input.mousePosition;
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
