using FullSerializer;
using System;
using System.Collections.Generic;
using UnityEngine;
public class Move 
{
    public string Id;

    public string platerId;

    public Vector2 from;

    public Vector2 to;
   
   public Vector2 eliminated;

   public bool isCrowned;



    public Move(string playerid , Vector2 from , Vector2 to, Vector2 eliminated)
    {
       
        this.platerId = playerid;
        this.from = from;
        this.to = to;
        this.eliminated = eliminated;
    }

    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        result["from"] = from ;
        result["to"] = to;
        result["playerId"] = platerId;
        result["eliminated"] = eliminated;
        return result;
    }
}
