﻿using SamplesCommon.SamplePages;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class PageTransitionsPage
    {
        public PageTransitionsPage()
        {
            InitializeComponent();

            TierTB.Text = "Rendering tier: " + (RenderCapability.Tier >> 16);
            frame.Navigate(new SamplePage1());
        }

        private void NavigateForward(object sender, RoutedEventArgs e)
        {
            var pageToNavigateTo = frame.BackStack.Cast<JournalEntry>().Count() % 2 == 1 ? new SamplePage1() as Page : new SamplePage2();
            frame.Navigate(pageToNavigateTo);
        }

        private void NavigateBackward(object sender, RoutedEventArgs e)
        {
            frame.GoBack();
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
        }

        private void DefaultRB_Checked(object sender, RoutedEventArgs e)
        {
            frame.ClearValue(StyleProperty);
        }

        private void DrillRB_Checked(object sender, RoutedEventArgs e)
        {
            frame.Style = (Style)Resources["DrillFrameStyle"];
        }

        private void SlideFromRightRB_Checked(object sender, RoutedEventArgs e)
        {
            frame.Style = (Style)Resources["SlideFromRightFrameStyle"];
        }

        private void SlideFromLeftRB_Checked(object sender, RoutedEventArgs e)
        {
            frame.Style = (Style)Resources["SlideFromLeftFrameStyle"];
        }
    }
}
