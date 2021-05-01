using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// lightning prefab

public class LightningSegment : MonoBehaviour {
    /** adjustable parameters **/
    public Vector3 startPos;
    public Vector3 direction;
    public float width;
    public float length;
    public Material lightningMaterial;
    Vector3 DefaultOrientation = Vector3.up;
    public int segmentNumber;
    public GameObject cylinderObject; // the cylinder game object in Unity that represents THIS segment

    public void createSegment() {
        // TODO use segment number to name current cylinder?
        direction = Vector3.Normalize(direction);
        Vector3 rotAxisV = Vector3.Normalize(direction + DefaultOrientation);

        cylinderObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinderObject.transform.position = startPos + direction * length / 2.0f;
        cylinderObject.transform.rotation = new Quaternion(rotAxisV.x, rotAxisV.y, rotAxisV.z, 0);
        cylinderObject.transform.localScale = new Vector3(width * 0, length / 2.0f, width * 0);
    }

    public void setBrightness(float newBrightness) {
        cylinderObject.transform.localScale = new Vector3(width * newBrightness, length / 2.0f, width * newBrightness);
    }

    /** Unity **/
    void Start() {
        createSegment();
    }

    void OnDestroy() {
        Destroy(cylinderObject);
    }
}

