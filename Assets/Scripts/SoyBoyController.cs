﻿using System.Collections;
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

    public bool isJumping;
    public float jumpSpeed = 8f;
    private float rayCastLengthCheck = 0.005f;
    private float width;
    private float height;
    public float jumpDurationThreshold = 0.25f;
    private float jumpDuration;

    void Awake()
    {
        //find components
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        //get size of soyboy's sprite
        width = GetComponent<Collider2D>().bounds.extents.x + 0.1f;
        height = GetComponent<Collider2D>().bounds.extents.y + 0.2f;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public bool PlayerIsOnGround()
    {
        //raycast to see if player is touching the ground
        bool groundCheck1 = Physics2D.Raycast(new Vector2(
            transform.position.x, transform.position.y - height),
            -Vector2.up, rayCastLengthCheck);
        bool groundCheck2 = Physics2D.Raycast(new Vector2(
            transform.position.x + (width - 0.2f),
            transform.position.y - height), -Vector2.up,
            rayCastLengthCheck);
        bool groundCheck3 = Physics2D.Raycast(new Vector2(
            transform.position.x - (width - 0.2f),
            transform.position.y - height), -Vector2.up,
            rayCastLengthCheck);

        if(groundCheck1 || groundCheck2 || groundCheck3)
        {
            return true;
        }
        else
        {
            return false;
        }
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

        if(input.y >= 1f)
        {
            jumpDuration += Time.deltaTime;
        }
        else
        {
            isJumping = false;
            jumpDuration = 0;
        }

        if(PlayerIsOnGround() && !isJumping)
        {
            if(input.y > 0f)
            {
                isJumping = true;
            }
        }

        if(jumpDuration > jumpDurationThreshold)
        {
            input.y = 0f;
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

        if(isJumping && jumpDuration < jumpDurationThreshold)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        }
    }
}