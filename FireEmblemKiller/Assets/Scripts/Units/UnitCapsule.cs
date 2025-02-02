using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnitCapsule : MonoBehaviour
{
    public bool unitIsSelected;
    public bool unitActionsFinished;
    public GameObject selectedIndicator;
    public MapTileData tileImOn;
    public Animator animCon;
    public UnitData thisUnitData; // this single unit data (might want to have a different script)


    public UnityEvent EvOnSelect = new UnityEvent();
    public UnityEvent EvOnDeselect = new UnityEvent();
    public UnityEvent EvOnAttack = new UnityEvent();
    public UnityEvent EvOnHitByAttack = new UnityEvent();
    public UnityEvent EvOnDeath = new UnityEvent();

    // update / upgrade stats
    // switching classes
    // attacking (example... if we have a bow, and a knife in our equipment... when we attack from a range we will automatically only use the bow, when we attack from melee we will only use the knife)
    // and more things a unit might want to do


    public void ChangeUnitSelection(bool _isSelected)
    {
        unitIsSelected = _isSelected;

        if (selectedIndicator)
            selectedIndicator.SetActive(unitIsSelected);

        if (unitIsSelected)
            EvOnSelect.Invoke();
        else
            EvOnDeselect.Invoke();
    }

    public int CalculateAttack(UnitData unit)
    {
        // damage starts with the unit's base strength stat
        int damage = unit.thisUnitsStats.strength;

        // check all equipment for attack bonuses
        foreach (Equipment eq in thisUnitData.thisUnitsEquipment)
        {
            damage += eq.strengthBonus;
            Debug.Log($"Adding {eq.equipmentName}'s strength bonus ({eq.strengthBonus}) to damage. New total: {damage}");
        }

        // basic critical chance check using charisma
        int randomNumber = Random.Range(1, 100);
        if (randomNumber <= unit.thisUnitsStats.charisma) damage *= 2;

        return damage;
    }

    public int CalculateDefense(UnitData unit)
    {
        // defense value starts at 0
        int defense = 0;

        // add armor values to defense
        foreach (Equipment eq in thisUnitData.thisUnitsEquipment)
        {
            // the added defense is just twice that of the equipment's constitution bonus
            defense += eq.constitutionBonus * 2;
            Debug.Log($"Adding {eq.equipmentName}'s constitution bonus ({eq.constitutionBonus}) to defens. New total: {defense}");
        }

        return defense;
    }

    private void ResetAllAnimations()
    {
        if(!animCon)
        { Debug.Log($"WARNING: No animation controllers hooked up to this character: {thisUnitData.unitName}"); return; }

        animCon.ResetTrigger("Attack");
        animCon.ResetTrigger("CounterAttack");
    }

    public void CallEvent_Attacking(bool _isCounter)
    {
        ResetAllAnimations();
        if (_isCounter)
            animCon.SetTrigger("CounterAttack");
        else
            animCon.SetTrigger("Attack");
        EvOnAttack.Invoke();
    }

    public void CallEvent_Hit()
    {
        ResetAllAnimations();
        animCon.SetTrigger("CounterAttack");
        EvOnHitByAttack.Invoke();
    }

    public void CallEvent_Death()
    {
        ResetAllAnimations();
        EvOnDeath.Invoke();
    }
} // end of UnitCapsule Base Class


/// <summary>
/// ---------------------------------------------------CUSTOM UNIT CLASSES
/// </summary>

// the custom data for unit information
[System.Serializable]
public class UnitData
{
    public string unitName = ""; // name of character
    public Color unitColor = Color.black;
    public Sprite unitIcon; // image to show during conversations
    public Transform unitPrefab; // 2d/3d model for the game  
    // owner of this unit
    public int level; // the level of this character
    public Vector2 experience = new Vector2(0,0); // x = current XP, y = needed XP to level up
    // class {enum types}
    public Vector2 healthPoints = new Vector2(0, 0); // x = current, y = max
    public int speed; // the amount of tiles or space we can move (roughly in meters)
    public int range; // the distance we can attack

    [Space]
    public UnitStats thisUnitsStats;
    [Space]
    public List<Equipment> thisUnitsEquipment = new List<Equipment>();


    public UnitData(string _newUName, Color _newClr, Sprite _newUIcon, Transform _newUPrefab, int _newLv, Vector2 _newExp, Vector2 _newHP, int _newSpd, int _newRng)
    {
        unitName = _newUName;
        unitColor = _newClr;
        unitIcon = _newUIcon;
        unitPrefab = _newUPrefab;
        level = _newLv;
        experience = _newExp;
        //class
        healthPoints = _newHP;
        speed = _newSpd;
        range = _newRng;
    }

}//end of data for units


// the custom data for unit stats
[System.Serializable]
public class UnitStats
{
   
    public int strength; // how much damage we do
    public int dexterity; // number of tiles we can move (speed)
    public int constitution; // the health we have
    public int wisdom; // the mana we have per turn
    public int charisma; // the luck, crit, and persuasion


    public UnitStats(int _newSTR, int _newDEX, int _newCON, int _newWIS, int _newCHA)
    {
        strength = _newSTR;
        dexterity = _newDEX;
        constitution = _newCON;
        wisdom = _newWIS;
        charisma = _newCHA;
    }

}//end of data for unit stats


// the custom data for unit equipment
[System.Serializable]
public class Equipment
{
    public string equipmentName = "";
    // enum equipment type
    // equipment image

    public int strengthBonus; // how much damage we do
    public int dexterityBonus; // number of tiles we can move (speed)
    public int constitutionBonus; // the health we have
    public int wisdomBonus; // the mana we have per turn
    public int charismaBonus; // the luck, crit, and persuasion

    public int rangeBonus; // how much dange this adds to attacks


    public Equipment(int _newSTR, int _newDEX, int _newCON, int _newWIS, int _newCHA)
    {
        strengthBonus = _newSTR;
        dexterityBonus = _newDEX;
        constitutionBonus = _newCON;
        wisdomBonus = _newWIS;
        charismaBonus = _newCHA;
    }

}//end of data for equipment


// the custom data for unit actions
[System.Serializable]
public class UnitAction
{
    public string actionDisplayName = "";
    // enum equipment type
    enum UNITACTIONS { }
    // equipment image

    

    public UnitAction()
    {

    }

}//end of data for equipment


// TODO

// actions
// talk
// attack (if no weapons then asssumed melee)
// abilities

// abilities:
// fireball - heal one - heal multiple - etc...

// hidden information (who we have talked to, battled, killed, fought next to, watched battle, etc...)

// hidden dialogue (this might be a list in the world we can pull from
