using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FirebaseInitializer : MonoBehaviour
{


    

    private void Awake()
    {
        StartCoroutine(CheckAndFixDependenciesCoroutine());
    }

    private IEnumerator CheckAndFixDependenciesCoroutine()
    {
        var checkDependenciesTask = Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => checkDependenciesTask.IsCompleted);

        var dependencyStatus = checkDependenciesTask.Result;
        if (dependencyStatus == Firebase.DependencyStatus.Available)
        {
            Debug.Log($"Firebase: {dependencyStatus} :)");
            FireBaseDBManager.InitDB();
            FireBaseAuthentification.InitAuth();
        }
        else
        {
            Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            // Firebase Unity SDK is not safe to use here.
        }
    }
}
