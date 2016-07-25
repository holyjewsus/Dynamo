using System.Collections.Generic;
using System.Linq;
using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using VMDataBridge;
using System;

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
        public Action Executed;
        public SetCurrentState()
        {
            Executed = new Action(() => { Console.WriteLine("executed"); });
            InPortData.Add(new PortData("state", "the name of the state to switch to"));
          
            OutPortData.Add(new PortData("na",""));

            RegisterAllPorts();
        }
        //when this node is executed it needs to schedule other nodes in the graph
        //we don't actually care about the values generated here...
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new AssociativeNode[]
            {
                 AstFactory.BuildAssignment(
                    AstFactory.BuildIdentifier(AstIdentifierBase),AstFactory.BuildDoubleNode(0)
                   ),

                AstFactory.BuildAssignment(
                    AstFactory.BuildIdentifier(AstIdentifierBase + "_dummy"),
                    DataBridge.GenerateBridgeDataAst(GUID.ToString(), GetAstIdentifierForOutputIndex(0)))
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
            //when this callback is invoked, we search the graph and schedule any work we need to do.
            //we can do this by raising an event here that the view will see, and do the scheduling since we have
            //access to the scheduler there...potentally an extension would be a better way to implement this
            this.Executed();
        }
    }

    [NodeName("OnCurrentState")]
    [NodeCategory("FiniteStateMachine")]
    [IsDesignScriptCompatible]
    public class OnCurrentState : NodeModel
    {
        public OnCurrentState()
        {
            InPortData.Add(new PortData("state", "the name of the state"));

            OutPortData.Add(new PortData("value", ""));

            RegisterAllPorts();
            Dynamo.Events.ExecutionEvents.GraphPostExecution += ExecutionEvents_GraphPostExecution;
        }

        private void ExecutionEvents_GraphPostExecution(Dynamo.Session.IExecutionSession session)
        {
            System.Threading.Thread.Sleep(5);
            this.OnNodeModified(true);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var random = new Random();
            return new AssociativeNode[]
            {
                AstFactory.BuildAssignment( GetAstIdentifierForOutputIndex(0),AstFactory.BuildDoubleNode(random.NextDouble()))

            };
        }
    }

}
