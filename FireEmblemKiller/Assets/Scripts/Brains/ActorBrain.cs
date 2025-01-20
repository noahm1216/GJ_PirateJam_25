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
    private float turnChangerTimeStamp, turnChangerTimeWait = 0.2f;

    public GameObject BattleForecastCanvas_go;

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

    public void SetMyTurn(bool _isMyTurn)
    {
        myTurn = _isMyTurn;
        turnChangerTimeStamp = Time.time;
    }

    public void EndMyTurn()
    {
        Debug.Log($"{playerName}'s Turn Ended");

        // disable on my units
        if(unitsImCommanding.Count > 0)
            foreach (UnitCapsule unit in unitsImCommanding)
                unit.unitIsSelected = false;

        if (ManagerMapHandler.Instance)
            ManagerMapHandler.Instance.NextPlayersTurn();
    }

    public void CycleMyUnits()
    {
        if (ManagerMapHandler.Instance)
            ManagerMapHandler.Instance.ResetTraversableTileVisuals();

        if (unitsImCommanding.Count == 0)
            return;
        unitsImCommanding[unitSelected].unitIsSelected = false;

        unitSelected += 1;
        if (unitSelected >= unitsImCommanding.Count)
            unitSelected = 0;

        unitsImCommanding[unitSelected].unitIsSelected = true;
        bool foundTile = false;
        int unitTileId = 0;
        if (unitsImCommanding[unitSelected].tileImOn != null && ManagerMapHandler.Instance)// Set selected tile to the one our selected unit is standing on
        {            
            for (int i = 0; i < ManagerMapHandler.Instance.gridTilesGenerated.Count; i++)
                if (ManagerMapHandler.Instance.gridTilesGenerated[i] == unitsImCommanding[unitSelected].tileImOn)
                {
                    unitTileId = i;
                    unitTileId = (unitTileId - tileSelected);
                    foundTile = true;                   
                    break;
                }            
        }

        if (foundTile)
            ChangeTileSelected(unitTileId);
        else
            MoveCameraTo(unitsImCommanding[unitSelected].transform);

        if (ManagerMapHandler.Instance)
            ManagerMapHandler.Instance.ShowTraversableTiles(unitsImCommanding[unitSelected].tileImOn.transform, unitsImCommanding[unitSelected].thisUnitData.speed);

    }

    public void ChangeTileSelected(int _tileJump)
    {
        if (!ManagerMapHandler.Instance || ManagerMapHandler.Instance.gridTilesGenerated.Count == 0)
            return;

        ManagerMapHandler.Instance.gridTilesGenerated[tileSelected].ChangeSelection(false);
        tileSelected += _tileJump;
        if (tileSelected >= ManagerMapHandler.Instance.gridTilesGenerated.Count)
            tileSelected -= ManagerMapHandler.Instance.gridTilesGenerated.Count;
        if (tileSelected < 0)
            tileSelected += ManagerMapHandler.Instance.gridTilesGenerated.Count;

        ManagerMapHandler.Instance.gridTilesGenerated[tileSelected].ChangeSelection(true);
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
        if (myTurn && Time.time > turnChangerTimeStamp + turnChangerTimeWait)
        {
            if (ownedByAHumanPlayer)
                Debug.Log($"Currently A Human Player's Turn: {playerName}");
            else
                Debug.Log($"Currently A Human Player's Turn: {playerName}");

            if (unitsImCommanding.Count > 0)
                CheckInputs();            
        }
    }
    
    public void CheckInputs()
    {
        Debug.Log("Press 'End' key to end your turn");
        if (Input.GetKeyUp(KeyCode.End))
            EndMyTurn();

        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter)) // open context menu for us to select
            print("bring up unit context menu");

        if (Input.GetKeyUp(KeyCode.T) && ManagerConversationHandler.Instance) // make the current unit talk || TODO - currently using temporary code, this should be called from a function and based on the unit's library of words
        {
            ManagerConversationHandler.Instance.AddSpeakerToConversation(unitsImCommanding[unitSelected].thisUnitData.unitColor, unitsImCommanding[unitSelected].thisUnitData.unitIcon, false, unitsImCommanding[unitSelected].thisUnitData.unitName);
            ManagerConversationHandler.Instance.AddDialogueToList(unitsImCommanding[unitSelected].thisUnitData.unitColor, "It's My Turn To Speak!", 0);
        }

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

        if (Input.GetKeyUp(KeyCode.C)) // calculate unit's attack
        {
            //ManagerMapHandler.Instance.gridTilesGenerated[tileSelected]
            int attackDamage = unitsImCommanding[unitSelected].CalculateAttack(unitsImCommanding[unitSelected].thisUnitData);
            Debug.Log($"Calculating Attack for {unitsImCommanding[unitSelected].thisUnitData.unitName}. Attack = {attackDamage}");

            // TODO
            // Select an enemy unit and calculate their defense.
            // Subtract defense from unit's attack
            // Send this result to ManagerMapHandler.SendHPChangToTarget(int _change, List<UnitCapsule> _unitsAffected)
        }

        if (Input.GetKeyUp(KeyCode.A)) // attempt to attack an enemy unit with selected unit
        {
            // Get reference to other player's brain
            ActorBrain otherBrain = null;
            foreach (ActorBrain ab in ManagerMapHandler.Instance.sampleBrainPlayers)
            {
                if (ab == this) continue;
                else if (ab != this) otherBrain = ab;
                else Debug.Log("Brain was not found. ERROR");
            }
            otherBrain?.CycleMyUnits();

            // Activate and Update Battle Forecast
            BattleForecastCanvas_go.SetActive(true);
            // Calculate Damage Inflicted
            int damageInflicted;
            int attackDamage = unitsImCommanding[unitSelected].CalculateAttack(unitsImCommanding[unitSelected].thisUnitData);
            int defenseValue = otherBrain.unitsImCommanding[otherBrain.unitSelected].CalculateDefense(otherBrain.unitsImCommanding[otherBrain.unitSelected].thisUnitData);
            damageInflicted = attackDamage - defenseValue;
            if (damageInflicted < 0) damageInflicted = 0;
            List<UnitCapsule> unitsAffected = new List<UnitCapsule>();
            unitsAffected.Add(otherBrain.unitsImCommanding[otherBrain.unitSelected]);
            ManagerMapHandler.Instance.SendHPChangeToTarget(damageInflicted, otherBrain.unitsImCommanding[otherBrain.unitSelected]);
            //Set Forecast Data
            ManagerBattleForecast.Instance.SetForecastData(unitsImCommanding[unitSelected].thisUnitData.unitName,
                otherBrain.unitsImCommanding[otherBrain.unitSelected].thisUnitData.unitName,
                attackDamage, unitsImCommanding[unitSelected].thisUnitData.speed,
                defenseValue, otherBrain.unitsImCommanding[otherBrain.unitSelected].thisUnitData.thisUnitsStats.constitution);
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            // Get reference to other player's brain
            ActorBrain otherBrain = null;
            foreach (ActorBrain ab in ManagerMapHandler.Instance.sampleBrainPlayers)
            {
                if (ab == this) continue;
                else if (ab != this) otherBrain = ab;
                else Debug.Log("Brain was not found. ERROR");
            }

            // Calculate Damage Inflicted
            int damageInflicted;
            int attackDamage = unitsImCommanding[unitSelected].CalculateAttack(unitsImCommanding[unitSelected].thisUnitData);
            int defenseValue = otherBrain.unitsImCommanding[otherBrain.unitSelected].CalculateDefense(otherBrain.unitsImCommanding[otherBrain.unitSelected].thisUnitData);
            damageInflicted = attackDamage - defenseValue;
            if (damageInflicted < 0) damageInflicted = 0;
            List<UnitCapsule> unitsAffected = new List<UnitCapsule>();
            unitsAffected.Add(otherBrain.unitsImCommanding[otherBrain.unitSelected]);
            ManagerMapHandler.Instance.SendHPChangeToTarget(damageInflicted, otherBrain.unitsImCommanding[otherBrain.unitSelected]);
        }
    }
}
