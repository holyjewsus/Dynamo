using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Xml;
using System.Windows.Media;
using Dynamo.Selection;
using System.Reflection;
using System.Collections;
using Dynamo.Controls;

namespace Dynamo.ViewModels
{
    public class PresetStateViewModel
    {
        private readonly DynamoViewModel dynamoViewModel;
        public PresetState Model { get; private set; }
        private PresetsModel ownerModel;
        public ReadOnlyObservableCollection<NodeViewModel> NodeListItems { get; private set; }

        public PresetStateViewModel(PresetState state, DynamoViewModel dynamoViewModel, PresetsModel ownerCollectionModel)
        {
            Model = state;
            this.dynamoViewModel = dynamoViewModel;
            this.ownerModel= ownerCollectionModel;
            var templist = new ObservableCollection<NodeViewModel>();
            foreach (var serializedNode in Model.SerializedNodes)
            {
                //button representing the node, will allow reassociation
                var button = new Button();
                NodeViewModel nodeviewmodel = null;
                //if the node id is missing from the nodes list or if the node is missing from the graph then make it red...
                if (!Model.Nodes.Select(x => x.GUID).Contains(Guid.Parse(serializedNode.GetAttribute("guid"))) ||
                    (!this.dynamoViewModel.CurrentSpace.Nodes.Select(x => x.GUID).Contains(Guid.Parse(serializedNode.GetAttribute("guid"))))
                   )
                {
                    button.Background = Brushes.Red;
                }
                else
                {
                   //if the node was found in the state Nodes list then grab it's view
                    nodeviewmodel = dynamoViewModel.CurrentSpaceViewModel.Nodes.ToList().Find
                        (x => x.NodeModel.GUID == (Guid.Parse(serializedNode.GetAttribute("guid"))));
                }
                button.Content = serializedNode.GetAttribute("nickname") + " : " + "show value here!";
                button.MinWidth = 200;
                button.MinHeight = 20;
                button.Tag = serializedNode;
                templist.Add(nodeviewmodel);
                //TODO
                //when the button is clicked we want to enter into a modal selection, for now we can just use the first selected node
                //of the same type
                button.Click += handleNodeButtonPress;
            }
            NodeListItems = new ReadOnlyObservableCollection<NodeViewModel>(templist);
        }

        private void handleNodeButtonPress(object sender, RoutedEventArgs e)
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type nodetype = null;
            string typeName = ((sender as Button).Tag as XmlElement).GetAttribute("type");

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
            var missingID = Guid.Parse((((sender as Button).Tag) as XmlElement).GetAttribute("guid"));

            //if we're attempting to replace some node with itself
            //bail
            if (missingID == firstnode.GUID)
            {
                return;
            }

            foreach (var state in ownerModel.DesignStates)
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
    }
}