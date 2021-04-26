using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CloudMaster : MonoBehaviour
{
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

    float sunny_cloudScale = 0.1f;
    float sunny_density = 1.0f;
    float sunny_densityOffset = -2.0f;

    float storm_cloudScale = 1.0f;
    float storm_density = 1.0f;
    float storm_densityOffset = -1.0f;

    Color color_blue = new Color(0.44f, 0.64f, 0.8f, 1.0f);
    Color color_grey = new Color(0.25f, 0.25f, 0.25f, 1.0f);
    void Awake()
    {
        var weatherMapGen = FindObjectOfType<WeatherMap>();
        if (Application.isPlaying)
        {
            weatherMapGen.UpdateMap();
        }
    }
    int frame = 0;
    void Start()
    {
        setToStormy();
    }

    void setToStormy()
    {
        cloudScale = storm_cloudScale;
        densityMultiplier = storm_density;
        densityOffset = storm_densityOffset;
        colA = color_grey;
        colB = color_grey;
        currentWeather = WeatherStage.Stormy;
    }

    void setToSunny()
    {
        cloudScale = sunny_cloudScale;
        densityMultiplier = sunny_density;
        densityOffset = sunny_densityOffset;
        colA = color_grey;
        colB = color_blue;
        currentWeather = WeatherStage.Sunny;
    }

void towardsStormy() {

}

void towardsSunny() {

}
    float stepPerSecond = 0.3f;

    void Update()
    {
        if (currentWeather == WeatherStage.Stormy)
        {
            towardsSunny();
        } else {
            towardsStormy();
        }

        // check WeatherStage
        
        // TESTING ONLY
        // if (frame % 100 == 0)
        // {
        //     if (currentWeather == WeatherStage.Stormy)
        //     {
        //         setToSunny();
        //         currentWeather = WeatherStage.Sunny;
        //     }
        //     else
        //     {
        //         setToStormy();
        //         currentWeather = WeatherStage.Stormy;
        //     }
        // }

        // // change cloud scale
        // if (cloudScale <= Mathf.Min(sunny_cloudScale, storm_cloudScale))
        // {
        //     cloudScale += stepPerSecond * Time.deltaTime;
        // }
        // else if (cloudScale >= Mathf.Max(sunny_cloudScale, storm_cloudScale))
        // {
        //     cloudScale -= stepPerSecond * Time.deltaTime;
        // }

        // // change density Multiplier
        // if (densityMultiplier <= Mathf.Min(sunny_density, storm_density))
        // {
        //     densityMultiplier += stepPerSecond * Time.deltaTime;
        // }
        // else if (densityMultiplier >= Mathf.Max(sunny_density, storm_density))
        // {
        //     densityMultiplier -= stepPerSecond * Time.deltaTime;
        // }

        // // change densityOffset
        // if (densityOffset <= Mathf.Min(sunny_densityOffset, storm_densityOffset))
        // {
        //     densityOffset += stepPerSecond * Time.deltaTime;
        // }
        // else if (densityOffset >= Mathf.Max(sunny_densityOffset, storm_densityOffset))
        // {
        //     densityOffset -= stepPerSecond * Time.deltaTime;
        // }

    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {

        // Validate inputs
        if (material == null || material.shader != shader)
        {
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
        if (!Application.isPlaying)
        {
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

    void SetDebugParams()
    {

        var noise = FindObjectOfType<NoiseGenerator>();
        var weatherMapGen = FindObjectOfType<WeatherMap>();

        int debugModeIndex = 0;
        if (noise.viewerEnabled)
        {
            debugModeIndex = (noise.activeTextureType == NoiseGenerator.CloudNoiseType.Shape) ? 1 : 2;
        }
        if (weatherMapGen.viewerEnabled)
        {
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