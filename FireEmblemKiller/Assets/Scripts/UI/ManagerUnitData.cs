using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManagerUnitData : MonoBehaviour
{
    public static ManagerUnitData Instance { get; private set; }

    public Image unitImage_UI;
    public TMP_Text unitName_UI;
    public TMP_Text unitAttkVal_UI;
    public TMP_Text unitSpeedVal_UI;
    public TMP_Text unitRangeVal_UI;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateUnitDataUI(UnitCapsule u)
    {
        unitImage_UI.sprite = u.thisUnitData.unitIcon;
        unitName_UI.text = u.thisUnitData.unitName;
        unitAttkVal_UI.text = u.thisUnitData.thisUnitsStats.strength.ToString();
        unitSpeedVal_UI.text = u.thisUnitData.speed.ToString();
        unitRangeVal_UI.text = u.thisUnitData.range.ToString();
    }
}
