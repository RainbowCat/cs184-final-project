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
    public int randomSeed = 0;

    public float lifeFactor;
    public Vector3 startPos;

    public int numReturnStrokes;
    Vector3 startDir = Vector3.down;
    static float DIRECTION_MEAN = 0.179f;
    static float DIRECTION_VARIANCE = 0.1f;

    static float BRANCH_ANGLE_MIN = 0.18f;
    static float BRANCH_ANGLE_MAX = 0.75f;

    static float MAX_BRANCH_REDUCTION_FACTOR = 0.6f;
    static float MIN_BRANCH_REDUCTION_FACTOR = 0.45f;

    static float branchSegmentWidthReductionFactor = 0.95f;


    // determined via desmos 
    static float returnStrokeVariance = 0.3f;
    static float lightningDecayFactor = 5f;

    static float returnStrokeDecayFactor = 0.15f;

    static float propagationSpeed = 10f;

    public static System.Random prng;
    int depth = 0;

    float age = 0;
    public float maxAge;
    public float branchProb = 0.05f; // initialize to no branching

    public int branchNumber = 0;

    List<LightningSegment> segments = new List<LightningSegment>();
    List<LightningBranch> children = new List<LightningBranch>();

    public Material lightningMaterial;
    public float groundZero; // TODO change to non-variable
    public float startTime;


    // Start is called before the first frame update
    void Start()
    {
        prng = new System.Random(randomSeed);
        constructLightningBranch();
    }

    void constructLightningBranch()
    {
        // build main branch that reaches the ground
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
                childBranch.depth = depth + 1;
                childBranch.startTime = startTime;
                childBranch.maxAge = maxAge;
                float childBranchWidthReductionFactor =  (float)(prng.NextDouble() *
                                           (MAX_BRANCH_REDUCTION_FACTOR - MIN_BRANCH_REDUCTION_FACTOR) + MIN_BRANCH_REDUCTION_FACTOR);
                if(isMainChannel) {
                    childBranch.branchRadius = branchRadius * childBranchWidthReductionFactor;
                    childBranch.lifeFactor = childBranchWidthReductionFactor * childBranchWidthReductionFactor * lifeFactor;
                }
                else {
                    childBranch.branchRadius = segments[i].cylinderRadius;
                    childBranch.lifeFactor = lifeFactor;
                }
                childBranch.branchNumber = i;
                childBranch.maxNumSegments = prng.Next() % (segmentsMax - segmentsMin) + segmentsMin;
                childBranch.constructLightningBranch();
                children.Add(childBranch);
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
        int count = 0;
        while (currStartPos.y > groundZero && (isMainChannel || segments.Count < maxNumSegments))
        {
            LightningSegment seg = gameObject.AddComponent<LightningSegment>() as LightningSegment;
            seg.length = ((float)prng.NextDouble()) * (maxSegmentLength - minSegmentLength) + minSegmentLength;
            seg.direction = currDir;
            seg.start = currStartPos;
            seg.lightningMaterial = lightningMaterial;
            if(isMainChannel || count == 0){
                seg.cylinderRadius = branchRadius;
            } 
            else {
                seg.cylinderRadius = segments[count - 1].cylinderRadius * branchSegmentWidthReductionFactor;
            }
            
            seg.segmentNumber = count + branchNumber;
            seg.createSegment();
            segments.Add(seg);
            currStartPos = seg.start + seg.direction * seg.length;
            currDir = generateUniformDirection(prng, startDir, BRANCH_ANGLE_MIN, BRANCH_ANGLE_MAX);
            count++;
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

    public void lightningBranchTick(float newAge){
        age = newAge - startTime;
        float brightNess = computeStrokeBrightness();
        
        foreach(LightningSegment seg in segments){
            if(age * propagationSpeed > seg.segmentNumber){
                seg.setBrightness(brightNess);
            } 
        }
        foreach(LightningBranch child in children){
            child.lightningBranchTick(newAge);
        }

    }

    float computeStrokeBrightness(){

        
        float percentAge = (float) (age) / maxAge;
        float brightness = Mathf.Exp(- lightningDecayFactor * percentAge);
        

        if(isMainChannel){

            brightness += returnStrokeVariance + Mathf.Pow(returnStrokeDecayFactor, percentAge) * Mathf.Sin((2 * numReturnStrokes + 1) * Mathf.PI * percentAge);
        }
        

        // exp is designed to ensure continuity at percentAge = 1 -- equal to 0 at this time. 
        return brightness * lifeFactor;

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnDestroy() {
        foreach(LightningSegment segment in segments){
            Destroy(segment);
        }

        foreach(LightningBranch child in children){
            Destroy(child);
        }
    }
}
