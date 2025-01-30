using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorBrain : MonoBehaviour
{
    public string playerName = "";
    public bool ownedByAHumanPlayer;
    public bool myTurn;
    public ActorButtonMap myKeyMapPrefs;
    public List<Transform> myUnitPrefabs = new List<Transform>();

    // During Map
    public List<UnitCapsule> unitsImCommanding { get; private set; } = new List<UnitCapsule>();

    private Camera mainCamera;
    private int unitSelected, tileSelected;
    private float turnChangerTimeStamp, turnChangerTimeWait = 0.2f;

    // Battle Forecast Logic
    private GameObject BattleForecastCanvas_go;
    private bool hasInitiatedBattleForecast;
    private GameObject UnitDataUICanvas_go;

    // Start is called before the first frame update
    void Start()
    {
        if (transform.tag == "Player")
            ownedByAHumanPlayer = true;

        if (!mainCamera)
            mainCamera = Camera.main;

        if (ManagerBattleForecast.Instance) BattleForecastCanvas_go = ManagerBattleForecast.Instance.gameObject;
        if (ManagerUnitData.Instance) UnitDataUICanvas_go = ManagerUnitData.Instance.gameObject;

        hasInitiatedBattleForecast = false;
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
                unit.ChangeUnitSelection(false);

        // reset their positions if needed
        ResetUnitsPositions();

        if (ManagerMapHandler.Instance)
            ManagerMapHandler.Instance.NextPlayersTurn();
    }

    public void CycleMyUnits()
    {
        if (ManagerMapHandler.Instance)
            ManagerMapHandler.Instance.ResetTraversableTileVisuals();

        if (unitsImCommanding.Count == 0)
        { Debug.Log("Cant Cycle Units With '0' Units In 'Units Im Commanding'"); return; }
        unitsImCommanding[unitSelected].ChangeUnitSelection(false);

        unitSelected += 1;
        if (unitSelected >= unitsImCommanding.Count)
            unitSelected = 0;

        unitsImCommanding[unitSelected].ChangeUnitSelection(true);
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

        if (hasInitiatedBattleForecast && ManagerBattleForecast.Instance)
        {
            ManagerBattleForecast.Instance.UpdateBattleForecast(unitsImCommanding[unitSelected],
                unitsImCommanding[unitSelected].CalculateAttack(unitsImCommanding[unitSelected].thisUnitData), 0, true);
        }
        else if (!hasInitiatedBattleForecast && ManagerUnitData.Instance)
        {
            Debug.Log("Trying to get: " + unitsImCommanding[unitSelected]);
            ManagerUnitData.Instance.UpdateUnitDataUI(unitsImCommanding[unitSelected]);
        }
    }

    public void ChangeTileSelected(int _tileJump)
    {
        if (!ManagerMapHandler.Instance || ManagerMapHandler.Instance.gridTilesGenerated.Count == 0)
        { Debug.Log("No Tiles Generated OR No 'Manager Map Handler Instance'"); return; }

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

    private void ResetUnitsPositions()
    {
        if (unitsImCommanding.Count == 0 || !ManagerMapHandler.Instance)
        { Debug.Log("Not Commanding Any Units OR No 'Manager Map Handler Instance'"); return; }

        foreach (UnitCapsule unit in unitsImCommanding)
            ManagerMapHandler.Instance.OfficiallyMoveUnit(unit.tileImOn, unit);
    }

    private void PreviewUnitMove()
    {
        // if we're trying to preview a unit on a tile it's already on, then we are officially moving it there
        if (unitsImCommanding[unitSelected].transform.position == ManagerMapHandler.Instance.gridTilesGenerated[tileSelected].transform.position)
            ManagerMapHandler.Instance.OfficiallyMoveUnit(ManagerMapHandler.Instance.gridTilesGenerated[tileSelected], unitsImCommanding[unitSelected]);
        else // then we just preview the spot
            unitsImCommanding[unitSelected].transform.position = ManagerMapHandler.Instance.gridTilesGenerated[tileSelected].transform.position;
    }


    private ActorBrain GetOtherBrain()
    {
        // Get reference to other player's brain
        ActorBrain otherBrain = null;
        foreach (ActorBrain ab in ManagerMapHandler.Instance.sampleBrainPlayers)
        {
            if (ab == this) continue;
            else if (ab != this) otherBrain = ab;
            else Debug.Log("Brain was not found. ERROR");
        }

        return otherBrain;
    }

    private bool CheckIsInRange(UnitCapsule attacker, UnitCapsule defender)
    {
        Vector2 attacker_coord = new Vector2(attacker.tileImOn.transform.position.x, attacker.tileImOn.transform.position.z);
        Vector2 defender_coord = new Vector2(defender.tileImOn.transform.position.x, defender.tileImOn.transform.position.z);
        int dist = Mathf.RoundToInt(Mathf.Abs(defender_coord.x - attacker_coord.x) + Mathf.Abs(defender_coord.y - attacker_coord.y));
        Debug.Log($"Attacker's tile position is: {attacker.tileImOn.transform.position.x}, {attacker.tileImOn.transform.position.z}");
        Debug.Log($"Defender's tile position is: {defender.tileImOn.transform.position.x}, {defender.tileImOn.transform.position.z}");
        Debug.Log($"Distance between attacker & defender is: {dist} and attacker's range is: {attacker.thisUnitData.range}");
        if (dist <= attacker.thisUnitData.range) return true;
        else return false;
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
        // if we are trying to change our key mapping then dont register any of these
        if (ManagerKeyInputHandler.Instance && ManagerKeyInputHandler.Instance.changingAKeyNow)
            return;

            Debug.Log("Press 'End' key to end your turn");
        if (Input.GetKeyUp(myKeyMapPrefs.endTurn))
            EndMyTurn();

        if (Input.GetKeyUp(myKeyMapPrefs.selectActiveOption1) || Input.GetKeyUp(myKeyMapPrefs.selectActiveOption2)) // open context menu for us to select
            print("bring up unit context menu");

        if (Input.GetKeyUp(myKeyMapPrefs.unitTalk) && ManagerConversationHandler.Instance) // make the current unit talk || TODO - currently using temporary code, this should be called from a function and based on the unit's library of words
        {
            ManagerConversationHandler.Instance.AddSpeakerToConversation(unitsImCommanding[unitSelected]);
            ManagerConversationHandler.Instance.AddDialogueToList(unitsImCommanding[unitSelected], "It's My Turn To Speak!", 0);
        }

        if (Input.GetKeyUp(myKeyMapPrefs.changeKeysMenu) && ManagerKeyInputHandler.Instance)
            ManagerKeyInputHandler.Instance.BrainRequestsKeyChange(this);

        if (Input.GetKeyUp(myKeyMapPrefs.unitsCycle))
            CycleMyUnits();

        if (ManagerMapHandler.Instance)
        {
            if (Input.GetKeyUp(myKeyMapPrefs.selectionUp))
                ChangeTileSelected(-ManagerMapHandler.Instance.mapSize.x);
            if (Input.GetKeyUp(myKeyMapPrefs.selectionRight))
                ChangeTileSelected(1);
            if (Input.GetKeyUp(myKeyMapPrefs.selectionLeft))
                ChangeTileSelected(-1);
            if (Input.GetKeyUp(myKeyMapPrefs.selectionDown))
                ChangeTileSelected(ManagerMapHandler.Instance.mapSize.x);
        }

        // Moving our selected unit
        if (Input.GetKeyUp(myKeyMapPrefs.unitMove) && ManagerMapHandler.Instance)
            if (ManagerMapHandler.Instance.gridTilesGenerated[tileSelected].isInRange || ManagerMapHandler.Instance.gridTilesGenerated[tileSelected] == unitsImCommanding[unitSelected].tileImOn)
                PreviewUnitMove();

        if (Input.GetKeyUp(myKeyMapPrefs.skipUnitTalk) && ManagerConversationHandler.Instance)
            ManagerConversationHandler.Instance.dialogueManager.FinishTypingOrNextDialogue();

        if (Input.GetKeyUp(myKeyMapPrefs.calculateAttack)) // calculate unit's attack
        {
            //ManagerMapHandler.Instance.gridTilesGenerated[tileSelected]
            int attackDamage = unitsImCommanding[unitSelected].CalculateAttack(unitsImCommanding[unitSelected].thisUnitData);
            Debug.Log($"Calculating Attack for {unitsImCommanding[unitSelected].thisUnitData.unitName}. Attack = {attackDamage}");

            // TODO
            // Select an enemy unit and calculate their defense.
            // Subtract defense from unit's attack
            // Send this result to ManagerMapHandler.SendHPChangToTarget(int _change, List<UnitCapsule> _unitsAffected)
        }

        // Plan an attack -- uses the battle forecast
        if (Input.GetKeyUp(myKeyMapPrefs.unitPlanAttack)) // attempt to attack an enemy unit with selected unit
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

            // Calculate Damage and Defense Values for the Forecast
            int attackDamage = unitsImCommanding[unitSelected].CalculateAttack(unitsImCommanding[unitSelected].thisUnitData);
            int defenseValue = otherBrain.unitsImCommanding[otherBrain.unitSelected].CalculateDefense(otherBrain.unitsImCommanding[otherBrain.unitSelected].thisUnitData);

            // Activate and Update Battle Forecast
            if (BattleForecastCanvas_go)
                BattleForecastCanvas_go.SetActive(true);
            if (UnitDataUICanvas_go)
                UnitDataUICanvas_go.SetActive(false);

            //Update Battle Forecast
            if (hasInitiatedBattleForecast == false && ManagerBattleForecast.Instance)
            {
                ManagerBattleForecast.Instance.UpdateBattleForecast(unitsImCommanding[unitSelected], attackDamage, defenseValue, true);
                ManagerBattleForecast.Instance.UpdateBattleForecast(otherBrain.unitsImCommanding[otherBrain.unitSelected], attackDamage, defenseValue, false);
                hasInitiatedBattleForecast = true;
            }
            else if (hasInitiatedBattleForecast == true && ManagerBattleForecast.Instance)
            {
                ManagerBattleForecast.Instance.UpdateBattleForecast(otherBrain.unitsImCommanding[otherBrain.unitSelected], attackDamage, defenseValue, false);
            }
        }

        // Battle with selected units - inflict and receive damage
        if (Input.GetKeyUp(KeyCode.B) && hasInitiatedBattleForecast) // I Left This For You To Make A KeyCode Below Gabriel 
        {
            // Get reference to other player's brain
            ActorBrain otherBrain = GetOtherBrain();

            // Check if the defending unit is within the attacker's range, and if so, Calculate Damage inflicted
            if (CheckIsInRange(unitsImCommanding[unitSelected], otherBrain.unitsImCommanding[otherBrain.unitSelected]) == true)
            {
                // Calculate Damage Inflicted
                int damageInflicted;
                int attackDamage = unitsImCommanding[unitSelected].CalculateAttack(unitsImCommanding[unitSelected].thisUnitData);
                int defenseValue = otherBrain.unitsImCommanding[otherBrain.unitSelected].CalculateDefense(otherBrain.unitsImCommanding[otherBrain.unitSelected].thisUnitData);
                damageInflicted = attackDamage - defenseValue;
                if (damageInflicted < 0) damageInflicted = 0;
                List<UnitCapsule> unitsAffected = new List<UnitCapsule>();
                unitsAffected.Add(otherBrain.unitsImCommanding[otherBrain.unitSelected]);
                bool isUnitStillAlive = ManagerMapHandler.Instance.SendHPChangeToTarget(damageInflicted, otherBrain.unitsImCommanding[otherBrain.unitSelected]);
                if (!isUnitStillAlive)
                {
                    UnitCapsule deadUnit = otherBrain.unitsImCommanding[otherBrain.unitSelected];
                    otherBrain.unitsImCommanding.Remove(deadUnit);
                    Destroy(deadUnit.gameObject);
                    if (otherBrain.unitsImCommanding.Count == 0)
                    {
                        ManagerGameStateHandler.Instance.ChangeGameState(ManagerGameStateHandler.GAMESTATE.PostBattle, this);
                    }
                }

                hasInitiatedBattleForecast = false;
                BattleForecastCanvas_go.SetActive(false);
                otherBrain.unitsImCommanding[otherBrain.unitSelected].ChangeUnitSelection(false);
                UnitDataUICanvas_go.SetActive(true);
                ManagerUnitData.Instance.UpdateUnitDataUI(unitsImCommanding[unitSelected]);
            }

            else
            {
                Debug.Log("Out of Range!!!"); // TODO Display some sort of message on the UI to let the player know their unit is out of range
            }
        }

        if (Input.GetKeyUp(myKeyMapPrefs.cancelActiveOption))
        {
            if (hasInitiatedBattleForecast)
            {
                BattleForecastCanvas_go.SetActive(false);
                UnitDataUICanvas_go.SetActive(true);
                foreach (UnitCapsule unit in unitsImCommanding)
                {
                    if (unit.unitIsSelected) ManagerUnitData.Instance.UpdateUnitDataUI(unit);
                }
                ActorBrain otherBrain = GetOtherBrain();
                foreach (UnitCapsule unit in otherBrain.unitsImCommanding) otherBrain.unitsImCommanding[otherBrain.unitSelected].ChangeUnitSelection(false);
                hasInitiatedBattleForecast = false;
            }
        }
    }
}
