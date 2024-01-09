using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enfabler.Attacking;

public class Trap : MonoBehaviour, ICanDealDamage
{
    public TrapStats trapStats;
    TrapApplyEffect applyEffect;
    Collider applyCollider;

    private void Awake()
    {
        applyEffect = GetComponentInChildren<TrapApplyEffect>();
        applyCollider = applyEffect.GetComponent<Collider>();

        SphereCollider sphereCollider = applyCollider as SphereCollider;

        if (sphereCollider != null)
            sphereCollider.radius = trapStats.range;
    }

    public MonoBehaviour GetScript()
    {
        return this;
    }

    public void ActivateTrap()
    {
        switch (trapStats.durationType)
        {
            case E_Duration.OnlyOnce:
                EffectTrigger();
                break;
            case E_Duration.OnceDestroy:
                EffectTrigger();
                DeactivateTrap();
                Destroy(this.gameObject, 0.15f);
                break;
            case E_Duration.Interval:
                TrapManager.instance.TrapActivation += EffectTrigger;
                break;
            default:

                break;
        }
    }

    public void DeactivateTrap()
    {
        if (trapStats.durationType == E_Duration.Interval)
        {
            TrapManager.instance.TrapActivation -= EffectTrigger;
        }
    }

    void EffectTrigger()
    {
        if (trapStats.durationType == E_Duration.Interval)
        {
            if (TrapManager.instance.trapTime % trapStats.activateInterval != 0)
                return;
        }

        applyEffect.Activate();
    }

    public E_DamageEvents ApplyEffect(IDamageable damageable, Vector3 hitPos)
    {
        return DealDamage(damageable, trapStats.damage, hitPos, new Vector3(0, 0, 0));
    }

    private void OnDrawGizmosSelected()
    {
        if (trapStats != null)
            Gizmos.DrawWireSphere(transform.position, trapStats.range);
    }

    public E_DamageEvents DealDamage(IDamageable target, int damage, Vector3 spawnPos, Vector3 spawnRot, E_AttackType attackType = E_AttackType.None)
    {
        return target.Damage(this, damage, spawnPos, spawnRot);
    }

    public bool HitDodged()
    {
        return false;
    }

    public bool HitBlocked(IDamageable other)
    {
        return trapStats.canBlock;
    }

    public bool HitParried(IDamageable other)
    {
        return trapStats.canParry;
    }

    private void OnDestroy()
    {
        DeactivateTrap();
    }

    private void OnDisable()
    {
        DeactivateTrap();
    }
}
