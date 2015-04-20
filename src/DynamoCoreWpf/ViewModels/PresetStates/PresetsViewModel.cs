using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace Dynamo.ViewModels
{
    public class PresetsViewModel
    {
        private readonly DynamoViewModel dynamoViewModel;
        public PresetsModel Model { get; private set; }

        public PresetsViewModel(PresetsModel presetCollection,DynamoViewModel dynamoViewModel)
        {
            Model = presetCollection;
            this.dynamoViewModel = dynamoViewModel;
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
