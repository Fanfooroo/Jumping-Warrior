using UnityEngine;
using System.Collections;

public class FloorController : MonoBehaviour {

	private Transform _mainCamera;

	// Use this for initialization
	void Start () {
		_mainCamera = Camera.main.transform;
	}

	void OnTriggerEnter(Collider coll)
	{
		if(coll.tag == "Player")
		{
			coll.GetComponent<PlayerController>().SetDieState(true);
		}
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3(0, 0, Mathf.Lerp(transform.position.z, _mainCamera.transform.position.z, Time.deltaTime));
	}
}
