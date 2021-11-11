using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[SerializeField]
public enum Coordinate
{
    s0_0,
    s2_0,
    s4_0,
    s6_0,
    s1_1,
    s3_1,
    s5_1,
    s7_1,
    s1_2,
    s3_2,
    s5_2,
    s7_2,
    s2_3,
    s4_3,
    s6_3,
    s0_4,
    s2_4,
    s4_4,
    s6_4,
    s1_5,
    s3_5,
    s5_5,
    s7_5,
    s1_6,
    s3_6,
    s5_6,
    s7_6,
    s2_7,
    s4_7,
    s6_7,
}
public class Square : MonoBehaviour
{
   // private Vector2 coordinate;
   // private Coordinate coordinate;
   // public Coordinate Scoordinate { set { coordinate = (Coordinate)Enum.Parse(typeof(Coordinate), gameObject.name); } get { return coordinate; } }

    public bool isFull = false;

    public bool isMine = false;

    public bool isCrowned = false;
  
    public  Vector2 coordinate;
    public  bool canGoToThisSquare=false;
    //Make sure it's same color set in the prefab  blackSquare;
    private Color notAvailableToMoveColor = Color.black;
    private Color availableToMoveColor = Color.green;
    private Animator anim;
    //animators parameters names
    private const string ANIMATORELEMINATE = "eliminate";
    private const string ANIMATORCREATE = "create";
    //piece to eliminate if the player move to this square
    private Square pieceToEliminate;

    private Action CallEndTurn;
    void Start ()
    {
        anim = GetComponentInChildren(typeof (Animator) , true) as Animator;

    }

    public void SelectSquare()
    {
        if (isMine) {
            GameManager.instance.EmptyReachableSquresList();
            GameManager.instance.ShowPossibleMoves(this, isCrowned);
        }
            
        else if (canGoToThisSquare)
        {
            Square previousSelectedSquare = GameManager.instance.selectedSquare;
            Vector2 eliminateCord;
            if(pieceToEliminate)
            eliminateCord =    pieceToEliminate.coordinate;           
            else
                //will always send Vecotr2 to eliminated to keep constincy but will check later if the value is real or fake (-1,-1)
             eliminateCord=    new Vector2(-1,-1);
            Debug.Log("Called");
            Move move = new Move(GameManager.instance.CurrentUser.uid, previousSelectedSquare.coordinate, coordinate, eliminateCord);
             CallEndTurn = () => {GameManager.instance.EndMyTurn(move);};
            MovePieceToSquare(previousSelectedSquare);
            GameManager.instance.EmptyReachableSquresList();
            GameManager.instance.BlockInput();
        }
            
    }

    public Dictionary<string,string> CreatedMoveDictionary(Vector2 from , Vector2 to , Vector2 eliminatedPos)
    {
        
        Dictionary<string, string> dic = new Dictionary<string, string>();
        dic["From"] = StringSerializationAPI.Serialize(typeof(Vector2),from);
        dic["To"] = StringSerializationAPI.Serialize(typeof(Vector2), to);
        dic["Eliminated"] = StringSerializationAPI.Serialize(typeof(Vector2), eliminatedPos);
        dic["PlayerId"] = GameManager.instance.CurrentUser.uid;
        return dic;
    }
    
    private void MovePieceToSquare(Square from)
    {
     
        StartCoroutine(from.DissapearPiece());
        if (pieceToEliminate!=null)
        {
            StartCoroutine(pieceToEliminate.DissapearPiece());
            GameManager.instance.ReducePiece(false);
        }
        StartCoroutine(ShowPiece(CallEndTurn,GameManager.instance.IsPlayerOne,true,from.isCrowned)) ;
    }

    public void MakeSquareReachable(Square pieceToEliminate)
    {
        canGoToThisSquare = true;
        GetComponent<Image>().color = availableToMoveColor;
        if (pieceToEliminate)
        {
            this.pieceToEliminate = pieceToEliminate;
            Debug.Log("GM = " + gameObject.name + "||" + this.pieceToEliminate.gameObject.name);
        }
    }
        
    public void MakeSquareUnReachable()
    {
        canGoToThisSquare = false;
        GetComponent<Image>().color = notAvailableToMoveColor;
        
        pieceToEliminate = null;
    }

   //callback action to call function just after the showPiece coroutine finish.
    public IEnumerator ShowPiece(Action callback,bool isplayerOne, bool isMine ,bool isCrowned)
    {
        GameObject piece = transform.GetChild(0).gameObject;
        piece.transform.localScale = Vector3.zero;
        piece.SetActive(true);
        if(isMine)
            piece.GetComponent<Image>().color = GameManager.instance.mycolor;
        else
            piece.GetComponent<Image>().color = GameManager.instance.opponentColor;
        anim.SetTrigger(ANIMATORCREATE);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);  
        isFull = true;
        this.isMine = isMine;
        if((isplayerOne && coordinate.y==GameManager.dimension-1) || (!
            isplayerOne && coordinate.y == 0) || isCrowned)
        {
            this.isCrowned = true;
            piece.transform.GetChild(0).gameObject.SetActive(true);
        }
        if(callback!=null)
        callback.Invoke();
    }

   public IEnumerator  DissapearPiece()
    {
        anim.SetTrigger(ANIMATORELEMINATE);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        Transform piece = transform.GetChild(0);
        piece.GetChild(0).gameObject.SetActive(false);
        piece.gameObject.SetActive(false);     
        isFull = false;
        isMine = false;
        isCrowned = false;
    }
        

 }

 
   



