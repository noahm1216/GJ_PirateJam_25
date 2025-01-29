using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// <para> GameState tracks the most important changes between experience. This is helpful for isolating functions on a higher level </para>
/// </summary>
public class ManagerGameStateHandler : MonoBehaviour
{
    public static ManagerGameStateHandler Instance { get; private set; }
    public enum GAMESTATE {None, PreBattle, MapBattle, PostBattle }


    public GAMESTATE gameActiveState = GAMESTATE.None;
    public GameObject canvasImgRootObj, canvasReloadButtonObj;
    public TextMeshProUGUI textGameState, textInformationHeader, textInformationBody;




    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    private void Start()
    {
        if (!FoundErrorsInReferences()) return;
        canvasImgRootObj.SetActive(false);
        canvasReloadButtonObj.SetActive(false);

    }

    public bool FoundErrorsInReferences()
    {
        string outputError = "";

        if (!canvasImgRootObj) outputError += "|Error Missing Canvas Obj For Canvas Img Root \n";
        if (!canvasReloadButtonObj) outputError += "|Error Missing Canvas Obj For Reload Button Obj \n";
        if (!textGameState) outputError += "|Error Missing Text Obj For Game State \n";
        if (!textInformationHeader) outputError += "|Error Missing Text Obj For Information Header \n";
        if (!textInformationBody) outputError += "|Error Missing Text Obj For Information Body \n";

        if (string.IsNullOrEmpty(outputError))
            return false;
        else
        { Debug.Log(outputError); return true; }
    }

    public void ChangeGameState(GAMESTATE _newState, ActorBrain _brain) // pass NULL brain if not ending the game
    {
        if (FoundErrorsInReferences())
            return;

        //if (gameActiveState == _newState) /// NOTE: actually we dont want to do this because we can use this to show the change in turn order on UI
        //{ Debug.Log($"WARNING: Tried Changing GameState To The Same State: {_newState} / Current State: {gameActiveState}"); return; }

        canvasReloadButtonObj.SetActive(false);

        print($"Game State Changed and Taken in Effect {gameActiveState}");
        switch (gameActiveState)
        {
            case GAMESTATE.PreBattle:
                StartCoroutine(G_State_PreBattle());
                break;
            case GAMESTATE.MapBattle:
                StartCoroutine(G_State_MapBattle(_brain));
                break;
            case GAMESTATE.PostBattle:
                StartCoroutine(G_State_PostBattle(_brain));
                break;
            default:
                Debug.Log("ERROR: No active Game State Condition");
                ChangeGameState(GAMESTATE.None, null);
                break;
        }
    }

    private IEnumerator G_State_PreBattle()
    {
        if (!FoundErrorsInReferences())
        {
            canvasImgRootObj.SetActive(true);
            textGameState.text = gameActiveState.ToString();
            // Introduction to the challenge
            textInformationHeader.text = "OBJECTIVE: Leave No Enemy Units Alive.";
            KeyCode controlKey = KeyCode.K;
            textInformationBody.text = $"Use your units to: Move, Attack, and Talk your way to victory. Press {controlKey} to see a list of controls when its your turn.\n\n\n (also thank you for trying our demo prototype!)";
            for(int i = 0; i < 5; i++)
            {
                textInformationBody.text += $"{5 - i}...";
                yield return new WaitForSeconds(1);
            }
            canvasImgRootObj.SetActive(false);
            // switch to the next game mode
            if (ManagerMapHandler.Instance)
                ChangeGameState(GAMESTATE.MapBattle, ManagerMapHandler.Instance.currentPlayersTurn);
            else
                ChangeGameState(GAMESTATE.MapBattle, null);
            
        }
    }

    private IEnumerator G_State_MapBattle(ActorBrain _brain)
    {
        if (!FoundErrorsInReferences())
        {
            canvasImgRootObj.SetActive(true);
            textGameState.text = gameActiveState.ToString();
            // say who is going first
            if (!_brain)
            {
                textInformationHeader.text = "TURN ORDER: Blue Player's Turn";
                textInformationBody.text = "Press K To See The Controls.";
            }
            else
            {
                textInformationHeader.text = $"TURN ORDER: {_brain.playerName}'s Turn";
                textInformationBody.text = $"Press {_brain.myKeyMapPrefs.changeKeysMenu} To See The Controls.";
            }
            for (int i = 0; i < 5; i++)
            {
                textInformationBody.text += $"{5 - i}...";
                yield return new WaitForSeconds(1);
            }
            canvasImgRootObj.SetActive(false);
        }
    }

    private IEnumerator G_State_PostBattle(ActorBrain _brain)
    {
        if (!FoundErrorsInReferences())
        {
            canvasImgRootObj.SetActive(true);
            textGameState.text = gameActiveState.ToString();
            // say who won
            if (!_brain)
            {
                textInformationHeader.text = "END GAME: No Players Survived";
                textInformationBody.text = "Missing Information Leads Us To Believe NO WINNER was left... Sometimes this is the reality of war, and a tragedy for everyone involved. The echoes of this battle will be felt across the universe. \n\n\n (thanks for trying our game/demo)";
            }
            else
            {
                textInformationHeader.text = $"END GAME: The Player With Units Left: {_brain.playerName}";
                textInformationBody.text = $"War is a violent act that pits ideals against each other and the winner is defined by the survivors. We simulate war so we can understand the world and ourselves and today the one who will tell this story is {_brain.playerName}.\n\n\n (also thank you for trying our demo prototype!)";
            }            
            for (int i = 0; i < 5; i++)
            {
                textInformationBody.text += $"{5 - i}...";
                yield return new WaitForSeconds(1);
            }
            // ask if want to play again... if so maybe just reload the level if we're strapped for time
            canvasReloadButtonObj.SetActive(true);
        }
    }

    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name, LoadSceneMode.Single);
    }


}
