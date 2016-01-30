﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using System;

public class PlayerMovement : NetworkBehaviour {

	public GameObject Player;
    public GameObject Fireball;

    private float TimeSinceLastFire;
    public const float FireballCooldown = 0.5f;

    public bool FacingLeft;

	public List<InputAction> InputActions;

	public const float leftThreshold = -0.1f;
	public const float rightThreshold = 0.1f;

    public const string IdleLeftAnimation = "IdleLeft";
    public const string IdleRightAnimation = "IdleRight";
    public const string RunLeftAnimation = "RunLeft";
    public const string RunRightAnimation = "RunRight";
    public const string JumpLeftAnimation = "JumpLeft";
    public const string JumpRightAnimation = "JumpRight";

	// Use this for initialization
	void Start () {
		InputActions = new List<InputAction> ();
		var inputJump = new InputAction 
		{
			IsTriggered = () => Input.GetButtonDown("Jump") && Player.GetComponent<Collider2D>().IsTouchingLayers(),
			PlayerAction = InputAction.JumpAction
		};

		var inputMoveLeft = new InputAction 
		{
			IsTriggered = () => Input.GetAxis("Horizontal") < leftThreshold,
			PlayerAction = InputAction.MoveLeftAction
		};

		var inputMoveRight = new InputAction 
		{
			IsTriggered = () => Input.GetAxis("Horizontal") > rightThreshold,
			PlayerAction = InputAction.MoveRightAction
		};

        var inputFireball = new InputAction
        {
            IsTriggered = () => (Input.GetAxis("Fire1") != 0) && CanFireball(),
            PlayerAction = p => CmdSpawnFireball()
        };

		InputActions.Add(inputJump);
		InputActions.Add(inputMoveLeft);
		InputActions.Add(inputMoveRight);
        InputActions.Add(inputFireball);
        TimeSinceLastFire = 0f;
        FacingLeft = false;

        var networkId = GetComponent<NetworkIdentity>();
        GetComponentInChildren<Camera>().enabled = hasAuthority;
        Debug.Log(String.Format("Is Local Player? {0}", hasAuthority));
	}
	
	// Update is called once per frame
	void Update ()
	{
        if (hasAuthority)
        {
            TimeSinceLastFire += Time.deltaTime;
            foreach (var action in InputActions)
            {
                if (action.IsTriggered())
                {
                    action.PlayerAction(Player);
                }
            }
        }

        var animator = GetComponentInChildren<Animator>();
        var rigidBody = GetComponent<Rigidbody2D>();

        if (FacingLeft && rigidBody.velocity == Vector2.zero)
        {
            //Debug.Log("Triggering Idle Left");
            animator.Play(IdleLeftAnimation);
        }
        else if (FacingLeft && rigidBody.velocity.x != 0 && rigidBody.velocity.y <= 0)
        {
            //Debug.Log("Triggering Run Left");
            animator.Play(RunLeftAnimation);
        }
        else if (FacingLeft && rigidBody.velocity.y >= 0)
        {
            //Debug.Log("Triggering Jump Left");
            animator.Play(JumpLeftAnimation);
        }
        else if (!FacingLeft && rigidBody.velocity == Vector2.zero)
        {
            //Debug.Log("Triggering Idle Right");
            animator.Play(IdleRightAnimation);
        }
        else if (!FacingLeft && rigidBody.velocity.x != 0 && rigidBody.velocity.y <= 0)
        {
            //Debug.Log("Triggering Run Right");
            animator.Play(RunRightAnimation);
        }
        else if (!FacingLeft && rigidBody.velocity.y >= 0)
        {
            //Debug.Log("Triggering Jump Right");
            animator.Play(JumpRightAnimation);
        }
	}

    [Command]
    void CmdSpawnFireball()
    {
        Vector2 position = Player.transform.position;
        if (FacingLeft)
        {
            position.x -= Player.GetComponent<Collider2D>().bounds.size.x;
        }
        else
        {
            position.x += Player.GetComponent<Collider2D>().bounds.size.x;
        }
        var fireball = (GameObject)Instantiate(Fireball, position, Quaternion.Euler(new Vector3(0, 0, 0)));
        var fireballVelocity = FacingLeft ? new Vector2(-5f, 0f) : new Vector2(5f, 0f);
        fireball.GetComponent<Rigidbody2D>().velocity = fireballVelocity;
        TimeSinceLastFire = 0f;

        NetworkServer.SpawnWithClientAuthority(fireball);
    }

    bool CanFireball()
    {
        return TimeSinceLastFire > FireballCooldown;
    }
}
