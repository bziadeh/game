using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;
using EZCameraShake;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerCombat : NetworkBehaviour
{

    [SerializeField] private Camera camera;
    private CameraShaker cameraShaker;

    // swing sword sounds
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private AudioClip[] swingClips;


    // passive status sounds
    [SerializeField] public AudioClip passiveClip;
    [SerializeField] public AudioClip activeClip;


    // current weapon
    [SerializeField] private GameObject weapon;

    // player animator
    private ClientNetworkAnimator animator;
    private PlayerMovement movement;

    private Transform currentTransform;
    private Transform currentGripRef;

    // objects that store transform data
    [SerializeField] private GameObject oneHandPassive;
    [SerializeField] private GameObject oneHandActive;

    [SerializeField] private GameObject weaponPassiveParent;
    [SerializeField] private GameObject weaponActiveParent;

    private bool blocking;

    private float originalSpeed;
    private float lastAttack = -1.0f;

    public Transform hipsTransform;
    public Transform leftHandTransform;
    public float sequence = 0;
    public float magnitude, roughness, fadeIn, fadeOut;

    // network variable for weapon position
    public NetworkVariable<bool> passive = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        animator = GetComponent<ClientNetworkAnimator>();
        movement = GetComponent<PlayerMovement>();

        // setup camera shaker
        cameraShaker = camera.GetComponent<CameraShaker>();

        // setup init stance and aggro settings
        blocking = false;

        // store original movement speed so we can restore if needed
        originalSpeed = movement.speed;
        lastAttack = Time.time;

        currentTransform = weapon.GetComponent<Transform>();
        currentGripRef = weapon.transform.Find("grip_ref");

        passive.OnValueChanged += (bool oldValue, bool newValue) => UpdatePassive(oldValue, newValue);
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Change Between Passive and Active Stance
        if (Input.GetKeyDown(KeyCode.Z))
        {
            passive.Value = !passive.Value;
        }

        // Basic Attack using Left-Click
        if(Input.GetMouseButtonDown(0))
        {
            if (passive.Value == false)
            {
                if(lastAttack == -1.0f)
                {
                    animator.Animator.SetFloat("LastAttack", 1.0f); // 1 second ago 
                } 
                else
                {
                    float elapsed = Time.time - lastAttack;

                    // MUST BE THE SAME AS ANIMATOR CONSTRAINTS 
                    if(elapsed > 0.40f) 
                    {
                        if (sequence == 0)
                        {
                            animator.SetTrigger("Attack1");
                            sequence++;
                        }

                        else
                        {
                            animator.SetTrigger("Attack2");
                            sequence = 0; // reset sequence
                        }

                        StartCoroutine(PlaySound());
                        lastAttack = Time.time;
                    }
                    animator.Animator.SetFloat("LastAttack", elapsed);
                }
            }
        }

        SetBlockingState();
       
        animator.Animator.SetBool("isBlocking", blocking);

        if (Time.time - lastAttack < 1)
        {
            // movement.speed = originalSpeed / 2;
        }
        else
        {
            if (movement.speed != originalSpeed) movement.speed = originalSpeed;
        }
    }

    IEnumerator PlaySound()
    {
        yield return new WaitForSeconds(0.4f);
        playerAudioSource.PlayOneShot(swingClips[Random.Range(0, swingClips.Length)], 1.5f);
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
    }

    [ClientRpc]
    private void SetPassiveClientRpc(bool passive)
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
        
        weapon.transform.parent = passive ? weaponPassiveParent.transform : weaponActiveParent.transform;
        playerAudioSource.PlayOneShot(passive ? passiveClip : activeClip, 0.5f);
    }

    private void UpdatePassive(bool oldStatus, bool newStatus)
    {
        SetPassiveClientRpc(newStatus);
    }
}
