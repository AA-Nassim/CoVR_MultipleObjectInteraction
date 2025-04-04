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
        player = GameObject.FindGameObjectWithTag("Player").transform;
        leftHand = GameObject.FindGameObjectWithTag("LeftHand").transform;
        rightHand = GameObject.FindGameObjectWithTag("RightHand").transform;
        column = GameObject.FindGameObjectWithTag("TangibleDoor").transform;

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
