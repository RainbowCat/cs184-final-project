using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningMaster : MonoBehaviour {
    const string headerDecoration = " --- ";
    [Header(headerDecoration + "Main" + headerDecoration)]

    // ignored for now, just try to generate one single lightning

    public Vector3 lightningSpawnMin;
    public Vector3 lightningSpawnMax;

    public float lightningSpawnFrequency;

    public float groundZero;

    public int ageMin; // in number of frames 20

    public int ageMax; // in number of frames 40


    public int time = 0;

    public float initialBranchRadius;

    public Material lightningMaterial;

    public int randomSeed = 0;

    static System.Random prng;

    List<LightningBranch> lightnings = new List<LightningBranch>();

    // Start is called before the first frame update
    public void Start() {
        prng = new System.Random(randomSeed);
        LightningBranch.prng = prng;
    }

    void generateLightningBolt() {

        Vector3 randVec = new Vector3((float) prng.NextDouble(), (float) prng.NextDouble(), (float) prng.NextDouble());
        Vector3 startPos = Vector3.Scale(randVec, lightningSpawnMax - lightningSpawnMin) + lightningSpawnMin;

        LightningBranch lightning = gameObject.AddComponent<LightningBranch>() as LightningBranch;
        lightning.isMainChannel = true;
        lightning.lifeFactor = 1;
        lightning.lightningMaterial = lightningMaterial;
        lightning.maxLifespan = prng.Next() % (ageMax - ageMin) + ageMin;
        lightning.numReturnStrokes = prng.Next() % 3 + 1;
        lightning.startPos = startPos;
        lightning.BranchRadius = initialBranchRadius;
        lightning.groundZero = groundZero;
        lightning.startTime = time;
        lightnings.Add(lightning);

    }

    // Update is called once per frame
    public void Update() {
        time += 1;
        for (int i = 0; i < lightnings.Count; i++) {
            LightningBranch lightning = lightnings[i];
            if (time > lightning.startTime + lightning.maxLifespan) {
                lightnings.RemoveAt(i);
                i--;
            } else {
                lightning.lightningBranchTick(time);
            }
        }
        if (prng.NextDouble() < lightningSpawnFrequency) {
            generateLightningBolt();
        }
    }


}
