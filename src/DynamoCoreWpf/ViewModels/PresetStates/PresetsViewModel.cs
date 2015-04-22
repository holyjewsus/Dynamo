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

namespace Dynamo.ViewModels
{
    public class PresetsViewModel
    {
        private readonly DynamoViewModel dynamoViewModel;
        public PresetsModel Model { get; set; }
        public ObservableCollection<PresetStateViewModel> StateViewModels{get;private set;}

        public PresetsViewModel(PresetsModel presetCollection,DynamoViewModel dynamoViewModel)
        {
            Model = presetCollection;
            this.dynamoViewModel = dynamoViewModel;
            StateViewModels = new ObservableCollection<PresetStateViewModel>();
           
            foreach (var state in Model.DesignStates)
            {
                StateViewModels.Add(new PresetStateViewModel(state, dynamoViewModel,Model));
            }
            ((INotifyCollectionChanged)Model.DesignStates).CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionChangedHandler);
            Model.CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionChangedHandler);
            dynamoViewModel.CurrentSpace.NodeAdded += new Action<NodeModel>((o)=> {CollectionChangedHandler(o,null);});
            dynamoViewModel.CurrentSpace.NodeRemoved += new Action<NodeModel>((o) => { CollectionChangedHandler(o, null); });
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
    }
}
