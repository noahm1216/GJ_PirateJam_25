using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTileData : MonoBehaviour
{
    public UnitCapsule unitOnThisTile;
    public bool isWalkable, isSelected, isInRange;    
    public GameObject selectedIndicator, occupiedIndicator, inRangeIndicator;
    public bool checkForTriggers;

    private float timeSpawned, timeToCheck = 2f;


    private void Start()
    {
        timeSpawned = Time.time;
        isWalkable = true;
        ChangeWalkable(isWalkable);
        ChangeInRange(isInRange);
        ChangeSelection(isSelected);
        
    }

    public bool TileIsFree()
    {
        ChangeWalkable(isWalkable); // just updates the visuals if needed

        if (isWalkable && unitOnThisTile == null)
            return true;
        else
            return false;
    }

    public void ChangeSelection(bool _isSelected)
    {
        isSelected = _isSelected;
        if (selectedIndicator)
            selectedIndicator.SetActive(isSelected);
    }

    public void ChangeWalkable(bool _isWalkable)
    {
        isWalkable = _isWalkable;

        if (occupiedIndicator)
            occupiedIndicator.SetActive(isWalkable && unitOnThisTile);
    }

    public void ChangeInRange(bool _isRange)
    {
        isInRange = _isRange;
        if (inRangeIndicator)
            inRangeIndicator.SetActive(isInRange);
    }   

    public void OnTriggerStay(Collider trig) // right now checking for the first X seconds if we are colliding with ANYTHING (units dont have collisions so we are checking for environment collisions)
    {
        if (Time.time > timeSpawned + timeToCheck) // ideally we want to run this check one time at spawn if the environment is clipping with a tile... we could also spawn the tiles and project them down onto the environment normals... depends
            checkForTriggers = false;

        if (!checkForTriggers)
            return;        

        print($"{transform.name} - Trig With - {trig.name}");
        ChangeWalkable(true);
    }

    public void OnTriggerExit(Collider trig)
    {
        if (!checkForTriggers)
            return;

        ChangeWalkable(false);
    }

}
