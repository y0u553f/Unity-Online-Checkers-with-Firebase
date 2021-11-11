using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase;
using Firebase.Extensions;
using System;

public static class FireBaseAuthentification
{

    
    private static FirebaseAuth auth;
   
   

    public static void InitAuth()
    {
        try
        {
            auth = FirebaseAuth.DefaultInstance;
            
            if (IsUserAuthentified())
            {
                FireBaseDBManager.GetUserFromID(auth.CurrentUser.UserId, (user) =>
                {

                    SaveCurrentUserInfosInTheGame(user);
                    UIHandler.instance.ShowMainMenu();
                    UIHandler.instance.ShowPlayersInfosInUI(user);

                });
            }
            else
            {
                UIHandler.instance.ShowLoginCanvas();
            }
        }catch(Exception e)
        {
            Debug.Log(e);
        }
    }

    private static bool IsUserAuthentified()
    {
        
        return (auth.CurrentUser!=null);
    }

    public static bool IsAnonymous()
    {
        return auth.CurrentUser.IsAnonymous;
    }

    public static void LoginToFireBaseWithFb(string accessToken)
    {
        Credential credential = FacebookAuthProvider.GetCredential(accessToken);
        Debug.Log("ahowa :" + IsUserAuthentified());
        if(IsUserAuthentified() && auth.CurrentUser.IsAnonymous)
        {
            LinkAccountToFaceBook(credential);
        }
        else 
        {
            LoginToFireBaseWithCredentialFb(credential);
        }
    }

    private static void LoginToFireBaseWithCredentialFb(Credential credential)
    {    
            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }
                Debug.Log("ahowa :" + IsUserAuthentified());  
                SaveCurrentUserInfos(task.Result,false);
            });
    }

    public static void LoginToFireBaseAnnonymous(string displayName)
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
        if (task.IsCanceled)
        {
            Debug.LogError("SignInAnonymouslyAsync was canceled.");
            return;
        }
        if (task.IsFaulted)
        {
            Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
            return;
        }
            Debug.Log("here");
            try
            {
                FirebaseUser newUser = task.Result;
                UserProfile profile = new Firebase.Auth.UserProfile
                {
                    DisplayName = displayName,
                    
                };
            newUser.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task2 =>
            {
                Debug.Log("here");
                SaveCurrentUserInfos(newUser,true);
            });

            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

        });
    }

    public static  void LinkAccountToFaceBook(Credential credential)
    {
      
        auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("LinkWithCredentialAsync was canceled.");
                return;
            }
            else
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("LinkWithCredentialAsync encountered an error: " + task.Exception);
                    return;
                }
                else if (task.IsCompleted)
                {
                    UIHandler.instance.ShowSettingsCanvas();
                }
            }

        });
    }



    //store the currentUser infos on the DataBase if it's the first time he logins and call method to save player's info in the device memory at runTime         
    private static void SaveCurrentUserInfos(FirebaseUser auth , bool annonymous )
    {
       
            // User Who has no Profile Pic  (Created Account Anonymously) their photoUrl cause NullReference Exception
            string photurl = (annonymous) ? User.NOPHOTO : auth.PhotoUrl.ToString();
            User user = new User(auth.DisplayName, auth.UserId, photurl, 0);
            FireBaseDBManager.IsNewPlayer(user.uid, (isNewPlayer) =>
                {

                    if (isNewPlayer)
                    {
                        FireBaseDBManager.SaveNewUserToDB(user);
                        Debug.Log("new player");
                    }
                    else
                    FireBaseDBManager.GetScore(user.uid, (score) => user.score = score);
                    SaveCurrentUserInfosInTheGame(user);
                    UIHandler.instance.ShowMainMenu();
                    UIHandler.instance.ShowPlayersInfosInUI(user);

                });
        
    }
    //save player's info in the device memory at runTime
    private  static void SaveCurrentUserInfosInTheGame(User user)
    { 
            GameManager.instance.CurrentUser = user;
    }
    public static void SignOut()
    {
        auth.SignOut();
    }
}
