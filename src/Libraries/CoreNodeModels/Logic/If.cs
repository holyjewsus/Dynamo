using System.Collections.Generic;
using System.Collections;
using System.Linq;
using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using VMDataBridge;
using System;
using Autodesk.DesignScript.Runtime;

namespace CoreNodeModels.Logic
{
    [NodeName("If")]
    [NodeCategory(BuiltinNodeCategories.LOGIC)]
    [NodeDescription("IfDescription", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.Logic.If")]
    public class If : NodeModel
    {
        public If()
        {
            InPortData.Add(new PortData("test", Resources.PortDataTestBlockToolTip));
            InPortData.Add(new PortData("true", Resources.PortDataTrueBlockToolTip));
            InPortData.Add(new PortData("false", Resources.PortDataFalseBlockToolTip));

            OutPortData.Add(new PortData("result", Resources.PortDataResultToolTip));

            RegisterAllPorts();

            //TODO: Default Values
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var lhs = GetAstIdentifierForOutputIndex(0);
            AssociativeNode rhs;

            if (IsPartiallyApplied)
            {
                var connectedInputs = Enumerable.Range(0, InPortData.Count)
                                            .Where(HasConnectedInput)
                                            .Select(x => new IntNode(x) as AssociativeNode)
                                            .ToList();
                var functionNode = new IdentifierNode(Constants.kInlineConditionalMethodName);
                var paramNumNode = new IntNode(3);
                var positionNode = AstFactory.BuildExprList(connectedInputs);
                var arguments = AstFactory.BuildExprList(inputAstNodes);
                var inputParams = new List<AssociativeNode>
                {
                    functionNode,
                    paramNumNode,
                    positionNode,
                    arguments,
                    AstFactory.BuildBooleanNode(true)
                };

                rhs = AstFactory.BuildFunctionCall("Function", inputParams);
            }
            else
            {
                rhs = new InlineConditionalNode
                {
                    ConditionExpression = inputAstNodes[0],
                    TrueExpression = inputAstNodes[1],
                    FalseExpression = inputAstNodes[2]
                };
            }

            return new[]
            {
                AstFactory.BuildAssignment(lhs, rhs)
            };
        }
    }



    [NodeName("SetCurrentState")]
    [NodeCategory("FiniteStateMachine")]
    [IsDesignScriptCompatible]
    public class SetCurrentState : NodeModel
    {
        public static string currentState { get; private set; }
        public Action Executed;

        public SetCurrentState()
        {
            Executed = new Action(() => { Console.WriteLine("executed"); });
            InPortData.Add(new PortData("state", "the name of the state to switch to"));

            RegisterAllPorts();
            CanUpdatePeriodically = true;

        }
        //when this node is executed it needs to schedule other nodes in the graph
        //we don't actually care about the values generated here...
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new AssociativeNode[]
            {
                AstFactory.BuildAssignment(
                    AstFactory.BuildIdentifier(AstIdentifierBase + "_dummy"),
                    DataBridge.GenerateBridgeDataAst(GUID.ToString(), inputAstNodes.FirstOrDefault()))
            };
        }

        public override void Dispose()
        {
            base.Dispose();
            DataBridge.Instance.UnregisterCallback(GUID.ToString());
        }

        protected override void OnBuilt()
        {
            base.OnBuilt();
            DataBridge.Instance.RegisterCallback(GUID.ToString(), DataBridgeCallback);
        }

        private void DataBridgeCallback(object data)
        {
            SetCurrentState.currentState = data as string;
            this.Executed();
        }
    }


    [NodeName("OnCurrentState")]
    [NodeCategory("FiniteStateMachine")]
    [IsDesignScriptCompatible]
    public class OnCurrentState : NodeModel
    {
        private object output;
        public OnCurrentState()
        {
            InPortData.Add(new PortData("state", "the name of the state to activate on"));
            InPortData.Add(new PortData("data", " data to pass through if state is active"));

            OutPortData.Add(new PortData("data", "pass through data"));

            RegisterAllPorts();
            CanUpdatePeriodically = true;

        }
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new AssociativeNode[]
            {
                //return the data if we execute
                  AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),inputAstNodes[1])

            };
        }

      //called from the UI thread using the engineMirror... may need to be scheduled...
        public void SetFreezeState(string thisState)
        {
            try
            {
                //if the current state is the same as the one referenced by this node then pass the value out
                //else return nothing...
                if (SetCurrentState.currentState == thisState)
                {
                    this.IsFrozen = false;
                }
                else
                {
                    //freeze the node now.
                    this.IsFrozen = true;
                }
            }
            catch
            {

            }
        }
    }

}
