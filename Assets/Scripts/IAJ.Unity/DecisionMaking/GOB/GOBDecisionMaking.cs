using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.GOB
{
    public class GOBDecisionMaking
    {
        public bool InProgress { get; set; }
        private List<Goal> goals { get; set; }
        private List<Action> actions { get; set; }

        public Dictionary<Action,float> ActionDiscontentment { get; set; }

        public Action secondBestAction;
        public Action thirdBestAction;

        // Utility based GOB
        public GOBDecisionMaking(List<Action> _actions, List<Goal> goals)
        {
            this.actions = _actions;
            this.goals = goals;
            this.ActionDiscontentment = new Dictionary<Action,float>();
        }

        //Predicting the Discontentment after executing the action
        public static float CalculateDiscontentment(Action action, List<Goal> goals, AutonomousCharacter character)
        {
            // Keep a running total
            var discontentment = 0.0f;
            var duration = action.GetDuration();

            foreach (var goal in goals)
            {
                 // Calculate the new value after the action
                float changeValue = action.GetGoalChange(goal) + duration * goal.ChangeRate;
                
                // The change rate is how much the goals changes per time
                var newValue = goal.NormalizeGoalValue(goal.InsistenceValue + changeValue, goal.Min, goal.Max);

                discontentment += goal.GetDiscontentment(newValue);
            }

            return discontentment;
        }

        public Action ChooseAction(AutonomousCharacter character)
        {
            // Find the action leading to the lowest discontentment
            InProgress = true;
            Action bestAction = null;
            var bestValue = float.PositiveInfinity;
            secondBestAction = null;
            thirdBestAction = null;
            ActionDiscontentment.Clear();
            
            foreach(Action action in actions)
            {
                if (action.CanExecute())
                {
                    float discontentment = CalculateDiscontentment(action, goals, character);
                    ActionDiscontentment.Add(action, discontentment);

                    if (discontentment < bestValue)
                    {
                        thirdBestAction = secondBestAction;
                        secondBestAction = bestAction;
                        bestAction = action;
                        bestValue = discontentment;
                    }
                }
            }
            InProgress = false;
            return bestAction;
        }
    }
}
