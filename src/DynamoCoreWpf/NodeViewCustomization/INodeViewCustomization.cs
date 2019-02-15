using System;
using System.Collections.Generic;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;

namespace Dynamo.Wpf
{
    public interface INodeViewCustomization<in T> : IDisposable where T : NodeModel
    {
        void CustomizeView(T model, NodeView nodeView);
    }
    public interface IZTNodeViewCustomization : IDisposable
    {
        void CustomizeView(NodeView nodeView);
        Action<Dictionary<string, string>> RequestBindData { get; set; }
    }

}
