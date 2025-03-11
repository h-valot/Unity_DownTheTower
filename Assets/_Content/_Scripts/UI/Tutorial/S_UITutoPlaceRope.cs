using Sirenix.OdinInspector;
using UnityEngine;

public class UITutoPlaceRope : UITutoOnColliderEnters
{
	[FoldoutGroup("Tweakable values")][SerializeField] private TutoColliderType m_type;


	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	private void OnColliderEnters(TutoColliderType type)
	{
		if (m_type == type)
		{
			Show();
		}
	}
}