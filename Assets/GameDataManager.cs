using UnityEngine;
using System.Collections;

public class GameDataManager {

	public enum GameDataID
	{
		IsUnCompletedData,
		WaypointItemCount,
	};

	public enum CurrentStateDataID
	{
		TileIndex,
		ClearCount,
		ComboCount,
		Gold,
		PlayerHp,
		PlayerMp,
		PlayerCriticalBonus,
		PlayerCriticalBonusCount,
	};

	static public void SetGameData(GameDataID dataId, object value)
	{
		switch(dataId)
		{
		case GameDataID.IsUnCompletedData:
			PlayerPrefsX.SetBool(GameDataID.IsUnCompletedData.ToString(), (bool)value);
			break;

		case GameDataID.WaypointItemCount:
			PlayerPrefs.SetInt(GameDataID.WaypointItemCount.ToString(), (int)value);
			break;
		}
	}

	static public void AddGameData(GameDataID dataId, object value)
	{
		switch(dataId)
		{
		case GameDataID.WaypointItemCount:
			PlayerPrefs.SetInt(GameDataID.WaypointItemCount.ToString(), PlayerPrefs.GetInt(GameDataID.WaypointItemCount.ToString(), 5) + (int)value);
			break;

		default:
			Debug.LogError("Wrong Data Id");
			break;

		}
	}

	static public object GetGameData(GameDataID dataId)
	{
		switch(dataId)
		{
		case GameDataID.IsUnCompletedData:
			return PlayerPrefsX.GetBool(dataId.ToString(), false);

		case GameDataID.WaypointItemCount:
			return PlayerPrefs.GetInt(dataId.ToString(), 5);

		default:
			Debug.Log("No Data");
			return null;
		}
	}

	static public void SaveCurrentGameState(int tileIndex, int clearCount, int comboCount, int gold, int hp, int mp, float criticalBouns, int criticalBonusCount)
	{
		PlayerPrefs.SetInt (CurrentStateDataID.TileIndex.ToString (), tileIndex);
		PlayerPrefs.SetInt (CurrentStateDataID.ClearCount.ToString (), clearCount);
		PlayerPrefs.SetInt (CurrentStateDataID.ComboCount.ToString (), comboCount);
		PlayerPrefs.SetInt (CurrentStateDataID.Gold.ToString (), gold);
		PlayerPrefs.SetInt (CurrentStateDataID.PlayerHp.ToString (), hp);
		PlayerPrefs.SetInt (CurrentStateDataID.PlayerMp.ToString (), mp);
		PlayerPrefs.SetFloat (CurrentStateDataID.PlayerCriticalBonus.ToString (), criticalBouns);
		PlayerPrefs.SetInt (CurrentStateDataID.PlayerCriticalBonusCount.ToString (), criticalBonusCount);
	}

	static public object LoadPrevGameState(CurrentStateDataID dataType)
	{
		switch(dataType)
		{
		case CurrentStateDataID.TileIndex:
			return PlayerPrefs.GetInt (CurrentStateDataID.TileIndex.ToString (), 0);

		case CurrentStateDataID.ClearCount:
			return PlayerPrefs.GetInt (CurrentStateDataID.ClearCount.ToString (), 0);

		case CurrentStateDataID.ComboCount:
			return PlayerPrefs.GetInt (CurrentStateDataID.ComboCount.ToString (), 0);

		case CurrentStateDataID.Gold:
			return PlayerPrefs.GetInt (CurrentStateDataID.Gold.ToString (), 0);

		case CurrentStateDataID.PlayerHp:
			return PlayerPrefs.GetInt (CurrentStateDataID.PlayerHp.ToString (), 0);

		case CurrentStateDataID.PlayerMp:
			return PlayerPrefs.GetInt (CurrentStateDataID.PlayerMp.ToString (), 0);

		case CurrentStateDataID.PlayerCriticalBonus:
			return PlayerPrefs.GetFloat(CurrentStateDataID.PlayerCriticalBonus.ToString(), 0);

		case CurrentStateDataID.PlayerCriticalBonusCount:
			return PlayerPrefs.GetInt(CurrentStateDataID.PlayerCriticalBonusCount.ToString(), 0);

		default:
			Debug.Log("No Data");
			return null;
		}
	}
}
