using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSpot : MonoBehaviour
{
    public float rotationSpeed = 10f;
    public float shiftMultiplier = 2f;
    public Animator playerCameraAnimator;
    public GameObject compass;

    // Nový øádek: promìnná pro rychlost rotace skyboxu
    public float skyboxRotationSpeed = 0.5f;

    // Nový øádek: promìnná pro náhodný smìr rotace skyboxu
    private float skyboxRotationDirection;

    private void Start()
    {
        // Nový øádek: nastavení náhodného smìru rotace skyboxu pøi startu
        skyboxRotationDirection = Random.Range(-1f, 1f);
    }

    void Update()
    {
        bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float multiplier = isShiftPressed ? shiftMultiplier : 1f;

        //pokus s rotaci Skyboxu
        // Rotace skyboxu náhodným smìrem
        //float rotation = Time.time * rotationSpeed;
        //RenderSettings.skybox.SetFloat("_Rotation", rotation);

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            playerCameraAnimator.SetBool("isMoving", true);
            transform.Rotate(Vector3.up, -rotationSpeed * multiplier * Time.deltaTime);
            compass.transform.Rotate(Vector3.back, -rotationSpeed * multiplier * Time.deltaTime);

            // Rotace skyboxu doleva
            RenderSettings.skybox.SetFloat("_Rotation", RenderSettings.skybox.GetFloat("_Rotation") + rotationSpeed * multiplier * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            playerCameraAnimator.SetBool("isMoving", true);
            transform.Rotate(Vector3.up, rotationSpeed * multiplier * Time.deltaTime);
            compass.transform.Rotate(Vector3.back, rotationSpeed * multiplier * Time.deltaTime);

            // Rotace skyboxu doprava
            RenderSettings.skybox.SetFloat("_Rotation", RenderSettings.skybox.GetFloat("_Rotation") - rotationSpeed * multiplier * Time.deltaTime);
        }
        else
        {
            playerCameraAnimator.SetBool("isMoving", false);
        }

        // Nový øádek: pomalá rotace skyboxu v náhodném smìru
        RenderSettings.skybox.SetFloat("_Rotation", RenderSettings.skybox.GetFloat("_Rotation") + skyboxRotationSpeed * skyboxRotationDirection * Time.deltaTime);

    }
}