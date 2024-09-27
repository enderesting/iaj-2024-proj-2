using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
	public class DivineSmite : Action
	{
		private float manaCost = 2.0f;
		private AutonomousCharacter character;
		private GameObject target;
		private float xpChange = 3;
		private float expectedXPChange = 2.7f;

		public DivineSmite(AutonomousCharacter character, GameObject target) : base("DivineSmite")
		{
			this.character = character;
			this.target = target;
		}

		public override bool CanExecute()
		{
			return target is not null && target.activeInHierarchy && target.tag.Equals("Skeleton") &&
			       GameManager.Instance.InDivineSmiteRange(target) && this.character.baseStats.Mana >= 2;
		}

		public override void Execute()
		{
			GameManager.Instance.DivineSmite(target);
		}

		public override float GetGoalChange(Goal goal)
		{
			float change = 0.0f;
			
			if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)
			{
				change += -this.expectedXPChange;
			}

			return change;
		}
	}
}