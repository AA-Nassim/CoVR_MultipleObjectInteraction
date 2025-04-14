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
                if (GUILayout.Button("Select surface 1")) SelectVOI(0);
                if (GUILayout.Button("Select surface 2")) SelectVOI(1);
                if (GUILayout.Button("Select surface 3")) SelectVOI(2);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select surface 4")) SelectVOI(3);
                if (GUILayout.Button("Select surface 5")) SelectVOI(4);
                if (GUILayout.Button("Select surface 6")) SelectVOI(5);
            GUILayout.EndHorizontal();
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

    private void SelectVOI(int i)
    {
        sceneManager.SelectSurface(i);
    }
}