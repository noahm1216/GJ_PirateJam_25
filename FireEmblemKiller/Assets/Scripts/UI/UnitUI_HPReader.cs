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

    private void LateUpdate() // ideally a scalable version with hundreds of units would only call this when a change to health is made
    {
        if (Time.time < delayTimeStamp + delayTimer)
            return;

        UpdateUIHP();
        delayTimeStamp = Time.time;

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
