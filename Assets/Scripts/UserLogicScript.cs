using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using UnityEngine.EventSystems;

public class UserLogicScript : MonoBehaviour
{
    EventSystem system;
    public Selectable firstInput;
    public bool userLoggedIn;
    public GameObject loginUI;
    public GameObject userDataUI;
    //Firebase variables
    [Header("Firebase")]
    public FirebaseAuth auth;    
    public FirebaseUser User;
    private DatabaseReference dbReference;
    //Login variables
    [Header("Login")]
    public InputField emailLoginField;
    public InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;
    //Register variables
    [Header("Register")]
    public InputField usernameRegisterField;
    public InputField emailRegisterField;
    public InputField passwordRegisterField;
    public InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;
    //User Data variables
    [Header("UserData")]
    public InputField usernameField;
    public InputField scoreField;
    [Header("ScoreboardData")]
    public GameObject scoreboardUI;
    public GameObject scoreElement;
    public Transform scoreboardContent;

    void Awake() {
        auth = UserControl.control.auth;
        dbReference = UserControl.control.dbReference;
        userLoggedIn = UserControl.control.loggedIn;
        User = auth.CurrentUser;
		if (User != null) {
			Debug.Log("Hello " + User.DisplayName);
            usernameField.text = User.DisplayName;
            StartCoroutine(LoadUserData());
		}
    }
    void Start() {
        system = EventSystem.current;
        firstInput.Select();
        if (userLoggedIn) {
            UIManager.instance.UserDataScreen();
        }
    }
    void Update() {
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift)) {
            Selectable previous = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
            if (previous != null) {
                previous.Select();
            }
        } else if (Input.GetKeyDown(KeyCode.Tab)) {
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            if (next != null) {
                next.Select();
            }
        }
    }
    public void ClearLoginFields() {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }
    public void ClearRegisterFields() {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }

    //Function for the login button
    public void LoginBtn() {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    //Function for the register button
    public void RegisterBtn() {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }
    //Function for the sign out button
    public void SignOutBtn() {
        UserControl.control.SignOutButton();
        ClearRegisterFields();
        ClearLoginFields();
    }
    //Function for the save button
    public void SaveDataBtn() {
        StartCoroutine(UpdateUsernameAuth(usernameField.text));
        StartCoroutine(UpdateUsernameDatabase(usernameField.text));
        StartCoroutine(UserControl.control.UpdateScore(int.Parse(scoreField.text)));
    }
    //Function for the scoreboard button
    public void ScoreboardBtn() {        
        StartCoroutine(LoadScoreboardData());
        UIManager.instance.ScoreboadScreen();
    }
    public void BackBtn() {
        UIManager.instance.UserDataScreen();
    }
    public void BackBtnToHome() {
        SceneManager.LoadScene(0);
    }
    private IEnumerator Login(string _email, string _password) {
        //Call the Firebase auth signin function passing the email and password
        Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null) {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode) {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            warningLoginText.text = message;
        } else {
            //User is now logged in
            //Now get the result
            Debug.Log(LoginTask.Result);
            User = LoginTask.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";
            
            // StartCoroutine(LoadUserData());

            UserControl.control.loggedIn = true;
            yield return new WaitForSeconds(1);
            SceneManager.LoadScene(0);
            confirmLoginText.text = "";
            ClearLoginFields();
            ClearRegisterFields();
        }
    }
    private IEnumerator Register(string _email, string _password, string _username) {
        if (_username == "") {
            //If the username field is blank show a warning
            warningRegisterText.text = "Missing Username";
        } else if (passwordRegisterField.text != passwordRegisterVerifyField.text) {
            //If the password does not match show a warning
            warningRegisterText.text = "Password Does Not Match!";
        } else {
            //Call the Firebase auth signin function passing the email and password
            Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null) {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode) {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                warningRegisterText.text = message;
            } else {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result.User;

                if (User != null) {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile{DisplayName = _username};

                    //Call the Firebase auth update user profile function passing the profile with the username
                    Task ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null) {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "Username Set Failed!";
                    } else {
                        //Username is now set
                        //Now return to login screen
                        StartCoroutine(UpdateUsernameDatabase(User.DisplayName));
                        StartCoroutine(UserControl.control.UpdateScore(0));
                        UIManager.instance.LoginScreen();
                        warningRegisterText.text = "";
                        ClearLoginFields();
                        ClearRegisterFields();
                    }
                }
            }
        }
    }
    private IEnumerator UpdateUsernameAuth(string _username) {
        //Create a user profile and set the username
        UserProfile profile = new UserProfile { DisplayName = _username };
        Debug.Log(User);
        //Call the Firebase auth update user profile function passing the profile with the username
        Task ProfileTask = User.UpdateUserProfileAsync(profile);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

        if (ProfileTask.Exception != null) {
            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
        } else {
            //Auth username is now updated
        }        
    }
    private IEnumerator UpdateUsernameDatabase(string _username) {
        //Set the currently logged in user username in the database
        Task DBTask = dbReference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        } else {
            //Database username is now updated
        }
    }
    private IEnumerator LoadUserData() {
        //Get the currently logged in user data
        Task<DataSnapshot> DBTask = dbReference.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        } else if (DBTask.Result.Value == null) { //No data exists yet
            scoreField.text = "0";
        } else { //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            scoreField.text = snapshot.Child("score").Value.ToString();
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