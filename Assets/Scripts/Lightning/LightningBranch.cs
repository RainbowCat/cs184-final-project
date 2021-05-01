using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomUtils;

// A lightning branch is a list of segments that are chained to form one line
public class LightningBranch : MonoBehaviour {
    /** constants **/
    static float DirectionMean = 0.179f;
    static float DirectionVariance = 0.1f;

    // set up variables
    public int randomSeed = 0;
    public static System.Random prng;
    public float GroundZero = 0.0f;

    // branch variables
    public bool isMainChannel; // TODO change to non-variable?
    public Vector3 startPos;
    public Vector3 startDir = Vector3.down;
    public float startTime;
    public float age = 0;

    // geometry

    List<LightningSegment> segments = new List<LightningSegment>();
    public int MaxNumSegments = 10;
    static float MinSegmentLength = 1f;
    static float MaxSegmentLength = 2.5f;
    static float MinSegmentAngle = 0.18f;
    static float MaxSegmentAngle = 0.75f;
    public int segmentsMax = 20;
    public int segmentsMin = 10;
    List<LightningBranch> children = new List<LightningBranch>();
    public int branchNumber = 0; // branch ID
    public float branchProb = 0.05f; // initialize to no branching
    public int depth = 0;
    public int maxDepth = 5;

    // apperance
    public float BranchWidth;
    static float BranchSegmentWidthReductionFactor = 0.95f;
    public Material lightningMaterial;

    // animation
    public float maxLifespan;
    public float lifeFactor;
    public int numReturnStrokes;
    static float ReturnStrokeVariance = 0.3f; // determined via desmos
    static float LightningDecayFactor = 3f; // determined via desmos
    static float ReturnStrokeDecayFactor = 0.15f; // determined via desmos
    static float PropagationSpeed = 10.0f;

    // glow
    static float MinGlowReductionFactor = 0.45f;
    static float MaxGlowReductionFactor = 0.6f;

    void initializeBranch(
        bool isMainChannel,
        int depth,
        float groundZero,
        Vector3 startPos,
        Vector3 startDir,
        float startTime,
        float lifespan,
        Material material
        ) {
        this.isMainChannel = isMainChannel;
        this.depth = depth;
        this.GroundZero = groundZero;
        this.startPos = startPos;
        this.startDir = startDir;
        this.startTime = startTime;
        this.maxLifespan = lifespan;
        this.lightningMaterial = material;
    }

    void constructLightningBranch() {
        // build main branch that reaches the ground
        buildBranch(prng);

        // build child branches
        buildChildrenBranches(prng);
    }

    void buildBranch(System.Random prng) {
        if (isMainChannel) {
            startDir = Vector3.down;
        }
        Vector3 currDir = startDir;
        Vector3 currStartPos = startPos;
        GroundZero = Mathf.Min(GroundZero, currStartPos.y); // just in case

        // make new segments if haven't reach the ground
        int count = 0;
        while (currStartPos.y > GroundZero && (isMainChannel || segments.Count < MaxNumSegments)) {
            LightningSegment currSeg = gameObject.AddComponent<LightningSegment>() as LightningSegment;

            // initialize params of current segment
            currSeg.startPos = currStartPos;
            currSeg.direction = currDir;
            currSeg.length = LightningUtils.randomFloat(MinSegmentLength, MaxSegmentLength, prng);
            currSeg.lightningMaterial = lightningMaterial;
            if (isMainChannel || count == 0) {
                currSeg.width = BranchWidth;
            } else {
                // segments become smaller and smaller
                currSeg.width = segments[count - 1].width * BranchSegmentWidthReductionFactor;
            }
            currSeg.segmentNumber = branchNumber + count;
            currSeg.createSegment();

            // assign created segment to THIS branch
            segments.Add(currSeg);

            // update
            currStartPos = currSeg.startPos + currSeg.length * currSeg.direction;
            currDir = LightningUtils.generateUniformDirection(startDir, MinSegmentAngle, MaxSegmentAngle, prng);
            count++;
        }
    }

    void buildChildrenBranches(System.Random prng) {

        float perSegmentBranchProb = branchProb;

        // for each segment of THIS branch (other than first segment), create sub-branches that branches out 
        for (int i = 1; i < segments.Count; i++) {
            // create sub-branch if conditions satisfied
            if ((float) (prng.NextDouble()) < perSegmentBranchProb && depth < maxDepth) {
                LightningBranch childBranch = gameObject.AddComponent<LightningBranch>() as LightningBranch;

                // initialize params of current sub-branch
                childBranch.isMainChannel = false;
                childBranch.depth = depth + 1;
                childBranch.startPos = segments[i].startPos; // branch out from the tip of ith segment of THIS branch
                childBranch.startDir = LightningUtils.generateNormalDirection(startDir, DirectionMean, DirectionVariance, prng);

                // inherit parameters from parent branch, i.e. THIS branch
                childBranch.branchProb = branchProb;
                childBranch.GroundZero = GroundZero;
                childBranch.startTime = startTime;
                childBranch.maxLifespan = maxLifespan;
                childBranch.lightningMaterial = lightningMaterial;

                // FIXME: these equations are for glow, not cylinder radius
                float childBranchWidthReductionFactor = LightningUtils.randomFloat(MinGlowReductionFactor, MaxGlowReductionFactor, prng);
                if (isMainChannel) {
                    // becomes smaller and dies out if it's the main channel
                    childBranch.BranchWidth = childBranchWidthReductionFactor * BranchWidth;
                    childBranch.lifeFactor = childBranchWidthReductionFactor * childBranchWidthReductionFactor * lifeFactor;
                } else {
                    // the sub-branches has same params as the segment it branches out from
                    childBranch.BranchWidth = segments[i].width;
                    childBranch.lifeFactor = lifeFactor;
                }
                // FIXME ends
                childBranch.branchNumber = i;
                childBranch.MaxNumSegments = segmentsMin + prng.Next() % (segmentsMax - segmentsMin);
                childBranch.constructLightningBranch();

                // assign created sub-branch to THIS branch
                children.Add(childBranch);
            }
        }
    }

    float computeStrokeBrightness() {
        // sinusoidal decaying exponential for brightness
        float percentAge = (float) age / maxLifespan;
        float brightness = Mathf.Exp(-LightningDecayFactor * percentAge);
        if (isMainChannel) {
            brightness += ReturnStrokeVariance + Mathf.Pow(ReturnStrokeDecayFactor, percentAge) * Mathf.Sin((2 * numReturnStrokes + 1) * Mathf.PI * percentAge);
        }
        // exp is designed to ensure continuity at percentAge = 1 -> brightness = 0 
        return brightness * lifeFactor;
    }

    public void lightningStrokeFlicker(float currTime) {
        age = currTime - startTime;
        float brightness = computeStrokeBrightness();

        foreach (LightningSegment seg in segments) {
            if (age * PropagationSpeed > seg.segmentNumber) {
                seg.setBrightness(brightness);
            }
        }
        foreach (LightningBranch child in children) {
            child.lightningStrokeFlicker(currTime);
        }
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

    // Deconstructor
    public void destroyLightning() {
        foreach (LightningSegment segment in segments) {
            segment.destroySegment();
        }
        foreach (LightningBranch child in children) {
            child.destroyLightning();
        }
    }
}
