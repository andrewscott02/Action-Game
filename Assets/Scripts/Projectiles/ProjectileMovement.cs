using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{
    public Vector3 lastPos { get; private set; }
    public float projectileSpeed = 1;
    public float mass = 0;

    Rigidbody rb;
    ProjectileHit hit;

    public void Fire(Vector3 target, TrapStats trap, GameObject caster, int overrideDamage = 0, float overrideSpeed = 0)
    {
        Vector3 dir = target - caster.transform.position;
        transform.rotation = Quaternion.LookRotation(dir.normalized, transform.up);

        rb = GetComponent<Rigidbody>();
        hit = GetComponentInChildren<ProjectileHit>();
        hit.trapStats = trap;
        hit.damage = overrideDamage == 0 ? trap.damage : overrideDamage;
        hit.caster = caster;
        hit.casterDamage = caster.GetComponent<ICanDealDamage>();
        hit.move = this;

        rb.mass = this.mass;
        projectileSpeed = overrideSpeed == 0 ? projectileSpeed : overrideSpeed;

        Vector3 force = DetermineForce(target);
        rb.velocity = force;
    }

    Vector3 DetermineForce(Vector3 targetPos)
    {
        //https://discussions.unity.com/t/getting-launch-angle-for-projectile-given-height-distance-and-speed-in-3d/182573
        Vector3 dir = targetPos - transform.position;
        Vector3 launchAngle = Vector3.up;

        float gSquared = Physics.gravity.sqrMagnitude;
        float b = projectileSpeed * projectileSpeed + Vector3.Dot(dir, Physics.gravity);
        float discriminant = b * b - gSquared * dir.sqrMagnitude;

        if (discriminant >= 0)
        {
            float discRoot = Mathf.Sqrt(discriminant);
            float tMax = Mathf.Sqrt((b + discRoot) * 2 / gSquared);
            float tMin = Mathf.Sqrt((b - discRoot) * 2 / gSquared);
            float tLowEnergy = Mathf.Sqrt(Mathf.Sqrt(dir.sqrMagnitude * 4 / gSquared));

            float time = tMin;

            launchAngle = dir / time - Physics.gravity * time / 2;
        }

        //Debug.Log("Determine force " + launchAngle);
        return launchAngle;
    }

    private void FixedUpdate()
    {
        lastPos = transform.position;
    }
}
