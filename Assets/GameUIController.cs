using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour {

	public GameManager _gameMgr;
	public PlayerController _playerCtrl;

	public Button _gameStartButton;
	public Button _wayPointButton;
	public Text _wayPointButtonText;

	public GameObject _waypointSelectUI;
	public Slider _wayPointSlider;
	public Text _wayPointIndexText;
	public Button _waypointStartButton;

	public Button _touchCheckButton;

	public Slider _hpGauge;
	public Text _hpText;
	public Slider _mpGauge;
	public Text _mpText;
	public Text _attackText;

	public Text _clearCountText;
	public Text _comboCountText;
	public Text _comboBounusText;
	public Text _goldCountText;

	public Button _potionButton;
	public Text _potionText;
	public Button _largePotionButton;
	public Text _largePotionText;

	public Text[] _monsterHpText;
	public Text[] _monsterAttackText;

	public Button _shopButton;
	public GameObject _shopUI;
	public Text _shopGoldText;
	public Button[] _itemButton;

	public GameObject _gameOverUI;

	public Image _damageEffectImage;

	public Text _textEffect;

	public Image _screenEffectImage;

	public GameObject _treasureBoxRewardUI;
	public Image _rewardImage;
	public Text _rewardCountText;

	void Awake()
	{
		_shopGoldText.text = "Gold:" + PlayerPrefs.GetInt ("Gold", 0);

		int waypointLevel = PlayerPrefs.GetInt ("Waypoint Level", 0);

		if(PlayerPrefs.GetInt("Waypoint Level", 0) == 0)
		{
			_wayPointButton.gameObject.SetActive(false);
		}
		else
		{
			_wayPointButton.gameObject.SetActive(true);

			int waypointItemCount = (int)GameDataManager.GetGameData(GameDataManager.GameDataID.WaypointItemCount);

			if(waypointItemCount == 0)
			{
				_wayPointButton.interactable = false;
			}
		}

		for(int i = 0 ; i < 3 ; i++)
		{
			if(PlayerPrefs.GetInt("Costume" + i, 0) == 0)
			{
				_itemButton[i + 2].transform.Find("Cost").GetComponent<Text>().text = "Gold:" + Constants._itemCost[i + 2];
			}
			else
			{
				if(PlayerPrefs.GetInt("CurrentCostume", i) == 1)
				{
					_itemButton[i + 2].transform.Find("Cost").GetComponent<Text>().text = "Equiped";
				}
				else
				{
					_itemButton[i + 2].transform.Find("Cost").GetComponent<Text>().text = "Purchased";
				}
			}
		}

		StartCoroutine("CheckWaypointTime");
	}

	public void ChangePlayUI()
	{
		_gameStartButton.gameObject.SetActive (false);
		_wayPointButton.gameObject.SetActive (false);
		_waypointSelectUI.gameObject.SetActive (false);

		_shopButton.gameObject.SetActive (false);

		_hpGauge.gameObject.SetActive (true);
		_touchCheckButton.gameObject.SetActive (true);
		_clearCountText.gameObject.SetActive (true);
		_comboCountText.gameObject.SetActive (true);
		_comboBounusText.gameObject.SetActive (true);
		_goldCountText.gameObject.SetActive (true);
		_goldCountText.text = "Gold:0";

		_potionButton.gameObject.SetActive (true);
		_largePotionButton.gameObject.SetActive (true);
		_potionText.text = PlayerPrefs.GetInt ("Potion Count", 0).ToString();
		_largePotionText.text = PlayerPrefs.GetInt ("LargePotion Count", 0).ToString();

		for(int i = 0 ; i < _monsterHpText.Length ; i++)
		{
			_monsterHpText[i].gameObject.SetActive(true);
		}

		StopCoroutine("CheckWaypointTime");
	}

	IEnumerator CheckWaypointTime()
	{
		int wayPointItemCnt = (int)GameDataManager.GetGameData (GameDataManager.GameDataID.WaypointItemCount);
		int waypointLevel = PlayerPrefs.GetInt("Waypoint Level", 0);

		if(wayPointItemCnt < 5)
		{
			double chargeTime = 0;
			double currTime = 0;

			double.TryParse(PlayerPrefs.GetString("WaypointItem ChargeTime", "0"), out chargeTime);

			System.TimeSpan chargeTimeSpan = System.TimeSpan.FromSeconds(chargeTime);
			System.DateTime chargeDateTime = new System.DateTime(chargeTimeSpan.Ticks);

			System.TimeSpan tspan = System.TimeSpan.FromTicks(System.DateTime.Now.Ticks);
			currTime = tspan.TotalSeconds;

			double t = chargeTime - currTime;

			if(t < 0)
			{
				GameDataManager.AddGameData(GameDataManager.GameDataID.WaypointItemCount, 1);

				GameDataManager.SetGameData(GameDataManager.GameDataID.WaypointItemCount, wayPointItemCnt + 1);
				
				tspan = System.TimeSpan.FromTicks(chargeDateTime.AddSeconds(30).Ticks);
				
				double totalSeconds = tspan.TotalSeconds;

				PlayerPrefs.SetString("WaypointItem ChargeTime", totalSeconds.ToString());

				_wayPointButton.interactable = true;

				_wayPointButtonText.text = wayPointItemCnt +
						"/5\n" + string.Format("{0:D2}", 0) + ":" + string.Format("{0:D2}", 0);
			}
			else
			{
				int hour = (int)t / 3600;
				int minute = ((int)t - (hour * 3600)) / 60;
				int second = (int)t % 60;

				_wayPointButtonText.text = wayPointItemCnt +
						"/5\n" + string.Format("{0:D2}", minute) + ":" + string.Format("{0:D2}", second);
			}

			yield return new WaitForSeconds(0.1f);

			StartCoroutine("CheckWaypointTime");
		}
		else
		{
			_wayPointButtonText.text = wayPointItemCnt + "/5";
		}
	}

	public void ChangeWaypointSelectUI()
	{
		_shopButton.gameObject.SetActive (false);
		_gameStartButton.gameObject.SetActive (false);
		_wayPointButton.gameObject.SetActive (false);

		_wayPointSlider.maxValue = Constants._waypointPosition[PlayerPrefs.GetInt ("Waypoint Level", 0)];
		_wayPointSlider.value = _wayPointSlider.maxValue;
		_wayPointIndexText.text = _wayPointSlider.value.ToString ();

		_waypointSelectUI.gameObject.SetActive (true);
	}

	public void ChangeWaypointSliderValue()
	{
		_wayPointIndexText.text = _wayPointSlider.value.ToString();
	}

	public void ChangeWaypointStartUI()
	{
		_waypointSelectUI.gameObject.SetActive (false);
	}

	public void ScreenFadeInOut(bool isFadeIn)
	{
		StartCoroutine (FadeInOutAction (isFadeIn));
	}

	IEnumerator FadeInOutAction(bool isFadeIn)
	{
		if(isFadeIn)
		{
			float duration = 0.0f;

			while(duration < 0.3f)
			{
				_screenEffectImage.color = new Color(0, 0, 0, 1.0f - duration * 3.333333333f);

				duration += Time.deltaTime;

				yield return null;
			}

			_screenEffectImage.color = new Color(0, 0, 0, 0);

			Debug.Log("Fade In");
		}
		else
		{
			float duration = 0.0f;
			
			while(duration < 0.3f)
			{
				_screenEffectImage.color = new Color(0, 0, 0, 0.0f + duration * 3.333333f);

				duration += Time.deltaTime;
				
				yield return null;
			}
			
			_screenEffectImage.color = new Color(0, 0, 0, 1);
		}
	}

	public void PlayGetTreasureBoxUI(TargetTileController.TreasureBoxRewardType rewardType, int rewardCount)
	{
		StartCoroutine (GetTreasureBoxUIAction (rewardType, rewardCount));
	}

	IEnumerator GetTreasureBoxUIAction(TargetTileController.TreasureBoxRewardType rewardType, int rewardCount)
	{
		_treasureBoxRewardUI.SetActive (true);
		_treasureBoxRewardUI.transform.localScale = Vector3.zero;

		_rewardImage.GetComponent<SpriteChangeController> ().SetSprite ((int)rewardType);
		_rewardCountText.text = "+" + rewardCount;

		iTween.ScaleTo (_treasureBoxRewardUI.gameObject, iTween.Hash ("scale", Vector3.one, "time", 0.3f, "easetype", iTween.EaseType.easeOutBack));

		yield return new WaitForSeconds (2.0f);

		iTween.ScaleTo(_treasureBoxRewardUI.gameObject , iTween.Hash ("scale", Vector3.zero, "time", 0.3f, "easetype", iTween.EaseType.linear));

		yield return new WaitForSeconds (0.3f);

		_treasureBoxRewardUI.gameObject.SetActive (false);
	}

	public void ProcessTouchBegin()
	{
		_playerCtrl.ProcessTouchDown ();
	}

	public void ProcessTouchEnd()
	{
		_playerCtrl.ProcessTouchUp ();
	}

	public void ChangeGameOverUI()
	{
		_gameOverUI.gameObject.SetActive (true);

		_potionButton.gameObject.SetActive (false);
		_largePotionButton.gameObject.SetActive (false);
	}

	public void SetOnOffShop(bool isOn)
	{
		_shopUI.gameObject.SetActive(isOn);
		_shopButton.gameObject.SetActive(!isOn);
		_gameStartButton.gameObject.SetActive(!isOn);
		_wayPointButton.gameObject.SetActive (!isOn);
	}

	public void ProcessShopItemButton(int index)
	{
		switch(index)
		{
		case 0:
			if(PlayerPrefs.GetInt("Gold", 0) > Constants._itemCost[index])
			{
				PlayerPrefs.SetInt("Gold", PlayerPrefs.GetInt("Gold") - Constants._itemCost[index]);
				PlayerPrefs.SetInt("Potion Count", PlayerPrefs.GetInt("Potion Count") + 1);

				_shopGoldText.text = "Gold:" + PlayerPrefs.GetInt("Gold", 0);
			}
			break;

		case 1:
			if(PlayerPrefs.GetInt("Gold", 0) > Constants._itemCost[index])
			{
				PlayerPrefs.SetInt("Gold", PlayerPrefs.GetInt("Gold") - Constants._itemCost[index]);
				PlayerPrefs.SetInt("LargePotion Count", PlayerPrefs.GetInt("LargePotion Count") + 1);
				
				_shopGoldText.text = "Gold:" + PlayerPrefs.GetInt("Gold", 0);
			}
			break;

		case 2:
			SetCostumeUI(0);
			break;

		case 3:
			SetCostumeUI(1);
			break;

		case 4:
			SetCostumeUI(2);
			break;
		}
	}

	void SetCostumeUI(int index)
	{
		for(int i = 0 ; i < 3 ; i++)
		{
			if(i == index)
			{
				if(PlayerPrefs.GetInt("Costume" + index, 0) == 0)
				{
					if(PlayerPrefs.GetInt("Gold", 0) > Constants._itemCost[index + 2])
					{
						PlayerPrefs.SetInt("Gold", PlayerPrefs.GetInt("Gold") - Constants._itemCost[index + 2]);
						_itemButton[index + 2].transform.Find("Cost").GetComponent<Text>().text = "Equiped";
						
						PlayerPrefs.SetInt("Costume" + index, 1);
						PlayerPrefs.SetInt("CurrentCostume", index);
						_playerCtrl.SetCostume((PlayerCostumeController.CostumeGrade)index);
					}
				}
				else
				{
					PlayerPrefs.SetInt("CurrentCostume", index);
					_itemButton[index + 2].transform.Find("Cost").GetComponent<Text>().text = "Equiped";
					
					_playerCtrl.SetCostume((PlayerCostumeController.CostumeGrade)index);
				}
			}
			else
			{
				if(PlayerPrefs.GetInt("Costume" + i, 0) == 0)
				{
					_itemButton[i + 2].transform.Find("Cost").GetComponent<Text>().text = "Gold:" + Constants._itemCost[i + 2];
				}
				else
				{
					_itemButton[i + 2].transform.Find("Cost").GetComponent<Text>().text = "Purchased";
				}
			}
		}
		
		_shopGoldText.text = "Gold:" + PlayerPrefs.GetInt("Gold");
	}

	public void ProcessPotionUseButton()
	{
		int count = PlayerPrefs.GetInt ("Potion Count", 0);

		if(count > 0)
		{
			_playerCtrl._playerHp += 100;
			PlayerPrefs.SetInt("Potion Count", count - 1);
			_potionText.text = PlayerPrefs.GetInt("Potion Count").ToString();

			if(_playerCtrl._playerHp > _playerCtrl._playerMaxHp)
			{
				_playerCtrl._playerHp = _playerCtrl._playerMaxHp;
			}
			SetPlayerHp(_playerCtrl._playerMaxHp, _playerCtrl._playerHp);
		}
	}

	public void ProcessLargePotionUseButton()
	{
		int count = PlayerPrefs.GetInt ("LargePotion Count", 0);
		
		if(count > 0)
		{
			_playerCtrl._playerHp += 200;
			PlayerPrefs.SetInt("LargePotion Count", count - 1);
			_largePotionText.text = PlayerPrefs.GetInt("LargePotion Count").ToString();

			if(_playerCtrl._playerHp > _playerCtrl._playerMaxHp)
			{
				_playerCtrl._playerHp = _playerCtrl._playerMaxHp;
			}
			SetPlayerHp(_playerCtrl._playerMaxHp, _playerCtrl._playerHp);
		}
	}

	public void PlayCriticalTextEffect()
	{
		StartCoroutine (CriticalTextEffectAction ());
	}

	IEnumerator CriticalTextEffectAction()
	{
		_textEffect.text = "CRITICAL";
		_textEffect.color = new Color (1.0f, 0.75f, 0.0f, 1.0f);
		_textEffect.transform.localPosition = new Vector3 (0.0f, 150.0f, 0);
		_textEffect.transform.localScale = Vector3.zero;

		_textEffect.gameObject.SetActive (true);
		
		iTween.ScaleTo (_textEffect.gameObject, iTween.Hash ("scale", new Vector3 (1.5f, 1.5f, 1.5f), "time", 0.2f, "easetype", iTween.EaseType.easeOutBack.ToString ()));

		yield return new WaitForSeconds (1.0f);

		_textEffect.gameObject.SetActive (false);
	}

	/*public void PlayGetBuffTextEffect(string text, Color textColor)
	{
		_textEffect.text = text;
		_textEffect.gameObject.SetActive (true);
		_textEffect.transform.localScale = Vector3.one;
		_textEffect.color = Color.green;

		iTween.MoveTo (_textEffect.gameObject, iTween.Hash ("position", 0.0f, 0.2f, "easetype", iTween.EaseType.easeOutBack.ToString ()));

		iTween.ColorTo (_textEffect.gameObject, iTween.Hash ("alpha", 0.0f, 0.2f, "easetype", iTween.EaseType.easeOutBack.ToString ()));
		
		yield return new WaitForSeconds (1.0f);
		
		_textEffect.gameObject.SetActive (false);
	}

	IEnumerator GetBuffTextEffectAction()
	{
	}*/

	public void SetPlayerAttack(int attack)
	{
		_attackText.text = "Attack : " + attack.ToString ();
	}

	public void SetPlayerHp(int maxHp, int hp)
	{
		_hpText.text = hp + "/" + maxHp;

		_hpGauge.value = (hp * 1.0f) / maxHp;
	}

	public void SetPlayerMp(int maxMp, int mp)
	{
		_mpText.text = mp + "/" + maxMp;

		_mpGauge.value = (mp * 1.0f) / maxMp;
	}

	public void SetClearAndComboCount(int clearCount, int comboCount, int goldCount)
	{
		_clearCountText.text = "Clear:" + clearCount;
		_comboCountText.text = "Combo:" + comboCount;
		_comboBounusText.text = comboCount < 10 ? "GOLD + " + (comboCount * 10) + "%" : "GOLD + 100%";
		_goldCountText.text = "Gold:" + goldCount;
	}

	public void PlayDamageEffect()
	{
		_damageEffectImage.gameObject.SetActive (true);
		_damageEffectImage.color = new Color (1, 1, 1, 0.3f);

		StopCoroutine ("DamageEffectCoroutine");
		StartCoroutine ("DamageEffectCoroutine");
	}

	IEnumerator DamageEffectCoroutine()
	{
		while(_damageEffectImage.color.a > 0.0f)
		{
			_damageEffectImage.color = new Color (1, 1, 1, _damageEffectImage.color.a - Time.deltaTime);

			yield return null;
		}

		_damageEffectImage.gameObject.SetActive (false);
	}

	public void GoToMain()
	{
		Application.LoadLevelAsync ("GameScene");
	}

	void Update()
	{
		List<Transform> tileList = _gameMgr.GetTileTransform ();

		if(tileList.Count > 0)
		{
			for(int i = 0 ; i < 2 ; i++)
			{
				if(tileList[i].GetComponent<TargetTileController>()._tileType == TargetTileController.TileType.MonsterTile)
				{
					MonsterController monsterCtrl = tileList[i].GetComponent<TargetTileController>()._monsterObj.GetComponent<MonsterController>();
					_monsterHpText[i].GetComponent<RectTransform> ().anchoredPosition = 
						UIHelper.WorldToCanvas (_monsterHpText[i].canvas, monsterCtrl.transform.position) + new Vector2(0, -60);

					_monsterHpText[i].text = monsterCtrl._hp.ToString();
					_monsterAttackText[i].text = monsterCtrl._attack.ToString();
				}
			}
		}

		if (Input.GetKeyUp (KeyCode.P)) 
		{
			PlayerPrefs.SetInt("Gold", 1000000);
		}
		else if(Input.GetKeyUp(KeyCode.O))
		{
			PlayerPrefs.DeleteAll();
		}
	}
}

public static class UIHelper
{
	public static Vector2 WorldToCanvas(Canvas canvas,
	                                    Vector3 world_position,
	                                    Camera camera = null)
	{
		if (camera == null)
		{
			camera = Camera.main;
		}
		
		var viewport_position = camera.WorldToViewportPoint(world_position);
		var canvas_rect = canvas.GetComponent<RectTransform>();
		
		return new Vector2((viewport_position.x * canvas_rect.sizeDelta.x) - (canvas_rect.sizeDelta.x * 0.5f),
		                   (viewport_position.y * canvas_rect.sizeDelta.y) - (canvas_rect.sizeDelta.y * 0.5f));
	}
}
