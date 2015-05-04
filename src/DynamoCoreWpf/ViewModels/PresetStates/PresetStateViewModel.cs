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
using Dynamo.Nodes;

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
            //TODO, WE NEED TO REMOVE THESE WORKSPACES, or they should not check for unsaved changes....
            var tempWorkSpace = new PresetWorkspaceModel(state, dynamoViewModel.Model.NodeFactory,
                ownerCollectionModel, Enumerable.Empty<NodeModel>(), Enumerable.Empty<NoteModel>(), Enumerable.Empty<AnnotationModel>(), new WorkspaceInfo());
            var tempWorkview = new WorkspaceViewModel(tempWorkSpace, dynamoViewModel);
            dynamoViewModel.Model.AddPresetWorkspace(tempWorkSpace);
            
            foreach (var serializedNode in Model.SerializedNodes)
            {
                //button representing the node, will allow reassociation
                var button = new Button();
                NodeViewModel nodeviewmodel = null;
                //create a nodemodel copy to show the nodes in the state
                 var newNodeModel = GetInstance(serializedNode.GetAttribute("type"),serializedNode);
                 ((NodeModel)newNodeModel).Deserialize(serializedNode, SaveContext.File);
                
                 nodeviewmodel = new NodeViewModel(tempWorkview, (NodeModel)newNodeModel);
                //if the node id is missing from the nodes list or if the node is missing from the graph then make it red...
                if (!Model.Nodes.Select(x => x.GUID).Contains(Guid.Parse(serializedNode.GetAttribute("guid"))) ||
                    (!this.dynamoViewModel.CurrentSpace.Nodes.Select(x => x.GUID).Contains(Guid.Parse(serializedNode.GetAttribute("guid"))))
                   )
                {
                    button.Background = Brushes.Red;
                    nodeviewmodel.SetStateCommand.Execute(ElementState.Error);
                }

                button.Content = serializedNode.GetAttribute("nickname") + " : " + "show value here!";
                button.MinWidth = 200;
                button.MinHeight = 20;
                button.Tag = serializedNode;
                templist.Add(nodeviewmodel);
                //TODO
                //when the button is clicked we want to enter into a modal selection, for now we can just use the first selected node
                //of the same type
                //button.Click += handleNodeButtonPress;
                tempWorkSpace.AddNode(nodeviewmodel.NodeModel, false);
            }
           
            NodeListItems = new ReadOnlyObservableCollection<NodeViewModel>(templist);
        }

       

        //TODO this is a broken version of what nodeFactory will do for us
        private object GetInstance(string strFullyQualifiedName,XmlElement existingNode)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null && HasDefaultConstructor(type))
            {
                return Activator.CreateInstance(type);
            }
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null && HasDefaultConstructor(type))
                {
                    return Activator.CreateInstance(type);
                }
                else if (type !=null)
                {
                    if (type == typeof(Dynamo.Nodes.CodeBlockNodeModel))
                    {
                        return Activator.CreateInstance(type, new object[] { dynamoViewModel.Model.LibraryServices });
                    }
                    if (type == typeof(Dynamo.Nodes.DSFunction))
                    {
                        return new DSFunction(dynamoViewModel.Model.LibraryServices.GetFunctionDescriptor(existingNode.GetAttribute("nickname")));
                    }

                }
            }
           
           
            return null;
        }

        bool HasDefaultConstructor(Type t)
        {
            return t.GetConstructor(Type.EmptyTypes) != null;
        }

    }
}