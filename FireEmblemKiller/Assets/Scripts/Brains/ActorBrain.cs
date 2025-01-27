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

        if (Input.GetKeyUp(KeyCode.B)) // I Left This For You To Make A KeyCode Below Gabriel 
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




