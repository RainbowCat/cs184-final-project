using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomUtils;

public class LightningMaster : MonoBehaviour {

    // spawning is ignored for now, just try to generate one single lightning
    public Vector3 MinSpawnPos;
    public Vector3 MaxSpawnPos;
    public float SpawnProb;
    public float SpawnReductionRate = 0.001f;
    public float PlaneSpeed;

    public float GroundZero;
    public float MinAge;
    public float MaxAge;

    static float MinSegmentAngle = 0.25f;
    static float MaxSegmentAngle = 0.75f;
    public float InitialBranchRadius;
    public Material lightningMaterial;
    public int randomSeed = 0;

    System.Random prng;
    List<LightningBranch> lightnings = new List<LightningBranch>();

    void generateLightningBolt() {
        LightningBranch lightningStrike = new LightningBranch();
        lightningStrike.isMainChannel = true;
        lightningStrike.startPos = LightningUtils.randomVec3(MinSpawnPos, MaxSpawnPos, prng);
        lightningStrike.startDir = LightningUtils.generateUniformDirection(Vector3.down, MinSegmentAngle, MaxSegmentAngle, prng);
        lightningStrike.startTime = Time.time;
        lightningStrike.BranchWidth = InitialBranchRadius;

        lightningStrike.LifeFactor = 1;
        lightningStrike.Lifespan = prng.Next() % (MaxAge - MinAge) + MinAge;
        lightningStrike.NumReturnStrokes = prng.Next() % 3;

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
        // optimization: don't spawn lightning behind plane
        MinSpawnPos += new Vector3(0, 0, -PlaneSpeed);
        MaxSpawnPos += new Vector3(0, 0, -PlaneSpeed);

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
            if (Time.time > stay_time * 0.75f && SpawnProb > 0.0f) {
                SpawnProb -= SpawnReductionRate;
                SpawnReductionRate *= 1.06f;
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

    public void OnDestroy() {
        for (int i = 0; i < lightnings.Count; i++) {
            LightningBranch lightning = lightnings[i];
            lightnings.RemoveAt(i);
            lightning.destroyLightning();
            i--;
        }
    }
}
