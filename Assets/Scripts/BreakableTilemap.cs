using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableTilemap : MonoBehaviour
{
    [Header("Tilemap")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TilemapCollider2D tilemapCollider;

    [Header("Breakable Tiles")]
    [SerializeField] private TileBase[] breakableTiles;
    [SerializeField] private string[] breakableTileNames = { "Terrain (16x16)_27" };

    private readonly HashSet<TileBase> breakableTileSet = new HashSet<TileBase>();
    private readonly HashSet<string> breakableTileNameSet = new HashSet<string>();

    private void Awake()
    {
        CacheComponents();
        RebuildBreakableTileSet();
    }

    private void OnValidate()
    {
        CacheComponents();
    }

    public bool TryBreakAtWorldPoint(Vector2 worldPoint, Vector2 shotDirection)
    {
        if (tilemap == null)
            return false;

        if (!TryFindBreakableCell(worldPoint, shotDirection, out Vector3Int cellPosition))
            return false;

        tilemap.SetTile(cellPosition, null);

        // Forces the tile collider to update immediately after the wall tile disappears.
        if (tilemapCollider != null)
            tilemapCollider.ProcessTilemapChanges();

        return true;
    }

    private bool TryFindBreakableCell(Vector2 worldPoint, Vector2 shotDirection, out Vector3Int cellPosition)
    {
        Vector2 direction = shotDirection.sqrMagnitude > 0.001f
            ? shotDirection.normalized
            : Vector2.right;

        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        Vector2[] samplePoints =
        {
            worldPoint,
            worldPoint + direction * 0.08f,
            worldPoint - direction * 0.08f,
            worldPoint + direction * 0.32f,
            worldPoint - direction * 0.32f,
            worldPoint + perpendicular * 0.18f,
            worldPoint - perpendicular * 0.18f
        };

        for (int i = 0; i < samplePoints.Length; i++)
        {
            cellPosition = tilemap.WorldToCell(samplePoints[i]);

            if (IsBreakable(tilemap.GetTile(cellPosition)))
                return true;
        }

        cellPosition = default;
        return false;
    }

    private bool IsBreakable(TileBase tile)
    {
        if (tile == null)
            return false;

        return breakableTileSet.Contains(tile) || breakableTileNameSet.Contains(tile.name);
    }

    private void RebuildBreakableTileSet()
    {
        breakableTileSet.Clear();
        breakableTileNameSet.Clear();

        if (breakableTileNames != null)
        {
            for (int i = 0; i < breakableTileNames.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(breakableTileNames[i]))
                    breakableTileNameSet.Add(breakableTileNames[i]);
            }
        }

        if (breakableTiles == null)
            return;

        for (int i = 0; i < breakableTiles.Length; i++)
        {
            if (breakableTiles[i] != null)
                breakableTileSet.Add(breakableTiles[i]);
        }
    }

    private void CacheComponents()
    {
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();

        if (tilemapCollider == null)
            tilemapCollider = GetComponent<TilemapCollider2D>();
    }
}
