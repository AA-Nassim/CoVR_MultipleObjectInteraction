using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class VOIBehaviour : MonoBehaviour
{
    [Header("VOI Properties")]
    public VOIType VOIType;
    public bool isActive;

    [Header("Weight properties")]
    public float weight;
    public float positionWeight;
    public float rotationWeight;

    private float coef = 0.5f;
    private float maxDistance = 15;
    private float rayCount = 10;

    private Camera mainCamera;
    private Collider collider;
    private MultipleObjectsInteractionSceneManager sceneManager; 

    #region Unity's Methods
    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("No Camera detected");

        collider = transform.GetComponent<Collider>();
        if (collider == null) Debug.LogError("No Collider");

        sceneManager = MultipleObjectsInteractionSceneManager.Instance;
        if (sceneManager == null) Debug.LogError("No MultipleObjectsInteractionSceneManager found.");
    }

    private void Update()
    {
        if (!isActive) return;
        if (mainCamera == null) mainCamera = Camera.main;

        if (sceneManager.useScenarioSystemic)
        {
            // No need to calculate the weight thingy if we're using the systemic scenario. 
            return; 
        }

        UpdateWeight();
        //upgrade : UpdateGrab only if player close to the VOI. 
        // TODO : 
        // If (a voi of the same type is grabbed) return 
        // Else (meaning no object of the same time is grabbed) adjust VOI position and rotation to match the PROP position. 

    }

    #endregion

    #region Weight Calculation functions
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

    #region VOI Adjustments 



    #endregion
}
