using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A PROP is a real object tracked via Optitrack amrkers. 
/// A PROP can only simulate VOIs of the same voiType. 
/// </summary>
/// 
[RequireComponent(typeof(Collider))]
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

    public float heightErrorRange;

    public float grabHoldDuration = 0.5f;
    public float releaseHoldDuration = 0.5f;
    private float grabTimer = 0f;
    private float releaseTimer = 0f;

    public UnityEvent onGrabEvents;
    public UnityEvent onReleaseEvents;

    private MultipleObjectsInteractionSceneManager sceneManager;
    private Transform initialParent; 

    private void Start()
    {
        sceneManager = MultipleObjectsInteractionSceneManager.Instance;
        if (sceneManager == null) Debug.LogError("No MultipleObjectsInteractionSceneManager found. ");

        StartCoroutine(SaveInitialPosition()); // Check the function summary to better understand
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
    /// The isGrabbed value is dependent of the  Y position of the PROP, the column position and if the prop is inside the column or not. 
    /// if the column did not reach the target or the PROP is not inside the column we do not change the value of isGrabbed. 
    /// else we check the Y pos of the PROP. If Y > intial Y + threshold then we assume it is grabbed else we assume it is not. 
    /// We call the events if the value of isGrabbed changed. 
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
    /// Links the adequate VOI to the PROP then calls the onGrabEvents. 
    /// The adequate VOI is a VOI of the same type as the prop  and that is inside the column. 
    /// </summary>
    public void OnGrab()
    {
        // TO DO : Ongrab link the PROP (this.transform) to the hand to keep the object moving even if optitrack doesn't detect the PROP.

        foreach (var voi in sceneManager.typeToVOIs[voiType])
            if (voi.isInsideColumn) voi.LinkToPROP(meshLocalPos);

        onGrabEvents?.Invoke();
    }

    /// <summary>
    /// Unlinks the adequate VOI from the PROP then calls the onReleaseEvents. 
    /// The adequate VOI is a VOI of the same type as the prop  and that is inside the column. 
    /// </summary>
    public void OnRelease()
    {
  
        
        //transform.SetParent(initialParent
        //AdjustVOIs(); 

        foreach (var voi in sceneManager.typeToVOIs[voiType])
            if (voi.isInsideColumn) voi.UnlinkFromPROP();

        onReleaseEvents?.Invoke();
    }

    /// <summary>
    /// Moves the VOIs from the initial position to an offsetted position. 
    /// The offset is calculate dbased on the PROP initial position and the center of the column. 
    /// </summary>
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

    /// <summary>
    /// Saves the initial position of the PROP. 
    /// </summary>
    private IEnumerator SaveInitialPosition()
    {
        yield return new WaitForSeconds(1);
        // We use a couritne to wait for the first OptiTrack data. 
        // Otherwise the onStart function is always equals to Vector3.zero which is the position of the prop in the scene. 
        initialPosition = transform.position;
        yield return null;
    }
}
