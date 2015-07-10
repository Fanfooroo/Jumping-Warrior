using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

	public enum PlayerState
	{
		MainMenu,
		WaitInput,
		SetGauge,
		Jumping,
		Attack,
		CriticalAttackState,
		Death,
	};

	public GameManager _gameManager;
	public CameraController _cameraCtrl;
	public GameUIController _gameUICtrl;

	public PlayerState _playerState = PlayerState.MainMenu;

	public Transform _nextTargetTransform;
	public int _currTileIndex;

	public Slider _jumpingGaugeSlider;
	public ParticleSystem _criticalBound;

	public ParticleSystem _attackEffect;
	public ParticleSystem _coinParticle;

	private bool _isFill;
	private float _fillSpeed = 0.5f;

	private float _V0;
	private float _gravity = 10;
	private float _time;

	public int _equipmentGrade;

	public int _playerMaxHp;
	public int _playerAttack;

	public int _playerHp;

	public float _criticalRadius = 0.75f;
	public float _criticalBonus = 1.0f;
	public int _criticalBonusCount = 0;

	void Start()
	{
		_gameManager = GameObject.Find ("GameManager").GetComponent<GameManager> ();
		_gameUICtrl.SetPlayerHp (_playerMaxHp, _playerMaxHp);
		_gameUICtrl.SetPlayerAttack (_playerAttack);

		SetCostume ((PlayerCostumeController.CostumeGrade)PlayerPrefs.GetInt ("CurrentCostume", 0));
	}

	public void SetCostume(PlayerCostumeController.CostumeGrade grade)
	{
		_playerHp = Constants._costumeHp [(int)grade];
		_playerMaxHp = _playerHp;
		_playerAttack = Constants._attack [(int)grade];

		_equipmentGrade = (int)grade;

		_criticalRadius = Constants._defaultPlayerCriticalRadius * Constants._critical [(int)grade];
		_criticalBound.startSize = Constants._defaultPlayerCriticalScale * Constants._critical [(int)grade];

		_gameUICtrl.SetPlayerHp (_playerMaxHp, _playerMaxHp);
		_gameUICtrl.SetPlayerAttack (_playerAttack);

		GetComponent<PlayerCostumeController> ().SetCostume ((PlayerCostumeController.CostumeGrade)PlayerPrefs.GetInt ("CurrentCostume", 0));
	}

	public void SetCriticalBounds()
	{
		_criticalBound.startSize = Constants._defaultPlayerCriticalScale * Constants._critical [_equipmentGrade] * _criticalBonus;
	}

	public void PlayWaypointStartAction(int level)
	{
		switch(level)
		{
		case 0:
			GetComponent<Animation>().CrossFade("hit_04", 0.15f);
			iTween.MoveTo(gameObject, iTween.Hash("y", 30.0f, "time", 0.5f, "delay", 0.5f));
			break;

		case 1:
			iTween.MoveTo(gameObject, iTween.Hash("y", 0.7f, "time", 0.2f));
			GetComponent<Animation>().CrossFade("skill_03_1", 0.15f);
			break;

		case 2:
			GetComponent<Animation>().CrossFade("idle");
			break;
		}
	}

	public void SetWaitInput()
	{
		int gaugeLevel = _gameManager._clearCount / 5;

		_jumpingGaugeSlider.GetComponent<Animator> ().speed = 0.5f + gaugeLevel * 0.05f;
		_fillSpeed = 0.5f + gaugeLevel * 0.05f;
		_jumpingGaugeSlider.value = 0;
		_jumpingGaugeSlider.gameObject.SetActive (true);
		_playerState = PlayerState.WaitInput;
	}

	public void SetAttackState()
	{
		if (_playerState != PlayerState.Death) 
		{
			GetComponent<Rigidbody> ().isKinematic = true;
			GetComponent<Rigidbody> ().velocity = Vector3.zero;

			_playerState = PlayerState.Attack;
			GetComponent<Animation> ().CrossFade ("escape");

			_cameraCtrl.SetAttackAngle ();

			StartCoroutine (DashToMonster ());

		}
	}

	IEnumerator DashToMonster()
	{
		Vector3 dir = transform.position - _nextTargetTransform.position;
		dir.Normalize ();
		dir *= 1.5f;

		Vector3 targetPos = _nextTargetTransform.position + dir;
		float duration = Vector3.Distance (targetPos, transform.position) / 5.0f;
		iTween.MoveTo (gameObject, targetPos, duration);

		yield return new WaitForSeconds (duration * 0.5f);

		GetComponent<Animation> ().CrossFade ("hit_01", 0.1f);
		_nextTargetTransform.gameObject.GetComponent<MonsterController> ().Attack ();

		yield return new WaitForSeconds (0.3f);

		int damage = _nextTargetTransform.gameObject.GetComponent<MonsterController> ().HitByPlayer(_playerAttack);

		_playerHp -= damage;

		_gameUICtrl.SetPlayerHp (_playerMaxHp, _playerHp);
		_gameUICtrl.PlayDamageEffect ();

		if(_criticalBonusCount > 0)
		{
			_criticalBonusCount--;
			
			if(_criticalBonusCount <= 0)
			{
				_criticalBonus = 1.0f;
				_criticalBound.startSize = Constants._defaultPlayerCriticalScale * Constants._critical [_equipmentGrade] * _criticalBonus;
			}
		}

		yield return new WaitForSeconds (0.2f);

		if(_playerHp <= 0)
		{
			SetDieState(false);
		}
		else
		{
			GetComponent<Animation> ().CrossFade ("idle");

			yield return new WaitForSeconds (0.5f);

			_gameManager.SetNextTarget ();
			SetWaitInput ();
			_cameraCtrl.SetWaitInputAngle ();

			_gameManager._clearCount++;
			_gameManager._comboCount = 0;

			_gameUICtrl.SetClearAndComboCount(_gameManager._clearCount, _gameManager._comboCount, _gameManager._goldCount);
		}
	}

	public void CriticalAttackState()
	{
		if(_playerState != PlayerState.Death)
		{
			_playerState = PlayerState.CriticalAttackState;

			_cameraCtrl.SetCriticalAngle ();

			GetComponent<Rigidbody> ().isKinematic = true;
			GetComponent<Rigidbody> ().velocity = Vector3.zero;

			GetComponent<Animation> ().CrossFade ("hit_03", 0.1f);

			StartCoroutine (CriticalAttack());
		}
	}

	IEnumerator CriticalAttack()
	{
		yield return new WaitForSeconds (0.3f);

		_attackEffect.Play ();
		_attackEffect.playbackSpeed = 1.5f;

		yield return new WaitForSeconds (0.1f);

		_nextTargetTransform.GetComponent<MonsterController> ().HitByPlayer (10000);
		_coinParticle.Play ();

		_gameUICtrl.PlayCriticalTextEffect ();

		yield return new WaitForSeconds (0.1f);

		int goldAmount = Random.Range (_gameManager._gameLevel * 100, (_gameManager._gameLevel + 1) * 100);
		
		if(_gameManager._comboCount < 10)
		{
			goldAmount += (int)(goldAmount * (_gameManager._comboCount * 0.1f));
		}
		else
		{
			goldAmount += goldAmount;
		}
		
		_gameManager._goldCount += goldAmount;

		_gameManager._clearCount++;
		_gameManager._comboCount++;

		_gameUICtrl.SetClearAndComboCount(_gameManager._clearCount, _gameManager._comboCount, _gameManager._goldCount);

		GetComponent<Animation> ().CrossFade ("idle");

		if(_criticalBonusCount > 0)
		{
			_criticalBonusCount--;

			if(_criticalBonusCount <= 0)
			{
				_criticalBonus = 1.0f;
				_criticalBound.startSize = Constants._defaultPlayerCriticalScale * Constants._critical [_equipmentGrade] * _criticalBonus;
			}
		}

		yield return new WaitForSeconds (0.5f);

		SetWaitInput ();
		_cameraCtrl.SetWaitInputAngle ();
		_gameManager.SetNextTarget ();
	}

	public void WaypointState()
	{
		_cameraCtrl.SetAttackAngle ();

		GetComponent<Rigidbody> ().isKinematic = true;
		GetComponent<Rigidbody> ().velocity = Vector3.zero;

		StartCoroutine (WaypointGetAction ());
	}

	IEnumerator WaypointGetAction()
	{
		GetComponent<Animation> ().CrossFade ("idle");

		yield return new WaitForSeconds (1.0f);

		_gameManager._clearCount++;

		_gameUICtrl.SetClearAndComboCount(_gameManager._clearCount, _gameManager._comboCount, _gameManager._goldCount);

		SetWaitInput ();
		_cameraCtrl.SetWaitInputAngle ();
		_gameManager.SetNextTarget ();
	}

	public void ShrineState(TargetTileController.TileType tileType)
	{
		_cameraCtrl.SetCriticalAngle ();

		GetComponent<Rigidbody> ().isKinematic = true;
		GetComponent<Rigidbody> ().velocity = Vector3.zero;

		StartCoroutine (ShrineGetAction (tileType));
	}

	IEnumerator ShrineGetAction(TargetTileController.TileType tileType)
	{
		GetComponent<Animation> ().CrossFade ("win", 0.15f);

		yield return new WaitForSeconds (1.0f);

		switch(tileType)
		{
		case TargetTileController.TileType.HealthShrine:
		{
			int targetHp = _playerHp + (int)(_playerMaxHp * 0.3f);
			
			if(targetHp > _playerMaxHp)
			{
				targetHp = _playerMaxHp;
			}

			StartCoroutine(PlayerGaugeRecharge(targetHp));
			break;
		}
			
		case TargetTileController.TileType.ManaShrine:
			break;
			
		case TargetTileController.TileType.CriticalShrine:
			_criticalBonus = 2.0f;
			_criticalBonusCount = 3;
			
			_criticalBound.startSize = Constants._defaultPlayerCriticalScale * Constants._critical [_equipmentGrade] * _criticalBonus;
			break;
		}
		
		yield return new WaitForSeconds (1.0f);

		GetComponent<Animation> ().CrossFade ("idle");
		
		SetWaitInput ();
		_cameraCtrl.SetWaitInputAngle ();
		_gameManager.SetNextTarget ();
	}

	IEnumerator PlayerGaugeRecharge(int targetHp)
	{
		float currHp = _playerHp;

		while(currHp < targetHp)
		{
			float hp = Mathf.Lerp(currHp, (float)targetHp, Time.deltaTime);

			if(hp > targetHp - 2)
			{
				currHp = targetHp;
			}
			else
			{
				currHp = hp;
			}

			_gameUICtrl.SetPlayerHp(_playerMaxHp, (int)currHp);

			yield return null;
		}

		_playerHp = targetHp;
		_gameUICtrl.SetPlayerHp(_playerMaxHp, _playerHp);
	}

	public void TreasureBoxState()
	{
		_cameraCtrl.SetCriticalAngle ();
		
		GetComponent<Rigidbody> ().isKinematic = true;
		GetComponent<Rigidbody> ().velocity = Vector3.zero;
		
		StartCoroutine (TreasureBoxGetAction ());
	}

	IEnumerator TreasureBoxGetAction()
	{
		GetComponent<Animation> ().CrossFade ("win", 0.15f);

		yield return new WaitForSeconds (1.0f);

		float rand = Random.value;

		if(rand < 0.7f)
		{
			int goldAmount = Random.Range(_gameManager._gameLevel * 500, _gameManager._gameLevel * 1000);

			_gameManager._goldCount += goldAmount;

			_gameUICtrl.PlayGetTreasureBoxUI (TargetTileController.TreasureBoxRewardType.Gold, goldAmount);
			_gameUICtrl.SetClearAndComboCount(_gameManager._clearCount, _gameManager._comboCount, _gameManager._goldCount);
		}
		else
		{
			_gameUICtrl.PlayGetTreasureBoxUI (TargetTileController.TreasureBoxRewardType.WaypointScroll, 1);
			GameDataManager.AddGameData(GameDataManager.GameDataID.WaypointItemCount, 1);
		}

		yield return new WaitForSeconds (1.0f);

		GetComponent<Animation> ().CrossFade ("idle", 0.15f);

		SetWaitInput ();
		_cameraCtrl.SetWaitInputAngle ();
		_gameManager.SetNextTarget ();
	}

	public void SetDamage(int damage)
	{
		_playerHp -= damage;

		_gameUICtrl.SetPlayerHp (_playerMaxHp, _playerHp);
		_gameUICtrl.PlayDamageEffect ();
		
		if(_playerHp <= 0)
		{
			SetDieState(false);
		}
	}

	public void SetDieState(bool isRagdoll)
	{
		if(_playerState == PlayerState.Death)
		{
			return;
		}

		if(isRagdoll)
		{
			_nextTargetTransform.parent.parent.parent.GetComponent<Collider> ().enabled = true;
			SetOnOffRagdoll (true);

			GetComponent<Animation> ().Stop ();
		}
		else
		{
			GetComponent<Animation> ().CrossFade ("death");
			GetComponent<Rigidbody>().isKinematic = true;
		}

		PlayerPrefs.SetInt ("Gold", PlayerPrefs.GetInt ("Gold") + _gameManager._goldCount);

		_playerState = PlayerState.Death;
		
		_cameraCtrl.SetDeathAngle();
		
		_jumpingGaugeSlider.gameObject.SetActive(false);

		StartCoroutine (DieCoroutine ());
	}

	IEnumerator DieCoroutine()
	{
		yield return new WaitForSeconds (3.0f);

		_gameUICtrl.ChangeGameOverUI();
	}

	public void SetNextTarget(Transform nextTransform)
	{
		_nextTargetTransform = nextTransform;
	}

	public Transform GetNextTarget()
	{
		return _nextTargetTransform;
	}

	void SetOnOffRagdoll(bool isOn)
	{
		Transform pelvis = transform.Find ("Bip001/Bip001 Pelvis");
		Transform leftThigh = pelvis.Find ("Bip001 L Thigh");
		Transform rightThigh = pelvis.Find ("Bip001 R Thigh");
		Transform leftCalf = leftThigh.Find ("Bip001 L Calf");
		Transform rightCalf = rightThigh.Find ("Bip001 R Calf");
		Transform spine = pelvis.Find ("Bip001 Spine");
		Transform head = spine.Find ("Bip001 Neck/Bip001 Head");
		Transform leftUpperArm = spine.Find ("Bip001 Neck/Bip001 L Clavicle/Bip001 L UpperArm");
		Transform rightUpperArm = spine.Find ("Bip001 Neck/Bip001 R Clavicle/Bip001 R UpperArm");
		Transform leftForearm = leftUpperArm.Find ("Bip001 L Forearm");
		Transform rightForearm = rightUpperArm.Find ("Bip001 R Forearm");
		Transform weapon = transform.Find ("weapon");

		pelvis.GetComponent<Rigidbody>().isKinematic = !isOn;
		pelvis.GetComponent<Collider>().enabled = isOn;
		leftThigh.GetComponent<Rigidbody>().isKinematic = !isOn;
		leftThigh.GetComponent<Collider>().enabled = isOn;
		rightThigh.GetComponent<Rigidbody> ().isKinematic = !isOn;
		rightThigh.GetComponent<Collider> ().enabled = isOn;
		leftCalf.GetComponent<Rigidbody> ().isKinematic = !isOn;
		leftCalf.GetComponent<Collider> ().enabled = isOn;
		rightCalf.GetComponent<Rigidbody> ().isKinematic = !isOn;
		rightCalf.GetComponent<Collider> ().enabled = isOn;
		spine.GetComponent<Rigidbody> ().isKinematic = !isOn;
		spine.GetComponent<Collider> ().enabled = isOn;
		head.GetComponent<Rigidbody> ().isKinematic = !isOn;
		head.GetComponent<Collider> ().enabled = isOn;
		leftUpperArm.GetComponent<Rigidbody> ().isKinematic = !isOn;
		leftUpperArm.GetComponent<Collider> ().enabled = isOn;
		rightUpperArm.GetComponent<Rigidbody> ().isKinematic = !isOn;
		rightUpperArm.GetComponent<Collider> ().enabled = isOn;
		leftForearm.GetComponent<Rigidbody> ().isKinematic = !isOn;
		leftForearm.GetComponent<Collider> ().enabled = isOn;
		rightForearm.GetComponent<Rigidbody> ().isKinematic = !isOn;
		rightForearm.GetComponent<Collider> ().enabled = isOn;
		weapon.GetComponent<Rigidbody> ().isKinematic = !isOn;
		weapon.GetComponent<Collider> ().enabled = isOn;

		if(isOn == true)
		{
			pelvis.GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity;
			leftThigh.GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity;
			rightThigh.GetComponent<Rigidbody> ().velocity = GetComponent<Rigidbody>().velocity;
			leftCalf.GetComponent<Rigidbody> ().velocity = GetComponent<Rigidbody>().velocity;
			rightCalf.GetComponent<Rigidbody> ().velocity = GetComponent<Rigidbody>().velocity;
			spine.GetComponent<Rigidbody> ().velocity = GetComponent<Rigidbody>().velocity;
			head.GetComponent<Rigidbody> ().velocity = GetComponent<Rigidbody>().velocity;
			leftUpperArm.GetComponent<Rigidbody> ().velocity = GetComponent<Rigidbody>().velocity;
			rightUpperArm.GetComponent<Rigidbody> ().velocity = GetComponent<Rigidbody>().velocity;
			leftForearm.GetComponent<Rigidbody> ().velocity = GetComponent<Rigidbody>().velocity;
			rightForearm.GetComponent<Rigidbody> ().velocity = GetComponent<Rigidbody>().velocity;
			weapon.GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity;
		}

		GetComponent<Rigidbody> ().isKinematic = isOn;
		GetComponent<Collider> ().enabled = !isOn;
	}

	public Transform GetPelvisTransform()
	{
		return transform.Find ("Bip001/Bip001 Pelvis");
	}

	public void ProcessTouchDown()
	{
		if(_playerState == PlayerState.WaitInput)
		{
			_jumpingGaugeSlider.animator.speed = 0;
			_jumpingGaugeSlider.value = 0;
			_isFill = true;
			_playerState = PlayerState.SetGauge;
		}
	}

	public void ProcessTouchUp()
	{
		if(_playerState == PlayerState.SetGauge)
		{
			_playerState = PlayerState.Jumping;
			GetComponent<Rigidbody>().isKinematic = false;
			
			Vector3 dir = _jumpingGaugeSlider.transform.right + Vector3.up * 0.5f;
			dir.Normalize();
			
			float angle = Vector3.Angle(dir, _jumpingGaugeSlider.transform.right);
			float distance = 5 + _jumpingGaugeSlider.value * 20;
			
			float V = distance * 0.5f * _gravity / (Mathf.Sin(Mathf.Deg2Rad * angle) * Mathf.Cos(Mathf.Deg2Rad * angle));
			
			V = Mathf.Sqrt(V);
			
			GetComponent<Rigidbody>().velocity = dir * V;
			
			_jumpingGaugeSlider.gameObject.SetActive(false);
			
			_cameraCtrl.SetJumpingAngle();
		}
	}

	// Update is called once per frame
	void Update () {
		switch(_playerState)
		{
		case PlayerState.MainMenu:

			break;

		case PlayerState.WaitInput:
			transform.forward = Vector3.Lerp(transform.forward,  _nextTargetTransform.position - transform.position, Time.deltaTime * 5.0f);
			break;

		case PlayerState.SetGauge:
			if(_isFill)
			{
				_jumpingGaugeSlider.value += Time.deltaTime * _fillSpeed;

				if(_jumpingGaugeSlider.value >= 1.0f)
				{
					_isFill = false;
				}
			}
			else
			{
				_jumpingGaugeSlider.value -= Time.deltaTime * _fillSpeed;

				if(_jumpingGaugeSlider.value <= 0.0f)
				{
					_isFill = true;
				}
			}
			break;

		case PlayerState.Jumping:

			break;

		case PlayerState.Attack:
		{
			Vector3 lookPos = new Vector3(_nextTargetTransform.position.x - transform.position.x, 0, _nextTargetTransform.position.z - transform.position.z);
			transform.forward = Vector3.Lerp(transform.forward, lookPos, Time.deltaTime * 30.0f);
			break;
		}

		case PlayerState.CriticalAttackState:
		{
			Vector3 lookPos = new Vector3(_nextTargetTransform.position.x - transform.position.x, 0, _nextTargetTransform.position.z - transform.position.z);
			transform.forward = Vector3.Lerp(transform.forward, lookPos, Time.deltaTime * 50.0f);
			break;
		}
		}
	}
}
