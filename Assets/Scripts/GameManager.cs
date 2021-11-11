using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{

    public Game CurrentGame { set; get; }

    public static GameManager instance;


    [SerializeField]
    private GameObject inputBlocker;

    public User CurrentUser { get; set; }
    private User opponent;

    public bool isMyturn;
    [SerializeField]
    private Color playerOneColor, playerTwoColor;
    [HideInInspector]
    public Color mycolor , opponentColor;

    private  bool isPlayerOne;

    public bool IsPlayerOne { get { return isPlayerOne; } }
  
    private GameObject[] squares;

    private const string BLACKSQUARETAG= "BlackSquare";

    public  Square selectedSquare;

   public const int dimension = 8;

    private int mypiecesCount = 12 ;

    private int opponentPiecesCount = 12;

    private List<Square> reachablesSquares = new List<Square>();
    private void Awake()
    { 
            instance = this;          
    }

 
    public void StartGame(Game game)
    {
        CurrentGame = game;
        string otherPlayerId = CurrentGame.GetOtherPlayerId();
        FireBaseDBManager.GetUserFromID(otherPlayerId, (otherplayer) =>
        {
            opponent = otherplayer;
            UIHandler.instance.ShowOtherPlayerInfo(otherplayer);
        });
        isPlayerOne =  CurrentGame.isPlayerOne(); 
        Debug.Log(isPlayerOne);
        if (isPlayerOne)
        {
            inputBlocker.gameObject.SetActive(false);
            mycolor = playerOneColor;
            opponentColor = playerTwoColor;
        }
        else
        {
            inputBlocker.gameObject.SetActive(true);
            mycolor = playerTwoColor;
            opponentColor = playerOneColor;

        }
        UIHandler.instance.CreateBoard(isPlayerOne);
        UIHandler.instance.ShowGameCanvas();
        if (isPlayerOne)
        {
            StartMyTurn();
        }
        squares= GameObject.FindGameObjectsWithTag(BLACKSQUARETAG);
    }

    public void EndGame()
    {
        if (CurrentGame!=null)
        {
            FireBaseDBManager.DeleteGameFromDB(CurrentGame.roomNumber.ToString());
        }
    }

    public void StartMyTurn()
    {
        inputBlocker.SetActive(false);
    }

    public void EndMyTurn(Move move)
    {
        inputBlocker.SetActive(true);
        FireBaseDBManager.AddMove(move);
    }

    public void ShowPossibleMoves(Square slectedSquare, bool isCrowned)
    {
        this.selectedSquare = slectedSquare;
        Vector2 squarecoordinate = slectedSquare.coordinate;
        int rowStep = (isPlayerOne) ? 1 : -1;
        if (squarecoordinate.y + rowStep >= 0 && squarecoordinate.y + rowStep < dimension)
        {
           
            if (squarecoordinate.x > 0)
            {
                int col = (int)squarecoordinate.x - 1;
                int row = (int)squarecoordinate.y + rowStep;
                CheckifCanGoToThisSquare(new Vector2(col, row),rowStep);
                Debug.Log("checked here");
            }
            if (squarecoordinate.x < dimension-1)
            {
                int col = (int)squarecoordinate.x + 1;
                int row = (int)squarecoordinate.y + rowStep;
                CheckifCanGoToThisSquare(new Vector2(col, row), rowStep);
                Debug.Log("checked here");
            }
        }
        if (isCrowned)
        {
            if (squarecoordinate.y - rowStep >= 0 && squarecoordinate.y - rowStep < dimension)
            {
                if (squarecoordinate.x > 0)
                {
                    int col = (int)squarecoordinate.x - 1;
                    int row = (int)squarecoordinate.y - rowStep;
                    CheckifCanGoToThisSquare(new Vector2(col, row),-rowStep);
                    Debug.Log("checked here");
                }
                if (squarecoordinate.x < dimension-1)
                {
                    int col = (int)squarecoordinate.x + 1;
                    int row = (int)squarecoordinate.y - rowStep;
                    CheckifCanGoToThisSquare(new Vector2(col, row),-rowStep);
                    Debug.Log("checked here");
                }
            }
        }
    }

    private void CheckifCanGoToThisSquare(Vector2 vector , int rowstep)
    {
        Debug.Log("can I go here :"+vector);
        Square square = FindSquare(vector);
        if (square.isFull)
        {
            if (square.isMine)
                return;
            else if(vector.x>0 && vector.x < dimension-1 && vector.y>0 && vector.y<dimension-1)
            {
                Vector2 vectorBehind = new Vector2(vector.x + (vector.x - selectedSquare.coordinate.x),vector.y+rowstep);
                Square squareBehind = FindSquare(vectorBehind);
                if (!squareBehind.isFull)
                { 
                   squareBehind.MakeSquareReachable(square);
                   reachablesSquares.Add(squareBehind);
                }
            }               
        }
        else
        {
            square.MakeSquareReachable(null);
            reachablesSquares.Add(square);
        }
    }

    private  Square FindSquare(Vector2 vector)
    {
        int i = 0;
        //The square GameObjects name must follow this pattern (we named the squares already with board generator script)
        string squarename = "s" + vector.x.ToString("0") + "_" + vector.y.ToString("0");
       
        do
        {
            if (squares[i].name == squarename)
                return  squares[i].GetComponent<Square>();
            else
                i++;
        } while (i < squares.Length);

        //this shouldn't even happen in the first place
        return null;
    }

    public void EmptyReachableSquresList()
    {
        if (reachablesSquares.Count == 0)
            return;
        foreach(Square square in reachablesSquares)
        {
            square.MakeSquareUnReachable();
        }
        reachablesSquares.Clear();
    }

    public void BlockInput()
    {
        inputBlocker.SetActive(true);
    }

    public void PlayOpenentMove(Move move)
    {
        
        Square from = FindSquare(move.from);
        StartCoroutine(from.DissapearPiece());
        if (move.eliminated.x !=-1.0f && move.eliminated.y!=-1.0f)
        {
            ReducePiece(true);
            Square elimnated = FindSquare(move.eliminated);
            StartCoroutine(elimnated.DissapearPiece());
        }
        Square to = FindSquare(move.to);
        StartCoroutine(to.ShowPiece(() => { StartMyTurn(); },!IsPlayerOne,false,from.isCrowned));
    }

    public void ReducePiece(bool mypiece)
    {
        if (mypiece)
        {
            mypiecesCount -= 1;
            if (mypiecesCount <= 0)
            {
                int newscore = CurrentUser.score - 1;
                FireBaseDBManager.SetScore(CurrentUser.uid, newscore);
                UIHandler.instance.ShowResult(false,newscore);
            }
        }
        else
        {
           opponentPiecesCount -= 1;
            if (opponentPiecesCount <= 0)
            {
                int newscore = CurrentUser.score + 1;
                FireBaseDBManager.SetScore(opponent.uid, newscore);
                UIHandler.instance.ShowResult(true,newscore);
            }
        }
    }    
 }
