using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;                      // The mesh object that holds all the vertices
    private Material material;              // Temp material of the mesh
    private MeshCollider meshCollider;

    private int chunkSize = 16;             // Width and length of a chunk (passed in)
    private int chunkHeight = 16;           // Max height of a chunk (passed in)
    private int surfaceHeight = 8;          // The target height of the mesh's surface in the chunk

    private float threshold3D = 0;          // 3D Perlin noise threshold (passed in)
    private float multiplier = 0.12f;       // Multiplier for noise (passed in)
    private float heightScalar = 3.0f;      // Scalar for 2D surface noise (passed in)
    private float seed = 0;                 // Random seed for noise generation (passed in)

    private Texture2D heightMap;

    // private int chunkPosX, chunkPosZ;       // The position of the chunk in the array

    private Voxel[,,] voxelData;             // The 3D array of the chunk's voxels (i.e., the chunk data)
    private List<Vector3> meshVertices;     // The list of the chunk's mesh's vertices
    private List<int> meshTris;             // The list of the chunk's meshs's tris

    // These values are used to debug specific voxels in the unity editor
    public Vector3 voxelTestPosition = Vector3.zero;
    public bool giveVertices = false;

    private IslandGeneration generator;

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

    // Consists of { x, y, z } offsets from voxel origin (the same as the vertex array but with ints)
    private int[][] cornerPositions = new int[][] {
            new int[] { 0, 0, 0 }, // 0
            new int[] { 0, 0, 1 }, // 1
            new int[] { 1, 0, 1 }, // 2
            new int[] { 1, 0, 0 }, // 3
            new int[] { 0, 1, 0 }, // 4
            new int[] { 0, 1, 1 }, // 5
            new int[] { 1, 1, 1 }, // 6
            new int[] { 1, 1, 0 }, // 7
    };

    public void Init(Material material, Texture2D heightMap) {
        generator = IslandGeneration.Instance; // Grab singleton instance of IslandGenerator

        // Assign global variables
        this.material = material;
        chunkSize = generator.chunkSize;
        chunkHeight = generator.chunkHeight;
        seed = generator.seed;
        threshold3D = generator.caveThreshold;
        surfaceHeight = generator.surfaceHeight;
        multiplier = generator.multiplier;
        heightScalar = generator.heightScalar;

        // chunkPosX = xIndex;
        // chunkPosZ = zIndex;

        this.heightMap = heightMap;

        // Add mesh filter, renderer, and materials
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshRenderer.sharedMaterial = material;

        // Initialize mesh and arrays
        mesh = new Mesh();
        meshVertices = new List<Vector3>();
        meshTris = new List<int>();

        float startingTime = Time.realtimeSinceStartup; // Begin tracking how long the chunk takes to generate

        // Generate the mesh values and add data to mesh
        GenerateSurface(false);
        // GenerateCaves(true);
        AddVertices();

        print("Chunk generated in " + (Time.realtimeSinceStartup - startingTime) * 1000 + " ms"); // Print the time it took the chunk to generate

        // Add the generated values to the mesh
        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshTris.ToArray();
        mesh.RecalculateNormals();

        // Add mesh to the filter (it will now be rendered)
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    void Update() {
        if (giveVertices) {
            try {
                foreach (Vector3 vert in voxelData[(int)voxelTestPosition.x, (int)voxelTestPosition.y, (int)voxelTestPosition.z].vertices) {
                    print(vert.x + ", " + vert.y + ", " + vert.z);
                }
            } catch (System.IndexOutOfRangeException e) {
                Debug.LogError("Specified voxel index is out of bounds! -> " + e.StackTrace);
            }
            giveVertices = false;
        }
    }

    /// <summary>
    /// Generates the surface of the terrain mesh
    /// </summary>
    private void GenerateSurface(bool cavesGenerated) {
        if (!cavesGenerated) {
            voxelData = new Voxel[chunkSize, chunkHeight, chunkSize];
        }

        // Generates the perlin noise map for the surface of the terrain, so it only needs to be 2D (size is one larger than voxel array so all corners can be read)
        float[,] perlinMap = NoiseGenerator.getPerlinMap(chunkSize + 1, seed, multiplier, heightScalar, transform.position, heightMap, generator.heightMapScalar);

        // bool onEdgeX = chunkPosX == 0;                      // If the chunk is on the close edge of the border (X)
        // bool onFarEdgeX = chunkPosX == generator.gridX - 1; // If the chunk is on the far edge of the border (X)
        // bool onEdgeZ = chunkPosZ == 0;                      // If the chunk is on the edge of the border (Z)
        // bool onFarEdgeZ = chunkPosZ == generator.gridZ - 1; // If the chunk is on the far edge of the border (Z)

        for (int x = 0; x < chunkSize; x++) {
            for (int z = 0; z < chunkSize; z++) {

                // Offset values to give chunk tapering-off effect on edges
                // float xOffset = (onEdgeX ? chunkSize - x : onFarEdgeX ? x : 0) / 2.0f;
                // float zOffset = (onEdgeZ ? chunkSize - z : onFarEdgeZ ? z : 0) / 2.0f;

                for (int y = 0; y < chunkHeight; y++) {
                    HashSet<int> corners = new HashSet<int>();

                    // Iterate through each corner of a voxel
                    for (int i = 0; i < cornerPositions.Length; i++) {

                        // If the corner's position (height) is lower than the value of the perlin map, the corner is considered buried
                        if (y + cornerPositions[i][1] /*+ xOffset + zOffset*/ <= (perlinMap[x + cornerPositions[i][0], z + cornerPositions[i][2]] + surfaceHeight)) {
                            corners.Add(i);
                        }
                    }

                    if (!cavesGenerated || (cavesGenerated && corners.Count > 0)) {
                        SetVoxel(x, y, z, corners);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generates the caves and 3D features using 3D Perlin Noise and a marching cubes algorithm
    /// </summary>
    /// <param name="surfaceGenerated">Whether or not the surface values have been generated and added to the mesh</param>
    private void GenerateCaves(bool surfaceGenerated) {
        if (!surfaceGenerated) {
            voxelData = new Voxel[chunkSize, chunkHeight, chunkSize];
        }

        // Generate the perlin noise map used for mesh generation (one larger than the chunk size so surrounding corners can be checked)
        // (i.e., even if you are on the last voxel, you can still check all of its corners)
        float[,,] perlinMap = NoiseGenerator.get3DPerlinMap(chunkSize + 1, chunkHeight + 1, seed, multiplier, transform.position);

        /*
         * Corners of the voxel (0 is the origin)
          5----6
          |\   |\
          | 4----7
          1-|--2 |
           \|   \|
            0----3
         */

        // Iterate through each voxel
        for (int x = 0; x < chunkSize; x++) {
            for (int y = 0; y < chunkHeight; y++) {
                for (int z = 0; z < chunkSize; z++) {
                    // Only run the following logic IFF the surface hasn't been generated or, if it has been generated, the voxel type is ground
                    if (!surfaceGenerated || (surfaceGenerated && voxelData[x, y, z].voxelType == VoxelType.GROUND)) {
                        // Check each corner of a voxel (there shouldn't be any out of bounds errors bc perlin map is increased by 1)
                        // Adds it to 'corners' HashSet if it is considered to be "buried" (less than or equal to the threshold)
                        HashSet<int> corners = new HashSet<int>();
                        for (int i = 0; i < cornerPositions.Length; i++) {
                            if (perlinMap[x + cornerPositions[i][0], y + cornerPositions[i][1], z + cornerPositions[i][2]] >= threshold3D) {
                                corners.Add(i);
                            }
                        }

                        // Sets the voxel at the indicated position
                        SetVoxel(x, y, z, corners);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Assigns the voxel at the given position based on the corners
    /// </summary>
    /// <param name="x">The x coordinate of the voxel in the 3D array</param>
    /// <param name="y">The y coodrinate of the voxel in the 3D array</param>
    /// <param name="z">The z coordinate of the voxel in the 3D array</param>
    /// <param name="corners">The 'buried' corners of the voxel</param>
    private void SetVoxel(int x, int y, int z, HashSet<int> corners) {
        // Evaluate triangulation index
        int triIndex = EvaluateCorners(corners);

        // Add voxel to array and assign vertices based on the triangulation tables values at the triIndex
        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();
        // Vector3 offset = new Vector3(0, heightOffset, 0);
        for (int i = 0; i < IslandGeneration.triangulationTable[triIndex].Length; i++) {
            vertices.Add(points[IslandGeneration.triangulationTable[triIndex][i]]);
            tris.Add(i);
        }

        // Creates a voxel object at [x, y, z] in the voxel array using relative vertices and tris
        voxelData[x, y, z] = new Voxel(vertices, tris, corners, corners.Count > 0 ? VoxelType.GROUND : VoxelType.AIR);
        // Sets the voxel's world position
        voxelData[x, y, z].SetWorldPosition(new Vector3(x + transform.position.x, y + transform.position.y, z + transform.position.z));

        if (corners.Count == 4) {
            // Randomly spawns an item on the voxel if it is a flat surface
            voxelData[x, y, z].SpawnItem(ItemType.crop, generator.itemManager); // TODO: Make items other than crops spawnable (like sticks)
        }
    }

    private void AddVertices() {
        for (int x = 0; x < chunkSize; x++) {
            for (int y = 0; y < chunkHeight; y++) {
                for (int z = 0; z < chunkSize; z++) {
                    for (int i = 0; i < voxelData[x, y, z].vertices.Count; i++) {
                        if (meshTris.Count > 0) {
                            meshTris.Add(meshTris[meshTris.Count - 1] + 1);
                        } else {
                            meshTris.Add(i);
                        }

                        meshVertices.Add(new Vector3(voxelData[x, y, z].vertices[i].x + x, voxelData[x, y, z].vertices[i].y + y, voxelData[x, y, z].vertices[i].z + z));
                    }
                }
            }
        }
    }

    private int EvaluateCorners(HashSet<int> corners) {
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
        return triIndex;
    }
}