using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUtils {
    public static class LightningUtils {
        public static float randomFloat(float min, float max, System.Random prng) {
            return min + (max - min) * ((float) prng.NextDouble());
        }
        public static Vector3 randomVec3(Vector3 min, Vector3 max, System.Random prng) {
            Vector3 r = new Vector3((float) prng.NextDouble(), (float) prng.NextDouble(), (float) prng.NextDouble());
            return Vector3.Scale(r, max - min) + min;
        }
        public static float randomNormal(float mean, float stdev, System.Random prng) {
            float u1 = (float) (1.0 - prng.NextDouble()); //uniform(0,1] random doubles
            float u2 = (float) (1.0 - prng.NextDouble());
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                         Mathf.Sin(2.0f * Mathf.PI * u2); //random normal(0,1)
            return mean + stdev * randStdNormal; //random normal(mean,stdDev^2)
        }

        public static Vector3 generateNormalDirection(Vector3 refDir, float mean, float variance, System.Random prng) {
            float azimuthalAngle = (float) (prng.NextDouble()) * 2.0f * Mathf.PI;
            float normalRVAngle = randomNormal(mean, Mathf.Sqrt(variance), prng);

            Vector3 azimuthalVector = Mathf.Cos(azimuthalAngle) * Vector3.forward + Mathf.Sin(azimuthalAngle) * Vector3.right;
            Vector3 newDirVector = Mathf.Cos(normalRVAngle) * Vector3.down + Mathf.Sin(normalRVAngle) * azimuthalVector;

            var rotate = Quaternion.FromToRotation(Vector3.down, newDirVector);
            return Vector3.Normalize(rotate * refDir);
        }

        public static Vector3 generateUniformDirection(Vector3 refDir, float minVal, float maxVal, System.Random prng) {
            float azimuthalAngle = (float) (prng.NextDouble()) * 2.0f * Mathf.PI;
            float normalRVAngle = ((float) prng.NextDouble() * (maxVal - minVal) + minVal);

            Vector3 azimuthalVector = Mathf.Cos(azimuthalAngle) * Vector3.forward + Mathf.Sin(azimuthalAngle) * Vector3.right;
            Vector3 newDirVector = Mathf.Cos(normalRVAngle) * Vector3.down + Mathf.Sin(normalRVAngle) * azimuthalVector;

            var rotate = Quaternion.FromToRotation(Vector3.down, newDirVector);
            return Vector3.Normalize(rotate * refDir);
        }
    }
}