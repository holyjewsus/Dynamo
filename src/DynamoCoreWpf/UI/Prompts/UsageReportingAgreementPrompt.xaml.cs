using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Dynamo.Logging;
using Dynamo.Services;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.UI.Prompts
{
    /// <summary>
    /// Interaction logic for UsageReportingAgreementPrompt.xaml
    /// </summary>
    public partial class UsageReportingAgreementPrompt : Window
    {
        private DynamoViewModel viewModel = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resourceProvider"></param>
        /// <param name="dynamoViewModel"></param>
        public UsageReportingAgreementPrompt(IBrandingResourceProvider resourceProvider, DynamoViewModel dynamoViewModel)
        {
            InitializeComponent();
            if (resourceProvider != null)
            {
                Title = resourceProvider.GetString(Wpf.Interfaces.ResourceNames.ConsentForm.Title);
                ConsentFormImageRectangle.Fill = new ImageBrush(
                    resourceProvider.GetImageSource(Wpf.Interfaces.ResourceNames.ConsentForm.Image));
            }
            viewModel = dynamoViewModel;
            var googleAnalyticsFile = "GoogleAnalyticsConsent.rtf";

            if (viewModel.Model.PathManager.ResolveDocumentPath(ref googleAnalyticsFile))
                GoogleAnalyticsConsent.File = googleAnalyticsFile;

            var dialogStrings = AnalyticsService.GetDialogStrings();
            var html = @"<html>
<head>
<style>
body {
  color: #BFBFBF;
  background-color: #363636;
  font-family: 'Veranda', Arial, sans-serif;
  font-size: 10pt;
  overflow:auto;
}
</style>
</head>
<body>

REPLACE_ME
</ body >
</ html >";

            html = html.Replace("REPLACE_ME",dialogStrings["DialogIntroduction"] as string);
             ADPAnalyticsConsent.NavigateToString(html);
            ADPAnalyticsConsent.Navigating += ADPAnalyticsConsent_Navigating;

            //build adp ui from dialog strings;
            foreach(var consent in dialogStrings["copies"] as IEnumerable)
            {
                var checkbox = new CheckBox();
                checkbox.Margin = new Thickness(15, 16, 15, 14);
                checkbox.Foreground = new SolidColorBrush(Color.FromArgb(255, 71, 144, 205));
                checkbox.Background = new SolidColorBrush(Colors.White);
                checkbox.VerticalAlignment = VerticalAlignment.Center;
                checkbox.FontSize = 13.333;
                var childtb = new TextBlock();
                childtb.Text = (consent as IDictionary)["consentName"] as string;
                childtb.Text += System.Environment.NewLine;
                childtb.Text += (consent as IDictionary)["consentText"] as string;
                childtb.TextWrapping = TextWrapping.Wrap;
                checkbox.Content = childtb;
                adpStackPanel.Children.Add(checkbox);
            }



           // AcceptADPAnalyticsTextBlock.Text =
          //      string.Format(Wpf.Properties.Resources.ConsentFormADPAnalyticsCheckBoxContent,
          //          dynamoViewModel.BrandingResourceProvider.ProductName);
          //  AcceptADPAnalyticsCheck.Visibility = System.Windows.Visibility.Visible;
         //   AcceptADPAnalyticsCheck.IsChecked = AnalyticsService.IsADPOptedIn;

            AcceptGoogleAnalyticsCheck.IsChecked = UsageReportingManager.Instance.IsAnalyticsReportingApproved;

            if (Analytics.DisableAnalytics)
            {
            //    AcceptADPAnalyticsCheck.IsChecked = false;
            //    AcceptADPAnalyticsTextBlock.IsEnabled = false;
            //    AcceptADPAnalyticsCheck.IsEnabled = false;

                AcceptGoogleAnalyticsCheck.IsChecked = false;
                AcceptGoogleAnalyticsTextBlock.IsEnabled = false;
                AcceptGoogleAnalyticsCheck.IsEnabled = false;
            }
        }

        private void ADPAnalyticsConsent_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {

            //cancel the current event
            e.Cancel = true;
            try { 
            //this opens the URL in the user's default browser
            Process.Start(e.Uri.ToString());
                }
            catch
            {

            }
        }

        private void ToggleIsUsageReportingChecked(object sender, RoutedEventArgs e)
        {
            UsageReportingManager.Instance.SetUsageReportingAgreement(
                AcceptUsageReportingCheck.IsChecked.HasValue &&
                AcceptUsageReportingCheck.IsChecked.Value);
            AcceptUsageReportingCheck.IsChecked = UsageReportingManager.Instance.IsUsageReportingApproved;
        }

        private void ToggleIsGoogleAnalyticsChecked(object sender, RoutedEventArgs e)
        {
            UsageReportingManager.Instance.SetAnalyticsReportingAgreement(
                AcceptGoogleAnalyticsCheck.IsChecked.HasValue &&
                AcceptGoogleAnalyticsCheck.IsChecked.Value);
        }

        private void OnContinueClick(object sender, RoutedEventArgs e)
        {
            // Update user agreement
        //    AnalyticsService.IsADPOptedIn = AcceptADPAnalyticsCheck.IsChecked.Value;

            UsageReportingManager.Instance.SetAnalyticsReportingAgreement(AcceptGoogleAnalyticsCheck.IsChecked.Value);
            Close();
        }

        private void OnLearnMoreClick(object sender, RoutedEventArgs e)
        {
            var aboutBox = viewModel.BrandingResourceProvider.CreateAboutBox(viewModel);
            aboutBox.Owner = this;
            aboutBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            aboutBox.ShowDialog();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            viewModel = null;
        }
    }
}