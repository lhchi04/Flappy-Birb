using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdScript : MonoBehaviour
{
    public Rigidbody2D myRigidBody;
    public float flapStrength;
    public GameLogicScript logic;
    public bool birdIsAlive = true;
    public float upperDeadZone = 15;
    public float lowerDeadZone = -15;

    // Start is called before the first frame update
    void Start()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameLogicScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && birdIsAlive) {
            myRigidBody.velocity = Vector2.up * flapStrength;
        }
        if (transform.position.y > upperDeadZone || transform.position.y < lowerDeadZone) {
            logic.GameOver();
            birdIsAlive = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (birdIsAlive) {
            logic.GameOver();
            birdIsAlive = false;
        }
    }
}
