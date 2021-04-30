using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A lightning branch is a list of segments that are chained to form one line
public class LightningBranch : MonoBehaviour {
    /** constants **/
    static float DirectionMean = 0.179f;
    static float DirectionVariance = 0.1f;
    static float MinSegmentLength = 1f;
    static float MaxSegmentLength = 2.5f;
    static float MinBranchAngle = 0.18f;
    static float MaxBranchAngle = 0.75f;
    static float MinBranchReductionFactor = 0.45f;
    static float MaxBranchReductionFactor = 0.6f;
    static float BranchSegmentWidthReductionFactor = 0.95f;
    static float ReturnStrokeVariance = 0.3f; // determined via desmos
    static float LightningDecayFactor = 3f; // determined via desmos
    static float ReturnStrokeDecayFactor = 0.15f; // determined via desmos
    static float PropagationSpeed = 10f;

    // private variables
    System.Random prng;
    int depth = 0;
    float lifespan = 0;
    Vector3 startDir = Vector3.down;

    /** adjustables **/
    public bool isMainChannel; // TODO change to non-variable
    public int maxNumSegments = 10;
    public int segmentsMax = 20;
    public int segmentsMin = 10;
    public int maxDepth = 5;

    public float BranchRadius;
    public int randomSeed = 0;

    public float lifeFactor;
    public Vector3 startPos;

    public int numReturnStrokes;

    public float maxLifespan;
    public float branchProb = 0.05f; // initialize to no branching

    public int branchNumber = 0;

    List<LightningSegment> segments = new List<LightningSegment>();
    List<LightningBranch> children = new List<LightningBranch>();

    public Material lightningMaterial;
    float groundZero = 0.0f;
    public float startTime;

    void constructLightningBranch() {
        // build main branch that reaches the ground
        buildBranch(prng);

        // build child branches
        buildChildrenBranches(prng);

    }

    void buildBranch(System.Random prng) {
        // if (isMainChannel)
        // {
        //     startDir = Vector3.down;
        // }
        Vector3 currDir = startDir;
        Vector3 currStartPos = startPos;
        groundZero = Mathf.Min(groundZero, currStartPos.y); // just in case

        // make new segments if haven't reach the ground
        int count = 0;
        while (currStartPos.y > groundZero && (isMainChannel || segments.Count < maxNumSegments)) {
            LightningSegment seg = gameObject.AddComponent<LightningSegment>() as LightningSegment;
            seg.length = ((float) prng.NextDouble()) * (MaxSegmentLength - MinSegmentLength) + MinSegmentLength;
            seg.direction = currDir;
            seg.start = currStartPos;
            seg.lightningMaterial = lightningMaterial;
            if (isMainChannel || count == 0) {
                seg.cylinderRadius = BranchRadius;
            } else {
                seg.cylinderRadius = segments[count - 1].cylinderRadius * BranchSegmentWidthReductionFactor;
            }

            seg.segmentNumber = count + branchNumber;
            seg.createSegment();
            segments.Add(seg);
            currStartPos = seg.start + seg.direction * seg.length;
            currDir = generateUniformDirection(prng, startDir, MinBranchAngle, MaxBranchAngle);
            count++;
        }
    }

    void buildChildrenBranches(System.Random prng) {

        float perSegmentBranchProb = branchProb;

        for (int i = 1; i < segments.Count; i++) {
            if ((float) (prng.NextDouble()) < perSegmentBranchProb && depth < maxDepth) {
                LightningBranch childBranch = gameObject.AddComponent<LightningBranch>() as LightningBranch;
                childBranch.isMainChannel = false;
                childBranch.lightningMaterial = lightningMaterial;
                childBranch.startDir = generateNormalDirection(prng, startDir, DirectionMean, DirectionVariance);
                childBranch.startPos = segments[i].start;
                childBranch.groundZero = groundZero;
                childBranch.branchProb = branchProb;
                childBranch.depth = depth + 1;
                childBranch.startTime = startTime;
                childBranch.maxLifespan = maxLifespan;
                float childBranchWidthReductionFactor = (float) (prng.NextDouble() *
                                           (BranchReductionFactorMax - MinBranchReductionFactor) + MinBranchReductionFactor);
                if (isMainChannel) {
                    childBranch.BranchRadius = BranchRadius * childBranchWidthReductionFactor;
                    childBranch.lifeFactor = childBranchWidthReductionFactor * childBranchWidthReductionFactor * lifeFactor;
                } else {
                    childBranch.BranchRadius = segments[i].cylinderRadius;
                    childBranch.lifeFactor = lifeFactor;
                }
                childBranch.branchNumber = i;
                childBranch.maxNumSegments = prng.Next() % (segmentsMax - segmentsMin) + segmentsMin;
                childBranch.constructLightningBranch();
                children.Add(childBranch);
            }
        }
    }

    Vector3 generateNormalDirection(System.Random prng, Vector3 refDir, float mean, float variance) {
        float azimuthalAngle = (float) (prng.NextDouble()) * 2.0f * Mathf.PI;
        float normalRVAngle = generateRandomNormal(prng, mean, Mathf.Sqrt(variance));

        Vector3 azimuthalVector = Mathf.Cos(azimuthalAngle) * Vector3.forward + Mathf.Sin(azimuthalAngle) * Vector3.right;
        Vector3 newDirVector = Mathf.Cos(normalRVAngle) * Vector3.down + Mathf.Sin(normalRVAngle) * azimuthalVector;

        var rotate = Quaternion.FromToRotation(Vector3.down, newDirVector);
        return Vector3.Normalize(rotate * refDir);
    }

    Vector3 generateUniformDirection(System.Random prng, Vector3 refDir, float minVal, float maxVal) {
        float azimuthalAngle = (float) (prng.NextDouble()) * 2.0f * Mathf.PI;
        float normalRVAngle = ((float) prng.NextDouble() * (maxVal - minVal) + minVal);

        Vector3 azimuthalVector = Mathf.Cos(azimuthalAngle) * Vector3.forward + Mathf.Sin(azimuthalAngle) * Vector3.right;
        Vector3 newDirVector = Mathf.Cos(normalRVAngle) * Vector3.down + Mathf.Sin(normalRVAngle) * azimuthalVector;

        var rotate = Quaternion.FromToRotation(Vector3.down, newDirVector);
        return Vector3.Normalize(rotate * refDir);
    }

    float generateRandomNormal(System.Random rand, float mean, float stdev) {
        float u1 = (float) (1.0 - rand.NextDouble()); //uniform(0,1] random doubles
        float u2 = (float) (1.0 - rand.NextDouble());
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                     Mathf.Sin(2.0f * Mathf.PI * u2); //random normal(0,1)
        return mean + stdev * randStdNormal; //random normal(mean,stdDev^2)
    }

    public void lightningBranchTick(float newLifespan) {
        lifespan = newLifespan - startTime;
        float brightNess = computeStrokeBrightness();

        foreach (LightningSegment seg in segments) {
            if (lifespan * PropagationSpeed > seg.segmentNumber) {
                seg.setBrightness(brightNess);
            }
        }
        foreach (LightningBranch child in children) {
            child.lightningBranchTick(newLifespan);
        }
    }

    float computeStrokeBrightness() {
        float percentLifespan = (float) (lifespan) / maxLifespan;
        float brightness = Mathf.Exp(-LightningDecayFactor * percentLifespan);

        if (isMainChannel) {
            brightness += ReturnStrokeVariance + Mathf.Pow(ReturnStrokeDecayFactor, percentLifespan) * Mathf.Sin((2 * numReturnStrokes + 1) * Mathf.PI * percentLifespan);
        }

        // exp is designed to ensure continuity at percentLifespan = 1 -- equal to 0 at this time. 
        return brightness * lifeFactor;
    }

    /** Unity **/

    // Start is called before the first frame update
    void Start() {
        prng = new System.Random(randomSeed);
        constructLightningBranch();
    }

    // Update is called once per frame
    void Update() {

    }
    void OnDestroy() {
        foreach (LightningSegment segment in segments) {
            Destroy(segment);
        }

        foreach (LightningBranch child in children) {
            Destroy(child);
        }
    }
}
