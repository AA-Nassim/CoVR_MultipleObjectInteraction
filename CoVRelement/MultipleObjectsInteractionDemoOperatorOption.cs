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

        GUILayout.Label("Systemic demo");
        GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
                if (GUILayout.Button("Hint Grab type A")) HighlightRandomVOI(VOIType.TypeA);
                if (GUILayout.Button("Hint Grab type B")) HighlightRandomVOI(VOIType.TypeB);
            GUILayout.EndHorizontal();

                if (GUILayout.Button("Hint Release Surface")) HighlightRandomVOI(VOIType.TypeB);

        GUILayout.EndVertical(); 
    }

    private void PrintVOIsWeights()
    {
        foreach(VOIBehaviour voi in sceneManager.VOIs)
        {
            Debug.Log("voi : " + voi.gameObject.name + " --- w : " + voi.weight);
        }
    }

    private void HighlightRandomVOI(VOIType type)
    {
        sceneManager.RandomGrabVOI(type);
    }

}