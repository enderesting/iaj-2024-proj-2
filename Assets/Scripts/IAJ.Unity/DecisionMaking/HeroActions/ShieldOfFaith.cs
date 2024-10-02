using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
	public class ShieldOfFaith : Action
	{
		public float manaCost = 5.0f;
		public float hpGain = 5.0f;
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
			return Character.baseStats.Mana >= manaCost;
		}

		public override void Execute()
		{
			GameManager.Instance.ShieldOfFaith();
		}

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

			float mana = (float)worldModel.GetProperty(PropertiesName.MANA);

			worldModel.SetProperty(PropertiesName.MANA, mana - this.manaCost);
			worldModel.SetProperty(PropertiesName.ShieldHP, this.hpGain);
        }
    }
}