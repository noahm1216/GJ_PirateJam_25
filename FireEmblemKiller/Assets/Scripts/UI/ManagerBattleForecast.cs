using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManagerBattleForecast : MonoBehaviour
{
    public static ManagerBattleForecast Instance { get; private set; }
    public TextMeshProUGUI attackingUnitName, defendingUnitName, attackText, speedText, defenseText, constitutionText;
    public Sprite attackerSprite, defenderSprite;
    public Image attackerImage, defenderImage;
    
    private string attackerName, defenderName;
    private int attackerAttack, attackerSpeed, defenderDefense, defenderConstitution;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // UpdateBattleForecast() updates all UI assets associated with the Battle Forecast
    // @Param unit - the unit who's data is getting updated
    // @Param attkDamage - the amount of attack being dealt. Only applicable if isAttacker == true
    // @Param defValue - the amount of defense for the defending unit. Only applicable if isAttacker == false
    // @Param isAttacker - whether or not the unit passed in is the attacker. This tells us if the function is updating
    //                     the left of ride side of the battle forecast's panels.
    public void UpdateBattleForecast(UnitCapsule unit, int attkDamage, int defValue, bool isAttacker)
    {
        // Update attacker information (left side of forecast)
        if (isAttacker)
        {
            attackerName = unit.thisUnitData.unitName;
            attackerAttack = attkDamage;
            attackerSpeed = unit.thisUnitData.speed;
            attackerImage.sprite = unit.thisUnitData.unitIcon;
            // Update UI Assets:
            attackingUnitName.text = attackerName;
            attackText.text = attackerAttack.ToString();
            speedText.text = attackerSpeed.ToString();
        }
        // Update defender information (right side of forecast)
        else
        {
            defenderName = unit.thisUnitData.unitName;
            defenderDefense = defValue;
            defenderConstitution = unit.thisUnitData.thisUnitsStats.constitution;
            defenderImage.sprite = unit.thisUnitData.unitIcon;
            // Update UI Assets
            defendingUnitName.text = defenderName;
            defenseText.text = defenderDefense.ToString();
            constitutionText.text = defenderConstitution.ToString();
        }
    }
}
