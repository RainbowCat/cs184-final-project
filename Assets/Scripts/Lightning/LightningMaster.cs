using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomUtils;

public class LightningMaster : MonoBehaviour {

    // spawning is ignored for now, just try to generate one single lightning
    public Vector3 MinSpawnPos = new Vector3(-800.0f, 150.0f, -800.0f);
    public Vector3 MaxSpawnPos = new Vector3(2000.0f, 180.0f, 2000.0f);
    public float SpawnProb;
    public float SpawnReductionRate = 0.001f;
    static float PlaneSpeed = 10.0f;

    public float GroundZero;
    public float MinAge;
    public float MaxAge;
    public float InitialBranchRadius;
    public Material lightningMaterial;
    public int randomSeed = 0;

    System.Random prng;
    List<LightningBranch> lightnings = new List<LightningBranch>();

    void generateLightningBolt() {
        LightningBranch lightningStrike = new LightningBranch();
        lightningStrike.isMainChannel = true;
        lightningStrike.startPos = LightningUtils.randomVec3(MinSpawnPos, MaxSpawnPos, prng);
        lightningStrike.startTime = Time.time;
        lightningStrike.BranchWidth = InitialBranchRadius;

        lightningStrike.lifeFactor = 1;
        lightningStrike.Lifespan = prng.Next() % (MaxAge - MinAge) + MinAge;
        lightningStrike.numReturnStrokes = prng.Next() % 3;

        lightningStrike.GroundZero = GroundZero;
        lightningStrike.lightningMaterial = lightningMaterial;
        lightningStrike.constructLightningBranch();
        lightnings.Add(lightningStrike);

    }

    /** Unity **/

    // Start is called before the first frame update
    public void Start() {
        prng = new System.Random(randomSeed);
        LightningBranch.prng = prng;
    }

    // match with CloudMaster
    static float transition_time = 20;
    static float stay_time = 10.0f;
    static float period = transition_time * 2 + stay_time * 2;

    // Update is called once per frame
    public void Update() {
        MinSpawnPos += new Vector3(PlaneSpeed, 0, PlaneSpeed); // optimization: don't spawn lightning behind plane

        if (Time.time < stay_time + transition_time) { // stage 1 + 2
            for (int i = 0; i < lightnings.Count; i++) {
                LightningBranch lightning = lightnings[i];
                if (Time.time > lightning.startTime + lightning.Lifespan) {
                    lightnings.RemoveAt(i);
                    lightning.destroyLightning();
                    i--;
                } else {
                    lightning.lightningStrokeFlicker(Time.time);
                }
            }
            if (prng.NextDouble() < SpawnProb) {
                generateLightningBolt();
            }
            if (Time.time > stay_time) {
                SpawnProb -= SpawnReductionRate;
                SpawnReductionRate *= 1.1f;
            }
        } else {
            for (int i = 0; i < lightnings.Count; i++) {
                LightningBranch lightning = lightnings[i];
                lightnings.RemoveAt(i);
                lightning.destroyLightning();
                i--;
            }
        }
    }
}
