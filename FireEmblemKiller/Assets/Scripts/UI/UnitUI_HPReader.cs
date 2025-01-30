using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitUI_HPReader : MonoBehaviour
{
    public UnitCapsule myUnit;
    public GameObject uiObj_HpBar_Root; // we dont use this, but maybe we will want to turn it off/ hide it in some scenario
    public Transform uiObj_HpBar_HealthTop; // the green / top bar that shows the current HP

    private float percetnHp = 0;
    private float delayTimeStamp, delayTimer = 2f;

    private void Update() // ideally a scalable version with hundreds of units would only call this when a change to health is made
    {
        UpdateUIHP();

        if (Time.time > delayTimeStamp + delayTimer)
        {
            //UpdateUIHP();
            delayTimeStamp = Time.time;
        }

    }

    public void UpdateUIHP() // just calculatesHP if we have the needed references
    {
        if (myUnit)
        {
            percetnHp = myUnit.thisUnitData.healthPoints.x / myUnit.thisUnitData.healthPoints.y;

            if (percetnHp < 0) percetnHp = 0;
            if (percetnHp > myUnit.thisUnitData.healthPoints.y) percetnHp = 1;
        }

        if (uiObj_HpBar_HealthTop)
            uiObj_HpBar_HealthTop.localScale = new Vector3(percetnHp, 1, 1);
    }


}



// the custom data for keycode lists
[System.Serializable]
public class CustomColorIntegers
{
    [Range(0,1)]
    public float healthMinium = 0;  
    public Color healthBarColor = Color.green;

    //public CustomColorIntegers(string _newPurpose, KeyCode _newKc)
    //{
    //    keyMapPurpose = _newPurpose;
    //    keycodeHotkey = _newKc;
    //}

}//end of class for keycode lists
