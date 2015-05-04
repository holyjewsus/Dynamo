﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Core;
using Dynamo.Selection;
using Dynamo.Nodes;
using System.Xml;
using Dynamo.Interfaces;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Dynamo.Utilities;
using System.Collections.Specialized;

namespace Dynamo.Models
{
    /// <summary>
    /// a class that holds a set of preset design options states
    /// there is one instance of this class per workspacemodel
    /// </summary>
    public class PresetsModel : ILogSource, INotifyCollectionChanged
    {
        #region private members
        private readonly TrulyObservableCollection<PresetState> designStates;

        private void LoadStateFromXml(string name, string description, List<NodeModel> nodes, List<XmlElement> serializednodes, Guid id)
        {
            var loadedState = new PresetState(name, description, nodes, serializednodes, id);
            designStates.Add(loadedState);
        }
        #endregion
        
        # region properties
        public ReadOnlyObservableCollection<PresetState> DesignStates { get { return new ReadOnlyObservableCollection<PresetState> (designStates);} }
        #endregion

        #region constructor
        public PresetsModel()
        {
            designStates = new TrulyObservableCollection<PresetState>(new List<PresetState>().AsEnumerable());
            //designStates.CollectionChanged += new NotifyCollectionChangedEventHandler((o,e) => { OnCollectionChanged(o, e); });   
        }
        #endregion

        #region serialization / deserialzation
        // we serialze the presets to xml like a model, but we deserialze them before the workspacemodel is constructed
        //during save and load of the graph, can just inject this into workspacemodel 
        //when a new designstate is created we'll serialize all the current nodes into a new xmlelement
        //but not actually write this xml to a file until the graph is saved

        //grabbed some methods needed from modelbase for serialization
        protected virtual XmlElement CreateElement(XmlDocument xmlDocument, SaveContext context)
        {
            string typeName = GetType().ToString();
            XmlElement element = xmlDocument.CreateElement(typeName);
            return element;
        }

        public XmlElement Serialize(XmlDocument xmlDocument, SaveContext context)
        {
            var element = CreateElement(xmlDocument, context);
            SerializeCore(element, context);
            return element;
        }

        protected virtual void SerializeCore(System.Xml.XmlElement element, SaveContext context)
        {

            foreach (var state in designStates)
            {
                var parent = element.OwnerDocument.CreateElement("PresetState");
                element.AppendChild(parent);
                parent.SetAttribute("Name", state.Name);
                parent.SetAttribute("Description", state.Description);
                parent.SetAttribute("guid", state.Guid.ToString());
                //the states are already serialized
                foreach (var serializedNode in state.SerializedNodes)
                {
                    //need to import the node to cross xml contexts
                    var importNode = parent.OwnerDocument.ImportNode(serializedNode, true);
                    parent.AppendChild(importNode);
                }

            }
        }

        internal static PresetsModel LoadFromXml(XmlDocument xmlDoc, NodeGraph nodegraph,ILogger logger)
        {
            var loadedStateSet = new PresetsModel();

            //create a new state inside the set foreach state present in the xmldoc

            foreach (XmlElement element in xmlDoc.DocumentElement.ChildNodes)
            {
                if (element.Name == typeof(PresetsModel).ToString())
                {
                    foreach (XmlElement stateNode in element.ChildNodes)
                    {
                        var name = stateNode.GetAttribute("Name");
                        var des = stateNode.GetAttribute("Description");
                        var stateguidString = stateNode.GetAttribute("guid");

                        Guid stateID;
                        if (!Guid.TryParse(stateguidString, out stateID))
                        {
                            logger.LogError("unable to parse the GUID for preset state: " +name+ ", will atttempt to load this state anyway");
                        }

                        var nodes = new List<NodeModel>();
                        var deserialzedNodes = new List<XmlElement>();
                        //now find the nodes we're looking for by their guids in the loaded nodegraph
                        //it's possible they may no longer be present, and we must not fail to set the
                        //TODO//rest of the nodes but log this to the console.

                        //iterate each actual saved nodemodel in each state
                        foreach (XmlElement node in stateNode.ChildNodes)
                        {
                            var nodename = stateNode.GetAttribute("nickname");
                            var guidString = node.GetAttribute("guid");
                            Guid nodeID;
                            if (!Guid.TryParse(guidString, out nodeID))
                            {
                                logger.LogError("unable to parse GUID for node " +nodename );
                                continue;
                            }

                            var nodebyGuid = nodegraph.Nodes.Where(x => x.GUID == nodeID).ToList();
                            if (nodebyGuid.Count > 0)
                            {
                                nodes.Add(nodebyGuid.First());
                                deserialzedNodes.Add(node);
                            }
                            else
                            {   //add the deserialized version anyway so we dont lose this node from all states.
                                deserialzedNodes.Add(node);
                                logger.Log(nodename + nodeID.ToString() + " could not be found in the loaded .dyn");
                            }

                        }

                        loadedStateSet.LoadStateFromXml(name, des, nodes, deserialzedNodes, stateID);
                    }
                }
            }
            return loadedStateSet;
        }
        #endregion

        #region public methods
        /// <summary>
        /// method to create and add a new state to this presets collection
        /// </summary>
        public void CreateNewState(string name, string description, IEnumerable<NodeModel> currentSelection, Guid id = new Guid())
        {
            var inputs = currentSelection;
            var newstate = new PresetState(name, description, inputs, id);
            designStates.Add(newstate);
        }

        public void RemoveState(PresetState state)
        {
            designStates.Remove(state);
           // OnPropertyChange("DesignStates");
        }

        public void ReplaceState(PresetState oldstate, PresetState newstate)
        {
            if (oldstate.Guid != newstate.Guid)
            {
                throw new Exception("these states do not share the same GUID");
            }
            var oldindex = designStates.IndexOf(oldstate);
            designStates.Remove(oldstate);
            designStates.Insert(Math.Max(0,oldindex - 1),newstate);
        }

        #endregion

        #region ILogSource implementation
        public event Action<ILogMessage> MessageLogged;

        protected void Log(ILogMessage obj)
        {
            var handler = MessageLogged;
            if (handler != null) handler(obj);
        }

        protected void Log(string msg)
        {
            Log(LogMessage.Info(msg));
        }

        protected void Log(string msg, WarningLevel severity)
        {
            switch (severity)
            {
                case WarningLevel.Error:
                    Log(LogMessage.Error(msg));
                    break;
                default:
                    Log(LogMessage.Warning(msg, severity));
                    break;
            }
        }

        #endregion



        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private void OnCollectionChanged(object o, NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(o, args);
            }
         
        }
      
    }
}
