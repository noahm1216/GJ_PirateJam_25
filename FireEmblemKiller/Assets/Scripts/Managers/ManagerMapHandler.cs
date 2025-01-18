using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>
/// This Map Handler's Primary focus is knowing all the units and options (like grid spaces) for each unit to check data from
/// </para>
/// </summary>
public class ManagerMapHandler : MonoBehaviour
{

    public Transform gridTilePrefab; // these are 1x1 meter units
    private Transform gridTileParent; // this is generated at runtime
    public List<MapTileData> gridTilesGenerated = new List<MapTileData>();

    [Header("TESTING\n___________")]
    public bool spawnMapOnStart = false;
    public Vector2 mapSize = new Vector2(10, 10);
    public Vector3 mapPos;
    public bool posIsCenter = true;

    public List<ActorBrain> sampleBrainPlayers = new List<ActorBrain>(); // these will be the brain / inputs and commands of players
    public List<UnitCapsule> spawnedUnits = new List<UnitCapsule>(); // the units we'll spawns


    // Start is called before the first frame update
    void Start()
    {
        if (spawnMapOnStart)
            InitializeGrid(mapSize, mapPos, posIsCenter);
    }

    public void InitializeGrid(Vector2 _gridDimension, Vector3 _gridStartPos, bool _startPosIsCenter)
    {
        if (_gridDimension == Vector2.zero || !gridTilePrefab)
            return;

        if (!gridTileParent)
        {
            gridTileParent = new GameObject("GridTileParent").transform;
            gridTileParent.SetParent(transform);
        }

        for (int z = 0; z < _gridDimension.y; z++)
        {
            for (int x = 0; x < _gridDimension.x; x++)
            {
                Transform tileClone = Instantiate(gridTilePrefab);
                tileClone.name = gridTilePrefab.name + $"_{gridTilesGenerated.Count}";
                tileClone.transform.position = _gridStartPos + new Vector3(1 * x, 0, 1 * z);
                tileClone.SetParent(gridTileParent);
                MapTileData dataRef = null;
                tileClone.TryGetComponent(out dataRef);
                if (dataRef)
                    gridTilesGenerated.Add(dataRef);
            }// end x
        }// end Z

        if (_startPosIsCenter)
        {
            gridTileParent.position += (new Vector3(0.5f, 0, 0.5f)); // otherwise it will be 0.5f off center
            gridTileParent.position -= new Vector3(_gridDimension.x / 2, 0, _gridDimension.y / 2);
        }

        SpawnUnits(); // handle Units
    }


    // get the brains we'll be using for this map and spawn their troops
    public void SpawnUnits()
    {
        if (sampleBrainPlayers.Count == 0)
            return;

        int totalUnitsTryingToSpawn = 0;

        foreach (ActorBrain brain in sampleBrainPlayers) // get the number of units and check if we have enough spots
        {
            totalUnitsTryingToSpawn += brain.myUnitPrefabs.Count;
        }

        if (totalUnitsTryingToSpawn > gridTilesGenerated.Count)
        { Debug.Log($"CANCELLING: Tried to spawn {totalUnitsTryingToSpawn} units onto {gridTilesGenerated.Count} tiles"); return; }

                
        foreach (ActorBrain brain in sampleBrainPlayers) // spawn the units
        {
            for(int i =0; i < brain.myUnitPrefabs.Count; i++)
            {
                Transform unitClone = Instantiate(brain.myUnitPrefabs[i]);
                spawnedUnits.Add(unitClone.GetComponent<UnitCapsule>());
            }
        }


        foreach(UnitCapsule unit in spawnedUnits) // placing units
        {
            print($"Unit: {unit.thisUnitData.unitName}");

            foreach(MapTileData tile in gridTilesGenerated)
            {
                print($"Tile: {tile.name}");

                if (tile.TileIsFree())
                {
                    print($"Tile: {tile.transform.name} - is Free For: {unit.thisUnitData.unitName}");
                    unit.transform.position = tile.transform.position;
                    tile.unitOnThisTile = unit;
                    break;
                }
            }
        }
    }
}