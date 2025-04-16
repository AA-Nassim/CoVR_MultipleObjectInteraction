using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A classic SceneManager. Acessible through a unique static instance. 
/// 
/// Regroups useful functions and variables used throught
/// </summary>
public class MultipleObjectsInteractionSceneManager : MonoBehaviour
{

    public static MultipleObjectsInteractionSceneManager Instance;

    [Header("General")]
    public bool isReady = false; 

    [Header("VOIs")]
    public VOIBehaviour[] VOIs; 
    public VOIBehaviour simulatedVOI; 

    [Header("PROPs")]
    public PROPBehaviour[] PROPs;

    public Dictionary<VOIType, PROPBehaviour> typeToPROP = new Dictionary<VOIType, PROPBehaviour>(); //Dict that takes a type and return the PROP of that type. 
    public Dictionary<VOIType, List<VOIBehaviour>> typeToVOIs = new Dictionary<VOIType, List<VOIBehaviour>>(); // Dict that takes a Type and returns the list of VOIs of the same Type. 

    [Header("Column")]
    public MultipleObjectInteractionColumnBehaviour columnBehaviour;
    public Transform column; 

    [Header("Player")]
    public Transform player; 
    public Transform leftHand;
    public Transform rightHand;

    [Header("Scenario systemique")]
    public bool useScenarioSystemic; 


    #region Unity's Methods

    public void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Get references
        player = GameObject.FindGameObjectWithTag("Player").transform;
        leftHand = GameObject.FindGameObjectWithTag("LeftHand").transform;
        rightHand = GameObject.FindGameObjectWithTag("RightHand").transform;
        column = GameObject.FindGameObjectWithTag("TangibleDoor").transform;

        // Get VOIs
        GameObject[] VOIsGameObjects = GameObject.FindGameObjectsWithTag("voi");
        VOIs = new VOIBehaviour[VOIsGameObjects.Length];
        for (int i = 0; i < VOIs.Length; i++)
            VOIs[i] = VOIsGameObjects[i].GetComponent<VOIBehaviour>();


        // Get PROPs
        GameObject[] PROPsGameObjects = GameObject.FindGameObjectsWithTag("prop");
        PROPs = new PROPBehaviour[PROPsGameObjects.Length];
        for (int i = 0; i < PROPs.Length; i++)
            PROPs[i] = PROPsGameObjects[i].GetComponent<PROPBehaviour>();

        // setup dict
        foreach (var prop in PROPs)
        {
            if (typeToPROP.ContainsKey(prop.voiType)) Debug.LogError("Warning : Multiple props uses with the same VOI Type.");
            typeToPROP.Add(prop.voiType, prop);
        }

        foreach (var voi in VOIs)
        {
            if (typeToVOIs.ContainsKey(voi.voiType)) typeToVOIs[voi.voiType].Add(voi);
            else
            {
                typeToVOIs[voi.voiType] = new List<VOIBehaviour>();
                typeToVOIs[voi.voiType].Add(voi);
            }
        }
    }

    public void Start()
    {
        isReady = true;
    }

    private void Update()
    {
        if (player == null ) player = GameObject.FindGameObjectWithTag("Player").transform;
        if (leftHand == null) leftHand = GameObject.FindGameObjectWithTag("LeftHand").transform;
        if (rightHand == null) rightHand = GameObject.FindGameObjectWithTag("RightHand").transform;
        if (column == null) column = GameObject.FindGameObjectWithTag("TangibleDoor").transform;
    }

    #endregion

    #region Public Methods
    public VOIBehaviour GetClosestVOIToPlayer()
    {
        float minDistanceWeight = VOIs[0].positionWeight;
        int minDistanceWeightID = 0;

        for (int i = 0; i < VOIs.Length; i++)
        {
            if (!VOIs[i].isActive) continue;
            if (VOIs[i].voiType == VOIType.Surfaces) continue;

            if (VOIs[i].positionWeight > minDistanceWeight)
            {
                minDistanceWeight = VOIs[i].positionWeight;
                minDistanceWeightID = i;
            }
        }

        return VOIs[minDistanceWeightID];
    }


    #endregion

    #region Scenario Systemique Functions
    /// <summary>
    /// Seleccts a random VOI. 
    /// A seletion is setting up the simulatedVOI to the selected VOI, highlighting the VOI and positioning the column to the VOI. 
    /// </summary>
    /// <param name="type">Type of the wanted VOI</param>
    public void SelectRandomVOI(VOIType type)
    {
        VOIBehaviour randomVOI = GetRandomVOI(type);
        if (simulatedVOI != null) UnhighlightVOI(simulatedVOI);
        simulatedVOI = randomVOI;
        HighlightVOI(simulatedVOI);
        PositionColumnToVOI(simulatedVOI);
    }

    /// <summary>
    /// Select a surface VOI. 
    /// A seletion is setting up the simulatedVOI to the selected VOI, highlighting the VOI and positioning the column to the VOI. 
    /// </summary>
    /// <param name="i">id of the VOI surface</param>
    public void SelectSurface(int i)
    {
        VOIBehaviour voi = typeToVOIs[VOIType.Surfaces][i];
        if (simulatedVOI != null) UnhighlightVOI(simulatedVOI);
        simulatedVOI = voi;
        HighlightVOI(simulatedVOI);
        PositionColumnToVOI(simulatedVOI);
    }
    
    ///<summary>
    /// Returns a random VOI of a certain type. 
    ///</summary>
    public VOIBehaviour GetRandomVOI(VOIType type)
    {
        int length = typeToVOIs[type].Count;
        int randomID = Random.Range(0, length);
        return typeToVOIs[type][randomID];
    }

    ///<summary>
    /// Add and setup of the "Outline" Component to highlight a VOI. The outline is then delted when the user picks the VOI.  
    ///</summary>
    public void HighlightVOI(VOIBehaviour voiToHighlight)
    {
        if (voiToHighlight == null) return;
        Outline outlineComponent = voiToHighlight.gameObject.AddComponent<Outline>();
        outlineComponent.OutlineMode = Outline.Mode.OutlineAll;
        outlineComponent.OutlineColor = Color.red; 
    }

    ///<summary>
    /// Destroys the "Outline" component if it exists.   
    ///</summary>
    public void UnhighlightVOI(VOIBehaviour voiToUnhighlight)
    {
        if (voiToUnhighlight == null) return; 
        Outline outlineComponent = voiToUnhighlight.GetComponent<Outline>(); 

        if (outlineComponent != null)
            Destroy(outlineComponent);
    }

    ///<summary>
    /// Moves the column so that the user can grab the chosen VOI. 
    ///</summary>
    public void PositionColumnToVOI(VOIBehaviour voi)
    {
        columnBehaviour.SetNewTargert(voi.transform.position);
    }

    #endregion
}
