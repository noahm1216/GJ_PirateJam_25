using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ManagerKeyInputHandler : MonoBehaviour
{
    public static ManagerKeyInputHandler Instance { get; private set; }

    public GameObject keyInputUIObject;
    public TextMeshProUGUI keyPrefsTitleText;
    public KeyInputData keyInputUiTemplate;
    public List<KeyInputData> keyUIDataClones = new List<KeyInputData>();


    public bool changingAKeyNow = false;
    private KeyCode nullKeycode = KeyCode.None; // we need to return a false
    private List<KeyMapCustomDict> keyMapping = new List<KeyMapCustomDict>();
    private ActorBrain brainRef;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    private void Start()
    {
        if (keyInputUIObject)
            keyInputUIObject.SetActive(false);
    }

    public void BrainRequestsKeyChange(ActorBrain _brain)
    {
        if (_brain == null)
        { Debug.Log("Missing 'ActorBrain _brain' reference"); return; }
        else
            brainRef = _brain;

        if (!keyInputUIObject)
        { Debug.Log("Missing Ref To 'Key Input UI Object'"); return; }

        keyInputUIObject.SetActive(!keyInputUIObject.activeSelf); // toggle the UI

        // if it's off now
        if (keyInputUIObject.activeSelf == false)
            ClearRefs();
        else // if it's on now
        {
            if (keyPrefsTitleText)
                keyPrefsTitleText.text = _brain.myKeyMapPrefs.keyMappingNickname;

            keyMapping = _brain.myKeyMapPrefs.keyMappingList;

            if (!keyInputUiTemplate)
            { Debug.Log("Missing Ref To 'Key UI Template'"); return; }

            if (keyMapping.Count == 0)
            { Debug.Log("No Keys Were Passed Into 'Key Mapping'"); return; }

            foreach (var keyEntry in keyMapping)
            {
                Transform keyUIClone = Instantiate(keyInputUiTemplate.transform, keyInputUiTemplate.transform.parent);
                KeyInputData cloneData = null;
                keyUIClone.TryGetComponent(out cloneData);
                if (cloneData)
                { cloneData.FillOutTheData(keyEntry.keycodeHotkey, keyEntry.keyMapPurpose); keyUIDataClones.Add(cloneData); }
                keyUIClone.gameObject.SetActive(true);                
            }
        }

    }

    public void TryingToChange(bool _isChanging)
    {
        changingAKeyNow = _isChanging;
    }

        public void Update()
    {
        if (changingAKeyNow && DetectInput() != nullKeycode)
            UpdateKeyStroke(DetectInput());
    }

    public KeyCode DetectInput()
    {
        foreach (KeyCode vkey in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey(vkey))
            {
                print($"Changed Key To: {vkey}");
                changingAKeyNow = false;
                return vkey;
            }
        }
        return nullKeycode;
    }


    // run through our list of UI elements and find the one that is OPEN to changing right now
    // then grab a references so we can swap any duplicate key mappings
    public void UpdateKeyStroke(KeyCode _newKey)
    {
        if (keyUIDataClones.Count == 0 || keyMapping.Count == 0)
        { Debug.Log("Cannot Update KeyStrokes without 'Key Mapping' OR UI Key Comparables"); return; }        

        KeyInputData uiTicketChanging = null;
        foreach (KeyInputData uiKey in keyUIDataClones)
            if (uiKey.changingNow)
            { uiTicketChanging = uiKey; break; }

        if (uiTicketChanging == null)
        { Debug.Log("Cannot Find A 'Ui Key' open to changing"); return; }

        if (uiTicketChanging.theKeycode == _newKey) // if we map the same key we can stop here
        { uiTicketChanging.FillOutTheData(_newKey, uiTicketChanging.keycodePurposeText.text); return; }

        KeyCode previousKeycode = uiTicketChanging.theKeycode;
        int id_TargetKeyToChange = -1;
        int id_ExistingKeyCopy = -1;

        for(int i = 0; i < keyMapping.Count; i++)
        {
            if (keyMapping[i].keycodeHotkey == previousKeycode)
                id_TargetKeyToChange = i;

            if (keyMapping[i].keycodeHotkey == _newKey)
                id_ExistingKeyCopy = i;
        }

        if (id_TargetKeyToChange >= 0)
            keyMapping[id_TargetKeyToChange].keycodeHotkey = _newKey; // update list with new key

        if (id_ExistingKeyCopy >= 0)
        {
            foreach (KeyInputData uiKey in keyUIDataClones)
                if (uiKey.theKeycode == _newKey  && !uiKey.changingNow)
                { uiKey.FillOutTheData(previousKeycode, keyMapping[id_ExistingKeyCopy].keyMapPurpose); break; }

            keyMapping[id_ExistingKeyCopy].keycodeHotkey = previousKeycode; // update list with old key
        }

        uiTicketChanging.FillOutTheData(_newKey, uiTicketChanging.keycodePurposeText.text); // update the ticket
        uiTicketChanging.TryingToChange(false);
        if (brainRef)
            brainRef.myKeyMapPrefs.keyMappingList = keyMapping;
    }

    private void ClearRefs() // reset lists and refs
    {
        if (keyUIDataClones.Count > 0)
            foreach (KeyInputData uiKey in keyUIDataClones)
                Destroy(uiKey.gameObject);

        keyUIDataClones = new List<KeyInputData>();
        brainRef = null;
    }

}




// the custom data for key preferences
[System.Serializable]
public class ActorButtonMap
{
    public string keyMappingNickname = "Key Preferences";

    public List<KeyMapCustomDict> keyMappingList = new List<KeyMapCustomDict>()
    {
        new KeyMapCustomDict("Select UP", KeyCode.UpArrow ),
        new KeyMapCustomDict("Select Down", KeyCode.DownArrow ),
        new KeyMapCustomDict("Select Right", KeyCode.RightArrow ),
        new KeyMapCustomDict("Select Left", KeyCode.LeftArrow ),

        new KeyMapCustomDict("Select Active (Option 1)", KeyCode.Return ),
        new KeyMapCustomDict("Select Active (Option 2)", KeyCode.KeypadEnter ),
        new KeyMapCustomDict("Cancel Active Option", KeyCode.Escape ),
        new KeyMapCustomDict( "Remove Active Option", KeyCode.Backspace ),

        new KeyMapCustomDict( "End Turn", KeyCode.End ),
        new KeyMapCustomDict("Cycle Units", KeyCode.Tab ),
        new KeyMapCustomDict("Unit Action: Move", KeyCode.M ),
        new KeyMapCustomDict("Unit Action: Talk", KeyCode.T ),
        new KeyMapCustomDict("Skip / Next Dialogue", KeyCode.Space ),
        new KeyMapCustomDict("Open Key-Mapping Menu" , KeyCode.K),
        new KeyMapCustomDict("Unit Action: Attack", KeyCode.A ),
        new KeyMapCustomDict( "Unit Action: Calculate", KeyCode.C ),
        new KeyMapCustomDict( "Unit Action: Confirm Attack", KeyCode.B ),
    };

 

    [Space]
    [Header("SELECT NAVIGATION\n_____________")]
    public KeyCode selectionUp = KeyCode.UpArrow;
    public KeyCode selectionDown = KeyCode.DownArrow;
    public KeyCode selectionRight = KeyCode.RightArrow;
    public KeyCode selectionLeft = KeyCode.LeftArrow;

    [Space]
    [Header("AFFIRMATION \n_____________")]
    public KeyCode selectActiveOption1 = KeyCode.Return;
    public KeyCode selectActiveOption2 = KeyCode.KeypadEnter;
    public KeyCode cancelActiveOption = KeyCode.Escape;
    public KeyCode removeActiveOption = KeyCode.Backspace;

    [Space]
    [Header("ACTION HOTKEYS \n_____________")] // Most of these will change as we complete a full design
    public KeyCode endTurn = KeyCode.End;
    public KeyCode unitsCycle = KeyCode.Tab;
    public KeyCode unitMove = KeyCode.M;
    public KeyCode unitTalk = KeyCode.T;
    public KeyCode skipUnitTalk = KeyCode.Space;
    public KeyCode changeKeysMenu = KeyCode.K;
    public KeyCode unitPlanAttack = KeyCode.A;
    public KeyCode calculateAttack = KeyCode.C;
    public KeyCode unitConfirmAttack = KeyCode.B; // Mostly Because I wasnt sure what it was doing


    //public ActorButtonMap(string _newSpkr)
    //{
    //    speaker = _newSpkr;       
    //}

}//end of class for keys



// the custom data for keycode lists
[System.Serializable]
public class KeyMapCustomDict
{    
    public string keyMapPurpose = "Key Preferences";
    public KeyCode keycodeHotkey = KeyCode.None;

    public KeyMapCustomDict(string _newPurpose, KeyCode _newKc)
    {
        keyMapPurpose = _newPurpose;
        keycodeHotkey = _newKc;        
    }

}//end of class for keycode lists
