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
    private Dictionary<KeyCode, string> keyMapping = new Dictionary<KeyCode, string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    public void BrainRequestsKeyChange(ActorBrain _brain)
    {
        if (!keyInputUIObject)
        { Debug.Log("Missing Ref To 'Key Input UI Object'"); return; }

        keyInputUIObject.SetActive(!keyInputUIObject.activeSelf); // toggle the UI

        // if it's off now
        if (keyInputUIObject.activeSelf == false)
        {
            if (keyUIDataClones.Count > 0)
                foreach (KeyInputData uiKey in keyUIDataClones)
                    Destroy(uiKey.gameObject);
            keyUIDataClones = new List<KeyInputData>(); // reset list
        }
        else // if it's on now
        {
            if (keyPrefsTitleText)
                keyPrefsTitleText.text = _brain.myKeyMapPrefs.keyMappingNickname;

            keyMapping = _brain.myKeyMapPrefs.keyMappingDictionary;

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
                    cloneData.FillOutTheData(keyEntry.Key, keyEntry.Value);
            }
        }        

    }


    public void Update()
    {
        if (changingAKeyNow)
            DetectInput();
    }

    public KeyCode DetectInput()
    {
        foreach (KeyCode vkey in System.Enum.GetValues(typeof(KeyCode)))
        {
            //KeyCode kCode;  //this stores your custom key

            if (Input.GetKey(vkey))
            {
                print($"Changed Key To: {vkey}");
                changingAKeyNow = false;
                return vkey;                
            }
        }
        return KeyCode.Help;
    }


}
