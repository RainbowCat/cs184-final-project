using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomUtils;

public class LightningMaster : MonoBehaviour {

    // spawning is ignored for now, just try to generate one single lightning
    public Vector3 MinSpawnPos;
    public Vector3 MaxSpawnPos;
    public float SpawnProb;

    public float GroundZero;
    public int MinAge; // in number of frames 20
    public int MaxAge; // in number of frames 40
    public int time = 0;
    public float InitialBranchRadius;
    public Material lightningMaterial;
    public int randomSeed = 0;

    System.Random prng;
    List<LightningBranch> lightnings = new List<LightningBranch>();

    void generateLightningBolt() {
        LightningBranch lightningStrike = gameObject.AddComponent<LightningBranch>() as LightningBranch;
        lightningStrike.isMainChannel = true;
        lightningStrike.startPos = LightningUtils.randomVec3(MinSpawnPos, MaxSpawnPos, prng);
        lightningStrike.startTime = time;
        lightningStrike.BranchWidth = InitialBranchRadius;

        lightningStrike.lifeFactor = 1;
        lightningStrike.maxLifespan = prng.Next() % (MaxAge - MinAge) + MinAge;
        lightningStrike.numReturnStrokes = prng.Next() % 3 + 1;

        lightningStrike.GroundZero = GroundZero;
        lightningStrike.lightningMaterial = lightningMaterial;

        lightnings.Add(lightningStrike);

    }

    /** Unity **/

    // Start is called before the first frame update
    public void Start() {
        prng = new System.Random(randomSeed);
        LightningBranch.prng = prng;
        // generateLightningBolt();
    }

    // Update is called once per frame
    public void Update() {
        time += 1;
        for (int i = 0; i < lightnings.Count; i++) {
            LightningBranch lightning = lightnings[i];
            if (time > lightning.startTime + lightning.maxLifespan) {
                lightnings.RemoveAt(i); // TODO call destroy
                lightning.destroyLightning();
                i--;
            } else {
                lightning.lightningStrokeFlicker(time);
            }
        }
        if (prng.NextDouble() < SpawnProb) {
            generateLightningBolt();
        }
    }


}
