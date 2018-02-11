using UnityEngine;
using System.Collections;

public class ColorControl : MonoBehaviour {

	//this script changes the color of tiles and ball after some score

	public Material tileMaterial;
	public Material blockM;
	public Material ballM;

	//list of all color which will be used for tiles and ball 
	public Color[] colorList;
	public PlayerScript playerS;
	int currentScore;
	int targetScore;	//score, when we want to change tile color
	int ranNumber;	//we will generate a random number
	int selectedNum; //we will decide a new number
	Color ranColor; 



	bool colorChanged; 
	void Start () {
		
		currentScore = 0;
		targetScore = 16;
		colorChanged = false;



		ranNumber = Random.Range (0, colorList.Length); //random number between zero and total color in color list

		ranColor = colorList [ranNumber];
		
		if (ranNumber < 6) {
			selectedNum = ranNumber + 6;
		} else {
			selectedNum = ranNumber - 6;
		}
		StartCoroutine (UpdateScore ());
	}
	

	void Update () {

		if (currentScore >= targetScore) {
			//desiding color and changing color using Lerp

			if(ranNumber == 6){
					blockM.color = Color.Lerp (blockM.color, colorList[11], Time.time * 0.0003f);	
					ballM.color = Color.Lerp (ballM.color, colorList[1], Time.time * 0.0003f);	
				}
				else if(ranNumber == 5){
					blockM.color = Color.Lerp (blockM.color, colorList[10], Time.time * 0.0003f);	
					ballM.color = Color.Lerp (ballM.color, colorList[0], Time.time * 0.0003f);	
			}else {
				blockM.color = Color.Lerp (blockM.color, colorList[selectedNum -1], Time.time * 0.0003f);	
				ballM.color = Color.Lerp (ballM.color, colorList[selectedNum +1], Time.time * 0.0003f);	
			}
			tileMaterial.color = Color.Lerp (tileMaterial.color, ranColor, Time.time * 0.0003f);
		
			// if color has not changed
			if (!colorChanged) {
				StartCoroutine (ControlColor ());
				colorChanged = true;

				//playing audio for color change
				GetComponent<AudioSource> ().Play();

			}
		}
	
				
		}

	IEnumerator UpdateScore(){
		//updating current score after 4 seconds for bringing just a little variation in color change time

		yield return new WaitForSeconds (4);
		currentScore = playerS.score;
		StartCoroutine (UpdateScore ());
	}

	IEnumerator ControlColor(){
		yield return new WaitForSeconds (3);

		//increasing target score every time it changes color

		targetScore += 19;

		//generating new random number for desiding colors
		ranNumber = Random.Range (0, colorList.Length);

		ranColor = colorList [ranNumber];
	

		if(ranNumber < 6)
			selectedNum = ranNumber +6;
		else
			selectedNum = ranNumber -6;
		colorChanged = false;
	}
}
