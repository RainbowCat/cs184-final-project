using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBranch : MonoBehaviour
{
    public bool isMainChannel; // TODO change to non-variable
    public int maxNumSegments = 10;
    public int maxDepth = 5;
    public static float minSegmentLength = 0.5f;
   public static float maxSegmentLength = 10.0f;

    public int randomSeed;
    public Vector3 startPos;
    Vector3 startDir = Vector3.down;
    static float DIRECTION_MEAN = 16.0f;
    static float DIRECTION_VARIANCE = 0.1f;
    
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

    void constructLightningBranch() {

        // build main branch that reaches the ground
        var prng = new System.Random(randomSeed);
        buildBranch(prng);

        // build child branches
        buildChildrenBranches(prng);

    }

    void buildChildrenBranches(System.Random prng) {

        float perSegmentBranchProb = branchProb;
        
        for(int i = 1; i < segments.Count; i++){
            if((float)(prng.NextDouble()) < perSegmentBranchProb && depth < maxDepth){
                LightningBranch childBranch = new LightningBranch();
                childBranch.isMainChannel = false;
                childBranch.lightningMaterial = lightningMaterial;
                childBranch.startDir = generateUniformDirection(prng, startDir);
                childBranch.startPos = segments[i].start;
                childBranch.groundZero = groundZero;
                childBranch.branchProb = branchProb;
                childBranch.randomSeed = randomSeed + i;
                childBranch.depth = depth + 1;

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
            LightningSegment seg = new LightningSegment();
            seg.length = ((float)prng.NextDouble()) * (maxSegmentLength - minSegmentLength) + minSegmentLength;
            seg.direction = currDir;
            seg.start = currStartPos;
            seg.lightningMaterial = lightningMaterial;

            seg.createSegment();
            segments.Add(seg);

            currStartPos = seg.start + seg.direction * seg.length;
            currDir = generateUniformDirection(prng, startDir);
        }
    }


    Vector3 generateUniformDirection(System.Random prng, Vector3 refDir)
    {
        float azimuthalAngle = (float)(prng.NextDouble()) * 2.0f * Mathf.PI;
        float normalRVAngle = generateRandomNormal(prng, DIRECTION_MEAN, Mathf.Sqrt(DIRECTION_VARIANCE)) * Mathf.PI / 180.0f;

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
