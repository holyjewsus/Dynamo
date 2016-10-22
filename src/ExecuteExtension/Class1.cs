using Dynamo.Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Utilities;
using System.Windows;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.ViewModels;
using Dynamo.Views;
using Dynamo.Extensions;
using System.Windows.Controls;
using System.Windows.Threading;
using Dynamo.Graph.Workspaces;
using System.Windows.Input;

namespace Dynamo.ExecuteExtension
{
    public class ExecuteExtension : IViewExtension
    {
        private ViewLoadedParams viewLoadedParams;

        public string Name
        {
            get
            {
                return "executeExtension";
            }
        }

        public string UniqueId
        {
            get
            {
                return "lf6cd025-514f-44cd-b6b1-69d9g3cce004";

            }
        }

        public void Dispose()
        {
        }

        public void Loaded(ViewLoadedParams p)
        {
            viewLoadedParams = p;
            //foreach nodeView inject a button that calls NodeModified on that node.
          p.DynamoWindow.LayoutUpdated += CurrentWorkspaceModel_NodeAdded;
        }

        private void CurrentWorkspaceModel_NodeAdded(object sender, EventArgs e)
        {

            var dynView = viewLoadedParams.DynamoWindow as DynamoView;
            var nodeViews = dynView.ChildrenOfType<WorkspaceView>().First().ChildrenOfType<NodeView>();
            
            //var nodeView = nodeViews.Where(x => x.ViewModel.NodeModel.GUID == obj.GUID).First();
            //inject button

            foreach(var nodeView in nodeViews)
            {
                //if the node doesnt already have a button add one
                if (!(nodeView.inputGrid.Children.OfType<Button>().Any(x=>x.Name == "executeButton")))
                {
                    var label = new Label();
                    label.Content = "Execute";
                    var bt = new Button();
                    bt.Background = new SolidColorBrush(Colors.DarkOrange);
                    bt.Name = "executeButton";
                    bt.Content = label;

                    nodeView.inputGrid.Children.Add(bt);
                }
               
            }
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void Startup(ViewStartupParams p)
        {
        }
    }
        public static class utils {
            public static T ChildOfType<T>(this DependencyObject parent, string childName = null)
            where T : DependencyObject
            {
                // Confirm parent and childName are valid. 
                if (parent == null) return null;

                if (childName != null)
                {
                    return parent.ChildrenOfType<T>()
                        .FirstOrDefault(x =>
                        {
                            var xf = x as FrameworkElement;
                            if (xf == null) return false;
                            return xf.Name == childName;
                        });
                }

                return parent.ChildrenOfType<T>().FirstOrDefault();
            }

            public static IEnumerable<T> ChildrenOfType<T>(this DependencyObject parent)
             where T : DependencyObject
            {
                foreach (var child in parent.Children())
                {
                    var childType = child as T;
                    if (childType == null)
                    {
                        foreach (var ele in ChildrenOfType<T>(child)) yield return ele;
                    }
                    else
                    {
                        yield return childType;
                    }
                }
            }

            public static IEnumerable<DependencyObject> Children(this DependencyObject parent)
            {
                var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
                for (var i = 0; i < childrenCount; i++)
                    yield return VisualTreeHelper.GetChild(parent, i);
            }

        }
    }
