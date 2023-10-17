using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseCharacterController : MonoBehaviour
{
    public bool invisible = false;
    protected Animator animator;
    protected CharacterCombat combat; public CharacterCombat GetCharacterCombat() { return combat; }
    protected Health health; public Health GetHealth() { return health; }
    public Transform model;

    public bool playerTeam = true;

    public virtual void Start()
    {
        animator = GetComponent<Animator>();

        health = GetComponent<Health>();
        health.animator = animator;
        combat = GetComponent<CharacterCombat>();

        SetupRagdoll();
    }

    Collider mainCollider;
    Collider chestCollider;
    List<Collider> ragdollColliders = new List<Collider>();

    void SetupRagdoll()
    {
        mainCollider = GetComponent<Collider>();
        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (var item in colliders)
        {
            if (item != mainCollider)
            {
                if (item.CompareTag("Chest"))
                    chestCollider = item;

                Rigidbody rbItem = item.GetComponent<Rigidbody>();
                rbItem.useGravity = false;

                item.isTrigger = this;
                ragdollColliders.Add(item);
            }
        }
    }

    public virtual void ActivateRagdoll(bool activate, ExplosiveForceData forceData)
    {
        foreach (var item in ragdollColliders)
        {
            item.gameObject.layer = activate ? 2 : 6;

            if (item != mainCollider)
            {
                Rigidbody rbItem = item.GetComponent<Rigidbody>();
                rbItem.useGravity = activate;

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

        animator.enabled = !activate;

        if (activate && health.dying)
        {
            combat.weapon.Disarm();
        }
    }
}
