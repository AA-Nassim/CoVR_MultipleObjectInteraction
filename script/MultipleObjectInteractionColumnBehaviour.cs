using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; 

public class MultipleObjectInteractionColumnBehaviour : MonoBehaviour
{
    [Header("Behaviour properties")]
    public float closestDistanceToSnap; 

    [Header("AI Navigation")]
    public Vector3 targetPosition;
    public NavMeshAgent navMeshAgent;

    private MultipleObjectsInteractionSceneManager sceneManager;
    private SecurityManager securityManager; 

    #region Unity's Methods
    private void Start()
    {
        sceneManager = MultipleObjectsInteractionSceneManager.Instance;
        if (sceneManager == null) Debug.LogError("No MultipleObjectsInteractionSceneManager found.");

        securityManager = GameObject.FindGameObjectWithTag("ColumControl").GetComponent<SecurityManager>();
        if (securityManager == null) Debug.LogError("No SecurityManager found."); 

        if (navMeshAgent == null) Debug.LogError("No NavMeshAgent assigned.");

        
    }

    private void Update()
    {
        if (!navMeshAgent.isActiveAndEnabled) return; 

        CalculateNewTarget();
        MoveColumnToTarget();
    }

    private void OnDrawGizmos()
    {
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

        // How it's working : 
        // if the player is close to a certain VOI, the column will select it
        // Else calculate the weighted mean of the positions. 

        // Ideas to explore : 
        // Add a minimum threshold for the rotation if plaer is not close to a certain VOI. 
        // If the greatest weight is > then the second greatest weight + threshold : snap. 
        // If the greatest weight > threshold : snap

        VOIBehaviour closestVOI = sceneManager.GetClosestVOIToPlayer();
        Vector3 playerPosProjection = new Vector3(sceneManager.player.position.x, 0, sceneManager.player.position.z);
        Vector3 closestVOIPosProjection = new Vector3(closestVOI.transform.position.x, 0, closestVOI.transform.position.z);

        if (Vector3.Distance(playerPosProjection, closestVOIPosProjection) < closestDistanceToSnap)
        {
            targetPosition = closestVOI.transform.position;
            return; 
        }   
        
        Vector3 meanVOIPos = CaculateMeanVOIsPos();
        targetPosition = meanVOIPos; 
    }

    private Vector3 CaculateMeanVOIsPos()
    {
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
    
    #endregion

    #region Move column to target

    private void MoveColumnToTarget()
    {
        navMeshAgent.SetDestination(targetPosition);

        transform.position = new Vector3(sceneManager.column.position.x, 0, sceneManager.column.position.z);

        if (navMeshAgent.path.corners.Length == 2)
        {
            if (!securityManager.IsTracking && !securityManager.IsPPEnabled)
            {
                securityManager.acX.VPEnable(false);
                securityManager.acY.VPEnable(false);
                securityManager.ChangePPControl(true);
                securityManager.ChangeTrackingStatus(true);
            }

            if (targetPosition != navMeshAgent.destination)
            {
                targetPosition = navMeshAgent.destination;
                securityManager.ChangeDefaultTrackedObjectPos(targetPosition);
            }
            return; 
        }

        if (navMeshAgent.path.corners.Length > 2)
        {
            if (!securityManager.acX._VPEnabled || !securityManager.acY._VPEnabled || securityManager.IsPPEnabled)
            {
                securityManager.ChangePPControl(false);
                securityManager.ChangeTrackingStatus(false);
                securityManager.acX.VPEnable(true);
                securityManager.acY.VPEnable(true);
            }

            Vector3 waypoint = navMeshAgent.path.corners[1];

            if (waypoint != targetPosition) targetPosition = waypoint;
            Vector3 actualPos = sceneManager.column.position;
            actualPos.y = waypoint.y;
            Vector3 speedVector = (waypoint - actualPos).normalized * (navMeshAgent.remainingDistance == float.PositiveInfinity ? 1 : navMeshAgent.remainingDistance);

            securityManager.acX._VPSliderValue = Mathf.Abs(speedVector.z) < 0.001 ? 0 : speedVector.z;
            securityManager.acY._VPSliderValue = Mathf.Abs(speedVector.x) < 0.001 ? 0 : speedVector.x;

            return; 
        }
    }



    #endregion


}
