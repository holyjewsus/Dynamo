using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Dynamo.Utilities;
using System.Xml;
using System.Reflection;
using Dynamo.Selection;

namespace Dynamo.ViewModels
{
    public class PresetsViewModel:IDisposable
    {
        private readonly DynamoViewModel dynamoViewModel;
        public PresetsModel Model { get; private set; }
        public ObservableCollection<PresetStateViewModel> StateViewModels{get;private set;}
        private  Action<NodeModel> collectionchange;

        public PresetsViewModel(PresetsModel presetCollection,DynamoViewModel dynamoViewModel)
        {
            Model = presetCollection;
            this.dynamoViewModel = dynamoViewModel;
            StateViewModels = new ObservableCollection<PresetStateViewModel>();
           
            foreach (var state in Model.DesignStates)
            {
                StateViewModels.Add(new PresetStateViewModel(state, dynamoViewModel,Model));
            }
            collectionchange = new Action<NodeModel>((o)=> {CollectionChangedHandler(o,null);});

            ((INotifyCollectionChanged)Model.DesignStates).CollectionChanged += CollectionChangedHandler;
            Model.CollectionChanged += CollectionChangedHandler;
            dynamoViewModel.CurrentSpace.NodeAdded += collectionchange;
            dynamoViewModel.CurrentSpace.NodeRemoved += collectionchange;
      //this view also needs to watch for changes on the workspaceView so that if nodes are deleted the view is updated...
        }

        private void CollectionChangedHandler(object o, NotifyCollectionChangedEventArgs args)
        {
            //when the underlying model collection changes, then modify the states
            StateViewModels.Clear();
            foreach (var state in Model.DesignStates)
            {
                StateViewModels.Add(new PresetStateViewModel(state, dynamoViewModel,Model));
            }
        } 
       
        public void RestoreState(object sender, RoutedEventArgs e)
        { 
            PresetState state = ((MenuItem)sender).Tag as PresetState;
            var workspace = dynamoViewModel.CurrentSpace;
            dynamoViewModel.ExecuteCommand(new DynamoModel.SetWorkSpaceToStateCommand(workspace.Guid, state.Guid));
        }

        public void DeleteState(object sender, RoutedEventArgs e)
        {
            PresetState state = ((MenuItem)sender).Tag as PresetState;
            var workspace = dynamoViewModel.CurrentSpace;
            workspace.HasUnsavedChanges = true;
            dynamoViewModel.Model.CurrentWorkspace.PresetsCollection.RemoveState(state);
        }

        public void handleNodeButtonPress(object sender, RoutedEventArgs e)
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type nodetype = null;
            string typeName = ((sender as FrameworkElement).Tag as NodeModel).GetType().ToString();

            foreach (var assembly in loadedAssemblies)
            {
                nodetype = assembly.GetType(typeName, false);
                if (nodetype != null)
                {
                    break;
                }
            }
            if (nodetype == null)
            {
                //TODO should log this, possibly alert the user we can't find this kind of node to replace
                throw (new ArgumentException("Type " + typeName + " doesn't exist in the current app domain"));
            }
            MethodInfo method = typeof(Enumerable).GetMethod("OfType");
            method = method.MakeGenericMethod(nodetype);
            var nodes = method.Invoke(null, new object[1] { DynamoSelection.Instance.Selection }) as IEnumerable<NodeModel>;
            var firstnode = nodes.First() as NodeModel;
            var missingID =(((sender as FrameworkElement).Tag) as NodeModel).GUID;

            //if we're attempting to replace some node with itself
            //bail
            if (missingID == firstnode.GUID)
            {
                return;
            }

            foreach (var state in this.Model.DesignStates)
            {
                foreach (var xmlnode in state.SerializedNodes)
                {
                    //now find all states where the old node GUID exists, 
                    if (Guid.Parse(xmlnode.GetAttribute("guid")) == missingID)
                    {
                        //now call replace on this state
                        state.ReAssociateNodeData(missingID, firstnode);

                    }
                }
            }

        }

        public void Dispose()
        {
            ((INotifyCollectionChanged)Model.DesignStates).CollectionChanged -= CollectionChangedHandler;
            Model.CollectionChanged-= CollectionChangedHandler;
            dynamoViewModel.CurrentSpace.NodeAdded -= collectionchange;
            dynamoViewModel.CurrentSpace.NodeRemoved -= collectionchange;
        }
    }
}
