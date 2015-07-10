using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SpriteChangeController : MonoBehaviour {

	public Sprite[] _sprite;

	public void SetSprite(int index)
	{
		GetComponent<Image> ().sprite = _sprite [index];
	}
}
