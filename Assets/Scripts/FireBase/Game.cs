using System.Collections;
using System.Collections.Generic;

using FullSerializer;

public class Game
{
    public string roomNumber;
    [fsProperty]
    public string[] playersId = new string[2];
    [fsProperty]
    public const string PLAYERIDPLACEHOLDER = "NA";

    public Game(string playerOneId)
    {
        this.roomNumber = UnityEngine.Random.Range(0, 10000).ToString("D4");
        playersId[0] = playerOneId;
        playersId[1] = PLAYERIDPLACEHOLDER;     
    }

    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        
        result["playersId"] = playersId;      
        result["moves"] = "N/A";
        return result;
    }

    public string GetOtherPlayerId()
    {
        if (GameManager.instance.CurrentUser.uid != playersId[0])
            return playersId[0];
        else
            return playersId[1];
    }

    public bool isPlayerOne() 
    {
        return (GameManager.instance.CurrentUser.uid == playersId[0]);
       
    }
}
