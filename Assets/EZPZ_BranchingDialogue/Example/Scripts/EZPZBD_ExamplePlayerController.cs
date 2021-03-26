using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine;

public class EZPZBD_ExamplePlayerController : MonoBehaviour
{
	public EZPZBD_ExampleUIManager ui;

	Rigidbody2D rb;
	Animator anim;
	SpriteRenderer characterSR;
	Transform cameraTrans;

	int coinCounter = 0;

	public float moveSpeed = 10f;
	public float jumpPower = 100f;
	public float maxVelocity = 8f;
	public float cameraSpeed = 8f;

	bool stopped = true;
	bool jumping = false;
	bool canJump = true;
	bool canStartConversation = false;
	GameObject conversationNPC;
	bool inConversation = false;

	// Use this for initialization
	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		anim = transform.GetChild(0).GetComponent<Animator>();
		characterSR = transform.GetChild(0).GetComponent<SpriteRenderer>();
		cameraTrans = Camera.main.gameObject.transform;
	}

	// Update is called once per frame
	void Update()
	{
		if (!inConversation)
			ProcessMovement();
		else
			ProcessConversation();
	}

	void FixedUpdate()
	{
		//FixedUpdate smooths the camera since the player is moved via Rigidbody
		CameraTracking();
	}

	void ProcessMovement()
	{
		//Start conversation
		if (Input.GetKeyDown(KeyCode.E) && canStartConversation)
		{
			//Stop all movement
			rb.velocity = Vector2.zero;

			//Turn character towards player and vice versa
			conversationNPC.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = (conversationNPC.transform.position.x <= gameObject.transform.position.x) ? false : true;
			gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = (gameObject.transform.position.x <= conversationNPC.transform.position.x) ? false : true;

			//Get the character index from the child gameobject name
			int charNumber;
			if (int.TryParse(conversationNPC.transform.GetChild(0).name[0].ToString(), out charNumber))
			{
				ui.ToggleConversationUI(true, charNumber);
				inConversation = true;
			}
		}

		//Move left
		if (Input.GetKey(KeyCode.A))
		{
			rb.AddForce(Vector2.left * moveSpeed);
			if (rb.velocity.x < -8)
				rb.velocity = new Vector2(-8, rb.velocity.y);

			characterSR.flipX = true;
		}

		//Move right
		if (Input.GetKey(KeyCode.D))
		{
			rb.AddForce(Vector2.right * moveSpeed);
			if (rb.velocity.x > 8)
				rb.velocity = new Vector2(8, rb.velocity.y);

			characterSR.flipX = false;
		}

		//Change sprite on fall down
		anim.SetFloat("yVel", rb.velocity.y);

		//Jump
		if (Input.GetKeyDown(KeyCode.Space) && canJump)
		{
			canJump = false;
			jumping = true;
			rb.AddForce(Vector2.up * jumpPower);
			anim.SetTrigger("JumpUp");
			anim.SetBool("JumpDown", false);
		}

		//Stopped walking
		if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D) || Mathf.Abs(rb.velocity.x) < .1f)
		{
			anim.SetBool("Walking", false);
		}

		//Check if stopped moving
		if (!stopped && Mathf.Abs(rb.velocity.x) < .1f)
			stopped = true;
		if (!jumping && Mathf.Abs(rb.velocity.x) > .1f)
		{
			anim.SetBool("Walking", true);
			stopped = false;
		}		
	}

	void CameraTracking()
	{
		if (!inConversation)
			cameraTrans.position = Vector3.Lerp(cameraTrans.position, new Vector3(gameObject.transform.position.x + ((characterSR.flipX ? -1 : 1) * 2), gameObject.transform.position.y + 1, -10), cameraSpeed);
		else
			cameraTrans.position = Vector3.Lerp(cameraTrans.position, new Vector3(gameObject.transform.position.x + ((conversationNPC.transform.position.x - gameObject.transform.position.x)/2), gameObject.transform.position.y - 2, -10), cameraSpeed * 1.5f);
	}

	//Use the player's input to advance a conversation
	void ProcessConversation()
	{
		//Next
		if (Input.GetKeyDown(KeyCode.E))
		{
			ui.ConversationUpdate();

			//If the trees update any outside variables, do it here
			UpdateVariables();
		}

		//Choice 1
		if (Input.GetKeyDown(KeyCode.A))
			ui.PlayerChoice(0);

		//Choice 2
		if (Input.GetKeyDown(KeyCode.D))
			ui.PlayerChoice(1);

		//Escape
		if (!ui.IsInConversation())
			inConversation = false;
	}

	//Update any variables for use in the scene that might have changed based on the node information
	void UpdateVariables()
	{
		//If a dialogue node edits the coin values, make sure it updates here as well
		coinCounter = EZPZ_TreeInfoHolder.Instance.GetGlobalVariableIntValue("Character");
		ui.SetCoinText(coinCounter);
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		if (col.transform.parent.gameObject.name == "_Walkable")
		{
			canJump = true;
			jumping = false;
			anim.SetBool("JumpDown", false);
		}
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		if (col.transform.parent.gameObject.name == "_Characters")
		{
			canStartConversation = true;
			conversationNPC = col.transform.gameObject;
		}
		else if (col.transform.name.Contains("Coin"))
		{
			col.gameObject.transform.parent.transform.gameObject.SetActive(false);
			coinCounter++;
			ui.SetCoinText(coinCounter);
			EZPZ_TreeInfoHolder.Instance.SetGlobalVariableIntValue("Character", coinCounter);
		}
	}

	void OnTriggerExit2D(Collider2D col)
	{
		if (col.transform.parent.gameObject.name == "_Characters")
		{
			canStartConversation = false;
			conversationNPC = null;
		}
	}
}
