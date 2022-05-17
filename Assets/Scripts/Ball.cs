using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class Ball : NetworkBehaviour
{
    [SerializeField] float speed = 30f;
    public Rigidbody2D rb;
    NetworkManagerPong networkManager;
    public int leftScore;
    public int rightScore;
    public TMP_Text textScore;


    IEnumerator StartBall()
    {
        rb.simulated = false;
        rb.velocity = Vector2.zero;
        transform.position = Vector2.zero;

        yield return new WaitForSeconds(2);
        rb.simulated = true;
        float randomDirection = Random.Range(0f, 1f) > 0.5 ? 1 : -1;
        rb.velocity = Vector2.right * speed * randomDirection;
    }
    // Start is called before the first frame update
    public override void OnStartServer()
    {
        
        StartCoroutine(StartBall()); 
        networkManager = FindObjectOfType<NetworkManagerPong>();
        
    }

    void Start()
    {
        
        textScore = FindObjectOfType<TMP_Text>();
    }

    float HitFactor(Vector2 ballPosition, Vector2 racketPosition, float racketHeight)
    {
        return (ballPosition.y - racketPosition.y) / racketHeight;
    }

    [ServerCallback]
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            rb.velocity = Vector2.Reflect(rb.velocity, collision.contacts[0].normal).normalized * speed;
            float y = HitFactor(transform.position, collision.transform.position, collision.collider.bounds.size.y);
            float x = collision.relativeVelocity.x > 0 ? 1 : -1;

            Vector2 dir = new Vector2(x, y).normalized;
            rb.velocity = dir * speed;

        }
    }

    [ServerCallback]
    void Update()
    {
        if (transform.position.x > networkManager.rightRacketSpawn.position.x)
        {
            StartCoroutine(StartBall());
            leftScore++;
            RpcUpdateTextScore(leftScore, rightScore);

        }
        else if (transform.position.x < networkManager.leftRacketSpawn.position.x)
        {
            StartCoroutine(StartBall());
            rightScore++;
            RpcUpdateTextScore(leftScore, rightScore);
        }
    }

    [ClientRpc]
    public void RpcUpdateTextScore(int leftScore, int rightScore) {
        textScore.text = $"{leftScore} - {rightScore}";
    }
}
