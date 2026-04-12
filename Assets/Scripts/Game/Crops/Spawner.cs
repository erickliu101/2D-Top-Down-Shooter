using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class Spawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private GameObject carrotPrefab;
    [SerializeField] private Transform player;
    private HeldProjectile heldProjectile;

    private PlayerMovement playerMovement;

    [Header("Settings")]
    [SerializeField] private float spawnOffset = 0.1f;
    [SerializeField] private float cropLifetime = 13f;
    [SerializeField] private float harvestRadius = 1f;
    [SerializeField] private float minHarvestAge = 3f;

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
        TrySpawnCarrot();
    }

    public void Harvest(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        Debug.Log("Spawner: Space pressed (Harvest action received)");
        TryHarvestCrop();
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

    private void TryHarvestCrop()
    {

        if (heldProjectile != null)
        {
            Debug.Log("Spawner: Already holding a projectile, cannot harvest.");
            return;
        }

        if (activeCrops.Count == 0)
        {
            Debug.Log("Spawner: No crops exist to harvest.");
            return;
        }

        Vector3 playerPos = player.position;

        float closestDist = float.MaxValue;
        int closestIndex = -1;

        Debug.Log("Spawner: Checking crops within harvest radius...");

        for (int i = 0; i < activeCrops.Count; i++)
        {
            if (activeCrops[i].crop == null)
                continue;

            float dist = Vector3.Distance(
                playerPos,
                activeCrops[i].crop.transform.position
            );

            Debug.Log(
                $"Detected crop [{i}] " +
                $"Type={activeCrops[i].crop.cropType}, " +
                $"Distance={dist:0.00}"
            );

            float age = Time.time - activeCrops[i].spawnTime;
            if (dist <= harvestRadius)
            {
                Debug.Log(
                    $"→ In range | Age={age:0.00}s " +
                    $"(Min required={minHarvestAge}s)"
                );

                if (age < minHarvestAge)
                {
                    Debug.Log("too young to harvest, skipping.");
                    continue;
                }

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIndex = i;
                }
            }
        }

        if (closestIndex == -1)
        {
            Debug.Log("Spawner: No crops close enough to harvest.");
            return;
        }

        Debug.Log(
            $"Spawner: Harvesting crop " +
            $"{activeCrops[closestIndex].crop.name} " +
            $"at distance {closestDist:0.00}"
        );

        Destroy(activeCrops[closestIndex].crop.gameObject);
        activeCrops.RemoveAt(closestIndex);
    }

    private void TrySpawnCarrot()
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
        if (HasActiveCropType(CropType.Carrot))
            Debug.Log("Spawner: Existing Carrot detected, spawn prevented");
            DebugActiveCrops();

        if (HasActiveCropType(CropType.Carrot))
            return;

        GameObject CarrotObj = Instantiate(carrotPrefab, cellCenter, Quaternion.identity);

        Crop crop = CarrotObj.GetComponent<Crop>();

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
