using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseCharacterController : MonoBehaviour
{
    public bool invisible = false;
    public bool checkedInRoomBounds = true;
    protected Animator animator;
    protected CharacterCombat combat; public CharacterCombat GetCharacterCombat() { return combat; }
    protected Health health; public Health GetHealth() { return health; }
    public Transform model;

    public bool playerTeam = true;

    public virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        health = GetComponent<Health>();
        health.animator = animator;
        combat = GetComponent<CharacterCombat>();

        characterDied += Died;

        SetupRagdoll();
    }

    public Rigidbody rb { get; protected set; }

    public Collider mainCollider { get; private set; }
    Collider chestCollider;
    List<Collider> ragdollColliders = new List<Collider>();

    void SetupRagdoll()
    {
        mainCollider = GetComponent<Collider>();
        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (var item in colliders)
        {
            if (item != mainCollider && !item.CompareTag("Weapon") && !item.CompareTag("IgnoreRagdoll"))
            {
                if (item.CompareTag("Chest"))
                    chestCollider = item;

                Rigidbody rbItem = item.GetComponent<Rigidbody>();
                rbItem.useGravity = false;
                //rbItem.detectCollisions = false;
                rbItem.isKinematic = true;

                item.isTrigger = true;
                ragdollColliders.Add(item);
            }
        }

        animator.enabled = true;
    }

    public virtual void ActivateRagdoll(bool activate, ExplosiveForceData forceData, bool disableAnimator = true)
    {
        foreach (var item in ragdollColliders)
        {
            if (item == null) break;

            item.gameObject.layer = activate ? 2 : 6;

            if (item != mainCollider)
            {
                Rigidbody rbItem = item.GetComponent<Rigidbody>();
                rbItem.useGravity = activate;
                //rbItem.detectCollisions = activate;
                rbItem.isKinematic = !activate;

                if (activate && item == chestCollider)
                {
                    //Debug.Log("Add explosive force");
                    rbItem.AddExplosionForce(forceData.explosiveForce * 100f, forceData.origin, 10f, 1.5f, ForceMode.Impulse);
                }

                item.isTrigger = !activate;
            }
        }

        mainCollider.isTrigger = activate;
        mainCollider.enabled = !activate;

        animator.enabled = !disableAnimator || !activate;

        if (activate && health.dying)
        {
            combat.weapon.Disarm();
        }
    }
    
    public virtual void ChangeTags(bool activate)
    {
        foreach (var item in ragdollColliders)
        {
            if (item == null) break;

            item.gameObject.layer = activate ? 2 : 6;
        }

        mainCollider.gameObject.layer = activate ? 2 : 10;
    }

    public delegate void DiedDelegate(BaseCharacterController controller);
    public DiedDelegate characterDied;

    public Vector2Int goldOnKill;

    public virtual void Killed()
    {
        checkedInRoomBounds = false;

        characterDied(this);
        CameraManager.instance.CombatZoom();
        TreasureManager.instance.D_GiveGold(Random.Range(goldOnKill.x, goldOnKill.y + 1));
    }

    public void Died(BaseCharacterController controller)
    {
        //Blank delegate
        //Debug.Log("Enemy Killed Delegate");
    }
}
