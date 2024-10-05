using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserControl : MonoBehaviour
{
    public static UserControl control;
    public bool loggedIn;
    //Firebase variables
    [Header("Firebase")]
    public FirebaseAuth auth;
    public DatabaseReference dbReference;
    public FirebaseUser User;
    
    void Awake()
    {
        if (control == null) {
            DontDestroyOnLoad(gameObject);
            control = this;
        } else if (control != this) {
            Destroy(gameObject);
        }
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        User = auth.CurrentUser;
        if (User != null) {
            loggedIn = true;
        } else {
            loggedIn = false;
        }
        Debug.Log("control " + loggedIn);
    }
    public void SignOutButton() {
        auth.SignOut();
        loggedIn = false;
        SceneManager.LoadScene(0);
    }
    public IEnumerator UpdateScore(int _score) {
        //Set the currently logged in user score
        User = auth.CurrentUser;
        Task DBTask = dbReference.Child("users").Child(User.UserId).Child("score").SetValueAsync(_score);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        } else {
            //Score is now updated
        }
    }
    
}
