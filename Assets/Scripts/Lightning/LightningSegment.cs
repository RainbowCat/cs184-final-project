using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// lightning prefab

public class LightningSegment : MonoBehaviour
{
    // Parameters
    float length;
    Vector3 ptA;
    Vector3 ptB;

    // constructor
    public LightningSegment(Vector3 startPt, float length, Vector3 direction)
    {
        ptA = startPt;
        // ptB = startPt + length * direction.Normalized();
    }
}

