using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomUtils;

// A lightning branch is a list of segments that are chained to form one line
public class LightningBranch {
    /** constants **/
    static float DirectionMean = 0.179f;
    static float DirectionVariance = 0.1f;

    // set up variables
    public int randomSeed = 0;
    public static System.Random prng;
    public float GroundZero = 0.0f;

    // branch variables
    public bool isMainChannel;
    public Vector3 startPos;
    public Vector3 startDir = Vector3.down;
    public float startTime;
    public float age = 0;

    // geometry
    List<LightningSegment> segments = new List<LightningSegment>();
    public int MaxNumSegments = 10;
    static float MinSegmentLength = 4f;
    static float MaxSegmentLength = 7.5f;
    static float MinSegmentAngle = 0.25f;
    static float MaxSegmentAngle = 0.75f;
    public int segmentsMax = 20;
    public int segmentsMin = 10;
    List<LightningBranch> children = new List<LightningBranch>();
    public int branchNumber = 0; // branch ID
    public float branchProb = 0.07f; // initialize to no branching
    public int depth = 0;
    public int maxDepth = 5;

    // apperance
    public float BranchWidth;
    static float BranchSegmentWidthReductionFactor = 0.95f;
    public Material lightningMaterial;

    // animation
    public float Lifespan;
    public float LifeFactor;
    public int NumReturnStrokes;
    static float ReturnStrokeVariance = 0.3f; // determined via desmos
    static float LightningDecayFactor = 2.5f; // determined via desmos
    static float ReturnStrokeDecayFactor = 0.15f; // determined via desmos
    static float AgeToSegNumConversion = 220.0f; // used for top-down brightness propagation

    // glow
    static float MinGlowReductionFactor = 0.85f;
    static float MaxGlowReductionFactor = 0.95f;

    public void constructLightningBranch() {
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
            LightningSegment currSeg = new LightningSegment();

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
            currDir = LightningUtils.generateNormalDirection(startDir, DirectionMean, DirectionVariance, prng);
            count++;
        }
    }

    void buildChildrenBranches(System.Random prng) {

        float perSegmentBranchProb = branchProb;

        // for each segment of THIS branch (other than first segment), create sub-branches that branches out 
        for (int i = 1; i < segments.Count; i++) {
            // create sub-branch if conditions satisfied
            if ((float) (prng.NextDouble()) < perSegmentBranchProb && depth < maxDepth) {
                LightningBranch childBranch = new LightningBranch();

                // initialize params of current sub-branch
                childBranch.isMainChannel = false;
                childBranch.depth = depth + 1;
                childBranch.startPos = segments[i].startPos; // branch out from the tip of ith segment of THIS branch
                childBranch.startDir = LightningUtils.generateUniformDirection(startDir, MinSegmentAngle, MaxSegmentAngle, prng);

                // inherit parameters from parent branch, i.e. THIS branch
                childBranch.branchProb = branchProb;
                childBranch.GroundZero = GroundZero;
                childBranch.startTime = startTime;
                childBranch.Lifespan = Lifespan;
                childBranch.lightningMaterial = lightningMaterial;

                // glow depends on width of branch
                float childBranchGlowReductionFactor = LightningUtils.randomFloat(MinGlowReductionFactor, MaxGlowReductionFactor, prng);
                if (isMainChannel) {
                    // becomes smaller and dies out if it's the main channel
                    childBranch.BranchWidth = childBranchGlowReductionFactor * BranchWidth;
                    childBranch.LifeFactor = LifeFactor;
                } else {
                    // the sub-branches has same params as the segment it branches out from
                    childBranch.BranchWidth = segments[i].width;
                    childBranch.LifeFactor = LifeFactor;
                }
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
        float percentAge = (float) age / Lifespan;
        float brightness = Mathf.Exp(-LightningDecayFactor * percentAge);
        if (isMainChannel) {
            brightness += ReturnStrokeVariance + Mathf.Pow(ReturnStrokeDecayFactor, percentAge) * Mathf.Sin((2 * NumReturnStrokes + 1) * Mathf.PI * percentAge);
        }
        return brightness * LifeFactor;
    }

    public void lightningStrokeFlicker(float currTime) {
        age = currTime - startTime;
        float brightness = computeStrokeBrightness();

        foreach (LightningSegment seg in segments) {
            if (age * AgeToSegNumConversion > seg.segmentNumber) {
                seg.setBrightness(brightness);
            }
        }
        foreach (LightningBranch child in children) {
            child.lightningStrokeFlicker(currTime);
        }
    }

    /** Unity **/

    // Deconstructor
    public void destroyLightning() {
        foreach (LightningSegment segment in segments) {
            segment.destroySegment();
        }
        foreach (LightningBranch child in children) {
            child.destroyLightning();
        }
    }

    public void OnDestroy() {
        destroyLightning();
    }
}
