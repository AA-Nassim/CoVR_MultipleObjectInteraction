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
        GUILayout.Label("Debug"); 
        if (GUILayout.Button("Print VOIs Weights")) PrintVOIsWeights();
        if (GUILayout.Button("Match column")) MatchColumn();

        GUILayout.Label("Systemic demo");
        GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select VOI type A")) SelectRandomVOI(VOIType.TypeA);
                if (GUILayout.Button("Select VOI type B")) SelectRandomVOI(VOIType.TypeB);
            GUILayout.EndHorizontal();

                if (GUILayout.Button("Select VOI Surface")) SelectRandomVOI(VOIType.Surfaces);

        GUILayout.EndVertical(); 
    }

    private void PrintVOIsWeights()
    {
        foreach(VOIBehaviour voi in sceneManager.VOIs)
        {
            Debug.Log("voi : " + voi.gameObject.name + " --- w : " + voi.weight);
        }
    }

    private void MatchColumn()
    {
        sceneManager.columnBehaviour.transform.position = new Vector3(sceneManager.column.position.x, 0, sceneManager.column.position.z);

    }

    private void SelectRandomVOI(VOIType type)
    {
        sceneManager.SelectRandomVOI(type);
        
    }

}