using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTrees;

public class GetClosestEnemyToTarget : Node
{
    public AIController agent;
    public GameObject target;

    /// <summary>
    /// Commands an agent to get the closest enemy to a specified target
    /// </summary>
    /// <param name="agent">The agent this command is given to</param>
    /// <param name="target">The target of the check</param>
    public GetClosestEnemyToTarget(AIController agent, GameObject target)
    {
        this.agent = agent;
        this.target = target;
    }

    public override NodeState Evaluate()
    {
        BaseCharacterController enemy = HelperFunctions.GetClosestEnemy(agent, target.transform.position, agent.GetSightDistance(), false);
        if (enemy != null)
        {
            agent.SetDestinationPos(enemy.transform.position);
            Debug.Log("Generated point at near target: " + enemy.name);

            agent.currentTarget = enemy;
            agent.alert = true;
            state = NodeState.Success;
        }
        else
        {
            Debug.Log("Failed to get enemy near target");

            state = NodeState.Failure;
        }

        return state;
    }
}
