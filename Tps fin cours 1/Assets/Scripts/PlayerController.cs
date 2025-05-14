using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float fightSpeed;
    [SerializeField] private GameObject CameraReference;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private GameObject colliderHead;
    [SerializeField] private bool crouch = false;
    [SerializeField] private GameObject attackRef;
    private bool run;
    private bool fight;
    private bool canMove = true;
    private Transform actualCamera;
    private float originalSpeed;
    

    private Rigidbody rbComponent;
    private Animator animatorComponent;
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        originalSpeed = moveSpeed;
        rbComponent = GetComponent<Rigidbody>();
        if (rbComponent == null)
        {
            Debug.LogWarning("Aucun rigidbody : " + gameObject.name);
        }
        

        animatorComponent = GetComponent<Animator>();
        if (animatorComponent == null)
        {
            Debug.LogWarning("Aucun animator : " + gameObject.name);
        }
    }
    void Update()
    {
        actualCamera = CameraReference.transform;
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 camForward = actualCamera.forward;
        Vector3 deplacement = verticalInput * camForward + horizontalInput * actualCamera.right;
        deplacement.y = 0;
        if (canMove)
        {
            Vector3 deplacementFinal = Vector3.ClampMagnitude(deplacement,1) * moveSpeed;
            rbComponent.linearVelocity = new Vector3(deplacementFinal.x, rbComponent.linearVelocity.y, deplacementFinal.z);
        }
        if (deplacement != Vector3.zero)
        {
            rbComponent.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(deplacement), Time.deltaTime * rotationSpeed);
        }
        if (horizontalInput != 0 || verticalInput != 0)
        {
            animatorComponent.SetBool("isWalking", true);
        }
        else 
        {
            animatorComponent.SetBool("isWalking", false);
            animatorComponent.SetBool("isRunning", false);
        }
        
        if (Input.GetKeyDown("f"))
        {
            fight = !fight;
        }
        if (fight)
        {
            animatorComponent.SetBool("isFighting", true);
            moveSpeed = fightSpeed;
            if (Input.GetMouseButtonDown(0))
            {
                animatorComponent.SetBool("normalAttack", true);
                canMove = false;
                StartCoroutine(WaitForAttack());
            }
            else if (Input.GetMouseButtonDown(1))
            {
                animatorComponent.SetBool("bigAttack", true);
                canMove = false;
                StartCoroutine(WaitForAttack());
            }
        }
        else
        {
            animatorComponent.SetBool("isFighting", false);
            moveSpeed = originalSpeed;
        
            if (Input.GetKeyDown("left shift"))
            {
                run = true;
                animatorComponent.SetBool("isRunning", true);
                moveSpeed = runSpeed;
            }
            else if (Input.GetKeyUp("left shift"))
            {
                run = false;
                animatorComponent.SetBool("isRunning", false);
                moveSpeed = originalSpeed;
            }
        
            if (Input.GetKeyDown("c"))
            {
                crouch = !crouch;
            }

            if (crouch)
            {
                animatorComponent.SetBool("isCrounching", true);
                moveSpeed = crouchSpeed;
                colliderHead.SetActive(false);
            }
            else 
            {
                animatorComponent.SetBool("isCrounching", false);
                colliderHead.SetActive(true);
                if (run)
                {
                    moveSpeed = runSpeed;
                }
                else
                {
                    moveSpeed = originalSpeed;
                }
            }
            if (Input.GetMouseButtonDown(0) && canMove)
            {
                animatorComponent.SetBool("isKilling", true);
                canMove = false;
                StartCoroutine(WaitForAnim("isKilling"));
                StartCoroutine(TriggerActive());
            }
        }
        
    }
    IEnumerator WaitForAttack()
    {
        yield return new WaitForSeconds(0.5f);
        animatorComponent.SetBool("normalAttack", false);
        animatorComponent.SetBool("bigAttack", false);
        yield return new WaitForSeconds(1.2f);
        canMove = true;
    }
    

    IEnumerator WaitForAnim(string anim)
    {
        yield return new WaitForSeconds(0.5f);
        animatorComponent.SetBool(anim, false);
        yield return new WaitForSeconds(2f);
        canMove = true;
        
    }

    IEnumerator TriggerActive()
    {
        yield return new WaitForSeconds(0.5f);
        attackRef.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        attackRef.SetActive(false);
    }
}
    
    
    
    
    
    