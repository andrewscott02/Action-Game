using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviourTrees;

public class AIController : CharacterController
{
    #region Setup

    protected GameObject player; public GameObject GetPlayer() { return player; }

    #region Behaviour Tree
    protected NavMeshAgent agent; public NavMeshAgent GetNavMeshAgent() { return agent; }
    public BehaviourTree bt;


    public override void Start()
    {
        base.Start();
        player = GameObject.FindObjectOfType<PlayerController>().gameObject;
        agent = GetComponent<NavMeshAgent>();

        currentDestination = transform.position;

        ActivateAI();
    }

    public virtual void ActivateAI()
    {
        AIManager.instance.AllocateTeam(this);

        bt.Setup(this);
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(currentDestination, distanceAllowance);

        Gizmos.DrawWireSphere(gameObject.transform.position, sightDistance);
        Gizmos.DrawWireSphere(gameObject.transform.position, roamDistance);
        Gizmos.DrawWireSphere(gameObject.transform.position, meleeDistance);

        if (currentTarget != null)
        {
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }

    public virtual void Update()
    {
        if (currentTarget != null)
        {
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;

            Quaternion desiredrot = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(transform.rotation, desiredrot, Time.deltaTime * agent.angularSpeed);
        }

        #region Animation

        Vector3 movement = agent.velocity;
        //movement = transform.TransformDirection(movement);

        //Gets the rotation of the model to offset the animations
        Vector2 realMovement = new Vector2(0, 0);
        realMovement.x = Vector3.Dot(movement, model.right);
        realMovement.y = Vector3.Dot(movement, model.forward);

        //Sets the movement animations for the animator
        //Debug.Log("X:" + rb.velocity.x + "Y:" + rb.velocity.z);
        animator.SetFloat("xMovement", Mathf.Lerp(animator.GetFloat("xMovement"), realMovement.x, lerpSpeed));
        animator.SetFloat("yMovement", Mathf.Lerp(animator.GetFloat("yMovement"), realMovement.y, lerpSpeed));

        #endregion
    }

    public float distanceAllowance = 1f;

    public float lerpSpeed = 0.01f;

    public float sightDistance = 40;
    public float roamDistance = 25;
    public float meleeDistance = 3;

    public Vector3 followVector;
    public float followDistance = 5;

    #endregion

    #region Behaviours

    #region Movement
    protected Vector3 currentDestination; public Vector3 GetDestination() { return currentDestination; }
    public void SetDestinationPos(Vector3 pos)
    {
        currentDestination = pos;
    }
    public bool roaming = false;
    public void MoveToDestination()
    {
        agent.SetDestination(currentDestination);
    }
    public bool NearDestination(float distanceAllowance)
    {
        return Vector3.Distance(transform.position, currentDestination) <= distanceAllowance;
    }

    bool NearDestination()
    {
        return Vector3.Distance(transform.position, currentDestination) < distanceAllowance;
    }

    #endregion

    public CharacterController currentTarget;

    public bool AttackTarget(CharacterController targetCheck)
    {
        if (targetCheck == null)
            return false;

        float distance = Vector3.Distance(this.gameObject.transform.position, targetCheck.gameObject.transform.position);
        //Debug.Log("Attack called");
        if (distance < meleeDistance)
        {
            //Debug.Log("Attack made");
            combat.LightAttack();

            return true;
        }

        return false;
    }

    #endregion
}
