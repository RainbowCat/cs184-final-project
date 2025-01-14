﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CloudMaster : MonoBehaviour {
    //At the beginning, set the variables to the Stormy cloud value, and change to cloudy, and then sunny.
    const string headerDecoration = " --- ";
    [Header(headerDecoration + "Main" + headerDecoration)]
    public Shader shader;
    public Transform container;
    public Vector3 cloudTestParams;

    [Header("March settings" + headerDecoration)]
    public int numStepsLight = 8;
    public float rayOffsetStrength;
    public Texture2D blueNoise;

    [Header(headerDecoration + "Base Shape" + headerDecoration)]
    public float cloudScale = 0.5f;
    public float densityMultiplier = 1;
    public float densityOffset;
    public Vector3 shapeOffset;
    public Vector2 heightOffset;
    public Vector4 shapeNoiseWeights;

    [Header(headerDecoration + "Detail" + headerDecoration)]
    public float detailNoiseScale = 10;
    public float detailNoiseWeight = .1f;
    public Vector3 detailNoiseWeights;
    public Vector3 detailOffset;


    [Header(headerDecoration + "Lighting" + headerDecoration)]
    public float lightAbsorptionThroughCloud = 1;
    public float lightAbsorptionTowardSun = 1;
    [Range(0, 1)]
    public float darknessThreshold = .2f;
    [Range(0, 1)]
    public float forwardScattering = .83f;
    [Range(0, 1)]
    public float backScattering = .3f;
    [Range(0, 1)]
    public float baseBrightness = .8f;
    [Range(0, 1)]
    public float phaseFactor = .15f;

    [Header(headerDecoration + "Animation" + headerDecoration)]
    public float timeScale = 1;
    public float baseSpeed = 1;
    public float detailSpeed = 2;

    [Header(headerDecoration + "Sky" + headerDecoration)]
    public Color colA;
    public Color colB;

    // Internal
    [HideInInspector]
    public Material material;

    // weather parametiers
    enum WeatherStage { Stormy, Cloudy, Sunny };
    WeatherStage currentWeather;

    static float sunny_cloudScale = 0.1f;
    static float sunny_density = 0.1f;
    static float sunny_densityOffset = -4.0f;

    static float stormy_cloudScale = 1.0f;
    static float stormy_density = 1.0f;
    static float stormy_densityOffset = -1.0f;

    Color color_blue = new Color(0.44f, 0.64f, 0.8f, 1.0f);
    Color color_grey = new Color(0.25f, 0.25f, 0.25f, 1.0f);

    // match with LightningMaster
    static float transition_time = 10.0f;
    static float stay_time = 10.0f;
    static float period = transition_time * 2 + stay_time * 2;

    float scaleStep = Mathf.Abs(stormy_cloudScale - sunny_cloudScale) / transition_time;
    float densityStep = Mathf.Abs(stormy_density - sunny_density) / transition_time;
    float offsetStep = Mathf.Abs(stormy_densityOffset - sunny_densityOffset) / transition_time;
    float colorStepR;
    float colorStepB;
    float colorStepG;

    void Awake() {
        var weatherMapGen = FindObjectOfType<WeatherMap>();
        if (Application.isPlaying) {
            weatherMapGen.UpdateMap();
        }
        colorStepR = Mathf.Abs(color_grey.r - color_blue.r) / transition_time;
        colorStepB = Mathf.Abs(color_grey.b - color_blue.b) / transition_time;
        colorStepG = Mathf.Abs(color_grey.g - color_blue.g) / transition_time;
    }

    void Start() {
        setToStormy();
    }

    void setToStormy() {
        cloudScale = stormy_cloudScale;
        densityMultiplier = stormy_density;
        densityOffset = stormy_densityOffset;
        colA = color_grey;
        colB = color_grey;
        currentWeather = WeatherStage.Stormy;
    }

    void setToSunny() {
        cloudScale = sunny_cloudScale;
        densityMultiplier = sunny_density;
        densityOffset = sunny_densityOffset;
        colA = color_blue;
        colB = color_blue;
        currentWeather = WeatherStage.Sunny;
    }

    void Update() {
        if (Time.time % period < stay_time) { // stage 1
            setToStormy();
            return;
        }
        if (Time.time % period > stay_time + transition_time
            && Time.time % period < stay_time * 2 + transition_time) { // stage 4
            setToSunny();
            return;
        }
        if (currentWeather == WeatherStage.Stormy) { // stage 2
            // towards sunny
            cloudScale -= scaleStep * Time.deltaTime;
            densityMultiplier -= densityStep * Time.deltaTime;
            densityOffset -= offsetStep * Time.deltaTime;
            colA.r += colorStepR * Time.deltaTime;
            colA.b += colorStepB * Time.deltaTime;
            colA.g += colorStepG * Time.deltaTime;

            colB.r += colorStepR * Time.deltaTime;
            colB.b += colorStepB * Time.deltaTime;
            colB.g += colorStepG * Time.deltaTime;
            // check if reached sunny
            if ((cloudScale <= sunny_cloudScale) | (densityMultiplier <= sunny_density) | (densityOffset <= sunny_densityOffset)) {
                setToSunny();
                currentWeather = WeatherStage.Sunny;
            }
        } else { // stage 3
            // towards stormy
            cloudScale += scaleStep * Time.deltaTime;
            densityMultiplier += densityStep * Time.deltaTime;
            densityOffset += offsetStep * Time.deltaTime;
            colA.r -= colorStepR * Time.deltaTime;
            colA.b -= colorStepB * Time.deltaTime;
            colA.g -= colorStepG * Time.deltaTime;

            colB.r -= colorStepR * Time.deltaTime;
            colB.b -= colorStepB * Time.deltaTime;
            colB.g -= colorStepG * Time.deltaTime;
            // check if reached stormy
            if ((cloudScale >= stormy_cloudScale) | (densityMultiplier >= stormy_density) | (densityOffset >= stormy_densityOffset)) {
                setToStormy();
                currentWeather = WeatherStage.Stormy;
            }
        }
    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture src, RenderTexture dest) {

        // Validate inputs
        if (material == null || material.shader != shader) {
            material = new Material(shader);
        }
        numStepsLight = Mathf.Max(1, numStepsLight);

        // Noise
        var noise = FindObjectOfType<NoiseGenerator>();
        noise.UpdateNoise();

        material.SetTexture("NoiseTex", noise.shapeTexture);
        material.SetTexture("DetailNoiseTex", noise.detailTexture);
        material.SetTexture("BlueNoise", blueNoise);

        // Weathermap
        var weatherMapGen = FindObjectOfType<WeatherMap>();
        if (!Application.isPlaying) {
            weatherMapGen.UpdateMap();
        }
        material.SetTexture("WeatherMap", weatherMapGen.weatherMap);

        Vector3 size = container.localScale;
        int width = Mathf.CeilToInt(size.x);
        int height = Mathf.CeilToInt(size.y);
        int depth = Mathf.CeilToInt(size.z);

        material.SetFloat("scale", cloudScale);
        material.SetFloat("densityMultiplier", densityMultiplier);
        material.SetFloat("densityOffset", densityOffset);
        material.SetFloat("lightAbsorptionThroughCloud", lightAbsorptionThroughCloud);
        material.SetFloat("lightAbsorptionTowardSun", lightAbsorptionTowardSun);
        material.SetFloat("darknessThreshold", darknessThreshold);
        material.SetVector("params", cloudTestParams);
        material.SetFloat("rayOffsetStrength", rayOffsetStrength);

        material.SetFloat("detailNoiseScale", detailNoiseScale);
        material.SetFloat("detailNoiseWeight", detailNoiseWeight);
        material.SetVector("shapeOffset", shapeOffset);
        material.SetVector("detailOffset", detailOffset);
        material.SetVector("detailWeights", detailNoiseWeights);
        material.SetVector("shapeNoiseWeights", shapeNoiseWeights);
        material.SetVector("phaseParams", new Vector4(forwardScattering, backScattering, baseBrightness, phaseFactor));

        material.SetVector("boundsMin", container.position - container.localScale / 2);
        material.SetVector("boundsMax", container.position + container.localScale / 2);

        material.SetInt("numStepsLight", numStepsLight);

        material.SetVector("mapSize", new Vector4(width, height, depth, 0));

        material.SetFloat("timeScale", (Application.isPlaying) ? timeScale : 0);
        material.SetFloat("baseSpeed", baseSpeed);
        material.SetFloat("detailSpeed", detailSpeed);

        // Set debug params
        SetDebugParams();

        material.SetColor("colA", colA);
        material.SetColor("colB", colB);

        // Bit does the following:
        // - sets _MainTex property on material to the source texture
        // - sets the render target to the destination texture
        // - draws a full-screen quad
        // This copies the src texture to the dest texture, with whatever modifications the shader makes
        Graphics.Blit(src, dest, material);
    }

    void SetDebugParams() {

        var noise = FindObjectOfType<NoiseGenerator>();
        var weatherMapGen = FindObjectOfType<WeatherMap>();

        int debugModeIndex = 0;
        if (noise.viewerEnabled) {
            debugModeIndex = (noise.activeTextureType == NoiseGenerator.CloudNoiseType.Shape) ? 1 : 2;
        }
        if (weatherMapGen.viewerEnabled) {
            debugModeIndex = 3;
        }

        material.SetInt("debugViewMode", debugModeIndex);
        material.SetFloat("debugNoiseSliceDepth", noise.viewerSliceDepth);
        material.SetFloat("debugTileAmount", noise.viewerTileAmount);
        material.SetFloat("viewerSize", noise.viewerSize);
        material.SetVector("debugChannelWeight", noise.ChannelMask);
        material.SetInt("debugGreyscale", (noise.viewerGreyscale) ? 1 : 0);
        material.SetInt("debugShowAllChannels", (noise.viewerShowAllChannels) ? 1 : 0);
    }
}