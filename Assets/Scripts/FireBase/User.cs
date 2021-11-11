using System.Collections;
using System.Collections.Generic;

using System;
using FullSerializer;
public class User
{

    public string displayName;
    [fsIgnore]
    public string uid;

    public string photoUrl;

    public int score;

    public const string NOPHOTO= "NOPHOTO";
    public Dictionary<string, object> ToDictionary()
    {
      
            
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["displayName"] = displayName;
            result["score"] = 0;
            result["photoUrl"] = photoUrl;     
            return result;   
    }


   public Dictionary<string , object> UpdateUserWithFbInfo()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        result["DisplayName"] = displayName;
        result["PhotoUrl"] = photoUrl;
        return result;
    }
    public User(string DisplayName,string Uid,string PhotoUrl,int score)
    {
      
        if (DisplayName != null)
            this.displayName = DisplayName;
        else
            this.displayName = "";
        this.uid = Uid;
        if (PhotoUrl != null)
            this.photoUrl = PhotoUrl;
        else
            this.displayName = DisplayName;
        this.score = score;
    }

   
}
