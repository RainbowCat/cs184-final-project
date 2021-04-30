using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// lightning prefab

public class LightningSegment : MonoBehaviour {
    // Parameters
    public float length;
    public Vector3 start;
    public Vector3 direction;

    public Material lightningMaterial;

    public float cylinderRadius;

    Vector3 cylDefaultOrientation = Vector3.up;

    public int segmentNumber;

    public GameObject cylinderSegment;

    float storedRadius;
    void Start() {
        createSegment();
    }

    public void createSegment() {
        // TODO later add change object name?

        cylinderSegment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinderSegment.transform.localScale = new Vector3(cylinderRadius * 0, length / 2.0f, cylinderRadius * 0);

        Vector3 directionNormal = Vector3.Normalize(direction);
        cylinderSegment.transform.position = start + directionNormal * length / 2.0f;
        Vector3 rotAxisV = Vector3.Normalize(directionNormal + cylDefaultOrientation);
        cylinderSegment.transform.rotation = new Quaternion(rotAxisV.x, rotAxisV.y, rotAxisV.z, 0);
    }

    public void setBrightness(float newBrightness) {
        cylinderSegment.transform.localScale = new Vector3(cylinderRadius * newBrightness, length / 2.0f, cylinderRadius * newBrightness);
    }

    void OnDestroy() {
        Destroy(cylinderSegment);
    }
}

