using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions.Action;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEditor.Animations;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTS
    {
        public const float C = 1.4f;
        public bool InProgress { get; private set; }
        protected int MaxIterations { get; set; }
        protected int MaxIterationsPerFrame { get; set; }
        protected int NumberPlayouts { get; set; }
        protected int PlayoutDepthLimit { get; set; }
        public MCTSNode BestFirstChild { get; set; }

        public List<MCTSNode> BestSequence { get; set; }
        public WorldModel BestActionSequenceEndState { get; set; }
        public int CurrentIterations { get; protected set; }
        protected int CurrentDepth { get; set; }
        protected int FrameCurrentIterations { get; set; }
        protected WorldModel InitialState { get; set; }
        protected MCTSNode InitialNode { get; set; }
        protected System.Random RandomGenerator { get; set; }
        
        //Information and Debug Properties
        public int MaxPlayoutDepthReached { get; set; }
        public int MaxSelectionDepthReached { get; set; }
        public float TotalProcessingTime { get; set; }
        
        //public List<Action> BestActionSequence { get; set; }
        //Debug
         

        public MCTS(WorldModel currentStateWorldModel, int maxIter, int maxIterFrame, int playouts, int playoutDepthLimit)
        {
            this.InitialState = currentStateWorldModel;
            this.MaxIterations = maxIter;
            this.MaxIterationsPerFrame = maxIterFrame;
            this.NumberPlayouts = playouts;
            this.PlayoutDepthLimit = playoutDepthLimit;
            this.InProgress = false;
            this.RandomGenerator = new System.Random();
        }

        public void InitializeMCTSearch()
        {
            this.InitialState.Initialize();
            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.FrameCurrentIterations = 0;
            this.TotalProcessingTime = 0.0f;
 
            // create root node n0 for state s0
            this.InitialNode = new MCTSNode(this.InitialState)
            {
                Action = null,
                Parent = null,
                PlayerID = 0
            };
            this.InProgress = true;
            this.BestFirstChild = null;
            //this.BestActionSequence = new List<Action>();
        }

        public Action ChooseAction()
        {
            MCTSNode selectedNode;
            float reward;

            var startTime = Time.realtimeSinceStartup;

            while ((this.MaxIterations <= 0) || this.CurrentIterations < this.MaxIterations)
            {
                // Selection (& Expansions)
                selectedNode = Selection(this.InitialNode);

                // Playout
                reward = Playout(selectedNode.State);

                // Backpropagation
                Backpropagate(selectedNode, reward);
                this.CurrentIterations++;
            }

            // return best initial child
            return BestFirstChild?.Action;
        }

        // Selection and Expansion
        // edit: based on https://youtu.be/UXW2yZndl7U about 1:10 in
        protected MCTSNode Selection(MCTSNode initialNode)
        {
            // selecting from existing tree the best node based on an equation
            // expand: whichever best node it lands on, find an unexplored action and explore the node
            Action nextAction;
            MCTSNode currentNode = initialNode;
            MCTSNode bestChild;

            // if it isn't leaf node...
            while (currentNode.ChildNodes.Count()>0){
                // recursively choose child with best UCT value until leaf node is reached
                bestChild = BestUCTChild(currentNode);
                currentNode = bestChild;
            }

            // if currentnode hasnt ever been visited, roll out currentnode
            if (currentNode.N == 0)
                return currentNode;

            // otherwise, expand current node -- add new node for every available action
            while ((nextAction = currentNode.State.GetNextAction()) is not null)
            {
                // expand the next action in a new node and add to the node's list
                return Expand(currentNode, nextAction);
            }
            return currentNode.ChildNodes[0];
        }

        protected virtual float Playout(WorldModel initialStateForPlayout)
        {
            WorldModel currentState = initialStateForPlayout;
            Action[] executableActions;
            float reward = 0;

            while (!currentState.IsTerminal() && ((this.PlayoutDepthLimit <= 0) || this.NumberPlayouts <= this.PlayoutDepthLimit))
            {
                WorldModel newState = currentState.GenerateChildWorldModel();
                
                executableActions = newState.GetExecutableActions();
                
                Action selectedAction = executableActions[this.RandomGenerator.Next(0, executableActions.Length)];
                
                selectedAction.ApplyActionEffects(newState);
                
                currentState = newState;
                this.NumberPlayouts++;
                reward += selectedAction.GetHValue(currentState);
            }
            return reward;
        }

        protected virtual void Backpropagate(MCTSNode node, float reward)
        {
            node.Q = reward;
            node.N = 1;
            MCTSNode currentNode = node;
            while (node.Parent != null){
                currentNode.Parent.Q += reward;
                currentNode.Parent.N += 1;
                currentNode = currentNode.Parent;
            }
        }

        // given a parent node and action, apply the action and adds a new node to the tree
        protected MCTSNode Expand(MCTSNode parent, Action action)
        {
            WorldModel newState = parent.State.GenerateChildWorldModel();
            
            action.ApplyActionEffects(newState); //apply action to wm like this

            MCTSNode newNode = new MCTSNode(newState);
            newNode.Action = action;
            newNode.Parent = parent;
            parent.ChildNodes.Add(newNode);
            return newNode;
        }

        protected virtual MCTSNode BestUCTChild(MCTSNode node)
        {
            MCTSNode bestChild = null;
            double bestUCT = float.MinValue;
            double childUCT;
            foreach (MCTSNode childNode in node.ChildNodes){
                //calculate best UCT score
                if (childNode.N == 0)
                    //childUCT = float.PositiveInfinity;
                    return childNode; // an unvisited node's UCT is +infinity
                else
                    childUCT = (childNode.Q/childNode.N) + C * Math.Sqrt(Math.Log(node.N)/childNode.N);

                //compare with best
                if (childUCT > bestUCT){
                    bestChild = childNode;
                    bestUCT = childUCT;
                }
            }
            return bestChild;
        }

        //this method is very similar to the bestUCTChild, but it is used to return the final action of the MCTS search, and so we do not care about
        //the exploration factor
        protected MCTSNode BestChild(MCTSNode node)
        {
            float bestRatio = float.MinValue;
            MCTSNode bestNode = null;
            foreach (MCTSNode childNode in node.ChildNodes)
            {
                if (childNode.Q / childNode.N > bestRatio)
                {
                    bestRatio = childNode.Q / childNode.N;
                    bestNode = childNode;
                }
            }

            return bestNode;
        }


        protected Action BestAction(MCTSNode node)
        {
            var bestChild = this.BestChild(node);
            if (bestChild == null) return null;

            this.BestFirstChild = bestChild;

            //this is done for debugging proposes only
            this.BestSequence = new List<MCTSNode> { bestChild };
            node = bestChild;
            this.BestActionSequenceEndState = node.State;

            while(!node.State.IsTerminal())
            {
                bestChild = this.BestChild(node);
                if (bestChild == null) {
                    break;
                }
                this.BestSequence.Add(bestChild);
                node = bestChild;
                this.BestActionSequenceEndState = node.State;
            }
            return this.BestFirstChild.Action;
        }

    }
}
