using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoves : MonoBehaviour
{
    public float turnSpeed = 3.0f; // Rychlost otáèení hráèe
    public float moveSpeed = 5.0f; // Základní rychlost pohybu hráèe
    public float pinpointMoveSpeed = 2.0f; // Rychlost pohybu hráèe v pinpoint režimu
    public Animator playerAnimator;
    public DetectManager detectManager; // Odkaz na DetectManager

    private Vector2 turn; // Promìnná pro uchování otáèení hráèe

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Otáèení hráèe pomocí myši
        turn.x += Input.GetAxis("Mouse X") * turnSpeed;

        transform.localRotation = Quaternion.Euler(0, turn.x, 0);

        // Výpoèet násobitele rychlosti pohybu
        float speedMultiplier = 1.0f;
        float currentMoveSpeed = moveSpeed; // Výchozí rychlost

        // Pokud je hráè v pinpoint režimu, použijeme nižší rychlost
        if (detectManager.pinpoint)
        {
            currentMoveSpeed = pinpointMoveSpeed;
        }

        // Pokud je stisknutá klávesa Shift a klávesa pro smìr pohybu
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKey(KeyCode.W))
            {
                speedMultiplier = 2.0f; // Zrychlený pohyb vpøed
            }
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                speedMultiplier = 1.5f; // Rychlejší pohyb do stran a vzad
            }
        }

        // Pohyb vpøed a vzad
        if (Input.GetKey(KeyCode.W))
        {
            playerAnimator.SetBool("isMoving", true);
            transform.Translate(Vector3.forward * Time.deltaTime * currentMoveSpeed * speedMultiplier);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            playerAnimator.SetBool("isMoving", true);
            transform.Translate(Vector3.back * Time.deltaTime * currentMoveSpeed * speedMultiplier); // 1.5x pomaleji vzad
        }
        else if (Input.GetKey(KeyCode.A))
        {
            playerAnimator.SetBool("isMoving", true);
            transform.Translate(Vector3.left * Time.deltaTime * currentMoveSpeed * speedMultiplier);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            playerAnimator.SetBool("isMoving", true);
            transform.Translate(Vector3.right * Time.deltaTime * currentMoveSpeed * 1.5f * speedMultiplier);
        }
        else
        {
            playerAnimator.SetBool("isMoving", false);
        }
    }
}
