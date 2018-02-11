using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour {

	//this script controls most functions of the game
	//handle game start animation
	//it generates tiles and closes tiles
	//calculates score
	//decides when player dies


	public GameObject xTilePrefab;
	public GameObject yTilePrefab;

	public Transform contactPoint;
	public LayerMask whatIsGround;
	public GameObject ps;	//particle system for coin
	public GameObject cam;	//main camera refrence 
	public float speed  ;	//ball speed
	
	public bool onXTile ; 	//on which tile ball is 
	public bool onYTile ;
	public bool generateTile;	//when to generate tile

	public Text scoreText;	//refrence for UI element
	public GameObject startObj;
	public Animator anim;		//UI animator
	public Animator scoreAnim;	//when game ends because of zero score

	//for storing active X and Y tiles
	public List<GameObject> currentTileX;
	public List<GameObject> currentTileY;

	//storing inactive X and Y tiles

	public List<GameObject> recycledTileX ;
	public 	List<GameObject> recycledTileY ;

	//list of all audio assets
	public AudioClip [] audios;


	Vector3 dir ;
	bool camMoved;
	public int score;
	bool restart;
	bool gmStarted;
	float tileGenerateSpeed;
	List<GameObject> closeAnimX;
	List <GameObject> closeAnimY;



	void Awake(){
		CreateTiles (30);

	}

	void Start () {

		gmStarted = false;
		CreateAtStart (10);
		generateTile = true;
		scoreText.text = "SCORE : " + score;
		restart = true;
	
		closeAnimX = new List<GameObject>();
		closeAnimY = new List<GameObject>();



	}


	void FixedUpdate(){
		
		if (currentTileY.Count != 0 && currentTileX.Count != 0 && IsGrounded()&& !camMoved) {
			CamFollow ();
		}

		//translating ball
		transform.Translate (dir * speed * Time .deltaTime);
	}

	void Update () {

		//handeling mouse click and checking player is on ground(tile) or not
			OnClickMouse ();
			IsGrounded ();

		//if player is not on any tile or score gets zero 
		if (!IsGrounded () && restart || score < 0 && restart ) {
			if(score < 0)
				scoreAnim.SetTrigger("Score0");
			
			StartCoroutine(PlayerDead());
			restart = false;
		}
	
	}
	IEnumerator PlayerDead(){

		//managing sound 
		GetComponent<AudioSource> ().volume = 1;
		GetComponent<AudioSource> ().pitch = 1;

		GetComponent<AudioSource>().clip = audios[5];
		GetComponent<AudioSource>().Play();

		//when player is dead, restart the game after 2 seconds

		yield return new WaitForSeconds (2);


		if (PlayerPrefs.GetInt ("GamePlayed", 0) >PlayerPrefs.GetInt("AD",30)) {

		}
		//reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

	//changes ball speed when it touchs blocker on the tile
	//reduces score every time it collides blocker
	//decides on which tile ball is

	void OnTriggerEnter(Collider other){


		if (other.CompareTag ("Block")) {

			//if ball trigers blocker, it changes ball direction and generates a number of new tiles which will be generaated
			dir = -dir;
			int randomTileNumber = Random.Range (5, 9);
			ActivateTiles (randomTileNumber);

			//reducing score by 1
			score--;

			if(score >= 0){
			scoreText.text  = "SCORE : " + score;
				GetComponent<AudioSource> ().volume = 1;
				GetComponent<AudioSource> ().pitch = 1;

				GetComponent<AudioSource>().clip = audios[1];
				GetComponent<AudioSource>().Play();
			}

			//calculating on which tile ball is

		} else if (other.CompareTag ("XTile")) {
			onYTile = false;
			onXTile = true;
		} else if (other.CompareTag ("YTile")) {
			onXTile = false;
			onYTile = true;
		} else if (other.CompareTag ("Coin")) {

			score += 2;
			Instantiate(ps,other.transform.position,other.transform.rotation);
			other.gameObject.SetActive (false);
			scoreText.text = "SCORE : " + score;
			GetComponent<AudioSource> ().pitch = 1;

			GetComponent<AudioSource> ().volume = .5f;
			GetComponent<AudioSource>().clip = audios[3];
			GetComponent<AudioSource>().Play();
		}
	}


	//handles mouse click, increases score by 5 on every click, changes direction of ball on mouse click

	void OnClickMouse(){


		if (Input.GetButtonDown("Submit") && IsGrounded() && score >= 0) {
			

			if (!gmStarted) {
				
				//handeling first click of game

				GetComponent<Rigidbody> ().isKinematic = false;
				anim.SetTrigger ("GameStarted");

				if (!anim.GetCurrentAnimatorStateInfo (0).IsName ("GameStarted"))
				
				scoreText.gameObject.SetActive (true);
				gmStarted = true;
				GetComponent<AudioSource> ().pitch = 1;
				GetComponent<AudioSource> ().volume = 1;
				GetComponent<AudioSource> ().clip = audios [0];
				GetComponent<AudioSource> ().Play ();
		
			} else {
				GetComponent<AudioSource> ().pitch = 1.35f;
				GetComponent<AudioSource> ().volume = 1;
				GetComponent<AudioSource>().clip = audios[1];
				GetComponent<AudioSource>().Play();
			}
		
	
				score += 5;
				
			scoreText.text = "SCORE : " + score;
			generateTile = true;

			//changing ball direction on every click

			if (dir == Vector3.right || dir == -Vector3.right) {
				dir = Vector3.forward;
			} else if (dir == Vector3.forward || -dir == Vector3.forward) {
				dir = -Vector3.right;
			}else{
				dir = -Vector3.right;
			}
		}
	}

	//hadels camera to follow the ball smoothly

	void CamFollow(){
		GameObject tmp = null;

		if (onYTile && currentTileX.Count != 0) {

			//if ball is on Y Tile, giving a target to camera to follow
			//you can try to change the target

			tmp = currentTileX[currentTileX.Count -1];

		}else if (onXTile && currentTileY.Count != 0) {

			tmp = currentTileY[currentTileY.Count - 1];

		}
		Vector3 point = cam.GetComponent<Camera>().WorldToViewportPoint(tmp.transform.position);
		Vector3 delta = tmp.transform.position - cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); 
		Vector3 destination = cam.transform.position + delta;
		Vector3 velocity = Vector3.zero;
		cam.transform.position =  Vector3.SmoothDamp(cam.transform.position, destination,ref velocity, .35f);

	}
		
	private bool IsGrounded(){

		//making a sphere at bottom of the ball and checking its collision to the tile

		Collider [] colliders = Physics.OverlapSphere (contactPoint.position, .3f, whatIsGround);
		for(int i = 0; i<colliders.Length; i++){
			if(colliders[i].gameObject != gameObject){

				return true;
			}
		}

		return false;

	}

		
	public void CreateTiles(int amount){

		//renaming all new tiles which we are going to generate

		for (int i = 0; i < amount; i++) {
			recycledTileX.Add (Instantiate (xTilePrefab));

			//renaming last element of array
			recycledTileX [recycledTileX.Count - 1].name = ("xTile");
			recycledTileX [recycledTileX.Count - 1].SetActive (false);
			recycledTileY.Add (Instantiate (yTilePrefab));
			recycledTileY [recycledTileY.Count - 1].name = ("yTile");		
			recycledTileY [recycledTileY.Count - 1].SetActive (false);
		}
	}

	void ManageSpeed(){
		//generating new speed for ball, when it collides to blocker
		//deviding by 4 so that we can get more variations in speed
		speed = Random .Range (16, 44)/4;
	}

	void CreateAtStart(int amount){
		//this function creates given amount of tile when game starts

		for (int i = 0; i < amount ; i++){
			GameObject tmp = recycledTileX[recycledTileX.Count-1];
			recycledTileX.Remove(tmp );


			tmp.transform.position = currentTileX[currentTileX.Count - 1].transform.GetChild(0).transform.GetChild (1).transform.position;
			currentTileX .Add( tmp);

			int spawnPickUp = Random.Range (0, 10);

			if (spawnPickUp == 0) {
				currentTileX[currentTileX.Count - 1]. transform.GetChild(2).gameObject.SetActive(true);
			}

			if(i == amount -1){
				tmp.transform.GetChild (1).gameObject.SetActive(true);
			}
		}
		tileGenerateSpeed = .3f;
		StartCoroutine(TileWakeUp());
	}

	//this function genaarates new tiles when ball is on X tile or on Y tile
	//plays animations for tile opening and closing
	//hanles seperately, when it is on X or Y tile
	//decides where to keep new blocker on tile

	public void ActivateTiles(int amount){
		//if tile amount which we generated is not enough

		if (recycledTileX.Count < 15 || recycledTileY.Count < 15 ) {
			CreateTiles(10);
			Debug.Log ("generating extra 10 tiles");
		}

		//when ball is on Y tile and ball is colliding to Y tile blocker first time 

		if(onYTile && generateTile){

			//generating a new speed for ball
			ManageSpeed();
			if(currentTileX.Count != 0){
				if(currentTileX.Count != 0){
				for(int i = 0; i < currentTileX.Count; i++){

						//deactivating previous tiles (X tiles because ball is on Y tile now)

					if(currentTileX[currentTileX.Count - 1-i].transform.GetChild(1).gameObject.activeSelf){
						currentTileX[currentTileX.Count - 1-i].transform.GetChild(1).gameObject.SetActive(false);
					}
						//running tile close animation

						closeAnimX.Add (currentTileX [currentTileX.Count - i - 1]);
				}

			}
				StartCoroutine (DeactivateXTile ());

				//clearing currentTileX so that we can keep new X tiles in it
				currentTileX.Clear();
			}
			// choosing a random tile to activate blocker on tile

			int randomBlockNumber = Random.Range(0, currentTileY.Count -3);
			int randomTileNumber = Random.Range(randomBlockNumber + 1, currentTileY.Count -1);

			//setting a random blocker active on tile
			currentTileY[randomBlockNumber].transform.GetChild (1).gameObject.SetActive (true);
			GameObject tmp = recycledTileX[recycledTileX.Count - 1];
			recycledTileX.Remove (tmp);
			
			//setting a random Y tile position to  new X tile 

			tmp.transform.position = currentTileY[randomTileNumber].transform.GetChild (0).transform.GetChild (1).transform.position;

			//adding that X tile to currentTileX
			currentTileX.Add(tmp);

			for(int i = 0; i < amount; i++){
				tmp = recycledTileX[recycledTileX.Count - 1];
				recycledTileX.Remove (tmp);
				
				//genarating new X tiles on next to last tile position

				tmp.transform.position = currentTileX[currentTileX.Count - 1].transform.GetChild (0).transform.GetChild (1).transform.position;
		
				currentTileX.Add(tmp);
				
				if(i == amount-1){
					tmp.transform.GetChild (1).gameObject.SetActive (true);
				}

				int spawnPickUp = Random.Range (0, 10);
				//setting a coin active if random number matches to zero for every tiles

				if (spawnPickUp == 0) {
					currentTileX[currentTileX.Count - 1]. transform.GetChild(2).gameObject.SetActive(true);
				}
			}
			//running tile open animation

			StartCoroutine (WakeUpWOnY());
			generateTile = false;
			camMoved = false;
		}

		//same as above, just at place of Y tile ball is on X tile

		if(onXTile && generateTile){

			//when ball is on X tile and ball is colliding to Y tile blocker first time 
			if(currentTileY.Count != 0){
				//generating a new speed for ball
				ManageSpeed();

				for(int i = 0; i < currentTileY.Count; i++){

					if(currentTileY.Count != 0){
					if(currentTileY[currentTileY.Count - 1-i].transform.GetChild(1).gameObject.activeSelf){
					
							//deactivating previous tiles (Y tiles because ball is on X tile now)
							currentTileY[currentTileY.Count - 1-i].transform.GetChild(1).gameObject.SetActive(false);
				}

				
						closeAnimY.Add (currentTileY [currentTileY.Count - i - 1]);
					}
				}
				StartCoroutine (DeactivateYTile ());

				//clearing currentTileY so that we can keep new Y tiles in it
				currentTileY.Clear();
			}

			// choosing a random tile to activate blocker on tile
			int randomBlockNumber = Random.Range(0, currentTileX.Count -3);
			int randomTileNumber = Random.Range(randomBlockNumber +1 , currentTileX.Count -1);
			//setting a random blocker active on tile

			currentTileX[randomBlockNumber].transform.GetChild (1).gameObject.SetActive (true);
			GameObject tmp = recycledTileY[recycledTileY.Count - 1];
			recycledTileY.Remove (tmp);

			//setting a random X tile position to  new Y tile 

			tmp.transform.position = currentTileX[randomTileNumber].transform.GetChild (0).transform.GetChild (0).transform.position;

			//adding that Y tile to currentTileY
			currentTileY.Add(tmp);

			for(int i = 0; i < amount; i++){
				tmp = recycledTileY[recycledTileY.Count - 1];
				recycledTileY.Remove (tmp);
			
				//genarating new X tiles on next to last tile position
				tmp.transform.position = currentTileY[currentTileY.Count - 1].transform.GetChild (0).transform.GetChild (0).transform.position;
			
				currentTileY.Add(tmp);

				if(i == amount-1){
					tmp.transform.GetChild (1).gameObject.SetActive (true);
				}
				int spawnPickUp = Random.Range (0, 10);
				
				if (spawnPickUp == 0) {
					currentTileY[currentTileY.Count - 1]. transform.GetChild(2).gameObject.SetActive(true);
				}
			}
			tileGenerateSpeed = .3f;
			StartCoroutine (WakeUpWOnX());
			generateTile = false;
			camMoved = false;
		}
	}
	
	IEnumerator DeactivateXTile(){
		tileGenerateSpeed = .3f;

		//closing every tile by playing CloseTile animation

		if (closeAnimX.Count != 0) {
			foreach(GameObject tile in closeAnimX) {

				tile .GetComponent<Animator> ().SetTrigger ("CloseTile");

				if (tileGenerateSpeed > .15f) {
					tileGenerateSpeed -= .1f;
					yield return new WaitForSeconds (tileGenerateSpeed);

				} else{
					yield return new WaitForSeconds (tileGenerateSpeed);
				}
				recycledTileX.Add (tile);
			}


			closeAnimX.Clear ();
		}
	}
	IEnumerator DeactivateYTile(){
		tileGenerateSpeed = .3f;
		if (closeAnimY.Count != 0) {

			//closing every tile by playing CloseTile animation

			foreach(GameObject tile in closeAnimY) {

				tile .GetComponent<Animator> ().SetTrigger ("CloseTile");

				if (tileGenerateSpeed > .15f) {
					tileGenerateSpeed -= .1f;
					yield return new WaitForSeconds (tileGenerateSpeed);

				} else{
					yield return new WaitForSeconds (tileGenerateSpeed);
				}
				recycledTileY.Add (tile);
			}

			closeAnimY.Clear ();
		}
	}

	IEnumerator WakeUpWOnX(){
		//playing OpenTile animation for Y tile

		foreach (GameObject tile in currentTileY) {
			tile.SetActive (true);
				tile.GetComponent<Animator>().SetTrigger ("OpenTile");
	
		
		if (tileGenerateSpeed > .15f) {
			tileGenerateSpeed -= .1f;
			yield return new WaitForSeconds (tileGenerateSpeed);
		} else{
			yield return new WaitForSeconds (tileGenerateSpeed);
		}

		}
	}
	IEnumerator WakeUpWOnY(){

		//playing OpenTile animation for X tile
		foreach (GameObject tile in currentTileX) {
			tile.SetActive (true);
			tile.GetComponent<Animator>().SetTrigger ("OpenTile");
		
			if (tileGenerateSpeed > .15f) {
				tileGenerateSpeed -= .1f;
				yield return new WaitForSeconds (tileGenerateSpeed);
			} else{
				yield return new WaitForSeconds (tileGenerateSpeed);
			}
			
		}
	}
	IEnumerator TileWakeUp(){

		// handles tile open animation when game starts

		foreach (GameObject tile in currentTileX) {
			tile.SetActive (true);
		
			tile.GetComponent<Animator>().SetTrigger ("OpenTile");



			if (tileGenerateSpeed > .15f) {
				tileGenerateSpeed -= .1f;
				yield return new WaitForSeconds (tileGenerateSpeed);
			} else{
				yield return new WaitForSeconds (tileGenerateSpeed);
			}
            
		}
	}

   

 
	
}
