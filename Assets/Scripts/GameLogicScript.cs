using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Threading.Tasks;
using System.Linq;

public class GameLogicScript : MonoBehaviour
{
    [Header("In Game")]
    public int playerScore;
    public int playerHighScore;
    public GameObject scoreTextUI;
    public GameObject highScoreTextUI;
    public Text scoreText;
    public Text highScoreText;
    [Header("Game Over")]
    public TMP_Text newRecord;
    public GameObject gameOverUI;
    public GameObject leaderboardBtn;
    public GameObject signOutBtn;
    [Header("Audio")]
    public AudioSource ding;
    public AudioSource lose;
    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser User;
    private DatabaseReference dbReference;
    //User Data variables
    [Header("UserData")]
    public GameObject scoreboardUI;
    public GameObject scoreElement;
    public Transform scoreboardContent;

    void Awake() {
        dbReference = UserControl.control.dbReference;
        auth = UserControl.control.auth;
        User = auth.CurrentUser;
		if (User != null) {
			Debug.Log("Hello in game " + User.DisplayName);
            StartCoroutine(LoadUserData());
		}
    }
    [ContextMenu("Increase Score")]
    public void AddScore(int scoreToAdd) {
        playerScore += scoreToAdd;
        scoreText.text = playerScore.ToString();
        ding.Play();
    }
    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void GameOver() {
        lose.Play();
        gameOverUI.SetActive(true);
        if (User == null) {
            signOutBtn.SetActive(false);
            leaderboardBtn.SetActive(false);
        } else {
            if (playerScore > playerHighScore) {
                StartCoroutine(UserControl.control.UpdateScore(playerScore));
                newRecord.text = "New Record!";
            }
        }
        highScoreTextUI.SetActive(false);
    }
    //Function for the sign out button
    public void SignOutButton() {
        UserControl.control.SignOutButton();
    }
    //Function for the scoreboard button
    public void ScoreboardButton() {        
        StartCoroutine(LoadScoreboardData());
        gameOverUI.SetActive(false);
        scoreTextUI.SetActive(false);
        scoreboardUI.SetActive(true);
    }
    public void BackButton() {
        scoreboardUI.SetActive(false);
        scoreTextUI.SetActive(true);
        gameOverUI.SetActive(true);
    }
    public void UserButton() {
        SceneManager.LoadScene(2);
    }
    private IEnumerator LoadUserData() {
        yield return new WaitForSeconds(1);
        //Get the currently logged in user data
        Task<DataSnapshot> DBTask = dbReference.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        } else if (DBTask.Result.Value == null) { //No data exists yet
            highScoreText.text = "0";
        } else { //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            highScoreText.text = snapshot.Child("score").Value.ToString();
            playerHighScore = int.Parse(highScoreText.text);
        }
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
