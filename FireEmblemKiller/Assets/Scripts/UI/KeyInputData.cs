using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyInputData : MonoBehaviour
{
    public bool changingNow;
    public KeyCode theKeycode;
    public TextMeshProUGUI keycodeText, keycodePurposeText;


    public void FillOutTheData(KeyCode _newKeycode, string _keyPurpose)
    {
        theKeycode = _newKeycode;

        if (keycodeText)
            keycodeText.text = theKeycode.ToString();

        if (keycodePurposeText)
            keycodePurposeText.text = _keyPurpose;
    }

    public void TryingToChange(bool _isChanging)
    {
        changingNow = _isChanging;

        if (_isChanging && ManagerKeyInputHandler.Instance)
            _isChanging = true;
    }
}
