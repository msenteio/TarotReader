using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cube : MonoBehaviour {

    Material mat;
    Vector3 pos;
	// Use this for initialization
	void Start () {
        mat = GetComponent<MeshRenderer>().material;
        pos = gameObject.transform.position;
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("pointer"))
        {
            mat.color = Color.white;
            other.GetComponentInParent<WandDemo>().enter(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("pointer"))
        {
            mat.color = Color.red;
            other.GetComponentInParent<WandDemo>().exit();
        }
    }

    // Update is called once per frame
    void Update () {
		if (gameObject.transform.position.y < -100)
        {
            gameObject.transform.position = pos;
        }
	}
}
