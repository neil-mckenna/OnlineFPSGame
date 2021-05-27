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

    // Jump
    public float jumpForce = 12f;
    public float gravityMod = 2.5f;
    public float rayCastJumpCheck = 1f;


    // raycast
    public Transform groundCheckPoint;
    private bool isGrounded;
    public LayerMask groundLayers;

    // bullets
    public GameObject bulletImpact;
    public float bulletZFightingOffset = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
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

        //Debug.Log(Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.25f, groundLayers));
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, rayCastJumpCheck, groundLayers);

        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            movement.y = jumpForce;

        }

        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod; 

        charCon.Move(movement * Time.deltaTime);

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;

        }
        else if(Cursor.lockState == CursorLockMode.None)
        {
            if(Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        if(Input.GetMouseButtonDown(0))
        {
            Shoot();

        }




        

    }

    private void Shoot()
    {
        // Shoot out a ray from the middle of the screen
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        ray.origin = transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("We hit " + hit.collider.gameObject.name);

            GameObject bulletImpactObject =  Instantiate(bulletImpact, hit.point + (hit.normal * bulletZFightingOffset), Quaternion.LookRotation(hit.normal, Vector3.up));

            Destroy(bulletImpactObject, 5f);

        }





    }




    private void LateUpdate() 
    {
        cam.transform.position = viewPoint.transform.position;
        cam.transform.rotation = viewPoint.transform.rotation;

    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, groundCheckPoint.position + new Vector3(0f, -2f, 0f));

        
    }
}
