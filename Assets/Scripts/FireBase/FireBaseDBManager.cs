using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System;
using Random = UnityEngine.Random;

public static class FireBaseDBManager
{

    static DatabaseReference rootReference;
    #region nodes
    const string PLAYERSNODE = "Players/";
    const string GAMESNODE = "Games/";
    #endregion
  
    
    public static void InitDB()
    {

        rootReference = FirebaseDatabase.DefaultInstance.RootReference;
    }


    #region Players Data
    public static void  IsNewPlayer(string uid , Action<bool> callback)
{
    Debug.Log(PLAYERSNODE + uid);
    FirebaseDatabase.DefaultInstance
  .GetReference(PLAYERSNODE+uid)
  .GetValueAsync().ContinueWithOnMainThread(task => {
      
      if (task.IsFaulted)
      {
          // Handle the error...
          Debug.LogError(task.Exception);
      }
      else if (task.IsCompleted)
      {
          callback((!task.Result.Exists));
      }
  });

}
public static void GetScore(string uid , Action<int> callback)
{
      FirebaseDatabase.DefaultInstance
      .GetReference(PLAYERSNODE + uid + "/score")
      .GetValueAsync().ContinueWithOnMainThread(task =>
      { 
               if (task.IsCompleted)
              {
              int score = Int32.Parse( task.Result.Value.ToString());
                  Debug.Log(score);
                  callback(score);

              }
      }); 
}

public static void SetScore(string uid,int score)
    {
        FirebaseDatabase.DefaultInstance.GetReference(PLAYERSNODE + uid + "/score").SetValueAsync(score);
    }
public static void SaveNewUserToDB(User user)
{

    Dictionary<string, object> entryValues = user.ToDictionary();

    Dictionary<string, object> childUpdates = new Dictionary<string, object>();

    childUpdates[PLAYERSNODE + user.uid] = entryValues;
        rootReference.UpdateChildrenAsync(childUpdates).ContinueWithOnMainThread(task =>
       {
           if (task.IsFaulted)
           {
               Debug.Log(task.Exception);
           }
           else if (task.IsCompleted)
           {
               Debug.Log("succsess");
           }
       });   
}
public static void GetUserFromID(string uid, Action<User>CallBack) 
{

        try
        {
            rootReference.Child(PLAYERSNODE + uid).GetValueAsync().ContinueWithOnMainThread(task =>
            {
               

                if (task.IsFaulted)
                {
                    Debug.LogError(task.Exception);
                }
                else if (task.IsCompleted)
                {
                        string userData = task.Result.GetRawJsonValue();
                        Debug.Log(userData);
                        User user = StringSerializationAPI.Deserialize(typeof(User), userData) as User;                        
                        Debug.Log(user.score);
                    Debug.Log(user.photoUrl);
                        user.uid = uid;
                        CallBack(user);      
                }
            });
        }catch(Exception e)
        {
            Debug.Log(e);
        }
}

    
    static IEnumerator  wait(int i)
    {
        yield return new WaitForSeconds(i);
        GetUserFromID(GameManager.instance.CurrentUser.uid, (user) => { Debug.Log(user.displayName); });
    }
    public static void UpdateUserLinkedToFb(User user)
{
    Dictionary<string, object> entryValues = user.UpdateUserWithFbInfo();
    Dictionary<string, object> childUpdates = new Dictionary<string, object>();
    childUpdates[PLAYERSNODE + user.uid] = entryValues;
    rootReference.UpdateChildrenAsync(childUpdates).ContinueWithOnMainThread(task =>
    {
        if (task.IsFaulted)
        {
            Debug.Log(task.Exception);
        }
        else if (task.IsCompleted)
        {
            Debug.Log("succsess");
        }
    });
}
    #endregion

    #region Game Data
    public static void CreateNewGame()
    {

        Game newGame = new Game(GameManager.instance.CurrentUser.uid);
      
        //Had to assign the newGame instane early here to the GameManager cos I couldn't add my own arguments to the ValueChanged listener below
        GameManager.instance.CurrentGame = newGame;
        

        Dictionary<string, object> entryValues = newGame.ToDictionary();

        Dictionary<string, object> childUpdates = new Dictionary<string, object>();

        childUpdates[GAMESNODE + newGame.roomNumber] = entryValues;

        rootReference.UpdateChildrenAsync(childUpdates).ContinueWithOnMainThread(task =>
        {
        if (task.IsCompleted)
        {
            string roomNumber = newGame.roomNumber;
            UIHandler.instance.ShowRoomNumber(roomNumber);
            //listen to "playersId/1" node to get notified when new player joins 
            rootReference.Child(GAMESNODE  + roomNumber + "/playersId/1").ValueChanged += NewPlayerJoined;
            rootReference.Child(GAMESNODE  + roomNumber + "/moves").ChildAdded += NotifyMeNewMove;
         }
        });
    }
    
    private static void NewPlayerJoined(object sender, ValueChangedEventArgs args )
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        else
        {
            string secondPlayerId = args.Snapshot.Value.ToString();
            if (secondPlayerId == Game.PLAYERIDPLACEHOLDER)
                return;
            Debug.Log(secondPlayerId);
            //add the  Second player ID
            GameManager.instance.CurrentGame.playersId[1] = args.Snapshot.Value.ToString(); 
            //We Already assigned the Game instance to the GameManager
            GameManager.instance.StartGame(GameManager.instance.CurrentGame);
        }
    }

    private static void NotifyMeNewMove(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        else
        {
            string json = args.Snapshot.GetRawJsonValue();
           
            Move move = StringSerializationAPI.Deserialize(typeof(Move), json) as Move;
            if (move.platerId != GameManager.instance.CurrentUser.uid)
            {
                GameManager.instance.PlayOpenentMove(move);
            }
        }
    }
    public static void DeleteGameFromDB(string roomNumber)
    {
        
        rootReference.Child(GAMESNODE + roomNumber).RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if(task.IsCompleted)
            Debug.Log("Deleted");
        });
    }

    public static void JoinGame (string roomNumber)
    {
        rootReference.Child(GAMESNODE + roomNumber).GetValueAsync().ContinueWithOnMainThread(Task =>
        {
            if (Task.IsCompleted)
            {
                if (Task.Result.Exists)
                {
                    string playerId = GameManager.instance.CurrentUser.uid;
                    // "/1" is the key of second playerId in fireBase  
                    rootReference.Child(GAMESNODE + roomNumber + "/playersId/1").SetValueAsync(playerId).ContinueWithOnMainThread(Task1 =>
                    {
                        DownloadGameData(roomNumber);
                        rootReference.Child(GAMESNODE + "/" + roomNumber + "/moves").ChildAdded += NotifyMeNewMove;
                    });
                }
                else
                {
                 
                    UIHandler.instance.ShowRoomNotFoundMsg(roomNumber);
                }
            }          
        });      
    }
public static void DownloadGameData(string roomnumber)
{

        rootReference.Child(GAMESNODE + roomnumber).GetValueAsync().ContinueWithOnMainThread((Action<System.Threading.Tasks.Task<DataSnapshot>>)(task =>
        {
        if (task.IsCompleted)
        {
            string gameData = task.Result.GetRawJsonValue();
            Game game = StringSerializationAPI.Deserialize(typeof(Game), gameData) as Game;
            game.roomNumber = task.Result.Key.ToString();
         
            GameManager.instance.StartGame(game);
        }
    }));
}

    public static void AddMove(Move move)
    {

        string node = GAMESNODE  + GameManager.instance.CurrentGame.roomNumber + "/moves/";
        string key = rootReference.Child(node).Push().Key;
        node +=   key;
        string json = StringSerializationAPI.Serialize(typeof(Move), move);
        rootReference.Child(node).SetRawJsonValueAsync(json);
           
    }
     
#endregion
}
