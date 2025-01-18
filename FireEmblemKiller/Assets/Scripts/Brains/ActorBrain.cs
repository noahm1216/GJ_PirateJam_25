using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorBrain : MonoBehaviour
{
    public bool ownedByAHumanPlayer;
    public bool myTurn;
    public List<Transform> myUnitPrefabs = new List<Transform>();
    [HideInInspector] public List<UnitCapsule> allMyUnits = new List<UnitCapsule>();


    // Start is called before the first frame update
    void Start()
    {
        if (transform.tag == "Player")
            ownedByAHumanPlayer = true;

        //if (myUnitPrefabs.Count > 0)
        //{
        //    foreach (Transform unit in myUnitPrefabs)
        //    {
        //        UnitCapsule uC = null;
        //        unit.TryGetComponent(out uC);
        //        if (uC != null)
        //            allMyUnits.Add(uC);
        //    }
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
