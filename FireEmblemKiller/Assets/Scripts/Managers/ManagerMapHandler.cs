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
    public static ManagerMapHandler Instance { get; private set; }

    public Transform gridTilePrefab; // these are 1x1 meter units
    private Transform gridTileParent; // this is generated at runtime to hold tiles
    public List<MapTileData> gridTilesGenerated = new List<MapTileData>();

    [Header("TESTING\n___________")]
    public bool spawnMapOnStart = false;
    public Vector2Int mapSize = new Vector2Int(10, 10);
    public Vector3 mapPos;
    public bool posIsCenter = true;

    public List<ActorBrain> sampleBrainPlayers = new List<ActorBrain>(); // these will be the brain / inputs and commands of players
    public List<UnitCapsule> spawnedUnits = new List<UnitCapsule>(); // the units we'll spawns
    private Transform spawnedUnitsParent; // this is generated at runtime to hold units

    private ActorBrain currentPlayersTurn; // the current player making moves


    private void Awake()
    {        
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (spawnMapOnStart)
            InitializeGrid(mapSize, mapPos, posIsCenter);

    }

    public void InitializeGrid(Vector2Int _gridDimension, Vector3 _gridStartPos, bool _startPosIsCenter)
    {
        Debug.Log("Creating Grid / Tiles");

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
                tileClone.transform.position = _gridStartPos + new Vector3(1 * x, 0, -1 * z);
                tileClone.SetParent(gridTileParent);
                MapTileData dataRef = null;
                tileClone.TryGetComponent(out dataRef);
                if (dataRef)
                    gridTilesGenerated.Add(dataRef);
            }// end x
        }// end Z

        if (_startPosIsCenter)
        {
            gridTileParent.position += (new Vector3(0.5f, 0, -0.5f)); // otherwise it will be 0.5f off center
            gridTileParent.position -= new Vector3(_gridDimension.x / 2, 0, _gridDimension.y / -2);
        }

        SpawnUnits(); // handle Units
    }


    // get the brains we'll be using for this map and spawn their troops
    public void SpawnUnits()
    {
        Debug.Log("Spawning Units");
        if (sampleBrainPlayers.Count == 0)
            return;

        int totalUnitsTryingToSpawn = 0;
        foreach (ActorBrain brain in sampleBrainPlayers) // get the number of units and check if we have enough spots
        {
            totalUnitsTryingToSpawn += brain.myUnitPrefabs.Count;
        }

        if (totalUnitsTryingToSpawn > gridTilesGenerated.Count)
        { Debug.Log($"CANCELLING: Too Many Units - Tried to spawn {totalUnitsTryingToSpawn} units onto {gridTilesGenerated.Count} tiles"); return; }

        if (!spawnedUnitsParent)
            spawnedUnitsParent = new GameObject("SpawnedUnitsParent").transform;
        spawnedUnitsParent.SetParent(transform);


        foreach (ActorBrain brain in sampleBrainPlayers) // spawn the units
        {
            for(int i =0; i < brain.myUnitPrefabs.Count; i++)
            {
                Transform unitClone = Instantiate(brain.myUnitPrefabs[i]);
                spawnedUnits.Add(unitClone.GetComponent<UnitCapsule>());
                brain.AddOneUnitToList(unitClone.GetComponent<UnitCapsule>());
                unitClone.SetParent(spawnedUnitsParent);
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
                    unit.tileImOn = tile;
                    break;
                }
            }
        }
        NextPlayersTurn(); // assign whose turn it is
    }


    public void NextPlayersTurn()
    {
        Debug.Log("Assigning Next Player's Turn");

        if (sampleBrainPlayers.Count == 0)
            return;

        if (currentPlayersTurn == null)
        { currentPlayersTurn = sampleBrainPlayers[0]; currentPlayersTurn.SetMyTurn(true); currentPlayersTurn.CycleMyUnits(); }
        else
        {
            for (int i = 0; i < sampleBrainPlayers.Count; i++)
            {
                if (currentPlayersTurn == sampleBrainPlayers[i])
                {                    
                    currentPlayersTurn.SetMyTurn(false);

                    i += 1;

                    if (i == sampleBrainPlayers.Count)
                        i = 0;

                    currentPlayersTurn = sampleBrainPlayers[i]; // right now is skipping 2nd players turn (but should not be)
                    currentPlayersTurn.SetMyTurn(true);                    
                    currentPlayersTurn.CycleMyUnits();
                    break;
                }
            }
            print("Next Player Turn - Break");
        }

    }

    public void ShowTraversableTiles(Transform _startPoint, float _acceptableDistance)
    {
        if (gridTilesGenerated.Count == 0)
            return;

        foreach (MapTileData tile in gridTilesGenerated)
        {
            float dist = Vector2.Distance(tile.transform.position, _startPoint.position);
            tile.ChangeInRange(tile.TileIsFree() && dist <= _acceptableDistance);
        }

    }
    public void ResetTraversableTileVisuals()
    {
        if (gridTilesGenerated.Count == 0)
            return;

        foreach (MapTileData tile in gridTilesGenerated)
        {
            tile.ChangeInRange(false);
        }
    }

    public void SendHPChangeToTarget(int _change, UnitCapsule _unitAffected)
    {
        _unitAffected.thisUnitData.healthPoints.x -= _change;
        Debug.Log($"{_unitAffected.thisUnitData.unitName} has taken {_change} damage. {_unitAffected.thisUnitData.unitName}'s New Health Total is: {_unitAffected.thisUnitData.healthPoints.x}");
    }
}
