using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator {

    public static float getPoint(int x, int z, int seed) {
        return Mathf.PerlinNoise(x + seed, z + seed);
    }

    /// <summary>
    /// Get a 2D array of perlin noise.
    /// </summary>
    /// <param name="size">The width and length of the array of points</param>
    /// <param name="seed">The random seed to change the position</param>
    /// <param name="multiplier">The scale of the perlin noise (smaller is more zoomed in because of smaller increments)</param>
    /// <param name="heightScalar">Scales the height to increase the difference between values</param>
    /// <returns>A 2D array of perlin noise</returns>
    public static float[,] getPerlinMap(int size, float seed, float multiplier, float heightScalar, Vector3 offset, Texture2D heightMap, float heightMapScalar) {
        float[,] map = new float[size, size];

        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                float heightMapOffset = (1.0f - heightMap.GetPixel(x, z).a) * heightMapScalar; // Use the alpha channel as a way to change height (minus )

                map[x, z] = (Mathf.PerlinNoise((x + seed + offset.x) * multiplier, (z + seed + offset.z) * multiplier) * heightScalar) - heightMapOffset;
            }
        }
        return map;
    }

    // public static float[,,] getPerlinHills(int size, int height, float seed, float multiplier) {
    //     float[,,] map = new float[size, height, size];

    //     for (int x = 0; x < size; x++) {
    //         for (int y = 0; y < height; y++) {
    //             for (int z = 0; z < size; z++) {
    //                 if ()
    //             }
    //         }
    //     }
    // }

    private static float get3DPerlinPoint(float x, float y, float z, float multiplier) {
        // TODO: fix symmetry
        float _x = x * multiplier;
        float _y = y * multiplier;
        float _z = z * multiplier;

        float XY = Mathf.PerlinNoise(_x, _y);
        float XZ = Mathf.PerlinNoise(_x, _z);
        float ZY = Mathf.PerlinNoise(_z, _y);

        float YX = Mathf.PerlinNoise(_y, _x);
        float ZX = Mathf.PerlinNoise(_z, _x);
        float YZ = Mathf.PerlinNoise(_y, _z);

        return (XY + XZ + ZY + YX + ZX + YZ) / 6.0f;
    }

    public static float[,,] get3DPerlinMap(int size, int height, float seed, float multiplier, Vector3 offset) {
        float[,,] map = new float[size, height, size];

        Debug.Log(offset);

        for (int x = 0; x < size; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < size; z++) {
                    map[x, y, z] = get3DPerlinPoint(x + seed + offset.x, y + seed, z + seed + offset.z, multiplier);
                    //Debug.Log(map[x, y, z]);
                }
            }
        }
        return map;
    }
}