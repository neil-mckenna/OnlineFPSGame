using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
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
    public float bulletImpactSurvialTime = 5f;
    public float bulletZFightingOffset = 0.3f;
    //public float timeBetweenShots = 0.1f;
    private float shotCounter;
    public float muzzleDisplayTime;
    private float muzzleCounter;

    public float maxHeatValue = 10f, /*heatPerShot = 1f,*/ coolRate = 4f, overheatCoolRate = 5f;
    private float heatCounter;
    private bool overheated;

    public Gun[] allGuns;
    private int selectedGun;

    public GameObject playerHitImpact;

    [Header("Player Health Points")]
    public int maxHealth = 100;
    private int currentHP;

    [Header("Player Animation")]
    public Animator anim;
    public GameObject playerModel;
    public Transform modelGunPoint, gunHolder;

    [Header("Skins")]
    public Material[] allSkins;

    [Header("Zoom Settings")]
    public float adsSpeed = 5f;
    public Transform adsOutPoint;
    public Transform adsInPoint;



    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;

        UIController.instance.weaponTempSlider.maxValue = maxHeatValue;
        

        selectedGun = 0;

        photonView.RPC("SetGun", RpcTarget.All , selectedGun);

        currentHP = maxHealth;

        if(photonView.IsMine)
        {
            //playerModel.SetActive(false);

            UIController.instance.currentHPSlider.maxValue = maxHealth;
            UIController.instance.currentHPSlider.value = currentHP;
        }
        else
        {
            playerModel.SetActive(true);

            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }

        playerModel.GetComponentInChildren<Renderer>().material = allSkins[photonView.Owner.ActorNumber % allSkins.Length]; 

        
        
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine)
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
                if(Input.GetMouseButtonDown(0) && !UIController.instance.optionsScreen.activeInHierarchy)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }

            if(allGuns[selectedGun].muzzleFlash.activeInHierarchy)
            {
                muzzleCounter -= Time.deltaTime;

                if(muzzleCounter <= 0)
                {
                    allGuns[selectedGun].muzzleFlash.SetActive(false);

                }
            }
            

            if(!overheated)
            {

                if(Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }

                if(Input.GetMouseButton(0) && allGuns[selectedGun].isAutomatic)
                {
                    shotCounter -= Time.deltaTime;

                    if(shotCounter <= 0f)
                    {
                        Shoot();
                    }
                }

                heatCounter -= coolRate * Time.deltaTime;

            }
            else
            {
                heatCounter -= overheatCoolRate * Time.deltaTime;
                if(heatCounter <= 0f)
                {
                    heatCounter = 0f;

                    overheated = false;

                    UIController.instance.overheatedMessage.gameObject.SetActive(false);
                }
            }

            if(heatCounter < 0f)
            {
                heatCounter = 0f;
            }

            UIController.instance.weaponTempSlider.value = heatCounter;

            if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            {
                selectedGun++;

                if(selectedGun >= allGuns.Length)
                {
                    selectedGun = 0;
                }
                photonView.RPC("SetGun", RpcTarget.All , selectedGun);
            }
            else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                selectedGun--;

                if(selectedGun <= 0)
                {
                    selectedGun = allGuns.Length - 1;
                }
                photonView.RPC("SetGun", RpcTarget.All , selectedGun);
            }

            for(int i = 0; i < allGuns.Length; i++)
            {
                if(Input.GetKeyDown((i + 1).ToString()))
                {
                    selectedGun = i;
                    photonView.RPC("SetGun", RpcTarget.All , i);
                }

            }

            UIController.instance.currentHPSlider.value = currentHP;

        }



        anim.SetBool("grounded", isGrounded);
        anim.SetFloat("speed", moveDirection.magnitude);

        if(Input.GetMouseButton(1))
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, allGuns[selectedGun].adsZoom, adsSpeed * Time.deltaTime);
            gunHolder.position = Vector3.Lerp(gunHolder.position, adsInPoint.position, adsSpeed * Time.deltaTime);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, adsSpeed * Time.deltaTime);
            gunHolder.position = Vector3.Lerp(gunHolder.position, adsOutPoint.position, adsSpeed * Time.deltaTime);
        }

    }

    private void LateUpdate() 
    {
        if(photonView.IsMine)
        {
            if(MatchManager.instance.state == MatchManager.GameState.Playing)
            {
                cam.transform.position = viewPoint.transform.position;
                cam.transform.rotation = viewPoint.transform.rotation;
            }
            else
            {
                cam.transform.position = MatchManager.instance.mapCamPoint.position;
                cam.transform.rotation = MatchManager.instance.mapCamPoint.rotation;
            }
            
        }
    }

    private void Shoot()
    {
        // Shoot out a ray from the middle of the screen
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        
        Vector3 rayOffset = transform.position + new Vector3(0f, (1.1f * 0.5f), 0f);
        
        ray.origin = rayOffset;

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            if(hit.collider.gameObject.CompareTag("Player"))
            {
                
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);

                hit.collider.gameObject.GetPhotonView().RPC(
                    nameof(DealDamage), 
                    RpcTarget.All, 
                    photonView.Owner.NickName, 
                    allGuns[selectedGun].shotDamage,
                    PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {

                GameObject bulletImpactObject =  Instantiate(bulletImpact, hit.point + (hit.normal * bulletZFightingOffset), Quaternion.LookRotation(hit.normal, Vector3.up));

                Destroy(bulletImpactObject, bulletImpactSurvialTime);

            }

            

        }

        shotCounter =  allGuns[selectedGun].timeBetweenShots;

        heatCounter += allGuns[selectedGun].heatPerShot;

        if(heatCounter >= maxHeatValue)
        {
            heatCounter = maxHeatValue;

            overheated = true;

            UIController.instance.overheatedMessage.gameObject.SetActive(true);
        }

        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
        
        
    }

    [PunRPC]
    public void DealDamage(string damager, int damageAmount, int actor)
    {
        TakeDamage(damager, damageAmount, actor);
    }

    public void TakeDamage(string damager, int damageAmount, int actor)
    {
        if(photonView.IsMine)
        {

            currentHP -= damageAmount;
            if(currentHP <= 0 )
            {
                currentHP  = 0;
                
                PlayerSpawner.instance.Die(damager);

                // 1 kill
                MatchManager.instance.UpdateStatsSend(actor, 0, 1);

            }

            UIController.instance.currentHPSlider.value = currentHP;

        }


    }

    

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, groundCheckPoint.position + new Vector3(0f, -2f, 0f));  
    }

    private void SwitchGun(int selectedGun)
    {
        foreach (Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }

        allGuns[selectedGun].gameObject.SetActive(true);

        allGuns[selectedGun].muzzleFlash.SetActive(false);

    }

    [PunRPC]
    public void SetGun(int gunToSwitchTo)
    {
        if(gunToSwitchTo < allGuns.Length)
        {
            selectedGun = gunToSwitchTo;
            SwitchGun(selectedGun);
        }



    }
}
