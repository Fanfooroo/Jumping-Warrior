using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public PlayerController _playerCtrl;
	public GameUIController _gameUICtrl;
	public CameraController _cameraCtrl;

	private List<Transform> _tileTransformList = new List<Transform>();

	public GameObject _startWaypointTile;

	public int _currTargetIndex;

	private GameObject _prevTarget;

	public int _gameLevel = 1;

	private int _minDistance;

	public int _clearCount;
	public int _comboCount;
	public int _goldCount;

	public int _shrineIndex;
	public int _treasureBoxIndex;

	void Awake()
	{
		Application.targetFrameRate = 60;

		_goldCount = 0;
		PlayerPrefs.SetInt ("Costume0", 1);

		_gameLevel = _currTargetIndex / 5;
		
		if(_gameLevel > 10)
		{
			_gameLevel = 10;
		}

		_minDistance = 9;
	}

	public void SetWaypointStart()
	{
		StartCoroutine (WaypointStartAction ());
	}

	IEnumerator WaypointStartAction()
	{
		_gameUICtrl.ChangeWaypointStartUI ();

		yield return new WaitForSeconds (0.5f);

		_startWaypointTile.transform.Find ("TeleportEffectParticle").GetComponent<ParticleSystem> ().Play ();

		yield return new WaitForSeconds (0.5f);

		_playerCtrl.PlayWaypointStartAction (0);

		yield return new WaitForSeconds (1.5f);

		_gameUICtrl.ScreenFadeInOut (false);

		yield return new WaitForSeconds (0.3f);

		_cameraCtrl.SetWaypointStartActionAngle ();

		_gameUICtrl.ScreenFadeInOut (true);

		SetStartTile (true);
		_startWaypointTile.transform.Find ("WaypointEffectParticle").GetComponent<ParticleSystem> ().Play ();

		yield return new WaitForSeconds (0.5f);

		_startWaypointTile.transform.Find ("TeleportEffectParticle").GetComponent<ParticleSystem> ().Play ();

		yield return new WaitForSeconds (0.5f);

		_playerCtrl.PlayWaypointStartAction (1);

		yield return new WaitForSeconds (1.0f);

		_playerCtrl.PlayWaypointStartAction (2);

		yield return new WaitForSeconds (1.0f);
		_playerCtrl.SetWaitInput ();
		_cameraCtrl.SetWaitInputAngle ();
		_gameUICtrl.ChangePlayUI ();
	}

	public void SetStartTile(bool isWaypointStart)
	{
		if(isWaypointStart == true)
		{
			_currTargetIndex = (int)_gameUICtrl._wayPointSlider.value;

			int itemCount = (int)GameDataManager.GetGameData(GameDataManager.GameDataID.WaypointItemCount);

			if(itemCount == 5)
			{
				System.TimeSpan tspan = System.TimeSpan.FromTicks(System.DateTime.Now.AddMinutes(5).Ticks);
			
				double totalSeconds = tspan.TotalSeconds;
			
				PlayerPrefs.SetString("WaypointItem ChargeTime", totalSeconds.ToString());
			}

			GameDataManager.SetGameData(GameDataManager.GameDataID.WaypointItemCount, itemCount - 1);

			_gameLevel = _currTargetIndex / 5;
			
			if(_gameLevel > 10)
			{
				_gameLevel = 10;
			}
		}

		_shrineIndex = Random.Range (_currTargetIndex + 15, _currTargetIndex + 20);
		_treasureBoxIndex = Random.Range (_currTargetIndex + 20, _currTargetIndex + 30);

		for(int i = 0 ; i < 4 ; i++)
		{
			GameObject tile = TargetTileController.CreateTile(_playerCtrl.transform, TargetTileController.TileType.MonsterTile, TargetTileController.TrapType.None, _gameLevel, _currTargetIndex);
			
			float posZ = _minDistance + Random.value * 5;
			
			if(_tileTransformList.Count > 0)
			{
				posZ += _tileTransformList[_tileTransformList.Count - 1].transform.localPosition.z;
			}
			
			tile.transform.position = new Vector3 (Random.value * 9 - 4.5f, 0, posZ);
			tile.transform.localRotation = Quaternion.Euler(-90, Random.value * 360, 0);
			
			_tileTransformList.Add (tile.transform);
			
			_currTargetIndex++;
		}
		
		_playerCtrl.SetNextTarget (_tileTransformList [0].GetComponent<TargetTileController>()._monsterObj.transform);
	}

	public void SetNextTarget()
	{
		int currTileIndex = _tileTransformList.FindIndex (r => r.GetComponent<TargetTileController> ()._index == _playerCtrl._currTileIndex);

		for(int i = currTileIndex - 1 ; i >= 0 ; i--)
		{
			GameObject go = _tileTransformList[i].gameObject;
			_tileTransformList.RemoveAt(i);
			Destroy(go, 3.0f);
		}

		int length = _tileTransformList.Count;

		for(int i = 0 ; i < 4 - length ; i++)
		{
			GameObject tile = null;

			if(_currTargetIndex == Constants._waypointPosition[PlayerPrefs.GetInt("Waypoint Level", 0) + 1] - 1)
			{
				tile = TargetTileController.CreateTile(_playerCtrl.transform, TargetTileController.TileType.Waypoint, TargetTileController.TrapType.None, _gameLevel, _currTargetIndex);

				float posZ = (_tileTransformList.Count == 0 ? 0.0f : _tileTransformList[_tileTransformList.Count - 1].transform.localPosition.z) + _minDistance + Random.value * (5 + (_gameLevel / 2));
				
				tile.transform.position = new Vector3 (Random.value * 9 - 4.5f, 0.0f, posZ);
				tile.transform.localRotation = Quaternion.Euler(90, Random.value * 360, 0);
			}
			else
			{
				if(_shrineIndex <= _currTargetIndex)
				{
					tile = TargetTileController.CreateTile(_playerCtrl.transform, TargetTileController.TileType.MonsterTile, TargetTileController.TrapType.None, _gameLevel, _currTargetIndex);

					float randomPosZ = 16 + Random.value * _gameLevel * 0.3f;
					float posZ = (_tileTransformList.Count == 0 ? 0.0f : _tileTransformList[_tileTransformList.Count - 1].transform.localPosition.z) + randomPosZ;
					
					tile.transform.position = new Vector3 (Random.value * 9 - 4.5f, 0, posZ);
					tile.transform.rotation = Quaternion.Euler(-90, Random.value * 360, 0);

					GameObject shrineObj = TargetTileController.CreateTile(_playerCtrl.transform,
					                                                       Random.value > 0.5f ? TargetTileController.TileType.CriticalShrine : TargetTileController.TileType.HealthShrine,
					                                                       TargetTileController.TrapType.None,
					                                                       _gameLevel, _currTargetIndex);
					
					float shrinePosX = ((_tileTransformList.Count == 0 ? 0.0f : _tileTransformList[_tileTransformList.Count - 1].transform.localPosition.x) + tile.transform.position.x) * 0.5f + (Random.value * 10.0f - 5.0f);
					shrineObj.transform.position = new Vector3(shrinePosX, 0.0f, posZ - randomPosZ * 0.5f);
					shrineObj.transform.rotation = Quaternion.Euler(-90, Random.value * 360, 0);
					shrineObj.transform.parent = tile.transform;
					
					_shrineIndex = Random.Range (_currTargetIndex + 15, _currTargetIndex + 20);
				}
				else if(_treasureBoxIndex <= _currTargetIndex)
				{
					tile = TargetTileController.CreateTile(_playerCtrl.transform, TargetTileController.TileType.TreasureBox, TargetTileController.TrapType.None, _gameLevel, _currTargetIndex);

					float randomPosZ = _minDistance + Random.value * (5 + (_gameLevel / 2));
					float posZ = (_tileTransformList.Count == 0 ? 0.0f : _tileTransformList[_tileTransformList.Count - 1].transform.localPosition.z) + randomPosZ;
					
					tile.transform.position = new Vector3 (Random.value * 9 - 4.5f, 0, posZ);
					tile.transform.rotation = Quaternion.Euler(-90, Random.value * 360, 0);

					_treasureBoxIndex = Random.Range (_currTargetIndex + 20, _currTargetIndex + 30);
				}
				else
				{
					tile = TargetTileController.CreateTile(_playerCtrl.transform, TargetTileController.TileType.MonsterTile, TargetTileController.TrapType.None, _gameLevel, _currTargetIndex);

					float randomPosZ = _minDistance + Random.value * (5 + (_gameLevel / 2));
					float posZ = (_tileTransformList.Count == 0 ? 0.0f : _tileTransformList[_tileTransformList.Count - 1].transform.localPosition.z) + randomPosZ;
					
					tile.transform.position = new Vector3 (Random.value * 9 - 4.5f, 0, posZ);
					tile.transform.rotation = Quaternion.Euler(-90, Random.value * 360, 0);
				}
			}
			
			_tileTransformList.Add (tile.transform);
			
			_currTargetIndex++;
		}
		_gameLevel = _currTargetIndex / 5;

		if(_gameLevel > 10)
		{
			_gameLevel = 10;
		}

		//Debug.Log ("Game Level : " + _gameLevel + "\t\t" + "Current TileIndex : " + _currTargetIndex);

		_playerCtrl.SetNextTarget (_tileTransformList[1].GetComponent<TargetTileController>()._monsterObj.transform);
	}

	public List<Transform> GetTileTransform()
	{
		return _tileTransformList;
	}
}
