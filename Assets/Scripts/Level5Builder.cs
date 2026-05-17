using UnityEngine;
using UnityEngine.Tilemaps;

public class Level5Builder : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap terrainTilemap;
    public Tilemap hazardTilemap;
    public Tilemap decorationTilemap;

    [Header("Main Tiles")]
    public TileBase wall;
    public TileBase grassTop;
    public TileBase dirt;
    public TileBase platform;
    public TileBase techBlock;
    public TileBase purpleGround;
    public TileBase orangeGround;
    public TileBase brick;
    public TileBase metal;

    [Header("Hazards / Objects")]
    public TileBase spike;
    public TileBase saw;
    public TileBase crusher;
    public TileBase fire;
    public TileBase portal;

    [Header("Optional Markers")]
    public TileBase playerStart;
    public TileBase dashMarker;
    public TileBase checkpoint;

    [ContextMenu("Build Gravity Flip Level 5")]
    public void BuildLevel()
    {
        if (terrainTilemap == null)
        {
            Debug.LogError("Assign Terrain Tilemap first.");
            return;
        }

        terrainTilemap.ClearAllTiles();
        if (hazardTilemap != null) hazardTilemap.ClearAllTiles();
        if (decorationTilemap != null) decorationTilemap.ClearAllTiles();

        // Outer room frame, approximately 95 x 38 tiles.
        Rect(0, 0, 96, 1, wall);
        Rect(0, 37, 96, 1, wall);
        Rect(0, 0, 1, 38, wall);
        Rect(95, 0, 1, 38, wall);

        // 1. Safe start.
        Ground(3, 5, 10, 3);
        Place(4, 8, playerStart, decorationTilemap);

        // 2. Dash tutorial gap.
        Ground(17, 6, 5, 2);
        Ground(28, 6, 6, 2);
        Spikes(23, 5, 4);
        Place(24, 8, dashMarker, decorationTilemap);

        // 3. Gravity flip plus dash climb.
        Platform(37, 10, 4);
        Platform(43, 14, 4);
        Platform(50, 18, 4);
        Platform(57, 22, 4);
        CeilingPlatform(61, 31, 6);
        CeilingPlatform(70, 31, 6);
        Rect(58, 26, 1, 6, wall);
        Rect(77, 27, 1, 5, wall);
        Spikes(64, 28, 3);

        // 4. Fire bridge, no longer a plain hallway.
        Ground(7, 24, 8, 2);
        Ground(17, 25, 5, 2);
        Ground(25, 24, 4, 2);
        Ground(34, 24, 7, 2);
        Ground(45, 25, 4, 2);
        Ground(53, 24, 8, 2);
        Spikes(30, 23, 3);
        Spikes(42, 23, 3);
        Fires(46, 26, 3);
        Rect(33, 22, 1, 3, wall);
        Rect(52, 22, 1, 3, wall);

        // 5. Required lower dash route.
        // Drop opening is between x 61-62 on the upper route.
        Ground(63, 24, 8, 2);
        Ground(75, 24, 7, 2);
        Rect(4, 15, 5, 5, techBlock);
        Rect(5, 16, 3, 3, null);
        GroundPurple(3, 12, 8, 2);
        Spikes(12, 9, 33);
        Platform(16, 13, 5);
        Platform(28, 14, 5);
        Platform(40, 13, 5);
        Platform(52, 14, 5);
        Place(22, 10, saw, hazardTilemap);
        Place(46, 10, saw, hazardTilemap);
        Rect(60, 10, 1, 15, wall);
        Platform(58, 18, 3);
        Platform(62, 21, 3);
        Ground(65, 24, 5, 2);

        // 6. Crusher room.
        Rect(68, 13, 18, 1, metal);
        Rect(68, 22, 18, 1, metal);
        Rect(68, 14, 1, 8, metal);
        Rect(85, 14, 1, 8, metal);
        Platform(70, 16, 4);
        Platform(80, 19, 4);
        Place(76, 17, crusher, hazardTilemap);
        Place(76, 18, crusher, hazardTilemap);
        Spikes(72, 14, 3);
        Spikes(80, 21, 3);

        // 7. Recovery checkpoint area.
        Ground(88, 20, 6, 3);
        Place(90, 23, checkpoint, decorationTilemap);

        // 8. Ceiling dash section.
        Spikes(65, 8, 17);
        CeilingPlatform(66, 33, 6);
        CeilingPlatform(76, 33, 6);
        CeilingPlatform(86, 33, 5);
        Rect(65, 28, 1, 6, wall);
        Rect(91, 28, 1, 6, wall);

        // 9. Final mixed challenge.
        Platform(69, 10, 4);
        Platform(75, 14, 4);
        Place(80, 11, saw, hazardTilemap);
        Platform(83, 16, 4);
        Fires(85, 17, 2);
        Platform(88, 20, 3);
        Spikes(82, 8, 5);

        // 10. Exit.
        Ground(89, 5, 5, 3);
        Place(92, 9, portal, decorationTilemap);

        Debug.Log("Gravity Flip Level 5 built.");
    }

    void Ground(int x, int y, int width, int height)
    {
        Rect(x, y + height - 1, width, 1, grassTop);
        Rect(x, y, width, height - 1, dirt);
    }

    void GroundPurple(int x, int y, int width, int height)
    {
        Rect(x, y + height - 1, width, 1, purpleGround != null ? purpleGround : grassTop);
        Rect(x, y, width, height - 1, dirt);
    }

    void Platform(int x, int y, int width)
    {
        Rect(x, y, width, 1, platform);
    }

    void CeilingPlatform(int x, int y, int width)
    {
        Rect(x, y, width, 1, platform);
        Rect(x, y + 1, width, 1, dirt);
    }

    void Spikes(int x, int y, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Place(x + i, y, spike, hazardTilemap);
        }
    }

    void Fires(int x, int y, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Place(x + i, y, fire, hazardTilemap);
        }
    }

    void Rect(int x, int y, int width, int height, TileBase tile)
    {
        if (tile == null) return;

        for (int ix = x; ix < x + width; ix++)
        {
            for (int iy = y; iy < y + height; iy++)
            {
                terrainTilemap.SetTile(new Vector3Int(ix, iy, 0), tile);
            }
        }
    }

    void Place(int x, int y, TileBase tile, Tilemap preferredTilemap)
    {
        if (tile == null) return;

        Tilemap target = preferredTilemap != null ? preferredTilemap : terrainTilemap;
        target.SetTile(new Vector3Int(x, y, 0), tile);
    }
}
