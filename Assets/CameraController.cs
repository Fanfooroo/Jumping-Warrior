using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public enum CameraState
	{
		MainMenu,
		WaitInput,
		WaypointStartAction,
		Jumping,
		Attack,
		CriticalAttack,
		Death,
	};

	public CameraState _cameraState = CameraState.MainMenu;
	public PlayerController _playerCtrl;

	private float _smoothValue;

	private Vector3 _diePosition;

	public void SetWaitInputAngle()
	{
		_smoothValue = 0;
		_cameraState = CameraState.WaitInput;
	}
	public void SetWaypointStartActionAngle()
	{
		_cameraState = CameraState.WaypointStartAction;
	}
	public void SetJumpingAngle()
	{
		_cameraState = CameraState.Jumping;
	}

	public void SetAttackAngle()
	{
		_cameraState = CameraState.Attack;
	}

	public void SetCriticalAngle()
	{
		_cameraState = CameraState.CriticalAttack;
	}

	public void SetDeathAngle()
	{
		_cameraState = CameraState.Death;
		_diePosition = _playerCtrl.transform.position;
	}
	
	// Update is called once per frame
	void Update () 
	{
		switch(_cameraState)
		{
		case CameraState.MainMenu:
			break;

		case CameraState.WaitInput:
		{
			Vector3 playerPos = _playerCtrl.transform.position;
			Vector3 nextTargetPos = _playerCtrl.GetNextTarget().position;
			
			float posYFromZ = 13 + (nextTargetPos.z - playerPos.z - 9) * 1.2f;

			transform.position = Vector3.Lerp(transform.position, new Vector3(playerPos.x, posYFromZ, playerPos.z - 5.0f), Time.deltaTime * 3.0f * _smoothValue);

			Vector3 lookPos = playerPos + new Vector3 ((nextTargetPos.x - playerPos.x) * 0.3f, 0, transform.position.y * 0.3f);
			transform.forward = Vector3.Lerp (transform.forward, lookPos - transform.position, Time.deltaTime * 1.5f * _smoothValue);

			if(_smoothValue < 1.0f)
			{
				_smoothValue += Time.deltaTime;
			}
			break;
		}

		case CameraState.WaypointStartAction:
		{
			transform.position = new Vector3(1.9f, 6.22f, -4.51f);
			transform.rotation = Quaternion.Euler(50, 340, 0);
			break;
		}

		case CameraState.Jumping:
		{
			Vector3 targetPos = _playerCtrl.transform.position;
			transform.position = Vector3.Lerp(transform.position, new Vector3(targetPos.x, 15, targetPos.z - 5.0f), Time.deltaTime * 3.0f);
			
			Vector3 lookPos = targetPos + new Vector3(0, 0, 7);
			transform.forward = Vector3.Slerp (transform.forward, lookPos - transform.position, Time.deltaTime * 3.0f);
			break;
		}
		case CameraState.CriticalAttack:
		{
			Vector3 targetPos = _playerCtrl.transform.position;
			transform.position = Vector3.Lerp(transform.position, new Vector3(targetPos.x, 5, targetPos.z - 5.0f), Time.deltaTime * 5.0f);
			
			Vector3 lookPos = targetPos + new Vector3(0, 0, 2);
			transform.forward = Vector3.Lerp (transform.forward, lookPos - transform.position, Time.deltaTime * 5.0f);
			break;
		}
		case CameraState.Attack:
		{
			Vector3 targetPos = _playerCtrl.transform.position;
			transform.position = Vector3.Lerp(transform.position, new Vector3(targetPos.x, 15, targetPos.z - 5.0f), Time.deltaTime * 3.0f);
			
			Vector3 lookPos = targetPos + new Vector3(0, 0, 2);
			transform.forward = Vector3.Lerp (transform.forward, lookPos - transform.position, Time.deltaTime * 3.0f);
			break;
		}
		case CameraState.Death:
		{
			transform.position = Vector3.Lerp(transform.position, new Vector3(_diePosition.x, 3, _diePosition.z - 3.0f), Time.deltaTime * 1.0f);

			transform.forward = Vector3.Slerp (transform.forward, _playerCtrl.GetPelvisTransform().position - transform.position, Time.deltaTime * 3.0f);
			break;
		}
		}
	}
}
