using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleObjectsInteractionSceneManager : MonoBehaviour
{

    public static MultipleObjectsInteractionSceneManager Instance;

    [Header("General")]
    public bool isReady = false; 

    [Header("VOIs")]
    public VOIBehaviour[] VOIs; //For now I dragged and drop them but we can use GameObject.findgameobjectswithttag. 
    public VOIBehaviour simulatedVOI; 

    [Header("PROPs")]
    public Transform PROPsParent;
    public PROPBehaviour[] PROPs;

    public Dictionary<VOIType, PROPBehaviour> typeToPROP = new Dictionary<VOIType, PROPBehaviour>(); // To easly access the PROP of a certain type of VOIs. 
    public Dictionary<VOIType, List<VOIBehaviour>> typeToVOIs = new Dictionary<VOIType, List<VOIBehaviour>>(); //Future TODO : For changing all the VOIs of a certain type based on the PROP orientation. 

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

        // Init dict
        DictInit();
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

    public void SelectRandomVOI(VOIType type)
    {
        VOIBehaviour randomVOI = GetRandomVOI(type);
        if (simulatedVOI != null) UnhighlightVOI(simulatedVOI);
        simulatedVOI = randomVOI;
        HighlightVOI(simulatedVOI);
        PositionColumnToVOI(randomVOI);
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

    #region Private Methods

    private void DictInit()
    {
        foreach (var prop in PROPs)
        {
            if (typeToPROP.ContainsKey(prop.VOIType)) Debug.LogError("Warning : Multiple props uses with the same VOI Type.");
            typeToPROP.Add(prop.VOIType, prop);
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

    #endregion


    
}
