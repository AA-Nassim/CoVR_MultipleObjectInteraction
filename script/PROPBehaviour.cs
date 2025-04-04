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

    public UnityEvent onGrab;
    public UnityEvent onRelease;

    public VOIBehaviour grabbedVOI;

    private MultipleObjectsInteractionSceneManager sceneManager;

    private void Start()
    {
        sceneManager = MultipleObjectsInteractionSceneManager.Instance;
        if (sceneManager == null) Debug.LogError("No MultipleObjectsInteractionSceneManager found. ");
    }

    private void Update()
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

    private void OnGrab()
    {
        onGrab.Invoke();
    }

    private void OnRelease()
    {
        AdjustVOIsPositions(); 
        onRelease.Invoke();
    }

    private void AdjustVOIsPositions()
    {
        // Since the vois of the same type have the same parents, we can just offset the parent by the same offset between the PROP and the center of the column
        Vector3 offset = transform.position - sceneManager.column.position;
        sceneManager.typeToParent[VOIType].position += offset; 
    }
}
