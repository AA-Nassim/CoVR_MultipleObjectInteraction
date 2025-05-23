using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A VOI is a virtual object that is simulated by a PROP. 
/// </summary>
/// 
[RequireComponent(typeof(Collider))]
public class VOIBehaviour : MonoBehaviour
{
    [Header("VOI Properties")]
    public VOIType voiType;
    public bool isActive;
    public bool isInsideColumn;
    public Vector3 initialPosition; 

    [Header("Weight properties")]
    /// Weight if a value 
    public float weight;
    public float positionWeight;
    public float rotationWeight;

    private float coef = 0.5f;
    private float maxDistance = 15;
    private float rayCount = 10;

    private Camera mainCamera;
    private Collider collider;
    private MultipleObjectsInteractionSceneManager sceneManager;
    private Transform initialParent; 

    #region Unity's Methods
    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("No Camera detected");

        collider = transform.GetComponent<Collider>();
        if (collider == null) Debug.LogError("No Collider");

        sceneManager = MultipleObjectsInteractionSceneManager.Instance;
        if (sceneManager == null) Debug.LogError("No MultipleObjectsInteractionSceneManager found.");

        initialPosition = transform.position;
        initialParent = transform.parent;
        isInsideColumn = false; 
    }

    /// <summary>
    /// We Use the OnTriggerEnter and OnTriggerExit functions to keep track if the VOI is inside the column or not. 
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

    #endregion



    #region Events
    /// <summary>
    /// Links the VOI to the PROP of the same Type. 
    /// </summary>
    /// <param name="localPos">Local position of the child. (offset to the Optitrack pivot.</param>
    public void LinkToPROP(Vector3 localPos)
    {
        transform.SetParent(sceneManager.typeToPROP[voiType].transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity; 
    }
    /// <summary>
    /// Unlink the VOI from the PROP. Resets the parent but keeps the position and the rotation. 
    /// </summary>
    public void UnlinkFromPROP()
    {
        transform.SetParent(initialParent);
    }
    #endregion

    #region Weight Calculation functions
    /// <summary>
    /// Calculates the weight of the VOI. 
    /// The Weight of the VOI is a value from 0 to 1. 
    /// It represents the probability that the user will interact with this VOI. 
    /// </summary>
    
    private void UpdateWeight()
    {
        float distanceWeight = CalculateDistanceWeight();
        float rotationWeight = CalculateRotationWeight();
        weight = coef * distanceWeight + (1 - coef) * rotationWeight;
    }

    private float CalculateDistanceWeight()
    {
        Vector3 playerPos = mainCamera.transform.position;
        float distPlayer = Vector3.Distance(playerPos, transform.position);
        float newPosWeight = (1 / (1 + distPlayer));
        positionWeight = newPosWeight; 
        return newPosWeight;
    }

    private float CalculateRotationWeight()
    {
        float theta = FoundAngle();
        float newRotationWeight = ((float)Math.Exp(Mathf.Cos(Mathf.Deg2Rad * theta) - 1));
        rotationWeight = newRotationWeight; 
        return newRotationWeight; 
    }

    private float FoundAngle()
    {
        RaycastHit hit;

        // Calculate the cone angle between rays
        float angleStep = 60f / rayCount;
        // Get the height of the collider
        float colliderHeight = collider.bounds.center.y;
        Vector3 forwardProjected = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
        // Calculate ray direction based on current angle

        Quaternion rotation = Quaternion.Euler(0, 0, 0);
        Vector3 rayDirection = rotation * forwardProjected;

        // Adjust ray origin to the height of the collider
        Vector3 rayOrigin = mainCamera.transform.position;
        rayOrigin.y = colliderHeight;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxDistance))
        {
            if (hit.collider == collider)
            {
                // Calculate the angle between the camera's forward direction and the hit normal
                float angle = Vector3.Angle(forwardProjected, rayDirection);

                // Do something with the angle, like printing it
                //Debug.Log("Angle to collide: " + angle);
                //Debug.Log("cos: " + Mathf.Cos(Mathf.Deg2Rad * angle));
                return angle; // Exit loop if collision found
            }
        }
        for (int i = 1; i < rayCount; i++)
        {
            // Calculate ray direction based on current angle
            rotation = Quaternion.Euler(0, angleStep * i, 0);
            rayDirection = rotation * forwardProjected;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxDistance))
            {
                if (hit.collider == collider)
                {
                    // Calculate the angle between the camera's forward direction and the hit normal
                    float angle = Vector3.Angle(forwardProjected, rayDirection);

                    // Do something with the angle, like printing it
                    //Debug.Log("Angle to collide: " + angle);
                    //Debug.Log("cos: " + Mathf.Cos(Mathf.Deg2Rad * angle));
                    return angle; // Exit loop if collision found
                }
            }
            rotation = Quaternion.Euler(0, -angleStep * i, 0);
            rayDirection = rotation * forwardProjected;

            if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxDistance))
            {
                if (hit.collider == collider)
                {
                    // Calculate the angle between the camera's forward direction and the hit normal
                    float angle = Vector3.Angle(forwardProjected, rayDirection);

                    // Do something with the angle, like printing it
                    //Debug.Log("Angle to collide: " + angle);
                    //Debug.Log("cos: " + Mathf.Cos(Mathf.Deg2Rad * angle));
                    return angle; // Exit loop if collision found
                }
            }
        }

        // If no collision found
        //Debug.Log("object not in -90 90 rotation");
        //Debug.Log("cos: " + 0);
        return 90;
    }

    #endregion
}
