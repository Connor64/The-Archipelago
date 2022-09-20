using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandGeneration : MonoBehaviour {
    public Material testMaterial;

    public int chunkSize = 1;           // Width and length of a chunk
    public int chunkHeight = 1;         // Max height of a chunk
    public int gridX = 1;               // Chunk count on X-axis
    public int gridZ = 1;               // Chunk count on Z-axis
    public int surfaceHeight = 8;       // Base height of the surface of the chunks

    public float caveThreshold = 0;     // 3D Perlin noise threshold
    public float multiplier = 0.12f;    // Multiplier for noise
    public float heightScalar = 3.0f;   // Height scalar for 2D noise
    public float seed = 0;              // Random seed for noise generation

    public bool randomSeed = true;      // If true, a random seed is generated. Otherwise it is the one chosen in inspector
    public bool regenerate = false;     // Acts as a button, if pressed it regenerates the mesh

    public Texture2D heightMap;         // The height map of the island (used for tapering the island edges)
    public float heightMapScalar;

    public ItemManager itemManager;

    private GameObject[,] chunks;
    public GameObject chunkPrefab;

    private Voxel[,,] voxels3D;

    private Vector3[] points = new Vector3[] {
            new Vector3(0, 0, 0), // 0              Coners of the voxel (0 is the origin)
            new Vector3(0, 0, 1), // 1                              5----6
            new Vector3(1, 0, 1), // 2                              |\   |\
            new Vector3(1, 0, 0), // 3                              | 4----7
            new Vector3(0, 1, 0), // 4                              1-|--2 |
            new Vector3(0, 1, 1), // 5                               \|   \|
            new Vector3(1, 1, 1), // 6                                0----3
            new Vector3(1, 1, 0), // 7
    };

    private List<Vector3> meshVertices;
    private List<int> meshTris;

    // Singleton
    private static IslandGeneration instance;
    public static IslandGeneration Instance {
        get {
            if (instance == null) {
                instance = new IslandGeneration();
            }
            return instance;
        }
    }

    void Awake() {
        // Singleton stuff
        if (instance != null && instance != this) {
            Destroy(gameObject);
        } else {
            instance = this;
        }
    }

    void Start() {
        generateChunks();
        // StartCoroutine(LateStart(0.25f));
    }

    void Update() {
        if (regenerate || Input.GetKeyDown(KeyCode.R)) {
            regenerateVoxels();
            regenerate = false;
        }
    }

    IEnumerator LateStart(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        generateChunks();
    }

    /// <summary>Generates a line of all the different triangle indices</summary>
    void debug() {
        voxels3D = new Voxel[256, 1, 1];

        for (int x = 0; x < 256; x++) {
            List<Vector3> vertices = new List<Vector3>();
            List<int> tris = new List<int>();
            for (int i = 0; i < triangulationTable[x].Length; i++) {
                vertices.Add(points[triangulationTable[x][i]]);
                tris.Add(i);

                // Adds to mesh vertices immediately to save time
                if (meshTris.Count >= 1) {
                    meshTris.Add(meshTris[meshTris.Count - 1] + 1);
                } else {
                    meshTris.Add(i);
                }
                meshVertices.Add(new Vector3(vertices[i].x + x, vertices[i].y, vertices[i].z));
            }
        }
    }

    /// <summary>Regenerates the mesh according to inspector settings</summary>
    void regenerateVoxels() {

        foreach (GameObject GO in chunks) {
            Destroy(GO);
        }

        GameObject entityContainer = GameObject.FindGameObjectWithTag("EntityContainer");

        foreach (Transform sprite in entityContainer.transform) {
            Destroy(sprite.gameObject);
        }

        generateChunks();
    }

    void generateChunks() {
        chunks = new GameObject[gridX, gridZ];

        if (randomSeed) {
            seed = UnityEngine.Random.Range(0.0f, 9999.99f);
        }

        Texture2D[,] chunkHeightMaps = new Texture2D[gridX, gridZ];

        for (int x = 0; x < gridX; x++) {
            for (int z = 0; z < gridZ; z++) {
                if (heightMap.width != (gridX * chunkSize) + 1 || heightMap.height != (gridZ * chunkSize) + 1) {
                    Debug.LogError("Heightmap does not meet the specified size requirements!");
                    return;
                }

                Texture2D chunkMap = new Texture2D(chunkSize + 1, chunkSize + 1); // The height map of the chunk at the specified location

                // Split the island height map into chunks
                for (int xVoxel = 0; xVoxel < chunkSize + 1; xVoxel++) {
                    for (int zVoxel = 0; zVoxel < chunkSize + 1; zVoxel++) {
                        chunkMap.SetPixel(xVoxel, zVoxel, heightMap.GetPixel(xVoxel + (x * (chunkSize)), zVoxel + (z * (chunkSize))));
                    }
                }

                chunkHeightMaps[x, z] = chunkMap; // Add the chunk height map to the array

                chunks[x, z] = Instantiate(chunkPrefab);
                chunks[x, z].transform.position = (new Vector3(x * chunkSize, transform.position.y, z * chunkSize));
                chunks[x, z].GetComponent<Chunk>().Init(testMaterial, chunkMap);
            }
        }
    }

    /// <summary>Generates the voxel data of a chunk</summary>
    void generate3D() {
        voxels3D = new Voxel[chunkSize, chunkSize, chunkSize];

        if (randomSeed) {
            seed = UnityEngine.Random.Range(0.0f, 9999.99f);
        }

        float[,,] perlinMap = NoiseGenerator.get3DPerlinMap(chunkSize + 1, chunkHeight + 1, seed, multiplier, Vector3.zero);
        //float[,,] perlinMap = NoiseGenerator.get3DPerlinMap(chunkSize + 1, 0.25f, multiplier);

        /*
         * Corners of the voxel (0 is the origin)
          5----6
          |\   |\
          | 4----7
          1-|--2 |
           \|   \|
            0----3
         */

        // Consists of { x, y, z } offsets from voxel origin (the same as the vertex array but with ints)
        int[][] cornerPositions = new int[][] {
            new int[] { 0, 0, 0 }, // 0
            new int[] { 0, 0, 1 }, // 1
            new int[] { 1, 0, 1 }, // 2
            new int[] { 1, 0, 0 }, // 3
            new int[] { 0, 1, 0 }, // 4
            new int[] { 0, 1, 1 }, // 5
            new int[] { 1, 1, 1 }, // 6
            new int[] { 1, 1, 0 }, // 7
        };

        // Iterate through each voxel
        for (int x = 0; x < chunkSize; x++) {
            for (int y = 0; y < chunkSize; y++) {
                for (int z = 0; z < chunkSize; z++) {
                    // Check each corner of a voxel (there shouldn't be any out of bounds errors bc perlin map is increased by 1)
                    // Adds it to 'corners' HashSet if it is considered to be "buried" (less than or equal to the threshold)
                    HashSet<int> corners = new HashSet<int>();
                    for (int i = 0; i < cornerPositions.Length; i++) {
                        if (perlinMap[x + cornerPositions[i][0], y + cornerPositions[i][1], z + cornerPositions[i][2]] >= caveThreshold) {
                            corners.Add(i);
                        }
                    }

                    // Evaluate triangulation index
                    int triIndex = 0;
                    foreach (int corner in corners) {
                        switch (corner) {
                            case 0:
                                triIndex += 1;
                                break;
                            case 1:
                                triIndex += 2;
                                break;
                            case 2:
                                triIndex += 4;
                                break;
                            case 3:
                                triIndex += 8;
                                break;
                            case 4:
                                triIndex += 16;
                                break;
                            case 5:
                                triIndex += 32;
                                break;
                            case 6:
                                triIndex += 64;
                                break;
                            case 7:
                                triIndex += 128;
                                break;
                            default:
                                Debug.LogError("Invalid corner buried: " + corner);
                                break;
                        }
                    }

                    // Add voxel to array and assign vertices based on the triangulation tables values at the triIndex
                    List<Vector3> vertices = new List<Vector3>();
                    List<int> tris = new List<int>();
                    for (int i = 0; i < triangulationTable[triIndex].Length; i++) {
                        vertices.Add(points[triangulationTable[triIndex][i]]);
                        tris.Add(i);

                        // Adds to mesh vertices immediately to save time
                        if (meshTris.Count >= 1) {
                            meshTris.Add(meshTris[meshTris.Count - 1] + 1);
                        } else {
                            meshTris.Add(i);
                        }
                        meshVertices.Add(new Vector3(vertices[i].x + x, vertices[i].y + y, vertices[i].z + z));
                    }

                    // Creates a voxel object at [x, y, z] in the voxel array using relative vertices and tris
                    voxels3D[x, y, z] = new Voxel(vertices, tris, corners, VoxelType.GROUND);
                }
            }
        }
    }

    // 7, 6, 5, 4, 3, 2, 1 -> binary order (each corner represents a digit in the 7-bit binary number)
    public static int[][] triangulationTable = new int[][] {
        new int[] { }, // nothing
        new int[] { 1, 3, 4 }, // 0
        new int[] { 0, 5, 2 }, // 1
        new int[] { 3, 4, 2, 2, 4, 5 }, // 1, 0

        new int[] { 3, 1, 6 }, // 2
        new int[] { 1, 3, 4, 3, 1, 6}, // 2, 0
        new int[] { 0, 5, 3, 3, 5, 6 }, // 2, 1
        new int[] { 4, 5, 3, 3, 5, 6 }, // 2, 1, 0

        new int[] { 2, 7, 0 }, // 3
        new int[] { 2, 7, 1, 1, 7, 4 }, // 3, 0
        new int[] { 2, 7, 0, 0, 5, 2 }, // 3, 1
        new int[] { 7, 4, 2, 2, 4, 5 }, // 3, 1, 0

        new int[] { 1, 6, 0, 0, 6, 7 }, // 3, 2
        new int[] { 7, 4, 1, 1, 6, 7 }, // 3, 2, 0
        new int[] { 0, 5, 6, 6, 7, 0}, // 3, 2, 1
        new int[] { 4, 5, 7, 7, 5, 6 }, // 3, 2, 1, 0

        new int[] { 0, 7, 5 }, // 4
        new int[] { 3, 7, 1, 1, 7, 5 }, // 4, 0
        new int[] { 0, 7, 5, 0, 5, 2 }, // 4, 1
        new int[] { 3, 7, 5, 5, 2, 3 }, // 4, 1, 0

        new int[] { 0, 7, 5, 3, 1, 6 }, // 4, 2
        new int[] { 3, 7, 1, 1, 7, 5, 2, 5, 7 }, // 4, 2, 0
        new int[] { 0, 7, 5, 5, 6, 0, 0, 6, 3 }, // 4, 2, 1
        new int[] { 3, 7, 5, 5, 6, 3 }, // 4, 2, 1, 0

        new int[] { 0, 7, 5, 0, 2, 7 }, // 4, 3
        new int[] { 5, 1, 7, 7, 1, 2 }, // 4, 3, 0
        new int[] { 0, 7, 5, 0, 2, 7, 0, 5, 2 }, // 4, 3, 1
        new int[] { 2, 7, 5 }, // 4, 3, 1, 0

        new int[] { 0, 7, 5, 0, 7, 1, 1, 7, 6 }, // 4, 3, 2
        new int[] { 1, 7, 5, 1, 6, 7 }, // 4, 3, 2, 0
        new int[] { 0, 7, 5, 5, 6, 0, 0, 6, 7 }, // 4, 3, 2, 1
        new int[] { 7, 5, 6 }, // 4, 3, 2, 1, 0

        new int[] { 1, 4, 6 }, // 5
        new int[] { 1, 4, 6, 1, 3, 4 }, // 5, 0
        new int[] { 0, 4, 2, 2, 4, 6 }, // 5, 1
        new int[] { 3, 4, 2, 2, 4, 6 }, // 5, 1, 0

        new int[] { 1, 4, 6, 6, 3, 1 }, // 5, 2
        new int[] { 3, 4, 1, 1, 6, 3, 1, 4, 6 }, // 5, 2, 0
        new int[] { 0, 4, 6, 6, 3, 0 }, // 5, 2, 1
        new int[] { 4, 6, 3 }, // 5, 2, 1, 0

        new int[] { 1, 4, 6, 2, 7, 0 }, // 5, 3
        new int[] { 2, 7, 1, 1, 7, 4 }, // 5, 3, 0
        new int[] { 0, 4, 2, 2, 4, 6, 2, 7, 0 }, // 5, 3, 1
        new int[] { 7, 4, 2, 2, 4, 6 }, // 5, 3, 1, 0

        new int[] { 1, 4, 6, 1, 6, 0, 0, 6, 7 }, // 5, 3, 2
        new int[] { 1, 4, 6, 1, 4, 7, 1, 7, 6 }, // 5, 3, 2, 0
        new int[] { 0, 4, 6, 6, 7, 0 }, // 5, 3, 2, 1
        new int[] { 4, 6, 7 }, // 5, 3, 2, 1, 0

        new int[] { 0, 7, 1, 1, 7, 6 }, // 5, 4
        new int[] { 3, 7, 1, 1, 7, 6 }, // 5, 4, 0
        new int[] { 0, 7, 6, 6, 2, 0 }, // 5, 4, 1
        new int[] { 3, 7, 2, 2, 7, 6 }, // 5, 4, 1, 0

        new int[] { 0, 7, 1, 1, 7, 6, 6, 3, 1 }, // 5, 4, 2
        new int[] { 1, 7, 6, 1, 3, 7, 1, 6, 3 }, // 5, 4, 2, 0
        new int[] { 0, 7, 6, 0, 6, 3 }, // 5, 4, 2, 1
        new int[] { 3, 7, 6 }, // 5, 4, 2, 1, 0

        new int[] { 0, 7, 1, 1, 7, 6, 2, 7, 0 }, // 5, 4, 3
        new int[] { 2, 7, 1, 1, 7, 6 }, // 5, 4, 3, 0
        new int[] { 2, 7, 0, 0, 7, 6, 6, 2, 0 }, // 5, 4, 3, 1
        new int[] { 2, 7, 6 }, // 5, 4, 3, 1, 0

        // next 4 are edge cases and probably need to be accounted for in the code
        new int[] { }, // 5, 4, 3, 2 (intersecting square)
        new int[] { }, // 5, 4, 3, 2, 0 (double-sided triangle)
        new int[] { }, // 5, 4, 3, 2, 1 (double-sided triangle)
        new int[] { }, // 5, 4, 3, 2, 1, 0 (not enough vertices)

        new int[] { 2, 5, 7 }, // 6
        new int[] { 2, 5, 7, 3, 4, 1 }, // 6, 0
        new int[] { 0, 5, 2, 2, 5, 7 }, // 6, 1
        new int[] { 2, 5, 7, 3, 4, 2, 2, 4, 5 }, // 6, 1, 0

        new int[] { 1, 5, 3, 3, 5, 7 }, // 6, 2
        new int[] { 3, 4, 1, 1, 5, 3, 3, 5, 7 }, // 6, 2, 0
        new int[] { 0, 5, 3, 3, 5, 7 }, // 6, 2, 1
        new int[] { 3, 4, 5, 5, 7, 3 }, // 6, 2, 1, 0

        new int[] { 2, 5, 7, 2, 7, 0 }, // 6, 3
        new int[] { 2, 5, 7, 2, 7, 1, 1, 7, 4 }, // 6, 3, 0
        new int[] { 2, 5, 7, 2, 7, 0, 0, 5, 2 }, // 6, 3, 1
        new int[] { 2, 5, 7, 2, 7, 4, 2, 4, 5 }, // 6, 3, 1, 0

        new int[] { 1, 5, 7, 7, 0, 1 }, // 6, 3, 2
        new int[] { 1, 5, 7, 1, 7, 4 }, // 6, 3, 2, 0
        new int[] { 0, 5, 7 }, // 6, 3, 2, 1
        new int[] { 4, 5, 7 }, // 6, 3, 2, 1, 0

        new int[] { 0, 7, 5, 2, 5, 7 }, // 6, 4
        new int[] { 3, 7, 1, 1, 7, 5, 2, 5, 7 }, // 6, 4, 0
        new int[] { 0, 7, 5, 0, 5, 2, 2, 5, 7 }, // 6, 4, 1
        new int[] { 2, 5, 7, 3, 7, 5, 3, 5, 2 }, // 6, 4, 1, 0

        new int[] { 0, 7, 5, 3, 7, 1, 1, 7, 5 }, // 6, 4, 2
        new int[] { }, // 6, 4, 2, 0 (edge case, intersecting square)
        new int[] { 0, 7, 5, 3, 5, 7, 3, 0, 5 }, // 6, 4, 2, 1
        new int[] { 3, 5, 7, 3, 7, 5 }, // 6, 4, 2, 1, 0 (possibly bad)

        new int[] { 2, 7, 0, 0, 7, 5, 2, 5, 7 }, // 6, 4, 3
        new int[] { 2, 7, 1, 1, 7, 5, 2, 5, 7 }, // 6, 4, 3, 0
        new int[] { }, // 6, 4, 3, 1 (would create an inverted diamond inside voxel)
        new int[] { }, // 6, 4, 3, 1, 0 (edge case, double-sided triangle)

        new int[] { 0, 7, 5, 1, 7, 0, 1, 5, 7 }, // 6, 4, 3, 2 (hmmmmm)
        new int[] { }, // 6, 4, 3, 2, 0 (double-sided triangle)
        new int[] { }, // 6, 4, 3, 2, 1 (double-sided triangle)
        new int[] { }, // 6, 4, 3, 2, 1, 0 (not enough vertices)

        new int[] { 1, 4, 2, 2, 4, 7 }, // 6, 5
        new int[] { 1, 4, 2, 2, 4, 7, 3, 4, 1 }, // 6, 5, 0
        new int[] { 0, 4, 2, 2, 4, 7 }, // 6, 5, 1
        new int[] { 3, 4, 2, 2, 4, 7 }, // 6, 5, 1, 0

        new int[] { 1, 7, 3, 1, 4, 7 }, // 6, 5, 2
        new int[] { 3, 4, 1, 1, 4, 7, 1, 7, 3 }, // 6, 5, 2, 0
        new int[] { 0, 4, 3, 3, 4, 7 }, // 6, 5, 2, 1
        new int[] { 3, 4, 7 }, // 6, 5, 2, 1, 0

        new int[] { 2, 7, 0, 1, 4, 2, 2, 4, 7 }, // 6, 5, 3
        new int[] { }, // 6, 5, 3, 0 // (edge case, intersecting square)
        new int[] { 0, 4, 2, 2, 4, 7, 2, 7, 0 }, // 6, 5, 3, 1
        new int[] { }, // 6, 5, 3, 1, 0 (double-sided triangle)

        new int[] { 1, 4, 7, 7, 0, 1 }, // 6, 5, 3, 2
        new int[] { }, // 6, 5, 3, 2, 0 (double-sided triangle)
        new int[] { 0, 4, 7 }, // 6, 5, 3, 2, 1 (double-sided triangle)
        new int[] { }, // 6, 5, 3, 2, 1, 0 (not enough vertices)

        new int[] { 0, 7, 1, 7, 2, 1 }, // 6, 5, 4
        new int[] { 3, 7, 1, 1, 7, 2 }, // 6, 5, 4, 0
        new int[] { 0, 7, 2 }, // 6, 5, 4, 1
        new int[] { 3, 7, 2 }, // 6, 5, 4, 1, 0

        new int[] { 0, 7, 1, 1, 7, 3 }, // 6, 5, 4, 2
        new int[] { }, // 6, 5, 4, 2, 0 (double-sided triangle)
        new int[] { 0, 7, 3 }, // 6, 5, 4, 2, 1
        new int[] { }, // 6, 5, 4, 2, 1, 0 (not enough vertices)

        new int[] { 0, 7, 1, 1, 7, 2, 2, 7, 0 }, // 6, 5, 4, 3
        new int[] { }, // 6, 5, 4, 3, 0 (double-sided triangle)
        new int[] { }, // 6, 5, 4, 3, 1 (double sided triangle)
        new int[] { }, // 6, 5, 4, 3, 1, 0 (not enough vertices)

        new int[] { }, // 6, 5, 4, 3, 2 (double-sided triangle)
        new int[] { }, // 6, 5, 4, 3, 2, 0 (not enough vertices)
        new int[] { }, // 6, 5, 4, 3, 2, 1 (not enough vertices)
        new int[] { }, // 6, 5, 4, 3, 2, 1, 0 (not enough vertices)

        new int[] { 3, 6, 4 }, // 7
        new int[] { 3, 6, 4, 3, 4, 1 }, // 7, 0
        new int[] { 3, 6, 4, 0, 5, 2 }, // 7, 1
        new int[] { 3, 4, 2, 2, 4, 5, 3, 6, 4 }, // 7, 1, 0

        new int[] { 3, 6, 4, 1, 6, 3 }, // 7, 2
        new int[] { 3, 6, 4, 1, 6, 3, 3, 4, 1 }, // 7, 2, 0
        new int[] { 3, 6, 4, 0, 5, 3, 3, 5, 6 }, // 7, 2, 1
        new int[] { 3, 6, 4, 3, 4, 5, 3, 5, 6 }, // 7, 2, 1, 0

        new int[] { 2, 6, 0, 0, 6, 4 }, // 7, 3
        new int[] { 2, 6, 4, 4, 1, 2 }, // 7, 3, 0
        new int[] { 0, 5, 2, 2, 6, 0, 0, 6, 4 }, // 7, 3, 1
        new int[] { 2, 4, 5, 2, 6, 4 }, // 7, 3, 1, 0

        new int[] { 1, 6, 0, 0, 6, 4 }, // 7, 3, 2
        new int[] { 1, 6, 4 }, // 7, 3, 2, 0
        new int[] { 0, 5, 6, 6, 4, 0 }, // 7, 3, 2, 1
        new int[] { 5, 6, 4 }, // 7, 3, 2, 1, 0

        new int[] { 3, 6, 0, 0, 6, 5 }, // 7, 4
        new int[] { 3, 5, 1, 3, 6, 5 }, // 7, 4, 0
        new int[] { 3, 6, 0, 0, 6, 5, 0, 5, 2 }, // 7, 4, 1
        new int[] { 3, 5, 2, 3, 6, 5 }, // 7, 4, 1, 0

        new int[] { 3, 6, 0, 0, 6, 5, 1, 6, 5 }, // 7, 4, 2
        new int[] { 3, 6, 1, 3, 6, 5, 3, 5, 1 }, // 7, 4, 2, 0
        new int[] { }, // 7, 4, 2, 1 (intersecting square)
        new int[] { }, // 7, 4, 2, 1, 0 (double-sided triangle)

        new int[] { 0, 6, 5, 2, 6, 0 }, // 7, 4, 3
        new int[] { 2, 6, 1, 1, 6, 5 }, // 7, 4, 3, 0
        new int[] { 0, 5, 2, 2, 6, 0 }, // 7, 4, 3, 1
        new int[] { 2, 6, 5 }, // 7, 4, 3, 1, 0

        new int[] { 0, 6, 5, 1, 6, 0 }, // 7, 4, 3, 2
        new int[] { 1, 6, 5 }, // 7, 4, 3, 2, 0
        new int[] { }, // 7, 4, 3, 2, 1 (double-sided triangle)
        new int[] { }, // 7, 4, 3, 2, 1, 0 (not enough vertices)

        new int[] { 3, 6, 4, 1, 4, 6 }, // 7, 5
        new int[] { 3, 4, 1, 3, 6, 4, 1, 4, 6 }, // 7, 5, 0
        new int[] { 0, 4, 2, 2, 4, 6, 3, 6, 4 }, // 7, 5, 1
        new int[] { 3, 6, 4, 3, 4, 2, 2, 4, 6 }, // 7, 5, 1, 0

        new int[] { 1, 6, 3, 3, 6, 4, 1, 4, 6 }, // 7, 5, 2
        new int[] { }, // 7, 5, 2, 0 (inverted diamond)
        new int[] { 3, 6, 4, 3, 0, 6, 0, 4, 6 }, // 7, 5, 2, 1
        new int[] { }, // 7, 5, 2, 1, 0 (double-sided triangle)

        new int[] { 2, 6, 0, 0, 6, 4, 1, 4, 6 }, // 7, 5, 3
        new int[] { 1, 4, 6, 2, 6, 4, 4, 2, 1 }, // 7, 5, 3, 0
        new int[] { 2, 4, 1, 1, 4, 6, 2, 6, 4 }, // 7, 5, 3, 1
        new int[] { }, // 7, 5, 3, 1, 0 (double-sided triangle)

        new int[] { 1, 4, 6, 1, 6, 0, 0, 6, 4 }, // 7, 5, 3, 2
        new int[] { }, // 7, 5, 3, 2, 0 (double-sided triangle)
        new int[] { }, // 7, 5, 3, 2, 1 (double-sided triangle)
        new int[] { }, // 7, 5, 3, 2, 1, 0 (not enough vertices)

        new int[] { 3, 6, 0, 0, 6, 1 }, // 7, 5, 4
        new int[] { 3, 6, 1 }, // 7, 5, 4, 0
        new int[] { 3, 6, 0, 0, 6, 2 }, // 7, 5, 4, 1
        new int[] { 3, 6, 2 }, // 7, 5, 4, 1, 0

        new int[] { 3, 6, 0, 0, 6, 1, 1, 6, 3 }, // 7, 5, 4, 2
        new int[] { }, // 7, 5, 4, 2, 0 (double-sided triangle)
        new int[] { }, // 7, 5, 4, 2, 1 (double-sided triangle)
        new int[] { }, // 7, 5, 4, 2, 1, 0 (not enough vertices)

        new int[] { 0, 6, 1, 2, 6, 0 }, // 7, 5, 4, 3
        new int[] { 2, 6, 1 }, // 7, 5, 4, 3, 0
        new int[] { }, // 7, 5, 4, 3, 1 (double-sided triangle)
        new int[] { }, // 7, 5, 4, 3, 1, 0 (not enough vertices)

        new int[] { }, // 7, 5, 4, 3, 2
        new int[] { }, // 7, 5, 4, 3, 2, 0 (not enough vertices)
        new int[] { }, // 7, 5, 4, 3, 2, 1 (not enough vertices)
        new int[] { }, // 7, 5, 4, 3, 2, 1, 0 (not enough vertices)

        new int[] { 2, 5, 3, 3, 5, 4 }, // 7, 6
        new int[] { 2, 5, 3, 3, 5, 4, 3, 4, 1 }, // 7, 6, 0
        new int[] { 2, 5, 3, 3, 5, 4, 0, 5, 2 }, // 7, 6, 1
        new int[] { }, // 7, 6, 1, 0 (intersecting square)

        new int[] { 1, 5, 3, 3, 5, 4 }, // 7, 6, 2
        new int[] { 1, 5, 3, 3, 5, 4, 3, 4, 1 }, // 7, 6, 2, 0
        new int[] { 0, 5, 3, 3, 5, 4 }, // 7, 6, 2, 1
        new int[] { }, // 7, 6, 2, 1, 0 (double-sided triangle)

        new int[] { 2, 5, 4, 2, 4, 0 }, // 7, 6, 3
        new int[] { 2, 5, 4, 2, 4, 1 }, // 7, 6, 3, 0
        new int[] { 0, 5, 2, 2, 5, 4 }, // 7, 6, 3, 1
        new int[] { }, // 7, 6, 3, 1, 0 (double-sided triangle)

        new int[] { 1, 5, 0, 0, 5, 4 }, // 7, 6, 3, 2
        new int[] { 1, 5, 4 }, // 7, 6, 3, 2, 0
        new int[] { 0, 5, 4 }, // 7, 6, 3, 2, 1
        new int[] { }, // 7, 6, 3, 2, 1, 0 (not enough vertices)

        new int[] { 2, 5, 3, 3, 5, 0 }, // 7, 6, 4
        new int[] { 2, 5, 3, 3, 5, 1 }, // 7, 6, 4, 0
        new int[] { 0, 5, 2, 2, 5, 3, 3, 5, 0 }, // 7, 6, 4, 1
        new int[] { }, // 7, 6, 4, 1, 0 (double-sided triangle)

        new int[] { 1, 5, 3, 3, 5, 0 }, // 7, 6, 4, 2
        new int[] { }, // 7, 6, 4, 2, 0 (double-sided triangle)
        new int[] { }, // 7, 6, 4, 2, 1 (double-sided triangle)
        new int[] { }, // 7, 6, 4, 2, 1, 0 (not enough vertices)

        new int[] { 2, 5, 0 }, // 7, 6, 4, 3
        new int[] { 2, 5, 1 }, // 7, 6, 4, 3, 0
        new int[] { }, // 7, 6, 4, 3, 1 (double-sided triangle)
        new int[] { }, // 7, 6, 4, 3, 1, 0 (not enough vertices)

        new int[] { 1, 5, 0 }, // 7, 6, 4, 3, 2
        new int[] { }, // 7, 6, 4, 3, 2, 0 (not enough vertices)
        new int[] { }, // 7, 6, 4, 3, 2, 1 (not enough vertices)
        new int[] { }, // 7, 6, 4, 3, 2, 1, 0 ( not enough vertices)

        new int[] { 1, 4, 2, 2, 4, 3 }, // 7, 6, 5
        new int[] { 1, 4, 2, 2, 4, 3, 3, 4, 1 }, // 7, 6, 5, 0
        new int[] { 0, 4, 2, 2, 4, 3 }, // 7, 6, 5, 1
        new int[] { }, // 7, 6, 5, 1, 0 (double-sided triangle)

        new int[] { 1, 4, 3 }, // 7, 6, 5, 2
        new int[] { }, // 7, 6, 5, 2, 0 (double-sided triangle)
        new int[] { 0, 4, 3 }, // 7, 6, 5, 2, 1
        new int[] { }, // 7, 6, 5, 2, 1, 0 (not enough vertices)

        new int[] { 1, 4, 2, 2, 4, 0 }, // 7, 6, 5, 3
        new int[] { }, // 7, 6, 5, 3, 0 (double-sided triangle)
        new int[] { }, // 7, 6, 5, 3, 1 (double-sided triangle)
        new int[] { }, // 7, 6, 5, 3, 1, 0 (not enough vertices)

        new int[] { 1, 4, 0 }, // 7, 6, 5, 3, 2
        new int[] { }, // 7, 6, 5, 3, 2, 0 (not enough vertices)
        new int[] { }, // 7, 6, 5, 3, 2, 1 (not enough vertices)
        new int[] { }, // 7, 6, 5, 3, 2, 1, 0 (not enough vertices)

        new int[] { 3, 2, 0, 0, 2, 1 }, // 7, 6, 5, 4
        new int[] { 3, 2, 1 }, // 7, 6, 5, 4, 0
        new int[] { 3, 2, 0 }, // 7, 6, 5, 4, 1
        new int[] { }, // 7, 6, 5, 4, 1, 0 (not enough vertices)

        new int[] { 3, 1, 0 }, // 7, 6, 5, 4, 2
        new int[] { }, // 7, 6, 5, 4, 2, 0 (not enough vertices)
        new int[] { }, // 7, 6, 5, 4, 2, 1 (not enough vertices)
        new int[] { }, // 7, 6, 5, 4, 2, 1, 0 (not enough vertices)

        new int[] { 0, 2, 1 }, // 7, 6, 5, 4, 3
        new int[] { }, // 7, 6, 5, 4, 3, 0 (not enough vertices)
        new int[] { }, // 7, 6, 5, 4, 3, 1 (not enough vertices)
        new int[] { }, // 7, 6, 5, 4, 3, 1, 0 (not enough vertices)

        new int[] { }, // 7, 6, 5, 4, 3, 2 (not enough vertices)
        new int[] { }, // 7, 6, 5, 4, 3, 2, 0 (not enough vertices)
        new int[] { }, // 7, 6, 5, 4, 3, 2, 1 (not enough vertices)
        new int[] { }, // 7, 6, 5, 4, 3, 2, 1, 0 (no vertices available)
    };
}