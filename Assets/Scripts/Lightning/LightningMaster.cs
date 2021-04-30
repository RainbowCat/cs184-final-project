using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomUtils;

public class LightningMaster : MonoBehaviour {
    const string headerDecoration = " --- ";
    [Header(headerDecoration + "Main" + headerDecoration)]

    // ignored for now, just try to generate one single lightning
    public Vector3 MinLightningSpawn; // ???: why is this a vec3?
    public Vector3 MaxLightningSpawn; // ???: why is this a vec3?
    public float LightningSpawnFrequency;
    public float GroundZero;
    public int MinAge; // in number of frames 20

    public int MaxAge; // in number of frames 40
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
        LightningBranch lightning = gameObject.AddComponent<LightningBranch>() as LightningBranch;

        Vector3 randVec = new Vector3((float) prng.NextDouble(), (float) prng.NextDouble(), (float) prng.NextDouble());

        lightning.isMainChannel = true;
        lightning.startPos = LightningUtils.randomVec3(MinLightningSpawn, MaxLightningSpawn, prng);
        lightning.startTime = time;
        lightning.BranchWidth = initialBranchRadius;

        lightning.lifeFactor = 1;
        lightning.maxLifespan = prng.Next() % (MaxAge - MinAge) + MinAge;
        lightning.numReturnStrokes = prng.Next() % 3 + 1;

        lightning.GroundZero = GroundZero;
        lightning.lightningMaterial = lightningMaterial;

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
        if (prng.NextDouble() < LightningSpawnFrequency) {
            generateLightningBolt();
        }
    }


}
