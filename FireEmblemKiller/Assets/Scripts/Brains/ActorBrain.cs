using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorBrain : MonoBehaviour
{
    public string playerName = "";
    public bool ownedByAHumanPlayer;
    public bool myTurn;
    public List<Transform> myUnitPrefabs = new List<Transform>();

    // During Map
    private List<UnitCapsule> unitsImCommanding = new List<UnitCapsule>();

    private Camera mainCamera;
    private int unitSelected, tileSelected;



    // Start is called before the first frame update
    void Start()
    {
        if (transform.tag == "Player")
            ownedByAHumanPlayer = true;

        if (!mainCamera)
            mainCamera = Camera.main;
    }

    public void AddOneUnitToList(UnitCapsule _unitToAdd)
    {
        if (!unitsImCommanding.Contains(_unitToAdd))
            unitsImCommanding.Add(_unitToAdd);
    }

    public void EndMyTurn()
    {
        Debug.Log($"{playerName}'s Turn Ended");
        if (ManagerMapHandler.Instance)
            ManagerMapHandler.Instance.NextPlayersTurn();
    }

    public void CycleMyUnits()
    {
        if (unitsImCommanding.Count == 0)
            return;
        unitsImCommanding[unitSelected].unitIsSelected = false;

        unitSelected += 1;
        if (unitSelected >= unitsImCommanding.Count)
            unitSelected = 0;

        unitsImCommanding[unitSelected].unitIsSelected = true;
        MoveCameraTo(unitsImCommanding[unitSelected].transform); // Also need to set selected tile to the one our selected unit is standing on
    }

    public void ChangeTileSelected(int _tileJump)
    {
        if (!ManagerMapHandler.Instance || ManagerMapHandler.Instance.gridTilesGenerated.Count == 0)
            return;

        ManagerMapHandler.Instance.gridTilesGenerated[tileSelected].isSelected = false;
        tileSelected += _tileJump;
        if (tileSelected >= ManagerMapHandler.Instance.gridTilesGenerated.Count)
            tileSelected -= ManagerMapHandler.Instance.gridTilesGenerated.Count;
        if (tileSelected < 0)
            tileSelected += ManagerMapHandler.Instance.gridTilesGenerated.Count;

        ManagerMapHandler.Instance.gridTilesGenerated[tileSelected].isSelected = true;
        MoveCameraTo(ManagerMapHandler.Instance.gridTilesGenerated[tileSelected].transform);
    }

    public void MoveCameraTo(Transform _newLookAt)
    {
        if (!mainCamera)
            mainCamera = Camera.main;

        if (mainCamera)
            mainCamera.transform.LookAt(_newLookAt);
    }

    // Update is called once per frame
    void Update()
    {
        if (myTurn)
        {
            if (ownedByAHumanPlayer)
                Debug.Log($"Currently A Human Player's Turn: {playerName}");
            else
                Debug.Log($"Currently A Human Player's Turn: {playerName}");

            if (unitsImCommanding.Count > 0 && mainCamera)
            {
                CheckInputs();
            }
        }
    }
    
    public void CheckInputs()
    {
        if (Input.GetKeyUp(KeyCode.Return))
            EndMyTurn();

        if (Input.GetKeyUp(KeyCode.Tab))
            CycleMyUnits();

        if (ManagerMapHandler.Instance)
        {
            if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W))
                ChangeTileSelected(-ManagerMapHandler.Instance.mapSize.x);
            if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D))
                ChangeTileSelected(1);
            if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
                ChangeTileSelected(-1);
            if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
                ChangeTileSelected(ManagerMapHandler.Instance.mapSize.x);
        }
    }
}
