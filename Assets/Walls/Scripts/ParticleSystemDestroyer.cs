using UnityEngine;
using System.Collections;

public class ParticleSystemDestroyer : MonoBehaviour {

	// this script destroys coin particle system after 4 seconds
	void Start () {
		StartCoroutine (Destroy ());
	}
	
	IEnumerator Destroy(){
		yield return new WaitForSeconds (4);
		Destroy (gameObject);
	}
}
