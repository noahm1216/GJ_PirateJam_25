using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTileData : MonoBehaviour
{
    public bool isWalkable;
    public UnitCapsule unitOnThisTile;
    public bool isSelected;
    public GameObject selectedIndicator, occupiedIndicator;
    public bool checkForTriggers;

    private float timeSpawned, timeToCheck = 2f;


    private void Start()
    {
        timeSpawned = Time.time;
    }

    public bool TileIsFree()
    {        
        if (isWalkable && unitOnThisTile == null)
            return true;
        else
            return false;
    }

    public void LateUpdate()
    {
        if (occupiedIndicator)
            occupiedIndicator.SetActive(isWalkable && unitOnThisTile); // eventually this should just be a function called from the Brain once (passing if we are selecting or not)
        if (selectedIndicator)
            selectedIndicator.SetActive(isSelected); // eventually this should just be a function called from the Brain once (passing if we are selecting or not)
    }

    public void OnTriggerStay(Collider trig)
    {
        if (Time.time > timeSpawned + timeToCheck)
            checkForTriggers = false;

        if (!checkForTriggers)
            return;        

        print($"{transform.name} - Trig With - {trig.name}");
        isWalkable = true;
    }

    public void OnTriggerExit(Collider trig)
    {
        if (!checkForTriggers)
            return;

        isWalkable = false;
    }

}
