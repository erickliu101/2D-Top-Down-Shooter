using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class Spawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private GameObject wheatPrefab;
    [SerializeField] private Transform player;

    private PlayerMovement playerMovement;

    [Header("Settings")]
    [SerializeField] private float spawnOffset = 0.1f;
    [SerializeField] private float cropLifetime = 13f;

    private List<SpawnedCrop> activeCrops = new List<SpawnedCrop>();
    private struct SpawnedCrop
    {
        public Crop crop;
        public float spawnTime;
    }

    private void Awake()
    {
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    public void Spawn(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        Debug.Log("Spawner: Z pressed (Spawn action received)");
        TrySpawnWheat();
    }

    private void Update()
    {
        UpdateCropLifetimes();
    }
    private bool HasActiveCropType(CropType type)
    {
        foreach (var crop in activeCrops)
        {
            if (crop.crop == null)
                continue;

            if (crop.crop.cropType == type)
                return true;
        }

        return false;
    }

    private void TrySpawnWheat()
    {
        Vector2 lastDir = playerMovement.GetLastMoveDirection();

        Vector2 spawnDir;
        if (lastDir.x > 0)
            spawnDir = Vector2.right;
        else if (lastDir.x < 0)
            spawnDir = Vector2.left;
        else
            spawnDir = Vector2.right;

        Vector3 spawnWorldPos = player.position + (Vector3)(spawnDir * spawnOffset);
        Vector3Int cellPos = tilemap.WorldToCell(spawnWorldPos);

        if (!tilemap.HasTile(cellPos))
            return;

        Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPos);

        Collider2D[] hits = Physics2D.OverlapPointAll(cellCenter);
        foreach (Collider2D hit in hits)
        {
            Renderer r = hit.GetComponent<Renderer>();
            if (r != null && r.sortingLayerName == "Collision")
                return;
        }
        if (HasActiveCropType(CropType.Wheat))
            Debug.Log("Spawner: Existing Wheat detected, spawn prevented");
            DebugActiveCrops();

        if (HasActiveCropType(CropType.Wheat))
            return;

        GameObject wheatObj = Instantiate(wheatPrefab, cellCenter, Quaternion.identity);

        Crop crop = wheatObj.GetComponent<Crop>();

        activeCrops.Add(new SpawnedCrop
        {
            crop = crop,
            spawnTime = Time.time
        });

        DebugActiveCrops();
    }
    private void DebugActiveCrops()
    {
        if (activeCrops.Count == 0)
        {
            Debug.Log("Spawner: activeCrops is EMPTY");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"Spawner: Active Crops ({activeCrops.Count})");

        for (int i = 0; i < activeCrops.Count; i++)
        {
            var entry = activeCrops[i];

            if (entry.crop == null)
            {
                sb.AppendLine($"[{i}] NULL crop reference");
                continue;
            }

            sb.AppendLine(
                $"[{i}] Type: {entry.crop.cropType}, " +
                $"Name: {entry.crop.name}, " +
                $"Alive For: {(Time.time - entry.spawnTime):0.00}s"
            );
        }

        Debug.Log(sb.ToString());
    }
    private void UpdateCropLifetimes()
    {
        for (int i = activeCrops.Count - 1; i >= 0; i--)
        {
            if (Time.time - activeCrops[i].spawnTime >= cropLifetime)
            {
                if (activeCrops[i].crop != null)
                {
                    Destroy(activeCrops[i].crop.gameObject);
                }

                activeCrops.RemoveAt(i);
            }
        }
    }
}
