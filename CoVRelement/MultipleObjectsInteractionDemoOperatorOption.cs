using UnityEngine;

public class MultipleObjectsInteractionDemoOperatorOption : DemoOperatorOption
{
    [Header("Debug")]
    public MultipleObjectsInteractionSceneManager sceneManager;


    private void Start()
    {
        sceneManager = MultipleObjectsInteractionSceneManager.Instance;
        if (sceneManager == null) Debug.LogError("No MultipleObjectsInteractionSceneManager found.");
    }

    public override void SceneControlEditorWindow()
    {
        if (GUILayout.Button("Print VOIs Weights")) PrintVOIsWeights();
        if (GUILayout.Button("Position PROP to Closest VOI")) ColumnPositionToColosestVOI();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Grab PROP type A")) GrabPROP(VOIType.TypeA);
        if (GUILayout.Button("Grab PROP type B")) GrabPROP(VOIType.TypeB);
        GUILayout.EndHorizontal(); 
    }

    private void PrintVOIsWeights()
    {
        foreach(VOIBehaviour voi in sceneManager.VOIs)
        {
            Debug.Log("voi : " + voi.gameObject.name + " --- w : " + voi.weight);
        }
    }

    private void ColumnPositionToColosestVOI()
    {
        VOIBehaviour closestVOI = sceneManager.GetClosestVOIToPlayer(); 

        // Update the target of the navmesh agent. 
        sceneManager.columnBehaviour.targetPosition = closestVOI.transform.position;
        sceneManager.columnBehaviour.navMeshAgent.SetDestination(sceneManager.columnBehaviour.targetPosition);
    }

    private void GrabPROP(VOIType type)
    {
     
    }

}