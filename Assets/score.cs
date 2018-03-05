using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class score : MonoBehaviour {

	private Color originalColor; 
	public int Score; 
	public GameObject explosion; 
	public Transform shell; 

	// Use this for initialization
	void Start () {
	
		originalColor = GetComponent<Renderer>().material.color = new Color (1, 1, 1, 1);
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter(Collision collision){
		if (collision.gameObject.tag == "Ball") {
			this.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 1, 0, 1);
			StartCoroutine (Wait()); 
			
		}
	}

	private IEnumerator Wait(){
		yield return new WaitForSeconds (1f);
		//this.gameObject.GetComponent<Renderer>().material.color = originalColor; 
		Instantiate (explosion, shell.position, shell.rotation); 
		Destroy (this.gameObject); 


	}
}
