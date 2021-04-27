using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// lightning prefab

public class LightningSegment : MonoBehaviour
{
    // Parameters
    public float length;
    public Vector3 start;
    public Vector3 direction;

    public Material lightningMaterial;

    Vector3 cylDefaultOrientation = Vector3.up;
    float cylinderRadius = 1f;
    void Start()
    {
        createSegment();
    }

    public void createSegment()
    {
        // TODO: later add change object name?
        GameObject initialSegment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        initialSegment.transform.localScale = new Vector3(cylinderRadius, length / 2.0f, cylinderRadius);

        Vector3 directionNormal = Vector3.Normalize(direction);
        initialSegment.transform.position = start + directionNormal * length / 2.0f;
        Vector3 rotAxisV = Vector3.Normalize(directionNormal + cylDefaultOrientation);
        initialSegment.transform.rotation = new Quaternion(rotAxisV.x, rotAxisV.y, rotAxisV.z, 0);
        initialSegment.GetComponent<MeshRenderer>().material = lightningMaterial;
    }
}

