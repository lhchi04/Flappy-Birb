using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using System;
using System.Threading.Tasks;
using Firebase.Database;
using System.Linq;

public class StartLogicScript : MonoBehaviour
{
    public bool userLoggedIn;
	public GameObject loginBtn;
	public GameObject userBtn;
	public GameObject scoreboardBtn;
	[Header("Firebase")]
    private DatabaseReference dbReference;

	public FirebaseUser User;
	[Header("ScoreboardData")]
    public GameObject scoreboardUI;
    public GameObject scoreElement;
    public Transform scoreboardContent;

	void Start() {
		userLoggedIn = UserControl.control.loggedIn;
        if (userLoggedIn) {
            loginBtn.SetActive(false);
			scoreboardBtn.SetActive(true);
			userBtn.SetActive(true);
        }
        dbReference = UserControl.control.dbReference;

    }
	public void StartGame() {
		SceneManager.LoadScene(1);
	}
	public void UserButton() {
		SceneManager.LoadScene(2);
	}
	//Function for the scoreboard button
    public void ScoreboardButton() {        
        StartCoroutine(LoadScoreboardData());
		scoreboardUI.SetActive(true);
		userBtn.SetActive(false);
		scoreboardBtn.SetActive(false);
    }
    public void BackButton() {
		scoreboardUI.SetActive(false);
		userBtn.SetActive(true);
		scoreboardBtn.SetActive(true);
    }
	public void signOutBtn() {
        UserControl.control.SignOutButton();
	}
	public void Exit() {
		Application.Quit();
	}
	private IEnumerator LoadScoreboardData() {
        //Get all the users data ordered by score
        Task<DataSnapshot> DBTask = dbReference.Child("users").OrderByChild("score").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        } else {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            //Destroy any existing scoreboard elements
            foreach (Transform child in scoreboardContent.transform) {
                Destroy(child.gameObject);
            }

            //Loop through every users UID
            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>()) {
                string username = childSnapshot.Child("username").Value.ToString();
                int score = int.Parse(childSnapshot.Child("score").Value.ToString());

                //Instantiate new scoreboard elements
                GameObject scoreboardElement = Instantiate(scoreElement, scoreboardContent);
                scoreboardElement.GetComponent<ScoreElement>().NewScoreElement(username, score);
            }
        }
    }
}
