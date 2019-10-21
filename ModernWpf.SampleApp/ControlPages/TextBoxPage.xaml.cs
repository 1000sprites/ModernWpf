﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModernWpf.SampleApp.ControlPages
{
    /// <summary>
    /// Interaction logic for TextBoxPage.xaml
    /// </summary>
    public partial class TextBoxPage
    {
        public TextBoxPage()
        {
            InitializeComponent();
        }

        private void ClearClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.Clear();
        }

        private void OptionsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            OptionsExpander.Header = "Hide options";
        }

        private void OptionsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            OptionsExpander.Header = "Show options";
        }
    }
}
