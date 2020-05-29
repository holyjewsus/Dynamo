using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using System;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    /// <summary>
    /// A Dynamo point class which supports the RenderCustom technique.
    /// </summary>
    [Obsolete("Do not use! This will be moved to a new project in a future version of Dynamo.")]
    public class DynamoPointGeometryModel3D : PointGeometryModel3D
    {

        public DynamoPointGeometryModel3D()
        {
        }

        protected override SceneNode OnCreateSceneNode()
        {
            return new DynamoPointNode() { Material = material };
        }


    }

    public class DynamoPointNode : PointNode
    {
        protected override IRenderTechnique OnCreateRenderTechnique(IRenderHost host)
        {
            return base.OnCreateRenderTechnique(host);
        }
    }
}
