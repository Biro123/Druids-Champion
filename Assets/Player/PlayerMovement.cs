using System;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof (ThirdPersonCharacter))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float stopMoveRadius = 0.2f;

    ThirdPersonCharacter m_Character;   // A reference to the ThirdPersonCharacter on the object
    CameraRaycaster cameraRaycaster;
    Vector3 currentClickTarget;
    bool isInDirectMode = false;        // TODO - may make static if used elsewhere
    Vector3 m_Move = Vector3.zero;

        
    private void Start()
    {
        cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
        m_Character = GetComponent<ThirdPersonCharacter>();
        currentClickTarget = transform.position;
    }

    // Fixed update is called in sync with physics
    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.G))  // TODO Add to menu (G for gamepad)
        {
            isInDirectMode = !isInDirectMode;
            m_Move = Vector3.zero;
            currentClickTarget = transform.position;
        }

        if (isInDirectMode)
        {
            ProcessDirectMovement();
        }
        else
        {
            ProcessMouseMovement();
        }
    }

    private void ProcessDirectMovement()
    {
        // read inputs
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // calculate camera relative direction to move:
        Vector3 m_CamForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        m_Move = v * m_CamForward + h * Camera.main.transform.right;

        m_Character.Move(m_Move, false, false);
    }

    private void ProcessMouseMovement()
    {
        if (Input.GetMouseButton(0))
        {
            switch (cameraRaycaster.layerHit)
            {
                case Layer.Walkable:
                    currentClickTarget = cameraRaycaster.hit.point;
                    break;
                case Layer.Enemy:
                    print("Not Moving To Enemy");
                    break;
                case Layer.RaycastEndStop:
                    break;
                default:
                    print("Unexpected Layer Found");
                    break;
            }
        }

        // stop wobbles once destination is reached
        m_Move = currentClickTarget - transform.position;
        if (m_Move.magnitude >= stopMoveRadius)
        {
            m_Character.Move(m_Move, false, false);
        }
        else
        {
            m_Character.Move(Vector3.zero, false, false);
        }        
    }
}

