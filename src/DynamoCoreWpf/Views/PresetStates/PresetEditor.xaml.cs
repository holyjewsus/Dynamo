using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Models;

namespace Dynamo.UI
{
    /// <summary>
    /// Interaction logic for PresetEditor.xaml
    /// </summary>
    public partial class PresetEditor : Window
    {
        private PresetsViewModel presetCollectionView;

        public PresetEditor(PresetsViewModel presetCollectionView)
        {
            InitializeComponent();

            this.Owner = WpfUtilities.FindUpVisualTree<DynamoView>(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.presetCollectionView = presetCollectionView;
            this.DataContext = presetCollectionView;
            this.Closed += PresetEditor_Closed;
          
        }

        void PresetEditor_Closed(object sender, EventArgs e)
        {
            presetCollectionView.Dispose();
        }
        private void MoreButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.ContextMenu.DataContext = button.DataContext;
            button.ContextMenu.IsOpen = true;
        }

        private void RestoreState_Click(object sender, RoutedEventArgs e)
        {
            presetCollectionView.RestoreState(sender,e);
        }

     
       
    }
}
