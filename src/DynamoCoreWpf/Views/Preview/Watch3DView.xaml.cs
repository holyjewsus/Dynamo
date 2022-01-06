using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Dynamo.Graph.Nodes;
using Dynamo.Visualization;
using Dynamo.Wpf.ViewModels.Watch3D;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Shaders;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Point = System.Windows.Point;

namespace Dynamo.Controls
{
    [StructLayout(LayoutKind.Sequential)]
    public struct vertposcol
    {
        public vertposcol(Vector3 position, SharpDX.Color color)
        {
            Position = position;
            Color = color;
        }
        /// <summary>
        /// XYZ position.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex color.
        /// </summary>
        public SharpDX.Color Color;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MatrixBufferType
    {
        public SharpDX.Matrix world;
        public SharpDX.Matrix view;
        public SharpDX.Matrix projection;
    };

    /// <summary>
    /// Interaction logic for WatchControl.xaml
    /// </summary>
    public partial class Watch3DView
    {
        #region private members

        private Point rightMousePoint;
        private Point3D prevCamera;
        private bool runUpdateClipPlane = false;

        #endregion

        #region public properties

        [Obsolete("Do not use! This will change its type in a future version of Dynamo.")]
        public Viewport3DX View
        {
            get { return watch_view; }
        }

        internal HelixWatch3DViewModel ViewModel { get; private set; }

        SharpDX.D3DCompiler.CompilationResult vertShader;
        SharpDX.D3DCompiler.CompilationResult pixelShader;
        SharpDX.Direct3D11.Buffer vertexBuffer;
        SharpDX.Direct3D11.Texture2D depthBuffer;
        #endregion

        #region constructors

        public Watch3DView()
        {
            InitializeComponent();
            Loaded += ViewLoadedHandler;
            Unloaded += ViewUnloadedHandler;
            compileShaders();

            View.OnRendered += (o,e) =>
            {

                InjectaTri();
            };
        }

        private void InjectaTri()
        {

            if (vertexBuffer == null)
            {


                vertexBuffer = global::SharpDX.Direct3D11.Buffer.Create(
          View.RenderHost.Device,
          global::SharpDX.Direct3D11.BindFlags.VertexBuffer,
          new[]
              {
                        new global::SharpDX.Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 0.0f, 0.0f, 1.0f), // Front
                        new global::SharpDX.Vector4(-1.0f, 1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                        new global::SharpDX.Vector4(1.0f, 1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                        new global::SharpDX.Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                        new global::SharpDX.Vector4(1.0f, 1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                        new global::SharpDX.Vector4(1.0f, -1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                        new global::SharpDX.Vector4(-1.0f, -1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 1.0f, 0.0f, 1.0f), // BACK
                        new global::SharpDX.Vector4(1.0f, 1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                        new global::SharpDX.Vector4(-1.0f, 1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                        new global::SharpDX.Vector4(-1.0f, -1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                        new global::SharpDX.Vector4(1.0f, -1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                        new global::SharpDX.Vector4(1.0f, 1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                        new global::SharpDX.Vector4(-1.0f, 1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 0.0f, 1.0f, 1.0f), // Top
                        new global::SharpDX.Vector4(-1.0f, 1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                        new global::SharpDX.Vector4(1.0f, 1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                        new global::SharpDX.Vector4(-1.0f, 1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                        new global::SharpDX.Vector4(1.0f, 1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                        new global::SharpDX.Vector4(1.0f, 1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                        new global::SharpDX.Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 1.0f, 0.0f, 1.0f), // Bottom
                        new global::SharpDX.Vector4(1.0f, -1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                        new global::SharpDX.Vector4(-1.0f, -1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                        new global::SharpDX.Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                        new global::SharpDX.Vector4(1.0f, -1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                        new global::SharpDX.Vector4(1.0f, -1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                        new global::SharpDX.Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 0.0f, 1.0f, 1.0f), // Left
                        new global::SharpDX.Vector4(-1.0f, -1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                        new global::SharpDX.Vector4(-1.0f, 1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                        new global::SharpDX.Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                        new global::SharpDX.Vector4(-1.0f, 1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                        new global::SharpDX.Vector4(-1.0f, 1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                        new global::SharpDX.Vector4(1.0f, -1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 1.0f, 1.0f, 1.0f), // Right
                        new global::SharpDX.Vector4(1.0f, 1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                        new global::SharpDX.Vector4(1.0f, -1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                        new global::SharpDX.Vector4(1.0f, -1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                        new global::SharpDX.Vector4(1.0f, 1.0f, -1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                        new global::SharpDX.Vector4(1.0f, 1.0f, 1.0f, 1.0f), new global::SharpDX.Vector4(0.0f, 1.0f, 1.0f, 1.0f),
              });
            }

            MatrixBufferType matrixCBBuffer;

            var view = View.Camera.GetViewMatrix3D().ToMatrix();
            var projection = View.Camera.GetProjectionMatrix3D(View.RenderHost.ActualHeight / View.RenderHost.ActualWidth).ToMatrix();
            var world = SharpDX.Matrix.Identity;
            view.Transpose();
            projection.Transpose();
            world.Transpose();


            matrixCBBuffer = new MatrixBufferType()
            {
                view = view,
                projection = projection,
                world = world
            };

            var context = View.RenderHost.ImmediateDeviceContext;
            if (context == null)
            {
                return;
            }

            var vshader = new HelixToolkit.Wpf.SharpDX.Shaders.VertexShader(View.RenderHost.Device,"vert", vertShader.Bytecode);
            var pshader = new HelixToolkit.Wpf.SharpDX.Shaders.PixelShader(View.RenderHost.Device,"pix", pixelShader.Bytecode);

          
            if(float.IsNaN(View.RenderHost.ActualHeight) || float.IsNaN(View.RenderHost.ActualWidth))
            {
                return;
            }
            if(depthBuffer == null)
            {
                depthBuffer = new Texture2D(View.RenderHost.Device, new Texture2DDescription()
                {
                    Format = Format.D32_Float_S8X24_UInt,
                    ArraySize = 1,
                    MipLevels = 1,
                    Width = (int)View.RenderHost.ActualWidth,
                    Height = (int)View.RenderHost.ActualHeight,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                });
            }
           


            var renderview = new RenderTargetView(View.RenderHost.Device, View.RenderHost.RenderBuffer.BackBuffer.Resource);
            var depthView = new DepthStencilView(View.RenderHost.Device, depthBuffer);
            var viewport = new Viewport(0, 0, (int)View.RenderHost.ActualWidth, (int)View.RenderHost.ActualHeight, 0f, 1f);
            context.SetViewport(ref viewport );
            context.SetRenderTarget(depthView, renderview);
            context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            //context.ClearRenderTargetView(renderview, SharpDX.Color.Green);


            var posElement = new InputElement("POSITION",0,SharpDX.DXGI.Format.R32G32B32A32_Float, 0,0);
            var ColorElement = new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,16, 0);
            var vinputdes = new InputLayoutDescription(vertShader.Bytecode, new InputElement[2] { posElement, ColorElement });

            var oldlayout = context.InputLayout;
            var oldprim = context.PrimitiveTopology;
            var oldshaderpass = context.CurrShaderPass;

            context.InputLayout = new InputLayout(View.RenderHost.Device, vinputdes.ShaderByteCode, vinputdes.InputElements);
            context.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            var cb = new global::SharpDX.Direct3D11.Buffer(
             View.RenderHost.Device,
             global::SharpDX.Utilities.SizeOf<MatrixBufferType>(),
             global::SharpDX.Direct3D11.ResourceUsage.Default,
             global::SharpDX.Direct3D11.BindFlags.ConstantBuffer,
             global::SharpDX.Direct3D11.CpuAccessFlags.None,
             global::SharpDX.Direct3D11.ResourceOptionFlags.None,
             0);
            context.UpdateSubresource<MatrixBufferType>(ref matrixCBBuffer, cb, 0);
            
            context.SetVertexBuffers(0, new SharpDX.Direct3D11.VertexBufferBinding(this.vertexBuffer, SharpDX.Utilities.SizeOf<SharpDX.Vector4>()*2, 0));
            context.SetShader(vshader);
            context.SetShader(pshader);
            ((SharpDX.Direct3D11.DeviceContext)context).VertexShader.SetConstantBuffer(0, cb);
            context.Draw(36, 0);
           
            context.Flush();
            View.InvalidateRender();
            context.InputLayout = oldlayout;
            context.PrimitiveTopology = oldprim;
            if(oldshaderpass != null)
            {
                context.SetShaderPass(oldshaderpass);

            }
        }

        private void compileShaders()
        {
            var vertexShaderSource = @"
/////////////
// GLOBALS //
/////////////
cbuffer MatrixBuffer
{
    matrix worldMatrix;
    matrix viewMatrix;
    matrix projectionMatrix;
};
//////////////
// TYPEDEFS //
//////////////
struct VertexInputType
{
    float4 position : POSITION;
    float4 color : COLOR;
};

struct PixelInputType
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};

////////////////////////////////////////////////////////////////////////////////
// Vertex Shader
////////////////////////////////////////////////////////////////////////////////
PixelInputType ColorVertexShader(VertexInputType input)
{
    PixelInputType output;
    

    // Change the position vector to be 4 units for proper matrix calculations.
    input.position.w = 1.0f;

    // Calculate the position of the vertex against the world, view, and projection matrices.
    output.position = mul(input.position, worldMatrix);
    output.position = mul(output.position, viewMatrix);
    output.position = mul(output.position, projectionMatrix);
    // Store the input color for the pixel shader to use.
    output.color = input.color;
     
    return output;
}

";

            var pixelShaderSource = @"
struct PixelInputType
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};
////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 ColorPixelShader(PixelInputType input) : SV_TARGET
{
    return input.color;
}
";


             this.vertShader = SharpDX.D3DCompiler.ShaderBytecode.Compile(vertexShaderSource, "ColorVertexShader", "vs_4_0", SharpDX.D3DCompiler.ShaderFlags.None, SharpDX.D3DCompiler.EffectFlags.None, "vert");
             this.pixelShader = SharpDX.D3DCompiler.ShaderBytecode.Compile(pixelShaderSource, "ColorPixelShader", "ps_4_0", SharpDX.D3DCompiler.ShaderFlags.None, SharpDX.D3DCompiler.EffectFlags.None, "pixel");




        }

        #endregion

        #region event registration

        private void UnregisterEventHandlers()
        {
            UnregisterButtonHandlers();

            CompositionTarget.Rendering -= CompositionTargetRenderingHandler;

            if (ViewModel == null) return;

            UnRegisterViewEventHandlers();

            ViewModel.RequestCreateModels -= RequestCreateModelsHandler;
            ViewModel.RequestRemoveModels -= RequestRemoveModelsHandler;
            ViewModel.RequestViewRefresh -= RequestViewRefreshHandler;
            ViewModel.RequestClickRay -= GetClickRay;
            ViewModel.RequestCameraPosition -= GetCameraPosition;
            ViewModel.RequestZoomToFit -= ViewModel_RequestZoomToFit;
            this.DataContext = null;
            if (watch_view != null)
            {
                watch_view.Items.Clear();
                watch_view.DataContext = null;
                watch_view.Dispose();
            }

        }

        private void RegisterEventHandlers()
        {
            CompositionTarget.Rendering += CompositionTargetRenderingHandler;

            RegisterButtonHandlers();

            RegisterViewEventHandlers();

            ViewModel.RequestCreateModels += RequestCreateModelsHandler;
            ViewModel.RequestRemoveModels += RequestRemoveModelsHandler;
            ViewModel.RequestViewRefresh += RequestViewRefreshHandler;
            ViewModel.RequestClickRay += GetClickRay;
            ViewModel.RequestCameraPosition += GetCameraPosition;
            ViewModel.RequestZoomToFit += ViewModel_RequestZoomToFit;

            ViewModel.UpdateUpstream();
            ViewModel.OnWatchExecution();

        }

        private void RegisterButtonHandlers()
        {
            MouseLeftButtonDown += MouseButtonIgnoreHandler;
            MouseLeftButtonUp += MouseButtonIgnoreHandler;
            MouseRightButtonUp += view_MouseRightButtonUp;
            PreviewMouseRightButtonDown += view_PreviewMouseRightButtonDown;
        }

        private void RegisterViewEventHandlers()
        {
            watch_view.MouseDown += ViewModel.OnViewMouseDown;
            watch_view.MouseUp += WatchViewMouseUphandler;
            watch_view.MouseMove += ViewModel.OnViewMouseMove;
            watch_view.CameraChanged += WatchViewCameraChangedHandler;

        }

        private void WatchViewCameraChangedHandler(object sender, RoutedEventArgs e)
        {
            var view = sender as Viewport3DX;
            if (view != null)
            {
                e.Source = view.GetCameraPosition();
            }
            ViewModel.OnViewCameraChanged(sender, e);
        }

        private void WatchViewMouseUphandler(object sender, MouseButtonEventArgs e)
        {
            ViewModel.OnViewMouseUp(sender, e);
            //Call update on completion of user manipulation of the scene
            runUpdateClipPlane = true;
        }

        private void UnRegisterViewEventHandlers()
        {
            watch_view.MouseDown -= ViewModel.OnViewMouseDown;
            watch_view.MouseUp -= WatchViewMouseUphandler;
            watch_view.MouseMove -= ViewModel.OnViewMouseMove;
            watch_view.CameraChanged -= WatchViewCameraChangedHandler;
        }

        private void UnregisterButtonHandlers()
        {
            MouseLeftButtonDown -= MouseButtonIgnoreHandler;
            MouseLeftButtonUp -= MouseButtonIgnoreHandler;
            MouseRightButtonUp -= view_MouseRightButtonUp;
            PreviewMouseRightButtonDown -= view_PreviewMouseRightButtonDown;
        }

        #endregion

        #region event handlers

        private void ViewUnloadedHandler(object sender, RoutedEventArgs e)
        {
            UnregisterEventHandlers();
            Loaded -= ViewLoadedHandler;
            Unloaded -= ViewUnloadedHandler;
        }

        private void ViewLoadedHandler(object sender, RoutedEventArgs e)
        {
            ViewModel = DataContext as HelixWatch3DViewModel;

            if (ViewModel == null) return;

            RegisterEventHandlers();
        }

        private void ViewModel_RequestZoomToFit(BoundingBox bounds)
        {
            var prevcamDir = watch_view.Camera.LookDirection;
            watch_view.ZoomExtents(bounds.ToRect3D(.05));
            //if after a zoom the camera is in an undefined position or view direction, reset it.
            if (watch_view.Camera.Position.ToVector3().IsUndefined() ||
                watch_view.Camera.LookDirection.ToVector3().IsUndefined() ||
                watch_view.Camera.LookDirection.Length == 0)
            {
                watch_view.Camera.Position = prevCamera;
                watch_view.Camera.LookDirection = prevcamDir;
            }
        }

        private void RequestViewRefreshHandler()
        {
            View.InvalidateRender();
            //Call update to the clipping plane after the scene items are updated
            runUpdateClipPlane = true;
        }

        private void RequestCreateModelsHandler(RenderPackageCache packages, bool forceAsyncCall = false)
        {
            if (!forceAsyncCall && CheckAccess())
            {
                ViewModel.GenerateViewGeometryFromRenderPackagesAndRequestUpdate(packages);
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => ViewModel.GenerateViewGeometryFromRenderPackagesAndRequestUpdate(packages)));
            }
        }

        private void RequestRemoveModelsHandler(NodeModel node)
        {
            if (CheckAccess())
            {
                ViewModel.DeleteGeometryForNode(node);
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => ViewModel.DeleteGeometryForNode(node)));
            }
        }

        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            var yAdjust = ActualHeight + e.VerticalChange;
            var xAdjust = ActualWidth + e.HorizontalChange;

            if (xAdjust >= inputGrid.MinWidth)
            {
                Width = xAdjust;
            }

            if (yAdjust >= inputGrid.MinHeight)
            {
                Height = yAdjust;
            }
        }

        private void CompositionTargetRenderingHandler(object sender, EventArgs e)
        {
            // https://github.com/DynamoDS/Dynamo/issues/7295
            // This should not crash Dynamo when View is null
            try
            {
                //Do not call the clip plane update on the render loop if the camera is unchanged or
                //the user is manipulating the view with mouse.  Do run when queued by runUpdateClipPlane bool 
                if (runUpdateClipPlane || (!View.Camera.Position.Equals(prevCamera) && !View.IsMouseCaptured))
                {
                    ViewModel.UpdateNearClipPlane();
                    runUpdateClipPlane = false;
                }
                ViewModel.ComputeFrameUpdate();
                prevCamera = View.Camera.Position;
            }
            catch (Exception ex)
            {
                ViewModel.CurrentSpaceViewModel.DynamoViewModel.Model.Logger.Log(ex.ToString());
            }
        }

        private void MouseButtonIgnoreHandler(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        private void view_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            rightMousePoint = e.GetPosition(watch3D);
        }

        private void view_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if the mouse has moved, and this is a right click, we assume 
            // rotation. handle the event so we don't show the context menu
            // if the user wants the contextual menu they can click on the
            // node sidebar or top bar
            if (e.GetPosition(watch3D) != rightMousePoint)
            {
                e.Handled = true;
            }
        }

        #endregion

        private IRay GetClickRay(MouseEventArgs args)
        {
            var mousePos = args.GetPosition(this);

            var ray = View.Point2DToRay3D(new Point(mousePos.X, mousePos.Y));

            if (ray == null) return null;

            var position = new Point3D(0, 0, 0);
            var normal = new Vector3D(0, 0, 1);
            var pt3D = ray.PlaneIntersection(position, normal);

            if (pt3D == null) return null;

            return new Ray3(ray.Origin, ray.Direction);
        }

        private Point3D GetCameraPosition()
        {
            return View.GetCameraPosition();
        }
    }


}
