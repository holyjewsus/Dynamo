using System.Collections.Generic;
using System.Linq;

using Dynamo.Engine.CodeGeneration;
using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;

using CodeBlockNode = ProtoCore.AST.AssociativeAST.CodeBlockNode;
using LanguageBlockNode = ProtoCore.AST.AssociativeAST.LanguageBlockNode;

namespace CoreNodeModels.Logic
{


    [NodeName("ForLoopTest"), NodeCategory(BuiltinNodeCategories.LOGIC)]
    [IsDesignScriptCompatible]
    public class ForLoopTest : ScopedNodeModel
    {
        private List<NodeModel> innerNodes = new List<NodeModel>();


        public ForLoopTest()
        {
            InPortData.Add(new PortData("BeforeLoop", ""));
            InPortData.Add(new PortData("nodesToLoop", ""));
            InPortData.Add(new PortData("iterationsList", ""));
            OutPortData.Add(new PortData("result", ""));
        
            RegisterAllPorts();

        }
        protected override bool IsScopedInport(int portIndex)
        {
            return portIndex != 2;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAstInScope(List<AssociativeNode> inputAstNodes, bool verboseLogging, AstBuilder builder)
        {

            var BeforeLoopnodes = GetInScopeNodesForInport(0,false);
            var BeforeCompiledNodes = builder.CompileToAstNodes(BeforeLoopnodes, CompilationContext.None, true);
            var BeforeAstNodes = BeforeCompiledNodes.SelectMany(t => t.Item2).ToList();

           
            //compile the nodes we want to loop over in
            var loopnodes = GetInScopeNodesForInport(1);
            var allAstNodes = builder.CompileToAstNodes(loopnodes, CompilationContext.None, true);
            var astNodes = allAstNodes.SelectMany(t => t.Item2).ToList();
            //hack which adds output = function(blahblahblah) to the forloop
            astNodes.Add(
                AstFactory.BuildAssignment(BeforeAstNodes.Where(x => x.Kind == AstKind.BinaryExpression)
                .Select(y => (y as BinaryExpressionNode).LeftNode).First(), inputAstNodes[1]));

            var toRemove = new List<AssociativeNode>();
             foreach(var node in astNodes)
            {
               foreach(var other in BeforeAstNodes)
                {
                    if(node.ToString() == other.ToString())
                    {
                        toRemove.Add(node);
                    }
                }
            }

            toRemove.ForEach(x => astNodes.Remove(x));

            /*
            // This function will compile FOR node to the following format:
            //
            //    BeforeBody...
            //     v = [Imperative]
            //     {
                       i = 0;
            //         for (i in 0..iterations)
                        {
                        Body...
                        }
            //        return output;
            //     }
            //
            */

            //for loopvar in expression
            var forLoopStatement = new ProtoCore.AST.ImperativeAST.ForLoopNode()
            {
                Body = astNodes.Select(x => x.ToImperativeAST()).ToList(),
                LoopVariable = new ProtoCore.AST.AssociativeAST.IdentifierNode("x").ToImperativeNode(),
                Expression  = inputAstNodes[2].ToImperativeAST()

            };

            // thisVariable = [Imperative]
            // {
            //     ...
            // }
            var outerBlock = new LanguageBlockNode
            {
                codeblock = new LanguageCodeBlock(Language.Imperative),
                CodeBlockNode = new ProtoCore.AST.ImperativeAST.CodeBlockNode
                {
                    Body = new List<ProtoCore.AST.ImperativeAST.ImperativeNode> {
                        AstFactory.BuildAssignment(new IdentifierNode("x"),AstFactory.BuildIntNode(0)).ToImperativeAST(),
                        forLoopStatement

                    }
                    
                }
            };

            (outerBlock.CodeBlockNode as ProtoCore.AST.ImperativeAST.CodeBlockNode).Body.InsertRange(0, BeforeAstNodes.Select(x => x.ToImperativeAST()));
            (outerBlock.CodeBlockNode as ProtoCore.AST.ImperativeAST.CodeBlockNode).Body.Add(
                AstFactory.BuildReturnStatement(BeforeAstNodes.Where(x => x.Kind == AstKind.BinaryExpression)
                .Select(y => (y as BinaryExpressionNode).LeftNode).First()).ToImperativeAST());


            var thisVariable = GetAstIdentifierForOutputIndex(0);
            var assignment = AstFactory.BuildAssignment(thisVariable, outerBlock);

            return new AssociativeNode[]
            {
                assignment
            };

        }

    }


    [NodeName("ScopeIf"), NodeCategory(BuiltinNodeCategories.LOGIC),
     NodeDescription("ScopeIfDescription", typeof(Resources)), IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.Logic.ScopedIf")]
    public class ScopedIf : ScopedNodeModel
    {
        public ScopedIf() : base()
        {
            InPortData.Add(new PortData("test", Resources.PortDataTestBlockToolTip));
            InPortData.Add(new PortData("true", Resources.PortDataTrueBlockToolTip));
            InPortData.Add(new PortData("false", Resources.PortDataFalseBlockToolTip));

            OutPortData.Add(new PortData("result", Resources.PortDataResultToolTip));
            RegisterAllPorts();
        }

        private List<AssociativeNode> GetAstsForBranch(int branch, List<AssociativeNode> inputAstNodes, bool verboseLogging, AstBuilder builder)
        {
            // Get all upstream nodes and then remove nodes that are not 
            var nodes = GetInScopeNodesForInport(branch, false).Where(n => !(n is Symbol));
            nodes = ScopedNodeModel.GetNodesInTopScope(nodes);

            // The second parameter, isDeltaExecution, is set to false so that
            // all AST nodes will be added to this IF graph node instead of 
            // adding to the corresponding graph node. 
            var allAstNodes = builder.CompileToAstNodes(nodes, CompilationContext.None, verboseLogging);
            var astNodes = allAstNodes.SelectMany(t => t.Item2).ToList();
            astNodes.Add(AstFactory.BuildReturnStatement(inputAstNodes[branch]));
            return astNodes;
        }

        private void SanityCheck()
        {
            // condition branch
            var condNodes = GetInScopeNodesForInport(0, false, true, true).Where(n => !(n is Symbol));
            var trueNodes = new HashSet<NodeModel>(GetInScopeNodesForInport(1, false).Where(n => !(n is Symbol)));
            var falseNodes = new HashSet<NodeModel>(GetInScopeNodesForInport(2, false).Where(n => !(n is Symbol)));

            trueNodes.IntersectWith(condNodes);
            falseNodes.IntersectWith(condNodes);
            trueNodes.UnionWith(falseNodes);

            if (trueNodes.Any())
            {
                foreach (var node in trueNodes)
                {
                    node.Error("A node cann't be both in condition and true/false branches of IF node");
                }
            }
        }

        /// <summary>
        /// Specify if upstream nodes that connected to specified inport should
        /// be compiled in the scope or not. 
        /// </summary>
        /// <param name="portIndex"></param>
        /// <returns></returns>
        protected override bool IsScopedInport(int portIndex)
        {
            return portIndex == 1 || portIndex == 2;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAstInScope(List<AssociativeNode> inputAstNodes, bool verboseLogging, AstBuilder builder)
        {
            // This function will compile IF node to the following format:
            //
            //     cond = ...;
            //     v = [Imperative]
            //     {
            //         if (cond) {
            //             return = [Associative] {
            //                 ...
            //             }
            //         }
            //         else {
            //             return = [Associative] {
            //                 ...
            //             }
            //         }
            //     }
            //

            var astsInTrueBranch = GetAstsForBranch(1, inputAstNodes, verboseLogging, builder);
            var astsInFalseBranch = GetAstsForBranch(2, inputAstNodes, verboseLogging, builder);

            // if (cond) {
            //     return = [Associative] {...}
            // }
            var ifBlock = new LanguageBlockNode
            {
                codeblock = new LanguageCodeBlock(Language.Associative),
                CodeBlockNode = new CodeBlockNode { Body = astsInTrueBranch }
            };
            var ifBranch = AstFactory.BuildReturnStatement(ifBlock).ToImperativeAST();

            // else {
            //     return = [Associative] { ... }
            // }
            var elseBlock = new LanguageBlockNode
            {
                codeblock = new LanguageCodeBlock(Language.Associative),
                CodeBlockNode = new CodeBlockNode { Body = astsInFalseBranch }
            };
            var elseBranch = AstFactory.BuildReturnStatement(elseBlock).ToImperativeAST();

            var ifelseStatement = new ProtoCore.AST.ImperativeAST.IfStmtNode()
            {
                IfExprNode = inputAstNodes[0].ToImperativeAST(),
                IfBody = new List<ProtoCore.AST.ImperativeAST.ImperativeNode> { ifBranch },
                ElseBody = new List<ProtoCore.AST.ImperativeAST.ImperativeNode> { elseBranch }
            };

            // thisVariable = [Imperative]
            // {
            //     ...
            // }
            var outerBlock = new LanguageBlockNode
            {
                codeblock = new LanguageCodeBlock(Language.Imperative),
                CodeBlockNode = new ProtoCore.AST.ImperativeAST.CodeBlockNode
                {
                    Body = new List<ProtoCore.AST.ImperativeAST.ImperativeNode> { ifelseStatement }
                }
            };

            var thisVariable = GetAstIdentifierForOutputIndex(0);
            var assignment = AstFactory.BuildAssignment(thisVariable, outerBlock);

            return new AssociativeNode[] 
            {
                assignment
            };
        }
    }
}