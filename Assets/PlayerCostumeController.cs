using UnityEngine;
using System.Collections;

public class PlayerCostumeController : MonoBehaviour {

	public enum CostumeGrade
	{
		Common,
		Uncommon,
		Rare,
	};

	public Renderer[] _commonSet;
	public Renderer[] _uncommonSet;
	public Renderer[] _rareSet;

	public void SetCostume(CostumeGrade level)
	{
		switch(level)
		{
		case CostumeGrade.Common:
			foreach(Renderer r in _commonSet)
			{
				r.enabled = true;
			}
			foreach(Renderer r in _uncommonSet)
			{
				r.enabled = false;
			}
			foreach(Renderer r in _rareSet)
			{
				r.enabled = false;
			}
			break;

		case CostumeGrade.Uncommon:
			foreach(Renderer r in _commonSet)
			{
				r.enabled = false;
			}
			foreach(Renderer r in _uncommonSet)
			{
				r.enabled = true;
			}
			foreach(Renderer r in _rareSet)
			{
				r.enabled = false;
			}
			break;

		case CostumeGrade.Rare:
			foreach(Renderer r in _commonSet)
			{
				r.enabled = false;
			}
			foreach(Renderer r in _uncommonSet)
			{
				r.enabled = false;
			}
			foreach(Renderer r in _rareSet)
			{
				r.enabled = true;
			}
			break;
		}
	}
}
