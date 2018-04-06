using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
	Animation anim;
	Vector3 movement;
	CharacterController controller;
	private GameController gameController;
	//public Camera mainCamera;

	public float h_speed;
	public float v_speed;
	public float jumpSpeed;
	public float gravity;

	private Vector3 moveDirection = Vector3.zero;

	private Vector3 offset;
	private GameObject mainCamera;
	private GameObject zombies;

	//方向判断
	float angle = 0.0f;

	//动画播放
	bool isJump = false;
	bool isSlip = false;
	float gap = 0.0f;
	float timer = 0.0f;
	float startTime;

	//人物转向延迟
	private float nextTurnTime = 0.0f;
	private float turnTimeGap = 0.2f;

	//摄像头转动
	private bool cameraTurn = false;
	private bool cameraTurnLeft = true;

	void Awake ()
	{
		DontDestroyOnLoad (transform.gameObject);
		if (FindObjectsOfType (GetType ()).Length > 1) {
			Destroy (gameObject);
		}
	}
	// Use this for initialization
	void Start ()
	{
		GameObject gameControllerObject = GameObject.FindWithTag ("GameController");
		if (gameControllerObject != null) {
			gameController = gameControllerObject.GetComponent<GameController> ();
		}
		controller = GetComponent<CharacterController> ();
		anim = GetComponent<Animation> ();
		mainCamera = GameObject.FindGameObjectWithTag ("MainCamera");
		anim ["jump"].speed = 2.0f;
		anim ["run"].speed = 2.0f;
		anim ["death"].speed = 1.5f;
		startTime = Time.time;
		nextTurnTime = 0.0f;
		timer = 0.0f;

		offset = transform.position - mainCamera.transform.position;
	}

	// Update is called once per frame
	void Update ()
	{
		float h = 0.0f;
		if (controller.isGrounded) {
			if (Time.time >= startTime + 2.5f) {
				h = Input.GetAxis ("Horizontal");
			} else {
				h = 0;
			}
			moveDirection = new Vector3 (h * h_speed / gameController.v_speed, 0, 1.0f);
			moveDirection = transform.TransformDirection (moveDirection);
			moveDirection *= gameController.v_speed;

			if (Input.GetButtonDown ("Jump") && isJump == false && isSlip == false) {
				gap = anim.GetClip ("jump").length / 2.0f;
				isJump = true;
				timer = Time.time + gap;

				moveDirection.y = jumpSpeed;

				GameObject[] obstructions = GameObject.FindGameObjectsWithTag ("Obstruction");
				for (int i = 0; i < obstructions.Length; i++) {
					Vector3 current = obstructions [i].transform.position;
					obstructions [i].transform.position = new Vector3 (current.x, current.y - 1.5f, current.z);
				}
			}

			if (Input.GetKeyDown (KeyCode.Q) && isJump == false && isSlip == false) {
				gap = anim.GetClip ("death").length / 1.5f;
				isSlip = true;
				timer = Time.time + gap;

				GameObject[] obstructions = GameObject.FindGameObjectsWithTag ("Obstruction2");
				for (int i = 0; i < obstructions.Length; i++) {
					Vector3 current = obstructions [i].transform.position;
					obstructions [i].transform.position = new Vector3 (current.x, current.y + 3.0f, current.z - 0.4f);
				}
			}
		}

		if (isJump == true && Time.time < timer) {
			anim.Play ("jump"); 
		} else if (isSlip == true && Time.time < timer) {
			anim.Play ("death");
		}/* else if (isJump == false && controller.isGrounded == false) {
			//todo
		} */else {
			if (isJump == true) {
				GameObject[] obstructions = GameObject.FindGameObjectsWithTag ("Obstruction");
				for (int i = 0; i < obstructions.Length; i++) {
					Vector3 current = obstructions [i].transform.position;
					obstructions [i].transform.position = new Vector3 (current.x, current.y + 1.5f, current.z);
				}
				isJump = false;
			}
			if (isSlip == true) {
				GameObject[] obstructions = GameObject.FindGameObjectsWithTag ("Obstruction2");
				for (int i = 0; i < obstructions.Length; i++) {
					Vector3 current = obstructions [i].transform.position;
					obstructions [i].transform.position = new Vector3 (current.x, current.y - 3.0f, current.z + 0.4f);
				}
				isSlip = false;
			}

			timer = 0;
			anim.Play ("run");
		}

		if (Input.GetKeyDown (KeyCode.LeftArrow) && Time.time > nextTurnTime) {
			cameraTurn = true;
			cameraTurnLeft = true;
			//mainCamera.transform.Rotate (0, -90, 0);
			transform.Rotate (0, -90, 0);
			angle -= Mathf.PI / 2;

			nextTurnTime = Time.time + turnTimeGap;
		}

		if (Input.GetKeyDown (KeyCode.RightArrow) && Time.time > nextTurnTime) {
			cameraTurn = true;
			cameraTurnLeft = false;
			//mainCamera.transform.Rotate (0, 90, 0);
			transform.Rotate (0, 90, 0);
			angle += Mathf.PI / 2;

			nextTurnTime = Time.time + turnTimeGap;
		}

		if (cameraTurn) {
			if (Time.time <= nextTurnTime) {
				if (cameraTurnLeft) {
					mainCamera.transform.Rotate (0, -90 * Time.deltaTime / turnTimeGap, 0);
				} else {
					mainCamera.transform.Rotate (0, 90 * Time.deltaTime / turnTimeGap, 0);
				}
			} else {
				//Debug.Log (mainCamera.transform.rotation.eulerAngles.y);
				float tmp_offset = mainCamera.transform.rotation.eulerAngles.y % 90;
				//Debug.Log (mainCamera.transform.rotation.eulerAngles.y % 90);
				if (tmp_offset >= 65) {
					tmp_offset = tmp_offset - 90;
				}
				mainCamera.transform.Rotate (0, -tmp_offset, 0);
				//Debug.Log (mainCamera.transform.rotation.eulerAngles.y);
				cameraTurn = false;
			}
		}

		moveDirection.y -= gravity * Time.deltaTime;
		controller.Move (moveDirection * Time.deltaTime);
	}

	void LateUpdate ()
	{
		Vector3 newOffset = new Vector3 (offset.z * Mathf.Sin (angle), offset.y, offset.z * Mathf.Cos (angle));
		mainCamera.transform.position = transform.position - newOffset;
	}
		

}
