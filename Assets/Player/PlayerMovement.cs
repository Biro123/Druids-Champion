using System;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof (ThirdPersonCharacter))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float stopMoveRadius = 0.2f;
    [SerializeField] float AttackRadius = 5.0f;

    ThirdPersonCharacter thirdPersonCharacter;   // A reference to the ThirdPersonCharacter on the object
    CameraRaycaster cameraRaycaster;
    bool isInDirectMode = false;
    Vector3 currentDestination, clickPoint;    
    Vector3 movement = Vector3.zero;

        
    private void Start()
    {
        cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
        thirdPersonCharacter = GetComponent<ThirdPersonCharacter>();
        currentDestination = transform.position;
    }

    // Fixed update is called in sync with physics
    private void FixedUpdate()
    {
        // TODO - Check commented code.. Was removed for improved raycaster
        //if (Input.GetKeyDown(KeyCode.G))  // TODO Add to menu (G for gamepad)
        //{
        //    isInDirectMode = !isInDirectMode;
        //    movement = Vector3.zero;
        //    currentDestination = transform.position;
        //}

        //if (isInDirectMode)
        //{
        //    ProcessDirectMovement();
        //}
        //else
        //{
        //    ProcessMouseMovement();
        //}
    }

    private void ProcessDirectMovement()
    {
        // read inputs
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // calculate camera relative direction to move:
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        movement = v * cameraForward + h * Camera.main.transform.right;

        thirdPersonCharacter.Move(movement, false, false);
    }

    private void ProcessMouseMovement()
    {
        //if (Input.GetMouseButton(0))
        //{
        //    clickPoint = cameraRaycaster.hit.point;
        //    switch (cameraRaycaster.currentLayerHit)
        //    {
        //        case Layer.Walkable:
        //            currentDestination = ShortDestination(clickPoint, stopMoveRadius );
        //            break;
        //        case Layer.Enemy:
        //            currentDestination = ShortDestination(clickPoint, AttackRadius);
        //            break;
        //        case Layer.RaycastEndStop:
        //            break;
        //        default:
        //            print("Unexpected Layer Found");
        //            break;
        //    }
        //}

        //// stop wobbles once destination is reached
        //movement = currentDestination - transform.position;
        //if (movement.magnitude >= stopMoveRadius)
        //{
        //    thirdPersonCharacter.Move(movement, false, false);
        //}
        //else
        //{
        //    thirdPersonCharacter.Move(Vector3.zero, false, false);
        //}           
    }

    private Vector3 ShortDestination(Vector3 destination, float shortening)
    {
        Vector3 reductionVector = (destination - transform.position).normalized * shortening;
        return destination - reductionVector;
    }

    private void OnDrawGizmos()
    {
        // Draw Movement Gizmos
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, currentDestination);
        Gizmos.DrawSphere(currentDestination, 0.1f);
        Gizmos.DrawSphere(clickPoint, 0.15f);

        // Draw Attack Gizmos
        Gizmos.color = new Color(255f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, AttackRadius);
    }
}

