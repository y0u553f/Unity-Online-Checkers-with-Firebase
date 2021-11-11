using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class BoardGenerator : EditorWindow
{

    public static GameObject blackSquare;
    public static GameObject whiteSquare;
    public static Transform board;
    public static int dimension;
    public static bool isPlayerOne;
    public static Color playerOneColor, playerTwoColor;

    [MenuItem("GameObject/UI/GenerateBoard")]
    static void Create()
    {
        GetWindow<BoardGenerator>("Board Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("create board squares", EditorStyles.boldLabel);
        blackSquare = (GameObject)EditorGUILayout.ObjectField("Black Square prefab", blackSquare, typeof(GameObject), true);
        whiteSquare = (GameObject)EditorGUILayout.ObjectField("White Square prefab", whiteSquare, typeof(GameObject), true);
        board = (Transform)EditorGUILayout.ObjectField("board Transform", board, typeof(Transform), true);
        dimension = EditorGUILayout.IntField("Board Dimension", dimension);
        isPlayerOne = EditorGUILayout.Toggle("is this playerOne's board ?", isPlayerOne);
        playerOneColor = EditorGUILayout.ColorField("PlayerOne's pieces color", playerOneColor);
        playerTwoColor = EditorGUILayout.ColorField("PlayerTwo's pieces color", playerTwoColor);
        if (GUILayout.Button("Create"))
        {
            if (isPlayerOne)
                CreatePlayerOneBoard();
            else
                CreatePlayerTwoBoard();
        }
    }



    public void CreatePlayerOneBoard()
    {
        bool isBlackSquare = true;
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                SetUpSquare(i, j, isBlackSquare);
                isBlackSquare = !isBlackSquare;
            }
            isBlackSquare = !isBlackSquare;
        }
    }

    public void CreatePlayerTwoBoard()
    {
        bool isBlackSquare = true;
        for (int i = 7; i >= 0; i--)
        {
            for (int j = 7; j >= 0; j--)
            {
                SetUpSquare(i, j, isBlackSquare);
                isBlackSquare = !isBlackSquare;
            }
            isBlackSquare = !isBlackSquare;
        }
    }

    public void SetUpSquare(int i, int j, bool isBlackSquare)
    {

        if (isBlackSquare)
        {
            GameObject gm = Instantiate(blackSquare, board);
            gm.name = string.Format("s{0}_{1}", j, i);
            Square square = gm.GetComponent<Square>();

            if (i < 3 || i > 4)
            {
                GameObject piece = gm.transform.GetChild(0).gameObject;
                piece.SetActive(true);
                if (i < 3)
                {
                    if (isPlayerOne)
                        square.isMine = true;
                    piece.GetComponent<Image>().color = playerOneColor;
                }
                else
                {
                    if (!isPlayerOne)
                        square.isMine = true;
                    piece.GetComponent<Image>().color = playerTwoColor;
                }
                square.isFull = true;
            }
            square.coordinate = new Vector2(j, i);
        }
        else
        {
            Instantiate(whiteSquare, board);
        }

    }
}