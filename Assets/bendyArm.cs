using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bendyArm : MonoBehaviour {

	public Transform wristPosition; 
	public Transform basePosition; 
	private LineRenderer lineRenderer; 
	public GameObject prefab;
	public Transform[] pts;
	public int ptLen;
	public Transform utilboop;


	Vector3 desiredPosition;

	// Use this for initialization
	void Start () {
		lineRenderer = gameObject.GetComponent<LineRenderer>();
		GameObject lastBoop = gameObject;
		pts = new Transform[ptLen];
		for (int i = 0; i < ptLen; i++) {
			GameObject boop = (GameObject)Instantiate (prefab, transform.position, transform.rotation);
			boop.transform.parent = lastBoop.transform;

			if (i == 0) {
				boop.transform.localPosition = Vector3.zero; 
			} else {
				boop.transform.localPosition = new Vector3 (0, 0, .5f);
			}
			lastBoop = boop;
			pts [i] = boop.transform;

		}
		utilboop = ((GameObject)Instantiate (prefab, transform.position, transform.rotation)).transform;
		lineRenderer.positionCount = ptLen;
		
		pts [ptLen - 1].gameObject.SetActive (false);

		desiredPosition = wristPosition.position;
	}

	// Update is called once per frame
	void Update () {

		desiredPosition = Vector3.Lerp (desiredPosition, wristPosition.position, 10 * Time.deltaTime);

		for (int j = 0; j < 100; j++) { // why is this here?
			
			pts [ptLen - 1].LookAt (desiredPosition);
			for (int i = 0; i < ptLen - 1; i++) {
				Vector3 fromVect = pts [ptLen - 1].position - pts [ptLen - 2 - i].position;
				Vector3 toVect = wristPosition.position - pts [ptLen - 2 - i].position;
				//pts [ptLen - 2 - i].LookAt (pts [ptLen - 1]);
				Vector3 axisBoy = (toVect.normalized - fromVect.normalized);
				utilboop.transform.position = pts [ptLen - 2 - i].transform.position;
				utilboop.LookAt (pts [ptLen - 1].position);
				Quaternion a = utilboop.rotation;
				utilboop.LookAt (wristPosition.position);
				Quaternion b = utilboop.rotation;
				pts [ptLen - 2 - i].rotation *= Quaternion.AngleAxis (Quaternion.Angle (a, b), axisBoy);
				pts [ptLen - 2 - i].localRotation = Quaternion.RotateTowards (Quaternion.identity, pts [ptLen - 2 - i].localRotation, 10f);
				//pts [ptLen - 2 - i].rotation = Quaternion.identity;
				//pts [ptLen - 2 - i].rotation *= Quaternion.FromToRotation (fromVect, toVect);



			}
		}
		//for (int i = 0; i < ptLen; i++) {
		//	lineRenderer.SetPosition (i, pts[i].position);
	//	}

		//for (int i = 0; i < pts; i++) {
		//	pts[i].transform.rotation (Quaternion.RotateTowards(transform.LookAt(wristPosition), );
		//}
		/*lineRenderer.positionCount = 2; 
		lineRenderer.SetPosition (0, basePosition.position); 
		lineRenderer.SetPosition (1, wristPosition.position);*/

	}
}