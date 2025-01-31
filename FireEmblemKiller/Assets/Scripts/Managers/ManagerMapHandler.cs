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

    public bool useNewSpawning = false;

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

    public ActorBrain currentPlayersTurn { get; private set; } // the current player making moves


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

        if (ManagerGameStateHandler.Instance)
            ManagerGameStateHandler.Instance.ChangeGameState(ManagerGameStateHandler.GAMESTATE.PreBattle, null);

    }

    #region BattleMap Initializations

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


        #region Attemping More Interesting Spawn
        if (useNewSpawning)
        {
            print("ATTEMPTING ADVANCED SPAWN - 0");
            int tilesDividedEvenlyByPlayer = gridTilesGenerated.Count / sampleBrainPlayers.Count; // if 100 tiles, 2 brains... 50-tiles per brain        
            Vector2Int tilesToCheck = new Vector2Int(0, tilesDividedEvenlyByPlayer - 1);
            for (int i = 0; i < sampleBrainPlayers.Count; i++)
            {
                print($"ATTEMPTING ADVANCED SPAWN - 1 || Brain: {sampleBrainPlayers[i].playerName}");
                // get our possible tiles for this Brain
                MapTileData[] freeTileInRange = new MapTileData[tilesDividedEvenlyByPlayer];
                for (int t = tilesToCheck.x; t < tilesToCheck.y; t++)
                { if (t >= gridTilesGenerated.Count - 1 || t >= freeTileInRange.Length - 1) break; freeTileInRange[t] = gridTilesGenerated[t]; }// adds our possible tiles to the array

                int availableTiles = tilesDividedEvenlyByPlayer;
                int placedUnits = 0;

                foreach (MapTileData tile in freeTileInRange)
                {
                    print("ATTEMPTING ADVANCED SPAWN - 2");
                    if (!tile || !tile.TileIsFree())
                    { availableTiles--; continue; }
                    print($"ATTEMPTING ADVANCED SPAWN - 3 || Tile: {tile.transform.name}");
                    foreach (UnitCapsule unit in sampleBrainPlayers[i].unitsImCommanding) // placing units || TODO - figure out a pattern or sample way to design basic spawns that are more interesting
                    {
                        print("ATTEMPTING ADVANCED SPAWN - 4");
                        if (!unit || unit.tileImOn) // skip if null or unit has a tile already
                        { continue; }

                        unit.ChangeUnitSelection(false); // deselect all units                      

                        print("ATTEMPTING ADVANCED SPAWN - 5");
                        if (tile.TileIsFree())
                        {
                            print($"ATTEMPTING ADVANCED SPAWN - 6 || {tile.transform.name} - is Free For: {unit.thisUnitData.unitName} ");
                            //print($"Tile: {tile.transform.name} - is Free For: {unit.thisUnitData.unitName}");
                            unit.transform.position = tile.transform.position;
                            tile.unitOnThisTile = unit;
                            unit.tileImOn = tile;
                            placedUnits++;
                            availableTiles--;
                            break;
                        }
                    }
                    if (placedUnits >= availableTiles || placedUnits >= sampleBrainPlayers[i].unitsImCommanding.Count)
                        break;
                }

                print($"ATTEMPTING ADVANCED SPAWN - 7 || Tiles To Check Was: {tilesToCheck.x},{tilesToCheck.y} - IS NOW: {tilesToCheck.x + tilesDividedEvenlyByPlayer},{tilesToCheck.y + tilesDividedEvenlyByPlayer}");
                tilesToCheck += new Vector2Int(tilesDividedEvenlyByPlayer, tilesDividedEvenlyByPlayer); // bumps up the tiles to the next set
            }
            print("ATTEMPTING ADVANCED SPAWN - 8");
        }
        #endregion Attemping More Interesting Spawn
        else
        {
            // WE WILL REPLACE THIS SOON
            foreach (UnitCapsule unit in spawnedUnits) // placing units || TODO - figure out a pattern or sample way to design basic spawns that are more interesting
            {
                unit.ChangeUnitSelection(false); // deselect all units

                // change the order depending on how many brains we have
                // we have 3 brians
                // divide up how many tiles we give each brain ( evenly into sections)
                // for each section of tiles spawn units

                foreach (MapTileData tile in gridTilesGenerated)
                {
                    if (tile.TileIsFree())
                    {
                        //print($"Tile: {tile.transform.name} - is Free For: {unit.thisUnitData.unitName}");
                        unit.transform.position = tile.transform.position;
                        tile.unitOnThisTile = unit;
                        unit.tileImOn = tile;
                        break;
                    }
                }
            }
            // END OF WE WILL REPLACE THIS SOON
        }
        NextPlayersTurn(); // assign whose turn it is
    }

    #endregion end BattleMap Initializations


    #region BattleMap Visuals

    public void ShowTraversableTiles(Transform _startPoint, float _acceptableDistance)
    {
        if (gridTilesGenerated.Count == 0)
            return;

        foreach (MapTileData tile in gridTilesGenerated)
        {
            float dist = Vector3.Distance(tile.transform.position, _startPoint.position);
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

    #endregion end BattleMap Visuals


    #region BattleMap BrainActions

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
            //print("Next Player Turn - Break");
            if (ManagerGameStateHandler.Instance)
                ManagerGameStateHandler.Instance.ChangeGameState(ManagerGameStateHandler.GAMESTATE.MapBattle, currentPlayersTurn);
        }              

    }

    // SendHPChangeToTarget() changes a given unit's HP value
    // @Param _change - the change in HP
    // @Param _unitAffected - the UnitCapsule class of the unit affected by the HP change
    // returns bool - true if the unit is still alive, false if the unit has died
    public bool SendHPChangeToTarget(int _change, UnitCapsule _unitAffected)
    {
        _unitAffected.thisUnitData.healthPoints.x -= _change;
        Debug.Log($"{_unitAffected.thisUnitData.unitName} has taken {_change} damage. " +
            $"{_unitAffected.thisUnitData.unitName}'s New Health Total is: {_unitAffected.thisUnitData.healthPoints.x}");

        if (_unitAffected.thisUnitData.healthPoints.x <= 0) // Unit has perished
        {
            return false;
        }
        return true;
    }


    public void OfficiallyMoveUnit(MapTileData _targetTile, UnitCapsule _unitMoving)
    {
        if (_unitMoving == null)
            return;

        if (_targetTile == null || _targetTile == _unitMoving.tileImOn) // we are returning the unit back to it's original spot
            _unitMoving.transform.position = _unitMoving.tileImOn.transform.position;

        // TODO - calculate and subtract remaining speed (if we want to allow units to move their full speed) || could also (or instead) do a single bool that tracks 'isDoneMoving' which locks a character in place

        // remove unit and their current tile from each other
        _unitMoving.tileImOn.unitOnThisTile = null;
        _unitMoving.transform.position = _targetTile.transform.position;
        _unitMoving.tileImOn = _targetTile;
        _targetTile.unitOnThisTile = _unitMoving;

        if (_unitMoving.unitIsSelected)
            ShowTraversableTiles(_unitMoving.transform, _unitMoving.thisUnitData.speed);
    }

    #endregion end BattleMap BrainActions
}
