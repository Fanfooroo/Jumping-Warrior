using UnityEngine;
using System.Collections;

public class PlayerCollisionController : MonoBehaviour {

	private PlayerController _playerCtrl;
	// Use this for initialization
	void Start () {
		_playerCtrl = GetComponent<PlayerController> ();
	}
	
	void OnTriggerEnter(Collider coll)
	{
		switch(coll.tag)
		{
		case "SpearTrap":
			_playerCtrl.SetDamage(10);
			Collider[] colls = coll.transform.parent.parent.GetComponentsInChildren<Collider>();

			foreach(Collider c in colls)
			{
				if(c.isTrigger == true)
				{
					c.enabled = false;
				}
			}
			break;
		}
	}
}
