using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MonsterController : MonoBehaviour {

	public int _hp;
	public int _attack;
	private Transform _playerTransform;

	public enum MonsterType
	{
		Goblin,
		Frog,
		Skeleton,
	}

	public MonsterType _monsterType;
	public float _boundRadius;

	public void SetMonster(Transform playerTransform, int level)
	{
		int grade = level / 3 + 1;

		_hp = Random.Range (grade * 10, (grade + 1) * 10) + Random.Range (grade * 10, (grade + 1) * 10);
		_attack = 5 * grade + Random.Range (0, 3) * grade;

		switch(_monsterType)
		{
		case MonsterType.Goblin:
			_hp /= 2;
			_boundRadius = 0.4f;

			GetComponent<Animation> ().CrossFade ("A_stand_0" + Random.Range (1, 4));
			break;

		case MonsterType.Frog:
			_hp /= 3;
			_attack = (int)(_attack * 1.5f);
			_boundRadius = 0.8f;

			GetComponent<Animation> ().CrossFade ("A_idle");
			break;
		}

		_playerTransform = playerTransform;
	}

	public void Attack()
	{
		iTween.LookTo (gameObject, _playerTransform.position, 0.1f);

		switch(_monsterType)
		{
		case MonsterType.Goblin:
			GetComponent<Animation> () ["A_hit_01"].time = 0.5f;
			GetComponent<Animation> () ["A_hit_01"].speed = 2.0f;
			GetComponent<Animation> ().CrossFade ("A_hit_01", 0.1f);
			break;

		case MonsterType.Frog:
			GetComponent<Animation> () ["A_hit_03"].time = 1.0f;
			GetComponent<Animation> ().CrossFade ("A_hit_03", 0.1f);
			break;
		}
	}

	public int HitByPlayer(int damage)
	{
		_hp -= damage;

		if(_hp <= 0)
		{
			_hp = 0;
			GetComponent<Animation>().CrossFade("A_death", 0.1f);
		}
		else
		{
			GetComponent<Animation>().CrossFade("A_stun");
		}

		return _attack;
	}

	public int GetAttackPoint()
	{
		return _attack;
	}
}
