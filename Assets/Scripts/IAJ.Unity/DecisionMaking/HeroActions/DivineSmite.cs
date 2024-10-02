using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
	public class DivineSmite : WalkToTargetAndExecuteAction
	{
		private int manaCost = 2;
		private AutonomousCharacter character;
		private GameObject target;
		private int xpChange = 3;
		private float expectedXPChange = 2.7f;

		public DivineSmite(AutonomousCharacter character, GameObject target) : base("DivineSmite", character, target)
		{
			this.character = character;
			this.target = target;
		}

		public override bool CanExecute()
		{
			return target != null && target.activeInHierarchy && target.CompareTag("Skeleton") && this.character.baseStats.Mana >= 2;
		}

		public override void Execute()
		{
			base.Execute();
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

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

			int xp = (int)worldModel.GetProperty(PropertiesName.XP);
			int mana = (int)worldModel.GetProperty(PropertiesName.MANA);

			worldModel.SetProperty(this.Target.name, false);
            worldModel.SetProperty(PropertiesName.XP, xp + this.xpChange);
			worldModel.SetProperty(PropertiesName.MANA, mana - this.manaCost);

        }
    }
}