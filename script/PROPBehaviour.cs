using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PROPBehaviour : MonoBehaviour
{
    [Header("PROP Properties")]
    public bool IsActive = false; 
    public VOIType VOIType;
    public Vector3 initialPosition;

    [Header("Grab properties")]
    public bool isGrabbed;
    public bool isGrabbedWithLeftHand;
    public bool isGrabbedWithRightHand;

    //Descrimination the Y Position
    public float heightErrorRange;

    public float grabHoldDuration = 0.5f;
    public float releaseHoldDuration = 0.5f;
    private float grabTimer = 0f;
    private float releaseTimer = 0f;

    public UnityEvent onGrabEvents;
    public UnityEvent onReleaseEvents;

    [Header("Debug")]
    public MeshRenderer mesh; 
    public Material grabbedMaterial; 
    public Material notGrabbedMaterial; 

    private MultipleObjectsInteractionSceneManager sceneManager;

    private void Start()
    {
        sceneManager = MultipleObjectsInteractionSceneManager.Instance;
        if (sceneManager == null) Debug.LogError("No MultipleObjectsInteractionSceneManager found. ");

        StartCoroutine(SaveInitialPosition());
    }

    private IEnumerator SaveInitialPosition()
    {
        yield return new WaitForSeconds(1);
        initialPosition = transform.position;
        IsActive = true; 
        yield return null; 
    }

    private void LateUpdate()
    {
        if (!IsActive) return; 
        UpdateGrab();
    }

    private void UpdateGrab()
    {
        bool isAbovePlatform = transform.position.y > (initialPosition.y + heightErrorRange);
        
        if (isAbovePlatform)
        {
            if (!isGrabbed)
            {
                grabTimer += Time.deltaTime;
                releaseTimer = 0f;

                if (grabTimer >= grabHoldDuration)
                {
                    isGrabbed = true;
                    OnGrab();
                }
            }
            
        }
        else
        {
            if (isGrabbed)
            {
                releaseTimer += Time.deltaTime;
                grabTimer = 0f;

                if (releaseTimer >= releaseHoldDuration)
                {
                    isGrabbed = false;
                    OnRelease();
                }
            }
            
        }
    }

    public void OnGrab()
    {
        print("OnGrab");
        mesh.material = grabbedMaterial; 
        //transform.SetParent(sceneManager.leftHand);    

        onGrabEvents?.Invoke();
    }

    public void OnRelease()
    {
        print("OnRelease");
        mesh.material = notGrabbedMaterial;
        //transform.SetParent(sceneManager.PROPsParent.transform);

        AdjustVOIs(); 
        onReleaseEvents?.Invoke();
    }

    private void AdjustVOIs()
    {
        Vector3 posOffset = new Vector3(transform.position.x - sceneManager.column.position.x, 0, transform.position.z - sceneManager.column.position.z);
        Quaternion rotOffset = transform.rotation;

        foreach (var voi in sceneManager.typeToVOIs[VOIType])
        {
            voi.transform.position = posOffset + voi.initialPosition;
            voi.transform.rotation = rotOffset;
        }
    }
}
