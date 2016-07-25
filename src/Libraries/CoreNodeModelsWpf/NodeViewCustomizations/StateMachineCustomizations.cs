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
                //schedule these nodes to be executed by calling onNodeModified with force Re-execute on them
                //potentially on all their upstream node as well...?
                //then call a graph update.
                nodesToExecute.ToList().ForEach(x => x.OnNodeModified(true));
                //if (nodesToExecute.ToList().Count > 0)
                //{
                //    dynamoViewModel.Model.Scheduler.ScheduleForExecution(new UpdateGraphAsyncTask(dynamoViewModel.Model.Scheduler, true));

                //}
            }
        }
    }
}
