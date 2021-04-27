using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningMaster : MonoBehaviour
{
    const string headerDecoration = " --- ";
    [Header(headerDecoration + "Main" + headerDecoration)]

    // ignored for now, just try to generate one single lightning
    float lightningDensity;



    // Start is called before the first frame update
    void Start()
    {
        GameObject initialSegment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        initialSegment.transform.position = new Vector3(-2, 1, 0);
        initialSegment.GetComponent<MeshRenderer>().material = Resources.Load("Materials/LightningMaterial.mat") as Material;
        // LightningSegment root = new LightningSegment();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
