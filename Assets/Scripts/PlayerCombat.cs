using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour
{
    public Camera camera;
    // current weapon
    [SerializeField] private GameObject weapon;

    // player animator
    private Animator animator;
    private PlayerMovement movement;

    private Transform currentTransform;
    private Transform currentGripRef;

    // objects that store transform data
    [SerializeField] private GameObject oneHandPassive;
    [SerializeField] private GameObject oneHandActive;

    [SerializeField] private GameObject weaponPassiveParent;
    [SerializeField] private GameObject weaponActiveParent;

    private bool passive;
    private bool blocking;

    private float originalSpeed;
    private float lastAttack;

    public Transform hipsTransform;
    public Transform leftHandTransform;

    private void Start()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();

        // setup init stance and aggro settings
        passive = true;
        blocking = false;

        // store original movement speed so we can restore if needed
        originalSpeed = movement.speed;
        lastAttack = Time.time;

        currentTransform = weapon.GetComponent<Transform>();
        currentGripRef = weapon.transform.Find("grip_ref");
    }

    private void Update()
    {
        // Change Between Passive and Active Stance
        if (Input.GetKeyDown(KeyCode.Z))
        {
            SetPassive(!passive);
            passive = !passive;
        }

        // Basic Attack using Left-Click
        if(Input.GetMouseButtonDown(0))
        {
            if (passive == false)
            {
                lastAttack = Time.time;
                animator.SetTrigger("Attack1");
            }
        }

        SetBlockingState();
       
        animator.SetBool("isBlocking", blocking);

        if (Time.time - lastAttack < 1)
        {
            // movement.speed = originalSpeed / 2;
        }
        else
        {
            if (movement.speed != originalSpeed) movement.speed = originalSpeed;
        }
    }

    private void SetBlockingState()
    {
        // determine blocking state
        if (Input.GetMouseButtonUp(1) && blocking)
        {
            blocking = false;
        }
        else if (Input.GetMouseButtonDown(1) && !blocking)
        {
            blocking = true;
        }

        if(blocking)
        {

        }
    }

    private void SetPassive(bool passive)
    {
        Transform transform;
        Transform gripRef;

        if (passive)
        {
            transform = oneHandPassive.GetComponent<Transform>();
            gripRef = oneHandPassive.transform.Find("grip_ref");
        }
        else
        {
            transform = oneHandActive.GetComponent<Transform>();
            gripRef = oneHandActive.transform.Find("grip_ref");
        }

        // update transform data
        currentTransform.position = transform.position;
        currentTransform.rotation = transform.rotation;
        currentGripRef.position = gripRef.position;
        currentGripRef.rotation = gripRef.rotation;

        weapon.transform.SetParent(passive ? weaponPassiveParent.transform : weaponActiveParent.transform);
    }
}
