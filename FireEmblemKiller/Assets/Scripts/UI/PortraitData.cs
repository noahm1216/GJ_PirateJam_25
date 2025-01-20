using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PortraitData : MonoBehaviour
{
    /// <summary>
    ///  could be cool to do something like this <https://tornioduva.itch.io/dd-card-sheet>
    /// </summary>

    public Color colorIndentifier = Color.black;
    public Image profilePicHolder;
    public bool picfacingLeft = true;
    public TextMeshProUGUI profileNameText;


    public void UpdateProfile(UnitCapsule _unitPassed, bool _isFacingLeft)
    {
        colorIndentifier = _unitPassed.thisUnitData.unitColor;
        profilePicHolder.sprite = _unitPassed.thisUnitData.unitIcon;
        if (picfacingLeft != _isFacingLeft)
            profilePicHolder.transform.Rotate(0, 180, 0);
        profileNameText.color = colorIndentifier;
        profileNameText.text = _unitPassed.thisUnitData.unitName;
    }

}