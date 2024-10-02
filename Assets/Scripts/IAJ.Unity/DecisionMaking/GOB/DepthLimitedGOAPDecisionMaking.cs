using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.Game;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.GOB
{
    public class DepthLimitedGOAPDecisionMaking
    {
        public const int MAX_DEPTH = 3;
        public int ActionCombinationsProcessedPerFrame { get; set; }
        public float TotalProcessingTime { get; set; }
        public int TotalActionCombinationsProcessed { get; set; }
        public bool InProgress { get; set; }

        public WorldModel InitialWorldModel { get; set; }
        private List<Goal> Goals { get; set; }
        private WorldModel[] Models { get; set; }
        private Action[] LevelAction { get; set; }
        public Action[] BestActionSequence { get; private set; }
        public Action BestAction { get; private set; }
        public float BestDiscontentmentValue { get; private set; }
        private int CurrentDepth {  get; set; }

        public DepthLimitedGOAPDecisionMaking(WorldModel currentStateWorldModel, AutonomousCharacter character)
        {
            this.ActionCombinationsProcessedPerFrame = 2000;
            this.Goals = character.Goals;
            this.InitialWorldModel = currentStateWorldModel;
        }

        public void InitializeDecisionMakingProcess()
        {
            this.InProgress = true;
            this.TotalProcessingTime = 0.0f;
            this.TotalActionCombinationsProcessed = 0;
            this.CurrentDepth = 0;
            this.Models = new DictionaryWorldModel[MAX_DEPTH + 1];
            this.Models[0] = this.InitialWorldModel;
            this.LevelAction = new Action[MAX_DEPTH];
            this.BestActionSequence = new Action[MAX_DEPTH];
            this.BestAction = null;
            this.BestDiscontentmentValue = float.MaxValue;
            this.InitialWorldModel.Initialize();
        }

        public Action ChooseAction()
        {
            var startTime = Time.realtimeSinceStartup;
            int actionCombinationsProcessedThisFrame = 0;

            float currentDiscontentment;
            Action nextAction;

            while (this.CurrentDepth >= 0)
            {
                if (actionCombinationsProcessedThisFrame >= this.ActionCombinationsProcessedPerFrame)
                {
                    break;
                }

                if (CurrentDepth >= MAX_DEPTH)
                {
                    currentDiscontentment = this.Models[CurrentDepth].Character.CalculateDiscontentment(this.Models[CurrentDepth]);
                    if (currentDiscontentment < BestDiscontentmentValue)
                    {
                        BestDiscontentmentValue = currentDiscontentment;
                        BestAction = this.LevelAction[0];
                        for (int i = 0; i < MAX_DEPTH; i++)
                        {
                            this.BestActionSequence[i] = this.LevelAction[i];
                        }
                    }
                    actionCombinationsProcessedThisFrame++;
                    TotalActionCombinationsProcessed++;
                    CurrentDepth--;
                    continue;
                }
                nextAction = this.Models[CurrentDepth].GetNextAction();
                if (nextAction != null)
                {
                    this.Models[CurrentDepth + 1] = this.Models[CurrentDepth].GenerateChildWorldModel();
                    nextAction.ApplyActionEffects(this.Models[CurrentDepth + 1]);
                    this.Models[CurrentDepth + 1].Character.UpdateGoalsInsistence(this.Models[CurrentDepth + 1]);
                    this.LevelAction[CurrentDepth] = nextAction;
                    CurrentDepth++;
                }
                else
                {
                    CurrentDepth--;
                }
            }

            if (this.CurrentDepth < 0)
            {
                this.InProgress = false;
            } 
            else
            {
                actionCombinationsProcessedThisFrame = 0;
            }

            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
            this.InProgress = false;
            return this.BestAction;
        }
    }
}
