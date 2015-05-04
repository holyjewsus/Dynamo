using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Nodes;
using ProtoCore.Namespace;

namespace Dynamo.Models
{
    public class PresetWorkspaceModel : WorkspaceModel
    {
        #region Contructors

        private PresetState state;
       
        public PresetWorkspaceModel(
            PresetsModel designOptions,
            PresetState state,
            WorkspaceInfo info,
            NodeFactory factory)
            : this(state,
                factory,
                designOptions,
                Enumerable.Empty<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                Enumerable.Empty<AnnotationModel>(),
                info) { }

        public PresetWorkspaceModel(
            PresetState state,
            NodeFactory factory,
            PresetsModel designOptions,
            IEnumerable<NodeModel> e,
            IEnumerable<NoteModel> n,
            IEnumerable<AnnotationModel> a,
            WorkspaceInfo info,
            ElementResolver elementResolver = null)
            : base(e, n, a, info, factory, designOptions)
        {
            HasUnsavedChanges = false;
            this.state = state;
            if (elementResolver != null)
            {
                ElementResolver.CopyResolutionMap(elementResolver);
            }
            PropertyChanged += OnPropertyChanged;
        }

        #endregion

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Name")
                OnInfoChanged();
        }

        public event Action InfoChanged;
        protected virtual void OnInfoChanged()
        {
            Action handler = InfoChanged;
            if (handler != null) handler();
        }

       
        //when nodes are modified, we'll search for their serialized representation in the presetStateModel and 
        //update the xml to reflect their new values
        protected override void NodeModified(NodeModel node)
        {
            base.NodeModified(node);
            updatedPresetStateModel();
        }

        private void updatedPresetStateModel()
        {
            //this simply reserialzes everything in the state on every node modification... definitely wasteful, but
            //it's unclear this will be a problem...definitely the simplest
           var updatedstate = new PresetState(state.Name,state.Description,this.Nodes,state.Guid);
           presetsCollection.ReplaceState(state, updatedstate);
           state = updatedstate;

            //find the node in the serialized nodes list that matches the id of the node thats been updated
           // var modifedxmlNode = state.SerializedNodes.ToList().Find(x => (Guid.Parse(x.GetAttribute("guid"))) == node.GUID);
            //now serialze it again...
           // modifiedxmlNode = node.Serialize()
        }
       
        protected override void SerializeSessionData(XmlDocument document, ProtoCore.Core core)
        {
            // Since custom workspace does not have any runtime data to persist,
            // do not allow base class to serialize any session data.
        }
    }
}
