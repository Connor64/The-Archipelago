using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VoxelType {
    AIR, GROUND
}

public class Voxel {

    public List<Vector3> vertices;
    public List<int> tris;

    public VoxelType voxelType;
    private HashSet<int> cornersBuried;

    private Vector3 worldPosition;

    public Voxel(List<Vector3> vertices, List<int> tris, HashSet<int> cornersBuried, VoxelType type) {
        this.vertices = vertices;
        this.tris = tris;
        this.voxelType = type;
        this.cornersBuried = cornersBuried;
    }

    // public Voxel(List<Vector3> vertices, List<int> tris, HashSet<int> cornersBuried, VoxelType type, Item item) : this(vertices, tris, cornersBuried, type) {
    //     this.item = item;
    // }

    /// <summary>
    /// Returns a HashSet of the corners of the voxel that are buried.
    /// </summary>
    public HashSet<int> GetCorners() {
        return cornersBuried;
    }

    /// <summary>
    /// Sets the world position of the origin of the voxel. Does not move or update the voxel.
    /// </summary>
    /// <param name="worldPosition">The position of the voxel's origin</param>
    public void SetWorldPosition(Vector3 worldPosition) {
        this.worldPosition = worldPosition;
    }

    // /// <summary>
    // /// Determines if the voxel has an item on top of it.
    // /// </summary>
    // /// <returns>True if the voxel has an item</returns>
    // public bool HasItem() {
    //     return (item != null);
    // }

    public bool SpawnItem(ItemType itemType, ItemManager itemManager) {
        // Chance of spawning a crop on a tile
        if (Random.Range(0.0f, 1.0f) > 0.9955f) {
            if (cornersBuried.Contains(3) && cornersBuried.Contains(2) && cornersBuried.Contains(1) && cornersBuried.Contains(0)
                && !cornersBuried.Contains(7) && !cornersBuried.Contains(6) && !cornersBuried.Contains(5) && !cornersBuried.Contains(4)) {

                GameObject itemObj = MonoBehaviour.Instantiate(itemManager.itemObjectPrefab, worldPosition + new Vector3(0.5f, 1, 0.5f), Quaternion.Euler(0, 0, 0));
                itemObj.GetComponent<ItemObject>().SpawnItem(itemType);
                return true;
            }
        }

        return false;
    }
}