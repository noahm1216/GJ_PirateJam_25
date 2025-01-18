using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTileData : MonoBehaviour
{
    public bool isWalkable;
    public UnitCapsule unitOnThisTile;
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
