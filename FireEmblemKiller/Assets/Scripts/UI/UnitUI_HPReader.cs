using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI_HPReader : MonoBehaviour
{
    public UnitCapsule myUnit;
    public Image uiImg_HpBar_HealthFront; // the green / top bar that shows the current HP
    public List<CustomColorIntegers> healthVisualMarks = new List<CustomColorIntegers>();


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

        if (uiImg_HpBar_HealthFront)
        {
            uiImg_HpBar_HealthFront.transform.localScale = new Vector3(percetnHp, 1, 1);
            uiImg_HpBar_HealthFront.color = CurrentColorToUse();
        }
    }

    private Color CurrentColorToUse()
    {
        if (healthVisualMarks.Count == 0)
            return Color.green;

        int closestValueID = 0;
        for (int i = 0; i < healthVisualMarks.Count; i++)
            if (healthVisualMarks[i].healthMinimum >= percetnHp)
                closestValueID = i;
            

        return healthVisualMarks[closestValueID].healthBarColor;
    }


}



// the custom data for keycode lists
[System.Serializable]
public class CustomColorIntegers
{
    [Range(0,1)]
    public float healthMinimum = 0;  
    public Color healthBarColor = Color.green;

    //public CustomColorIntegers(string _newPurpose, KeyCode _newKc)
    //{
    //    keyMapPurpose = _newPurpose;
    //    keycodeHotkey = _newKc;
    //}

}//end of class for keycode lists
