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
    }

    public void Start()
    {
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
            if (VOIs[i].VOIType == VOIType.Surfaces) continue;

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

    public void RandomGrabVOI(VOIType type)
    {
        VOIBehaviour randomVOI = GetRandomVOI(type);
        GrabVOI(randomVOI);
    }

    public void RandomReleaseVOI()
    {
        VOIBehaviour randomVOISurface = GetRandomVOI(VOIType.Surfaces);
        ReleaseVOI(randomVOISurface); 
    }

    ///<summary>
    /// Returns a random VOI of a certain type. 
    ///</summary>
    public VOIBehaviour GetRandomVOI(VOIType type)
    {
        int length = typeToVOIs[type].Capacity;
        int randomID = Random.Range(0, length);
        return typeToVOIs[type][randomID];
    }

    ///<summary>
    /// Moves the column so that the user can grab the chosen VOI. 
    ///</summary>
    public void GrabVOI(VOIBehaviour voiToGrab)
    {
        
    }


    ///<summary>
    /// Moves the column to the surface where the user should put the grabbed VOI. 
    ///</summary>
    public void ReleaseVOI(VOIBehaviour voiSurfaceToReleaseOn)
    {
        
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
            if (typeToVOIs.ContainsKey(voi.VOIType)) typeToVOIs[voi.VOIType].Add(voi); 
            else
            {
                typeToVOIs[voi.VOIType] = new List<VOIBehaviour>();
                typeToVOIs[voi.VOIType].Add(voi); 
            }
        }
    }

    #endregion


    
}
