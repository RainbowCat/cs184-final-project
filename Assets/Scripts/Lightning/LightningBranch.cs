using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBranch : MonoBehaviour
{
    public bool isMainChannel; // TODO change to non-variable
    public int maxNumSegments = 10;

    public int segmentsMax = 20;
    public int segmentsMin = 10;
    public int maxDepth = 5;
    public static float minSegmentLength = 1f;
    public static float maxSegmentLength = 2.5f;


    public float branchRadius;
    public int randomSeed;
    public Vector3 startPos;
    Vector3 startDir = Vector3.down;
    static float DIRECTION_MEAN = 0.179f;
    static float DIRECTION_VARIANCE = 0.1f;

    static float BRANCH_ANGLE_MIN = 0.18f;
    static float BRANCH_ANGLE_MAX = 0.75f;

    static float MAX_BRANCH_REDUCTION_FACTOR = 0.6f;
    static float MIN_BRANCH_REDUCTION_FACTOR = 0.45f;
    int depth = 0;
    public float branchProb = 0.0f; // initialize to no branching

    List<LightningSegment> segments = new List<LightningSegment>();
    List<(int, LightningBranch)> children = new List<(int, LightningBranch)>();

    public Material lightningMaterial;
    public float groundZero; // TODO change to non-variable

    // Start is called before the first frame update
    void Start()
    {
        constructLightningBranch();
    }

    void constructLightningBranch()
    {

        // build main branch that reaches the ground
        var prng = new System.Random(randomSeed);
        buildBranch(prng);

        // build child branches
        buildChildrenBranches(prng);

    }

    void buildChildrenBranches(System.Random prng)
    {

        float perSegmentBranchProb = branchProb;

        for (int i = 1; i < segments.Count; i++)
        {
            if ((float)(prng.NextDouble()) < perSegmentBranchProb && depth < maxDepth)
            {
                LightningBranch childBranch = gameObject.AddComponent<LightningBranch>() as LightningBranch;
                childBranch.isMainChannel = false;
                childBranch.lightningMaterial = lightningMaterial;
                childBranch.startDir = generateNormalDirection(prng, startDir, DIRECTION_MEAN, DIRECTION_VARIANCE);
                childBranch.startPos = segments[i].start;
                childBranch.groundZero = groundZero;
                childBranch.branchProb = branchProb;
                childBranch.randomSeed = randomSeed + i;
                childBranch.depth = depth + 1;
                childBranch.branchRadius = branchRadius * (float)(prng.NextDouble() *
                                           (MAX_BRANCH_REDUCTION_FACTOR - MIN_BRANCH_REDUCTION_FACTOR) + MIN_BRANCH_REDUCTION_FACTOR);
                childBranch.maxNumSegments = prng.Next() % (segmentsMax - segmentsMin) + segmentsMin;
                childBranch.constructLightningBranch();
                children.Add((i, childBranch));
            }
        }
    }

    void buildBranch(System.Random prng)
    {
        if (isMainChannel)
        {
            startDir = Vector3.down;
        }
        Vector3 currDir = startDir;
        Vector3 currStartPos = startPos;
        groundZero = Mathf.Min(groundZero, currStartPos.y); // just in case

        // make new segments if haven't reach the ground
        while (currStartPos.y > groundZero && (isMainChannel || segments.Count < maxNumSegments))
        {
            LightningSegment seg = gameObject.AddComponent<LightningSegment>() as LightningSegment;
            seg.length = ((float)prng.NextDouble()) * (maxSegmentLength - minSegmentLength) + minSegmentLength;
            seg.direction = currDir;
            seg.start = currStartPos;
            seg.lightningMaterial = lightningMaterial;
            seg.cylinderRadius = branchRadius;
            seg.createSegment();
            segments.Add(seg);
            currStartPos = seg.start + seg.direction * seg.length;
            currDir = generateUniformDirection(prng, startDir, BRANCH_ANGLE_MIN, BRANCH_ANGLE_MAX);
        }
    }


    Vector3 generateNormalDirection(System.Random prng, Vector3 refDir, float mean, float variance)
    {
        float azimuthalAngle = (float)(prng.NextDouble()) * 2.0f * Mathf.PI;
        float normalRVAngle = generateRandomNormal(prng, mean, Mathf.Sqrt(variance));

        Vector3 azimuthalVector = Mathf.Cos(azimuthalAngle) * Vector3.forward + Mathf.Sin(azimuthalAngle) * Vector3.right;
        Vector3 newDirVector = Mathf.Cos(normalRVAngle) * Vector3.down + Mathf.Sin(normalRVAngle) * azimuthalVector;

        var rotate = Quaternion.FromToRotation(Vector3.down, newDirVector);
        return Vector3.Normalize(rotate * refDir);
    }

    Vector3 generateUniformDirection(System.Random prng, Vector3 refDir, float minVal, float maxVal)
    {
        float azimuthalAngle = (float)(prng.NextDouble()) * 2.0f * Mathf.PI;
        float normalRVAngle = ((float)prng.NextDouble() * (maxVal - minVal) + minVal);

        Vector3 azimuthalVector = Mathf.Cos(azimuthalAngle) * Vector3.forward + Mathf.Sin(azimuthalAngle) * Vector3.right;
        Vector3 newDirVector = Mathf.Cos(normalRVAngle) * Vector3.down + Mathf.Sin(normalRVAngle) * azimuthalVector;

        var rotate = Quaternion.FromToRotation(Vector3.down, newDirVector);
        return Vector3.Normalize(rotate * refDir);
    }

    float generateRandomNormal(System.Random rand, float mean, float stdev)
    {
        float u1 = (float)(1.0 - rand.NextDouble()); //uniform(0,1] random doubles
        float u2 = (float)(1.0 - rand.NextDouble());
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                     Mathf.Sin(2.0f * Mathf.PI * u2); //random normal(0,1)
        return mean + stdev * randStdNormal; //random normal(mean,stdDev^2)
    }

    // Update is called once per frame
    void Update()
    {

    }
}
