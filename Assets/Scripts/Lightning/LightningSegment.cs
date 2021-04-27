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

    void Start()
    {
        createSegment(start, length, direction);
    }

    public void createSegment(Vector3 startPt, float length, Vector3 direction)
    {
        GameObject initialSegment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        initialSegment.transform.position = start + direction * length / 2;
        initialSegment.transform.localScale = new Vector3(0.1f, length, 0.1f);
        initialSegment.transform.rotation.SetFromToRotation(new Vector3(0, 1, 0), direction);
        initialSegment.GetComponent<MeshRenderer>().material = lightningMaterial;
    }
}

