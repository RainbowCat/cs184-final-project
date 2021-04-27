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

    Vector3 cylDefaultOrientation = new Vector3(0, 1, 0);
    List<LightningSegment> children = new List<LightningSegment>();

    void Start()
    {
        createSegment(start, length, direction);
    }

    public void createSegment(Vector3 startPt, float length, Vector3 direction)
    {
        // TODO: later add change object name?
        GameObject initialSegment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        initialSegment.transform.localScale = new Vector3(0.1f, length / 2.0f, 0.1f);

        Vector3 directionNormal = Vector3.Normalize(direction);
        initialSegment.transform.position = start + directionNormal * length / 2.0f;
        Vector3 rotAxisV = Vector3.Normalize(directionNormal + cylDefaultOrientation);
        initialSegment.transform.rotation = new Quaternion(rotAxisV.x, rotAxisV.y, rotAxisV.z, 0);
        initialSegment.GetComponent<MeshRenderer>().material = lightningMaterial;
    }
}

