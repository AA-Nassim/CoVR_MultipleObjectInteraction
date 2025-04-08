using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PROPBehaviour : MonoBehaviour
{
    [Header("PROP Properties")]
    public VOIType VOIType;

    [Header("Grab properties")]
    public bool isGrabbed;
    public bool isGrabbedWithLeftHand;
    public bool isGrabbedWithRightHand;
    public float grabMaxDistance;

    public UnityEvent onGrabEvents;
    public UnityEvent onReleaseEvents;

    [Header("Debug")]
    public MeshRenderer mesh; 
    public Material grabbedMaterial; 
    public Material notGrabbedMaterial; 

    private MultipleObjectsInteractionSceneManager sceneManager;
    [HideInInspector] public Vector3 previousPositionOnColumn; 

    private void Start()
    {
        sceneManager = MultipleObjectsInteractionSceneManager.Instance;
        if (sceneManager == null) Debug.LogError("No MultipleObjectsInteractionSceneManager found. ");

        previousPositionOnColumn = transform.localPosition;
    }

    private void LateUpdate()
    {
        UpdateGrab();
    }

    private void UpdateGrab()
    {
        bool lastIsGrabbed = isGrabbed;

        isGrabbed = false;
        isGrabbedWithLeftHand = false;
        isGrabbedWithRightHand = false;

        float distLeftHand = Vector3.Distance(transform.position, sceneManager.leftHand.position);
        if (distLeftHand < grabMaxDistance)
        {
            isGrabbed = true;
            isGrabbedWithLeftHand = true;
        }
        else
        {
            float distRightHand = Vector3.Distance(transform.position, sceneManager.rightHand.position);
            if (distRightHand < grabMaxDistance)
            {
                isGrabbed = true;
                isGrabbedWithRightHand = true;
            }
        }

        if (!lastIsGrabbed & isGrabbed) OnGrab();
        else if (lastIsGrabbed & !isGrabbed) OnRelease();
    }

    public void OnGrab()
    {
        mesh.material = grabbedMaterial; 
        transform.SetParent(sceneManager.leftHand);    

        onGrabEvents.Invoke();
    }

    public void OnRelease()
    {
        mesh.material = notGrabbedMaterial;
        transform.SetParent(sceneManager.PROPsParent.transform);

        AdjustVOIs(); 
        onReleaseEvents.Invoke();
    }

    private void AdjustVOIs()
    {
        Vector3 posOffset = transform.localPosition - previousPositionOnColumn;
        Quaternion rotOffset = transform.rotation;
        previousPositionOnColumn = transform.localPosition;

        foreach (var voi in sceneManager.typeToVOIs[VOIType])
        {
            voi.transform.localPosition += posOffset;
            voi.transform.rotation = rotOffset;
        }
    }
}
