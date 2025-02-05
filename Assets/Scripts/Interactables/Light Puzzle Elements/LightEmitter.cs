using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEmitter : MonoBehaviour, IEmitLight
{
    public bool emitOnStart = true;
    public bool canEmit = true;
    bool emitting = false;
    public bool canHarm = false;

    public LayerMask hitObjects, lightReceivers;
    public GameObject rayObject;
    public float rayRadius = 2f;
    public float maxDistance = 100;

    IReceiveLight lastLightReceiver;

    void Start()
    {
        StopEmitLight();

        if (emitOnStart)
        {
            StartCoroutine(IDelayEmitStart(1f));
        }
    }

    IEnumerator IDelayEmitStart(float delay)
    {
        yield return new WaitForSeconds(delay);

        EmitLight();
    }

    // Update is called once per frame
    void Update()
    {
        rayObject.SetActive(emitting);
        if (!emitting) return;

        RaycastHit rayHit;
        float distance = maxDistance;

        if (SphereCast(rayObject.transform.position, out rayHit, maxDistance, out Collider overrideCol))
        {
            distance = Vector3.Distance(rayObject.transform.position, rayHit.point);

            Collider col = overrideCol == null ? rayHit.collider : overrideCol;

            IReceiveLight lightReceiver = col.GetComponent<IReceiveLight>();

            if (lastLightReceiver != null && lightReceiver != lastLightReceiver)
            {
                lastLightReceiver.StopReceiveLight();

                lastLightReceiver = null;
            }

            if (lightReceiver != null)
            {
                lightReceiver.ReceiveLight(canHarm);
                lastLightReceiver = lightReceiver;
            }
        }
        else
        {
            if (lastLightReceiver != null)
            {
                lastLightReceiver.StopReceiveLight();

                lastLightReceiver = null;
            }
        }

        Vector3 scale = rayObject.transform.localScale;
        scale.z = distance / 2;
        rayObject.transform.localScale = scale;
    }

    bool SphereCast(Vector3 origin, out RaycastHit rayHit, float distance, out Collider overrideCol)
    {
        overrideCol = null;
        if (Physics.SphereCast(origin, rayRadius, transform.forward, out rayHit, distance, hitObjects))
        {
            float distanceLeft = distance - Vector3.Distance(origin, rayHit.point);

            if (rayHit.collider.CompareTag("IgnoreProjectiles") && distanceLeft > 0)
            {
                Debug.LogWarning("SphereCast: Warning, hit an object with ignore projectiles: " + rayHit.collider.name);
                Collider receiverCol = SphereTrigger(rayHit.point);
                if (receiverCol != null)
                {
                    rayHit.point = receiverCol.transform.position;
                    overrideCol = receiverCol;
                    return true;
                }

                SphereCast(rayHit.point, out rayHit, distanceLeft, out overrideCol);
            }

            return true;
        }

        return false;
    }

    Collider SphereTrigger(Vector3 origin)
    {
        Collider[] cols = Physics.OverlapSphere(origin, rayRadius * 2, lightReceivers);
        foreach (var item in cols)
        {
            Debug.LogWarning("SphereCast: Warning, checking object with correct layer: " + item.name);
            IReceiveLight lightReceiver = item.GetComponent<IReceiveLight>();

            if (lightReceiver != null)
            {
                lightReceiver.ReceiveLight(canHarm);
                lastLightReceiver = lightReceiver;
                return item;
            }
        }

        return null;
    }

    Vector3 hitCheck;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(rayObject.transform.position, rayRadius);
        Gizmos.DrawWireSphere(rayObject.transform.position + (transform.forward * maxDistance), rayRadius);

        if (hitCheck != null)
        {
            Gizmos.DrawWireSphere(hitCheck, rayRadius * 2);
        }
    }

    public void EmitLight()
    {
        if (!canEmit) return;
        if (emitting) return;
        //Debug.Log(gameObject.name + " is emitting light");
        emitting = true;
    }

    public void StopEmitLight()
    {
        if (!emitting) return;

        //Debug.Log(gameObject.name + " has stopped emitting light");
        emitting = false;
        if (lastLightReceiver != null)
            lastLightReceiver.StopReceiveLight();
        lastLightReceiver = null;
    }
}
