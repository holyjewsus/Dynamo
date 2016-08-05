namespace Dynamo.Wpf.NodeViewCustomizations
{

    using Dynamo.Controls;
    using Dynamo.ViewModels;
    using CoreNodeModels.Input;
    using Dynamo.Wpf;
    using CoreNodeModels.Logic;
    using Dynamo.Events;
    using Graph.Nodes;
    using System.Collections.Generic;
    using System.Linq;
    using Scheduler;
    using System;

    namespace CoreNodeModelsWpf.Nodes
    {
        public class StatemachineCustomization : INodeViewCustomization<SetCurrentState>
        {
            private DynamoViewModel dynamoViewModel;
            private NodeModel model;

            public void Dispose()
            {

            }

            public void CustomizeView(SetCurrentState model, NodeView nodeView)
            {
                dynamoViewModel = nodeView.ViewModel.DynamoViewModel;
                this.model = model;
                model.Executed += onExecuted;
            }

            private void onExecuted()
            {
                //find all nodes of type OnState
                var nodesToExecute = dynamoViewModel.CurrentSpace.Nodes.OfType<OnCurrentState>();
                var engine = dynamoViewModel.EngineController;
                var stateString = "default";
                nodesToExecute.ToList().ForEach(node => {
                    if (node.HasConnectedInput(0))
                    {
                        var stateNode = node.InPorts[0].Connectors[0].Start.Owner;
                        var stateIndex = node.InPorts[0].Connectors[0].Start.Index;
                        var startId = stateNode.GetAstIdentifierForOutputIndex(stateIndex).Name;
                        var stateMirror = engine.GetMirror(startId);
                        stateString = stateMirror.GetData().Data as string;
                    }
                    var copy = stateString;
                    var task = new DelegateBasedAsyncTask(dynamoViewModel.Model.Scheduler, () => { node.SetFreezeState(copy); });
                    task.scheduler.ScheduleForExecution(task);
                } );
               
            }
        }

    }
}
