using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>
/// This MonoBehaviour defins how the column moves. 
/// It uses Unity's NavMeshAgent for pathfinding. 
/// Warning : The NavMeshAgent's speed need to be setup to 0. This script will be moving the agent in order to match the agent's speed to the real column's speed. 
/// There is a NavMeshObstacle setup in the player's area (The pathfindign system will take that into consideration and automatically avoid that area)    
/// In the Update function we calculate a new target for the column position based on the player's position and rotation. 
/// Once a new target found, we use the NavMesh pathfinding system to get the waypints.
/// For each waypoint, we calculate a speedvector that we apply to the column using VP.
/// </summary>
public class MultipleObjectInteractionColumnBehaviour : MonoBehaviour
{
    


    [Header("Behaviour properties")]
    public float closestDistanceToSnap; 

    [Header("AI Navigation")]
    public Vector3 targetPosition;
    public NavMeshAgent navMeshAgent;

    [Header("Events")]
    public bool onTarget; 
    public UnityEvent OnTargetReach;
    public UnityEvent OnTargetStay;
    public UnityEvent OnTargetLeave; 

    private MultipleObjectsInteractionSceneManager sceneManager;
    private SecurityManager securityManager;

    

    #region Unity's Methods
    private void Start()
    {
        // Fetch all the references needed. 
        sceneManager = MultipleObjectsInteractionSceneManager.Instance;
        if (sceneManager == null) Debug.LogError("No MultipleObjectsInteractionSceneManager found.");

        securityManager = GameObject.FindGameObjectWithTag("ColumControl").GetComponent<SecurityManager>();
        if (securityManager == null) Debug.LogError("No SecurityManager found.");

        if (navMeshAgent == null) Debug.LogError("No NavMeshAgent assigned.");

        // Set position to match the realColumnPosition :
        //THis doesn't really work becaus 
        transform.position = new Vector3(sceneManager.column.position.x, 0, sceneManager.column.position.z);

        // TODO: Change column mode to VP.


    }

    private void Update()
    {

        // targe is only setup based on the scenario. 
        if (sceneManager.simulatedVOI == null) return; 


        MoveColumnToTarget();
        OnTargetUpdate();
        return; 
        
    }

    private void OnDrawGizmos()
    {
        // Draw the path followed by the NavMeshAgent to reach the currentTarget. 
        Gizmos.color = Color.yellow;
        foreach(var point in navMeshAgent.path.corners)
            Gizmos.DrawSphere(point, 0.1f);

        for (int i = 1; i < navMeshAgent.path.corners.Length; i++)
            Gizmos.DrawLine(navMeshAgent.path.corners[i - 1], navMeshAgent.path.corners[i]);
    }

    #endregion

    #region Calculate New Target
    
    private void CalculateNewTarget()
    {
        /*
        To calculate a new target : 
        - We calcualte the distance to the closest VOI. 
        - if this distance < to a threshold than define the VOI position as the target. 
        - Else we calculate a meanPosition of the active VOIs using the CalculateMeanVOIsPos(). 
         */
        sceneManager.simulatedVOI = null; 
        VOIBehaviour closestVOI = sceneManager.GetClosestVOIToPlayer();
        Vector3 playerPosProjection = new Vector3(sceneManager.player.position.x, 0, sceneManager.player.position.z);
        Vector3 closestVOIPosProjection = new Vector3(closestVOI.transform.position.x, 0, closestVOI.transform.position.z);

        if (Vector3.Distance(playerPosProjection, closestVOIPosProjection) < closestDistanceToSnap)
        {
            targetPosition = closestVOI.transform.position;
            sceneManager.simulatedVOI = closestVOI; 
            return; 
        }   
        
        Vector3 meanVOIPos = CaculateMeanVOIsPos();
        targetPosition = meanVOIPos;
        navMeshAgent.SetDestination(targetPosition);
    }

    private Vector3 CaculateMeanVOIsPos()
    {
        // This calculate a weighted position based on the VOIs position and the VOIs weights. 

        float weightMax = float.NegativeInfinity;
        Vector3 weightMaxPos = Vector3.zero;
        float weightSum = 0;
        Vector3 voiWeightedPos = Vector3.zero;

        foreach (var voi in sceneManager.VOIs)
        {
            if (!voi.isActive) continue;

            if (voi.weight > weightMax)
            {
                weightMax = voi.weight;
                weightMaxPos = voi.transform.position;
            }

            weightSum += voi.weight;
            voiWeightedPos += voi.transform.position * voi.weight;
        }
        voiWeightedPos = voiWeightedPos / weightSum;
        return voiWeightedPos;
    }

    public void SetNewTargert(Vector3 newTarget)
    {
        targetPosition = newTarget;
        navMeshAgent.SetDestination(targetPosition);
        onTarget = false; 
    }

    #endregion

    #region Move column to target

    /// <summary>
    /// How it works : 
    ///    - Update the destination of the NavMeshAgent.
    ///    - Adjust the position of the transform to match the column's position. 
    ///    - Get the second corner of the path found by the NavMeshAgent.
    ///    - Calculate a speed vector based on the remaining distance.
    ///    - Setup the VP values based on the speed vector
    /// </summary>
    private void MoveColumnToTarget()
    {
        if (onTarget)
            return; 

        transform.position = new Vector3(sceneManager.column.position.x, 0, sceneManager.column.position.z);

        Vector3 waypoint = navMeshAgent.path.corners[1];

        if (waypoint != targetPosition) targetPosition = waypoint;
        Vector3 actualPos = sceneManager.column.position;
        actualPos.y = waypoint.y;
        Vector3 speedVector = (waypoint - actualPos).normalized * (navMeshAgent.remainingDistance == float.PositiveInfinity ? 1 : navMeshAgent.remainingDistance);

        securityManager.acX._VPSliderValue = Mathf.Abs(speedVector.z) < 0.001 ? 0 : speedVector.z;
        securityManager.acY._VPSliderValue = Mathf.Abs(speedVector.x) < 0.001 ? 0 : speedVector.x;
    }
    
    private void OnTargetUpdate()
    {
        // Implementing events

        bool lastOnTarget = onTarget; 

        if (securityManager.acX._VPSliderValue == 0 && securityManager.acY._VPSliderValue == 0)
        {
            onTarget = true; 
        }

        if (!lastOnTarget & onTarget)
        {
            sceneManager.simulatedVOI.GetComponent<Outline>().OutlineColor = Color.green; 
            OnTargetReach.Invoke();
            return; 
        }

        if (lastOnTarget && onTarget)
        {
            OnTargetStay.Invoke();
            return; 
        }

        if (lastOnTarget && !onTarget)
        {
            OnTargetLeave.Invoke();
            return; 
        }
    }
    
    #endregion


}
