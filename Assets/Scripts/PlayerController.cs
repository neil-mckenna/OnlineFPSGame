using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Transform viewPoint;
    public float mouseSensitivity = 1f;
    private float verticalRotStore;
    private Vector2 mouseInput;
    public bool invertLook;

    // movement
    public float moveSpeed = 5f, runSpeed = 8f;
    private float activeMoveSpeed; 
    private Vector3 moveDirection, movement;

    public CharacterController charCon;

    // Camera
    private Camera cam;

    public float jumpForce = 12f;
    public float gravityMod = 2.5f;

    

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;

        transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            transform.rotation.eulerAngles.y + mouseInput.x, 
            transform.rotation.eulerAngles.z);

        if(invertLook)
        {
            verticalRotStore += mouseInput.y;
        }
        else
        {
            verticalRotStore -= mouseInput.y;
        }
        
        verticalRotStore = Mathf.Clamp(verticalRotStore, -60, 60f);
        
        viewPoint.rotation = Quaternion.Euler(
            verticalRotStore,
            viewPoint.rotation.eulerAngles.y,
            viewPoint.rotation.eulerAngles.z);

        // WASD
        moveDirection = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        );

        if(Input.GetKey(KeyCode.LeftShift))
        {
            activeMoveSpeed = runSpeed;
        }
        else
        {
            activeMoveSpeed = moveSpeed;
        }

        // local var 
        float yVel = movement.y;

        // normalized move speed in 4 directions
        movement = ((transform.forward * moveDirection.z)  +  (transform.right * moveDirection.x)).normalized * activeMoveSpeed;
        movement.y = yVel;

        if(charCon.isGrounded)
        {
            //movement.y =  yVel;
            movement.y = 0f;
        }

        if(Input.GetButtonDown("Jump"))
        {
            movement.y = jumpForce;

        }

        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod; 

        charCon.Move(movement * Time.deltaTime);

    }

    private void LateUpdate() 
    {
        cam.transform.position = viewPoint.transform.position;
        cam.transform.rotation = viewPoint.transform.rotation;

    }
}
