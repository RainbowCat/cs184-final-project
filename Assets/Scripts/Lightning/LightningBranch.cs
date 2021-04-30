﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

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

    /** adjustable parameters **/
    public bool isMainChannel; // TODO change to non-variable?
    public Vector3 startPos;
    public float BranchWidth;
    public int MaxNumSegments = 10;
    public int segmentsMax = 20;
    public int segmentsMin = 10;
    public int maxDepth = 5;
    public int randomSeed = 0;
    public float lifeFactor;


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
        while (currStartPos.y > groundZero && (isMainChannel || segments.Count < MaxNumSegments)) {
            LightningSegment currSeg = gameObject.AddComponent<LightningSegment>() as LightningSegment;

            // initialize params of current segment
            currSeg.startPos = currStartPos;
            currSeg.direction = currDir;
            currSeg.length = Utils.randomWith(MinSegmentLength, MaxSegmentLength, prng);
            currSeg.lightningMaterial = lightningMaterial;
            if (isMainChannel || count == 0) {
                currSeg.width = BranchWidth;
            } else {
                // segments become smaller and smaller
                currSeg.width = segments[count - 1].width * BranchSegmentWidthReductionFactor;
            }
            currSeg.segmentNumber = count + branchNumber;
            currSeg.createSegment();

            // assign created segment to THIS branch
            segments.Add(currSeg);

            // update
            currStartPos = currSeg.startPos + currSeg.length * currSeg.direction;
            currDir = generateUniformDirection(prng, startDir, MinBranchAngle, MaxBranchAngle);
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
                childBranch.startDir = Utils.generateNormalDirection(startDir, DirectionMean, DirectionVariance, prng);

                // inherit parameters from parent branch, i.e. THIS branch
                childBranch.branchProb = branchProb;
                childBranch.groundZero = groundZero;
                childBranch.startTime = startTime;
                childBranch.maxLifespan = maxLifespan;
                childBranch.lightningMaterial = lightningMaterial;

                float childBranchWidthReductionFactor = Utils.randomWith(MinBranchReductionFactor, MaxBranchReductionFactor, prng);
                if (isMainChannel) {
                    // becomes smaller and dies out if it's the main channel
                    childBranch.BranchWidth = childBranchWidthReductionFactor * BranchWidth;
                    childBranch.lifeFactor = childBranchWidthReductionFactor * childBranchWidthReductionFactor * lifeFactor;
                } else {
                    // the sub-branches has same params as the segment it branches out from
                    childBranch.BranchWidth = segments[i].width;
                    childBranch.lifeFactor = lifeFactor;
                }
                childBranch.branchNumber = i;
                childBranch.MaxNumSegments = segmentsMin + prng.Next() % (segmentsMax - segmentsMin);
                childBranch.constructLightningBranch();

                // assign created sub-branch to THIS branch
                children.Add(childBranch);
            }
        }
    }

    public void lightningBranchTick(float newLifespan) {
        lifespan = newLifespan - startTime;
        float brightness = computeStrokeBrightness();

        foreach (LightningSegment seg in segments) {
            if (lifespan * PropagationSpeed > seg.segmentNumber) {
                seg.setBrightness(brightness);
            }
        }
        foreach (LightningBranch child in children) {
            child.lightningBranchTick(newLifespan);
        }
    }

    float computeStrokeBrightness() {
        float percentLifespan = (float) lifespan / maxLifespan;
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

    // Deconstructor
    void OnDestroy() {
        foreach (LightningSegment segment in segments) {
            Destroy(segment);
        }
        foreach (LightningBranch child in children) {
            Destroy(child);
        }
    }
}
