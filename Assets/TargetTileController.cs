using UnityEngine;
using System.Collections;

public class TargetTileController : MonoBehaviour {

	public enum TileType
	{
		MonsterTile,
		Waypoint,
		HealthShrine,
		ManaShrine,
		CriticalShrine,
		TreasureBox,
	};

	public enum TrapType
	{
		None,
		Spear,
		Fire,
		Poison,
	};

	public enum TreasureBoxRewardType
	{
		Gold,
		WaypointScroll,
	};

	public GameObject _monsterObj;

	public int _index;

	public TileType _tileType;
	public TrapType _trapType;

	static public GameObject CreateTile(Transform playerTransform, TileType tileType, TrapType trapType, int level, int index)
	{
		GameObject go = null;

		switch(tileType)
		{
		case TileType.MonsterTile:
			string path = "Prefab/Background/";
			
			int randNum = Random.Range (0, 3);
			
			switch(randNum)
			{
			case 0:
				path += "Easy/EasyTargetTile" + Random.Range(0, 2);
				break;
				
			case 1:
				path += "Normal/NormalTargetTile0";
				break;
				
			case 2:
				path += "Hard/HardTargetTile0";
				break;
			}
			
			go = (GameObject)Instantiate (Resources.Load (path));
			
			go.GetComponent<TargetTileController> ().SetTileInfo (TileType.MonsterTile, trapType, index);
			go.GetComponent<TargetTileController> ().SetTileMonster (playerTransform, level);
			
			return go;

		case TileType.Waypoint:
			go = (GameObject)Instantiate (Resources.Load ("Prefab/Background/Waypoint"));
			go.GetComponent<TargetTileController> ().SetTileInfo (TileType.Waypoint, trapType, index);
			
			return go;

		case TileType.HealthShrine:
		case TileType.ManaShrine:
		case TileType.CriticalShrine:
			go = (GameObject)Instantiate(Resources.Load("Prefab/Background/ComponentTile/ShrineTile"));
			go.GetComponent<TargetTileController>().SetTileInfo(tileType, trapType, index);
			return go;

		case TileType.TreasureBox:
			go = (GameObject)Instantiate(Resources.Load("Prefab/Background/ComponentTile/TreasureBoxTile"));
			go.GetComponent<TargetTileController>().SetTileInfo(tileType, trapType, index);
			return go;

		default:
			return null;
		}
	}

	public void SetTileInfo(TileType tileType, TrapType trapType, int index)
	{
		_tileType = tileType;
		_trapType = trapType;
		_index = index;

		switch(tileType)
		{
		case TileType.HealthShrine:
			_monsterObj.transform.Find("ShrineEffect").GetComponent<ParticleSystem>().startColor = Color.red;
			break;
			
		case TileType.ManaShrine:
			_monsterObj.transform.Find("ShrineEffect").GetComponent<ParticleSystem>().startColor = Color.blue;
			break;
			
		case TileType.CriticalShrine:
			_monsterObj.transform.Find("ShrineEffect").GetComponent<ParticleSystem>().startColor = Color.yellow;
			break;
		}
	}
	
	public void SetTileMonster(Transform playerTransform, int level)
	{
		if(_monsterObj == null)
		{
			Transform pointTransform = null;

			if(level > 6)
			{
				pointTransform = transform.Find("HardPoint");

				if(Random.value < 0.5f)
				{
					_monsterObj = (GameObject)Instantiate (Resources.Load ("Prefab/Enemy/Goblin"));
				}
				else
				{
					_monsterObj = (GameObject)Instantiate (Resources.Load ("Prefab/Enemy/FrogRed"));
				}
			}
			else if(level > 3)
			{
				pointTransform = transform.Find("NormalPoint");

				if(Random.value < 0.6f)
				{
					_monsterObj = (GameObject)Instantiate (Resources.Load ("Prefab/Enemy/Goblin"));
				}
				else
				{
					_monsterObj = (GameObject)Instantiate (Resources.Load ("Prefab/Enemy/FrogRed"));
				}
			}
			else
			{
				pointTransform = transform.Find("EasyPoint");
				_monsterObj = (GameObject)Instantiate (Resources.Load ("Prefab/Enemy/Goblin"));
			}

			_monsterObj.transform.parent = pointTransform.Find("Node" + Random.Range(0, pointTransform.childCount));

			_monsterObj.transform.localPosition = new Vector3 (0, 0, 0);
			_monsterObj.transform.localRotation = Quaternion.Euler (0, 0 + Random.value * 360, -180);

			_monsterObj.GetComponent<MonsterController>().SetMonster(playerTransform, level);
		}
	}

	void OnCollisionEnter(Collision coll)
	{
		if(coll.collider.tag == "Player")
		{
			switch(_tileType)
			{
			case TileType.MonsterTile:
			{
				Transform playerTransform = coll.transform;
				PlayerController playerCtrl = playerTransform.GetComponent<PlayerController>();
				
				playerCtrl._currTileIndex = _index;
				
				playerCtrl.SetNextTarget(_monsterObj.transform);
				
				float distance = Vector3.Distance(playerTransform.position, _monsterObj.transform.position);
				
				float criticalDistance = playerCtrl._criticalRadius * playerCtrl._criticalBonus + _monsterObj.GetComponent<MonsterController>()._boundRadius;
				
				if(distance <= criticalDistance)
				{
					playerCtrl.CriticalAttackState();
				}
				else
				{
					playerCtrl.SetAttackState();
				}
				
				gameObject.GetComponent<Collider>().enabled = false;

				Collider[] colls = transform.GetComponentsInChildren<Collider>();
				
				foreach(Collider c in colls)
				{
					c.enabled = false;
				}
				break;
			}
			
			case TileType.Waypoint:
			{
				transform.Find("WaypointEffectParticle").GetComponent<ParticleSystem>().Play();

				Transform playerTransform = coll.transform;
				PlayerController playerCtrl = playerTransform.GetComponent<PlayerController>();

				int nextLevel = PlayerPrefs.GetInt ("Waypoint Level", 0) + 1;

				if(nextLevel < 10)
				{
					PlayerPrefs.SetInt("Waypoint Level", nextLevel);
				}

				playerCtrl._currTileIndex = _index;
				playerCtrl.SetNextTarget(_monsterObj.transform);

				playerCtrl.WaypointState();

				gameObject.GetComponent<Collider>().enabled = false;
				break;
			}

			case TileType.HealthShrine:
			case TileType.ManaShrine:
			case TileType.CriticalShrine:
			{
				Transform playerTransform = coll.transform;
				PlayerController playerCtrl = playerTransform.GetComponent<PlayerController>();

				playerCtrl._currTileIndex = _index - 1;
				playerCtrl.SetNextTarget(_monsterObj.transform);
				
				playerCtrl.ShrineState(_tileType);
				
				gameObject.GetComponent<Collider>().enabled = false;
				break;
			}

			case TileType.TreasureBox:
			{
				Transform playerTransform = coll.transform;
				PlayerController playerCtrl = playerTransform.GetComponent<PlayerController>();
				
				playerCtrl._currTileIndex = _index;
				playerCtrl.SetNextTarget(_monsterObj.transform);
				
				playerCtrl.TreasureBoxState();
				
				gameObject.GetComponent<Collider>().enabled = false;
				break;
			}

			}
		}
	}

	private float _animationTime;

	void Update()
	{
		//_animationTime += Time.deltaTime;

		//transform.position = new Vector3 (transform.position.x, Mathf.Sin (_animationTime) * 0.2f, transform.position.z);
	}
}
