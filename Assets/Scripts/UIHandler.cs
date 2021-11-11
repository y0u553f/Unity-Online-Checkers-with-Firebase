using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIHandler : MonoBehaviour
{
    #region UI Canvas
    [Header("canvas")]
    [SerializeField]
    private GameObject loginCanvas;
    [SerializeField]
    private GameObject mainMenuCanvas;
    [SerializeField]
    private GameObject settingsCanvas;
    [SerializeField]
    private GameObject gameCanvas;
    [SerializeField]
    private GameObject roomPanelCanvas;
    #endregion
    private GameObject currentCanvas = null;

   

    #region Authentification 
    public static UIHandler instance;
    [Header("Authentification UI")]
    [SerializeField]
    private Button linkAccountWithFbButton;
    [SerializeField]
    private InputField anonymousName;
    #endregion

    #region RoomSettings ui
    [Header ("RoomSettings ui")]
    [SerializeField]
    private Text roomCreatedText;
    [SerializeField]
    private InputField roomNumberToJoin;
    private const string ROOMCREATEDMSG = "Room Created : {0} \n waiting for other players...";
    [SerializeField]
    private Text noRoomFoundText;
    [SerializeField]
    private GameObject roomJoinUI;
    [SerializeField]
    private GameObject CreateJoinRoom;
    
    #endregion

    #region playereinfo
    [Header("Player's info ui")]
    [SerializeField]
    private Text displayName;
    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private Image displayImage;
    [SerializeField]
    private GameObject header;
    #endregion

    #region Game UI
    [Header ("Game UI")]
    [SerializeField]
    private GameObject playerOneBoard;
    [SerializeField]
    private GameObject playerTwoBoard;
    [SerializeField]
    private GameObject resultPanel;
    [SerializeField]
    private Text resultTitle;
    [SerializeField]
    private Text resultText;
    [Header("Game UI  other player info")]
    [SerializeField]
    private GameObject otherheader;
    [SerializeField]
    private Text otherdisplayName;
    [SerializeField]
    private Text otherscoreText;
    [SerializeField]
    private Image otherdisplayImage;
    

    #endregion
    private void Awake()
    {
        instance = this;    
    }


    #region Authentification
    public void LoginWithFacebook()
    {
        FbManager.instance.LoginFacebook();
    }

    public void ShowLoginCanvas()
    {
        ShowCanvas(loginCanvas);
    }

    public void SignOut()
    {
        FireBaseAuthentification.SignOut();
        ShowHideHeader(false);
        ShowLoginCanvas();
    }

    public void LoginAnanoymous()
    {

        FireBaseAuthentification.LoginToFireBaseAnnonymous(anonymousName.text);
    }

    public void LinkAccountWithFb()
    {
        FbManager.instance.LoginFacebook();
    }

    public void ShowPlayersInfosInUI(User user)
    {
        displayName.text = user.displayName;
        scoreText.text = user.score.ToString();
        if (user.photoUrl != User.NOPHOTO)
        {
            Debug.Log("Downloading Pics");
            StartCoroutine(WebRequestDownloader.DownloadImage(user.photoUrl, (sprite) => displayImage.sprite = sprite));
        }
        ShowHideHeader(true);
    }
    #endregion

    #region show hide Canvas/UI elements

    public void ShowGameCanvas()
    {
        ShowCanvas(gameCanvas);
        HideRoomSettingsCanvas();
    }
    public void ShowSettingsCanvas()
    {
        ShowCanvas(settingsCanvas);
        if (FireBaseAuthentification.IsAnonymous())
        {
            linkAccountWithFbButton.interactable = true;
            linkAccountWithFbButton.GetComponentInChildren<Text>().text = "Link Account With FB";
        }
        else
        {
            linkAccountWithFbButton.interactable = false;
            linkAccountWithFbButton.GetComponentInChildren<Text>().text = "Account Linked With FB";
        }
    }

    private void ShowHideHeader(bool showIt)
    {
        header.SetActive(showIt);
    }

    public void ShowMainMenu()
    {
        ShowCanvas(mainMenuCanvas);
        
    }

    private void ShowCanvas(GameObject gm)
    {

        if (currentCanvas)
        {
            currentCanvas.SetActive(false);

        }
        gm.SetActive(true);
        currentCanvas = gm;
    }
    #endregion


    #region Room Settings;
    public void CreateNewRoom()
    {
        FireBaseDBManager.CreateNewGame();
    }

    public void ShowRoomNumber(string roomNumber)
    {
        roomCreatedText.text = String.Format(ROOMCREATEDMSG, roomNumber);

    }

    public void JoinGame()
    {
        FireBaseDBManager.JoinGame(roomNumberToJoin.text);
    }

    public void CancelGame()
    {
        GameManager.instance.EndGame();
    }

    public void ShowRoomNotFoundMsg(string roomNumber)
    {
        noRoomFoundText.text = String.Format("No Room with the number {0} Found", roomNumber);
        noRoomFoundText.gameObject.SetActive(true);
    }

    public void HideRoomSettingsCanvas()
    {
        roomPanelCanvas.SetActive(false);
        roomCreatedText.gameObject.SetActive(false);
        roomJoinUI.SetActive(false);
        roomCreatedText.text = "";
        roomNumberToJoin.text = "";
        CreateJoinRoom.SetActive(true);
        noRoomFoundText.text = "";
        noRoomFoundText.gameObject.SetActive(false);
    }
    #endregion


    #region Game UI
    public void ShowOtherPlayerInfo(User user)
    {
        otherdisplayName.text = user.displayName;
        otherscoreText.text = user.score.ToString();
        if (user.photoUrl != User.NOPHOTO)
        {
            Debug.Log("Downloading Pics");
            StartCoroutine(WebRequestDownloader.DownloadImage(user.photoUrl, (sprite) => otherdisplayImage.sprite = sprite));
        }
        otherheader.SetActive(true);
    }

    public void CreateBoard(bool isPlayerOne)
    {
        Transform board;
        if (isPlayerOne)
        {
           board = Instantiate(playerOneBoard, gameCanvas.transform).transform;
        }
        else
        {
           board = Instantiate(playerTwoBoard, gameCanvas.transform) .transform;
        }
        board.SetAsFirstSibling();
    }

    public void ShowResult(bool win, int score)
    {
        resultPanel.SetActive(true);
        if (win)
            resultTitle.text = "win !!";
        else
            resultTitle.text = "lost !!";
        resultText.text = "Your new Score is " + score.ToString();
        
    }

    public void RestartGame()
    {
        GameManager.instance.EndGame();
        GameSceneManager.RestartGame();
    }
    #endregion

}
