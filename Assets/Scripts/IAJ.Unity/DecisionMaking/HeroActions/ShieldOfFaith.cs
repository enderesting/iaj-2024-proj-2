using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
	public class ShieldOfFaith : Action
	{
		public int manaCost = 5;
		public int hpGain = 5;
		protected AutonomousCharacter Character { get; set; }

		public ShieldOfFaith(AutonomousCharacter character) : base("ShieldOfFaith")
		{
			this.Character = character;
		}
		
		public override float GetGoalChange(Goal goal)
		{
			float change = base.GetGoalChange(goal);

			if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
			{
				change -= Character.baseStats.MaxShieldHp - Character.baseStats.ShieldHP;
			}
			return change;
		}
		
		public override bool CanExecute()
		{
			bool res = Character.baseStats.Mana >= manaCost;
			// Debug.Log("ShieldOfFaith:" +  Character.baseStats.Mana + " mana cost:" + manaCost + " res:" + res);
			return res;
		}

		public override bool CanExecute(WorldModel worldModel){
			return (int)worldModel.GetProperty(PropertiesName.MANA) >= manaCost;
		}

		public override void Execute()
		{
			GameManager.Instance.ShieldOfFaith();
		}

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

			int mana = (int)worldModel.GetProperty(PropertiesName.MANA);

			worldModel.SetProperty(PropertiesName.MANA, mana - this.manaCost);
			worldModel.SetProperty(PropertiesName.ShieldHP, this.hpGain);
        }
    }
}