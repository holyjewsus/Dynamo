using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Wpf;

using CoreNodeModelsWpf.Controls;
using CoreNodeModels.Input;
using Dynamo.Graph.Workspaces;
using DSColor = DSCore.Color;

namespace CoreNodeModelsWpf.Nodes
{
    public class ColorPaletteNodeViewCustomization : NotificationObject, INodeViewCustomization<ColorPalette>
    {
        /// <summary>
        ///     WPF Control.
        /// </summary>
        private ColorPaletteUI ColorPaletteUINode;
        private ColorPalette colorPaletteNode;
        private Color mcolor;
        /// <summary>
        /// Selected Color
        /// </summary>
        public Color MColor
        {
            get { return mcolor; }
            set
            {
                if(mcolor != value)
                {
                    mcolor = value;
                    colorPaletteNode.dsColor = DSColor.ByARGB(mcolor.A, mcolor.R, mcolor.G, mcolor.B);
                    RaisePropertyChanged("MColor");
                }
               
            }
        }
        /// <summary>
        ///     Customize View.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="nodeView"></param>
        public void CustomizeView(ColorPalette model, NodeView nodeView)
        {
            colorPaletteNode = model;
            mcolor = Color.FromArgb(model.dsColor.Alpha, model.dsColor.Red, model.dsColor.Green, model.dsColor.Blue);
            model.PropertyChanged += Model_PropertyChanged;
            ColorPaletteUINode = new ColorPaletteUI(model, nodeView);
            nodeView.inputGrid.Children.Add(ColorPaletteUINode);
            ColorPaletteUINode.DataContext = this;
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
           if(e.PropertyName == "dsColor")
            {
                this.MColor = Color.FromArgb(this.colorPaletteNode.dsColor.Alpha,
                    this.colorPaletteNode.dsColor.Red,
                    this.colorPaletteNode.dsColor.Green,
                    this.colorPaletteNode.dsColor.Blue);
            }
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose() { }
    }
}
