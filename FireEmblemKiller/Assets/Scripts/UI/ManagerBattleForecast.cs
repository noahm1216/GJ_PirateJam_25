using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManagerBattleForecast : MonoBehaviour
{
    public static ManagerBattleForecast Instance { get; private set; }
    public TextMeshProUGUI attackingUnitName, defendingUnitName, attackText, speedText, defenseText, constitutionText;

    private Sprite attackerSprite, defenderSprite;
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

    public void SetForecastData(string attkName, string defName, int attk, int speed, int def, int constitution)
    {
        //might be unnecessary
        attackerName = attkName;
        defenderName = defName;
        attackerAttack = attk;
        attackerSpeed = speed;
        defenderDefense = def;
        defenderConstitution = constitution;

        // Actual UI Updating:
        attackingUnitName.text = attkName;
        defendingUnitName.text = defName;
        attackText.text = attk.ToString();
        speedText.text = speed.ToString();
        defenseText.text = def.ToString();
        constitutionText.text = constitution.ToString();
    }
}
