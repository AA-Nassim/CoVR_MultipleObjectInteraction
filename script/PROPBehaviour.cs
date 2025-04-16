using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A PROP is a real object tracked via Optitrack amrkers. 
/// it has a type that defines which VOIs are simulated by this PROP. 
/// </summary>
public class PROPBehaviour : MonoBehaviour
{
    [Header("PROP Properties")]
    public bool IsActive = false; 
    public VOIType voiType;
    public Vector3 initialPosition;
    public bool isInsideColumn;
    public Vector3 meshLocalPos; 

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
    private Transform initialParent; 

    private void Start()
    {
        sceneManager = MultipleObjectsInteractionSceneManager.Instance;
        if (sceneManager == null) Debug.LogError("No MultipleObjectsInteractionSceneManager found. ");

        StartCoroutine(SaveInitialPosition());
        initialParent = transform.parent;
    }

    private void LateUpdate()
    {
        if (!IsActive) return; 
        UpdateGrab();
    }


    /// <summary>
    /// We Use the OnTriggerEnter and OnTriggerExit functions to keep track if the PROP is inside the column or not. 
    /// A collider is attached to the NavMesh Agent that moves with the column. 
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (voiType.Equals(VOIType.Surfaces)) return;
        isInsideColumn = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (voiType.Equals(VOIType.Surfaces)) return;
        isInsideColumn = false;
    }

    /// <summary>
    /// This function updates the isGrabbed bool. And calls the OnGrab() and the OnRelease() events. 
    /// </summary>
    private void UpdateGrab()
    {
        if (!sceneManager.columnBehaviour.onTarget) return;
        if (!isInsideColumn) return;

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

    /// <summary>
    /// Updates the material
    /// </summary>
    public void OnGrab()
    {
        // TO DO : Ongrab link the PROP (this.transform) to the hand to keep the object moving even if optitrack doesn't detect the PROP.

        foreach (var voi in sceneManager.typeToVOIs[voiType])
            if (voi.isInsideColumn) voi.LinkToPROP(meshLocalPos);

        onGrabEvents?.Invoke();
    }

    public void OnRelease()
    {
        print("OnRelease");
        mesh.material = notGrabbedMaterial;
        //transform.SetParent(initialParent
        //AdjustVOIs(); 

        foreach (var voi in sceneManager.typeToVOIs[voiType])
            if (voi.isInsideColumn) voi.UnlinkFromPROP();

        onReleaseEvents?.Invoke();
    }

    private void AdjustVOIs()
    {
        Vector3 posOffset = new Vector3(transform.position.x - sceneManager.column.position.x, 0, transform.position.z - sceneManager.column.position.z);
        Quaternion rotOffset = transform.rotation;

        foreach (var voi in sceneManager.typeToVOIs[voiType])
        {
            voi.transform.position = posOffset + voi.initialPosition;
            voi.transform.rotation = rotOffset;
        }
    }

    private IEnumerator SaveInitialPosition()
    {
        yield return new WaitForSeconds(1);
        initialPosition = transform.position;
        IsActive = true;
        yield return null;
    }
}
