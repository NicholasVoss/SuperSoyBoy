using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Animator))]

public class SoyBoyController : MonoBehaviour
{
    //vars for movement
    public float speed = 14f;
    public float accel = 6f;
    private Vector2 input;

    //game object components
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Animator animator;

    void Awake()
    {
        //find components
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //check for player input
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Jump");

        //if player is moving right, face right
        if(input.x > 0f)
        {
            sr.flipX = false;
        }
        else if (input.x < 0f) //if player is moving left, face left
        {
            sr.flipX = true;
        }
    }

    void FixedUpdate()
    {

        var acceleration = accel;
        var xVelocity = 0f;

        //if player isn't trying to move, set speed to 0
        if(input.x == 0)
        {
            xVelocity = 0f;
        }
        else //if player is trying to move, set speed to the velocity of the rigidbody
        {
            xVelocity = rb.velocity.x;
        }

        //add force to rigidbody
        rb.AddForce(new Vector2(((input.x * speed) - rb.velocity.x) * acceleration, 0));

        //set rigidbody's velocity
        rb.velocity = new Vector2(xVelocity, rb.velocity.y);
    }
}
