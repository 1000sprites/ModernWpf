﻿using Microsoft.Win32;
using ModernWpf.DesignTime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace ModernWpf
{
    public class ThemeManager : DependencyObject
    {
        internal const string LightKey = "Light";
        internal const string DarkKey = "Dark";
        internal const string HighContrastKey = "HighContrast";

        private static readonly Binding _highContrastBinding = new Binding(nameof(BindableSystemParameters.HighContrast))
        {
            Source = BindableSystemParameters.Current
        };

        private static readonly Dictionary<string, ResourceDictionary> _defaultThemeDictionaries = new Dictionary<string, ResourceDictionary>();

        private bool _isInitialized;
        private bool _applicationStarted;

        #region ApplicationTheme

        /// <summary>
        /// Identifies the ApplicationTheme dependency property.
        /// </summary>
        public static readonly DependencyProperty ApplicationThemeProperty =
            DependencyProperty.Register(
                nameof(ApplicationTheme),
                typeof(ApplicationTheme?),
                typeof(ThemeManager),
                new PropertyMetadata(OnApplicationThemeChanged));

        /// <summary>
        /// Gets or sets a value that determines the light-dark preference for the overall
        /// theme of an app.
        /// </summary>
        /// <returns>
        /// A value of the enumeration. The initial value is the default theme set by the
        /// user in Windows settings.
        /// </returns>
        public ApplicationTheme? ApplicationTheme
        {
            get => (ApplicationTheme?)GetValue(ApplicationThemeProperty);
            set => SetValue(ApplicationThemeProperty, value);
        }

        private static void OnApplicationThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeManager)d).UpdateActualApplicationTheme();
        }

        #endregion

        #region ActualApplicationTheme

        internal static readonly DependencyProperty ActualApplicationThemeProperty =
            DependencyProperty.Register(
                nameof(ActualApplicationTheme),
                typeof(ApplicationTheme?),
                typeof(ThemeManager),
                new PropertyMetadata(OnActualApplicationThemeChanged));

        internal ApplicationTheme? ActualApplicationTheme
        {
            get => (ApplicationTheme?)GetValue(ActualApplicationThemeProperty);
            set => SetValue(ActualApplicationThemeProperty, value);
        }

        private static void OnActualApplicationThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeManager)d).OnActualApplicationThemeChanged(e);
        }

        private void OnActualApplicationThemeChanged(DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert(e.NewValue != null);
            ApplyApplicationTheme();
        }

        private void UpdateActualApplicationTheme()
        {
            if (_applicationStarted)
            {
                if (UsingSystemTheme)
                {
                    ActualApplicationTheme = GetDefaultAppTheme();
                }
                else
                {
                    ActualApplicationTheme = ApplicationTheme ?? ModernWpf.ApplicationTheme.Light;
                }
            }
        }

        private void ApplyApplicationTheme()
        {
            if (_applicationStarted)
            {
                Debug.Assert(ThemeResources.Current != null);
                ThemeResources.Current.ApplyApplicationTheme(ActualApplicationTheme);
            }
        }

        #endregion

        #region AccentColor

        public static readonly DependencyProperty AccentColorProperty =
            DependencyProperty.Register(
                nameof(AccentColor),
                typeof(Color?),
                typeof(ThemeManager),
                new PropertyMetadata(OnAccentColorChanged));

        public Color? AccentColor
        {
            get => (Color?)GetValue(AccentColorProperty);
            set => SetValue(AccentColorProperty, value);
        }

        private static void OnAccentColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeManager)d).UpdateActualAccentColor();
        }

        #endregion

        #region ActualAccentColor

        private static readonly DependencyProperty ActualAccentColorProperty =
            DependencyProperty.Register(
                nameof(ActualAccentColor),
                typeof(Color),
                typeof(ThemeManager),
                new PropertyMetadata(OnActualAccentColorChanged));

        private Color ActualAccentColor
        {
            get => (Color)GetValue(ActualAccentColorProperty);
            set => SetValue(ActualAccentColorProperty, value);
        }

        private static void OnActualAccentColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeManager)d).ApplyAccentColor();
        }

        private void ApplyAccentColor()
        {
            if (ColorsHelper.Current.IsInitialized)
            {
                UpdateAccentColors();

                foreach (var themeDictionary in _defaultThemeDictionaries.Values)
                {
                    ColorsHelper.Current.UpdateBrushes(themeDictionary);
                }
            }
        }

        private void UpdateActualAccentColor()
        {
            if (ColorsHelper.Current.IsInitialized)
            {
                if (UsingSystemAccentColor)
                {
                    ActualAccentColor = ColorsHelper.GetSystemAccentColor();
                }
                else
                {
                    ActualAccentColor = AccentColor ?? ColorsHelper.DefaultAccentColor;
                }
            }
        }

        private void UpdateAccentColors()
        {
            if (UsingSystemAccentColor)
            {
                ColorsHelper.Current.FetchSystemAccentColors();
            }
            else
            {
                ColorsHelper.Current.SetAccent(ActualAccentColor);
            }
        }

        #endregion

        #region RequestedTheme

        /// <summary>
        /// Gets the UI theme that is used by the UIElement (and its child elements)
        /// for resource determination. The UI theme you specify with RequestedTheme can
        /// override the app-level RequestedTheme.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>A value of the enumeration, for example **Light**.</returns>
        public static ElementTheme GetRequestedTheme(FrameworkElement element)
        {
            return (ElementTheme)element.GetValue(RequestedThemeProperty);
        }

        /// <summary>
        /// Sets the UI theme that is used by the UIElement (and its child elements)
        /// for resource determination. The UI theme you specify with RequestedTheme can
        /// override the app-level RequestedTheme.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetRequestedTheme(FrameworkElement element, ElementTheme value)
        {
            element.SetValue(RequestedThemeProperty, value);
        }

        /// <summary>
        /// Identifies the RequestedTheme dependency property.
        /// </summary>
        public static readonly DependencyProperty RequestedThemeProperty = DependencyProperty.RegisterAttached(
            "RequestedTheme",
            typeof(ElementTheme),
            typeof(ThemeManager),
            new PropertyMetadata(ElementTheme.Default, OnRequestedThemeChanged));

        private static void OnRequestedThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;

            StartOrStopListeningForHighContrastChanges(element);

            if (element.IsInitialized)
            {
                ApplyRequestedTheme(element);
            }
            else
            {
                SetSubscribedToInitialized(element, true);
            }
        }

        private static void ApplyRequestedTheme(FrameworkElement element)
        {
            var resources = element.Resources;
            var requestedTheme = GetRequestedTheme(element);
            ThemeResources.Current.UpdateMergedThemeDictionaries(resources, requestedTheme);

            if (element is Window window)
            {
                UpdateWindowActualTheme(window);
            }
            else if (requestedTheme == ElementTheme.Default)
            {
                element.ClearValue(ActualThemePropertyKey);
            }
            else
            {
                SetActualTheme(element, requestedTheme);
            }
        }

        #endregion

        #region ActualTheme

        /// <summary>
        /// Gets the UI theme that is currently used by the element, which might be different
        /// than the RequestedTheme.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>A value of the enumeration, for example **Light**.</returns>
        public static ElementTheme GetActualTheme(FrameworkElement element)
        {
            return (ElementTheme)element.GetValue(ActualThemeProperty);
        }

        internal static void SetActualTheme(FrameworkElement element, ElementTheme value)
        {
            element.SetValue(ActualThemePropertyKey, value);
        }

        private static readonly DependencyPropertyKey ActualThemePropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "ActualTheme",
                typeof(ElementTheme),
                typeof(ThemeManager),
                new FrameworkPropertyMetadata(
                    ElementTheme.Light,
                    FrameworkPropertyMetadataOptions.Inherits,
                    OnActualThemeChanged));

        /// <summary>
        /// Identifies the ActualTheme dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualThemeProperty =
            ActualThemePropertyKey.DependencyProperty;

        private static void OnActualThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe && GetHasThemeResources(fe))
            {
                UpdateThemeResourcesForElement(fe);
            }
        }

        internal static void UpdateWindowActualTheme(Window window)
        {
            var requestedTheme = GetRequestedTheme(window);
            if (requestedTheme == ElementTheme.Default)
            {
                var actualAppTheme = Current.ActualApplicationTheme;
                if (actualAppTheme.HasValue)
                {
                    SetActualTheme(window, actualAppTheme.Value == ModernWpf.ApplicationTheme.Dark ?
                        ElementTheme.Dark : ElementTheme.Light);
                }
            }
            else
            {
                SetActualTheme(window, requestedTheme);
            }
        }

        #endregion

        #region HasThemeResources

        public static readonly DependencyProperty HasThemeResourcesProperty =
            DependencyProperty.RegisterAttached(
                "HasThemeResources",
                typeof(bool),
                typeof(ThemeManager),
                new PropertyMetadata(OnHasThemeResourcesChanged));

        public static bool GetHasThemeResources(FrameworkElement element)
        {
            return (bool)element.GetValue(HasThemeResourcesProperty);
        }

        public static void SetHasThemeResources(FrameworkElement element, bool value)
        {
            element.SetValue(HasThemeResourcesProperty, value);
        }

        private static void OnHasThemeResourcesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;
            if ((bool)e.NewValue)
            {
                if (element.IsInitialized)
                {
                    UpdateThemeResourcesForElement(element);
                }
                else
                {
                    SetSubscribedToInitialized(element, true);
                }
            }
            else
            {
                UpdateSubscribedToInitialized(element);
            }
        }

        private static void UpdateThemeResourcesForElement(FrameworkElement element)
        {
            UpdateElementThemeResources(element.Resources, GetEffectiveThemeKey(element));
        }

        private static void UpdateElementThemeResources(ResourceDictionary resources, string themeKey)
        {
            if (resources is ElementThemeResources themeResources)
            {
                themeResources.Update(themeKey);
            }

            foreach (ResourceDictionary dictionary in resources.MergedDictionaries)
            {
                UpdateElementThemeResources(dictionary, themeKey);
            }
        }

        private static string GetEffectiveThemeKey(FrameworkElement element)
        {
            if (SystemParameters.HighContrast)
            {
                return HighContrastKey;
            }
            else
            {
                switch (GetActualTheme(element))
                {
                    case ElementTheme.Light:
                        return LightKey;
                    case ElementTheme.Dark:
                        return DarkKey;
                }
            }

            throw new InvalidOperationException();
        }

        #endregion

        #region SubscribedToInitialized

        private static readonly DependencyProperty SubscribedToInitializedProperty =
            DependencyProperty.RegisterAttached(
                "SubscribedToInitialized",
                typeof(bool),
                typeof(ThemeManager),
                new PropertyMetadata(default(bool), OnSubscribedToInitializedChanged));

        private static bool GetSubscribedToInitialized(FrameworkElement element)
        {
            return (bool)element.GetValue(SubscribedToInitializedProperty);
        }

        private static void SetSubscribedToInitialized(FrameworkElement element, bool value)
        {
            element.SetValue(SubscribedToInitializedProperty, value);
        }

        private static void OnSubscribedToInitializedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;
            if ((bool)e.NewValue)
            {
                element.Initialized += OnElementInitialized;
            }
            else
            {
                element.Initialized -= OnElementInitialized;
            }
        }

        private static void UpdateSubscribedToInitialized(FrameworkElement element)
        {
            if (ShouldSubscribeToInitialized(element))
            {
                SetSubscribedToInitialized(element, true);
            }
            else
            {
                element.ClearValue(SubscribedToInitializedProperty);
            }
        }

        private static bool ShouldSubscribeToInitialized(FrameworkElement element)
        {
            return !element.IsInitialized && (GetRequestedTheme(element) != ElementTheme.Default || GetHasThemeResources(element));
        }

        private static void OnElementInitialized(object sender, EventArgs e)
        {
            var element = (FrameworkElement)sender;
            element.ClearValue(SubscribedToInitializedProperty);

            if (GetRequestedTheme(element) != ElementTheme.Default)
            {
                ApplyRequestedTheme(element);
            }

            if (GetHasThemeResources(element))
            {
                UpdateThemeResourcesForElement(element);
            }
        }

        #endregion

        #region HighContrast

        //private static bool GetHighContrast(FrameworkElement element)
        //{
        //    return (bool)element.GetValue(HighContrastProperty);
        //}

        //private static void SetHighContrast(FrameworkElement element, bool value)
        //{
        //    element.SetValue(HighContrastProperty, value);
        //}

        private static readonly DependencyProperty HighContrastProperty =
            DependencyProperty.RegisterAttached(
                "HighContrast",
                typeof(bool),
                typeof(ThemeManager),
                new PropertyMetadata(OnHighContrastChanged));

        private static void OnHighContrastChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;
            if (element.IsInitialized)
            {
                ApplyRequestedTheme(element);
                UpdateThemeResourcesForElement(element);
            }
            else
            {
                SetSubscribedToInitialized(element, true);
            }
        }

        #endregion

        #region IsListeningForHighContrastChanges

        private static bool GetIsListeningForHighContrastChanges(FrameworkElement element)
        {
            return (bool)element.GetValue(IsListeningForHighContrastChangesProperty);
        }

        private static void SetIsListeningForHighContrastChanges(FrameworkElement element, bool value)
        {
            element.SetValue(IsListeningForHighContrastChangesProperty, value);
        }

        private static readonly DependencyProperty IsListeningForHighContrastChangesProperty =
            DependencyProperty.RegisterAttached(
                "IsListeningForHighContrastChanges",
                typeof(bool),
                typeof(ThemeManager),
                new PropertyMetadata(OnIsListeningForHighContrastChangesChanged));

        private static void OnIsListeningForHighContrastChangesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;

            if ((bool)e.OldValue)
            {
                element.ClearValue(HighContrastProperty);
            }

            if ((bool)e.NewValue)
            {
                element.SetBinding(HighContrastProperty, _highContrastBinding);
            }
        }

        private static bool ShouldListenForHighContrastChanges(FrameworkElement element)
        {
            return GetRequestedTheme(element) != ElementTheme.Default || GetHasThemeResources(element);
        }

        private static void StartOrStopListeningForHighContrastChanges(FrameworkElement element)
        {
            if (ShouldListenForHighContrastChanges(element))
            {
                SetIsListeningForHighContrastChanges(element, true);
            }
            else
            {
                element.ClearValue(IsListeningForHighContrastChangesProperty);
            }
        }

        #endregion

        static ThemeManager()
        {
            MenuDropAlignmentHelper.EnsureStandardPopupAlignment();
        }

        private ThemeManager()
        {
        }

        public static ThemeManager Current { get; } = new ThemeManager();

        //internal static Uri DefaultLightSource { get; } = GetDefaultSource(LightKey);

        //internal static Uri DefaultDarkSource { get; } = GetDefaultSource(DarkKey);

        //internal static Uri DefaultHighContrastSource { get; } = GetDefaultSource(HighContrastKey);

        //internal static ResourceDictionary DefaultLightThemeDictionary => GetDefaultThemeDictionary(LightKey);

        //internal static ResourceDictionary DefaultDarkThemeDictionary => GetDefaultThemeDictionary(DarkKey);

        //internal static ResourceDictionary DefaultHighContrastThemeDictionary => GetDefaultThemeDictionary(HighContrastKey);

        internal bool UsingSystemTheme => ColorsHelper.SystemColorsSupported && ApplicationTheme == null;

        internal bool UsingSystemAccentColor => ColorsHelper.SystemColorsSupported && AccentColor == null;

        private static Uri GetDefaultSource(string theme)
        {
            return PackUriHelper.GetAbsoluteUri($"ThemeResources/{theme}.xaml");
        }

        private static ApplicationTheme GetDefaultAppTheme()
        {
            try
            {
                var personalize = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (personalize != null)
                {
                    var appsUseLightTheme = personalize.GetValue("AppsUseLightTheme");
                    if (appsUseLightTheme is int value && value != 1)
                    {
                        return ModernWpf.ApplicationTheme.Dark;
                    }
                }
            }
            catch (Exception)
            {
            }

            return ModernWpf.ApplicationTheme.Light;
        }

        internal static ResourceDictionary GetDefaultThemeDictionary(string key)
        {
            if (!_defaultThemeDictionaries.TryGetValue(key, out ResourceDictionary dictionary))
            {
                dictionary = new ResourceDictionary { Source = GetDefaultSource(key) };
                _defaultThemeDictionaries[key] = dictionary;
            }
            return dictionary;
        }

        private static ResourceDictionary FindDictionary(ResourceDictionary parent, Uri source)
        {
            if (parent.Source == source)
            {
                return parent;
            }
            else
            {
                foreach (var md in parent.MergedDictionaries)
                {
                    if (md != null)
                    {
                        if (md.Source == source)
                        {
                            return md;
                        }
                        else
                        {
                            var result = FindDictionary(md, source);
                            if (result != null)
                            {
                                return result;
                            }
                        }
                    }
                }
            }

            return null;
        }

        internal void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            Debug.Assert(ThemeResources.Current != null);

            SystemParameters.StaticPropertyChanged += OnSystemParametersChanged;

            if (Application.Current != null)
            {
                Application.Current.Startup += OnApplicationStartup;
            }

            _isInitialized = true;
        }

        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            _applicationStarted = true;

            var appResources = Application.Current.Resources;
            appResources.MergedDictionaries.RemoveAll<IntellisenseResourcesBase>();

            ColorsHelper.Current.BackgroundColorChanged += OnSystemBackgroundColorChanged;
            ColorsHelper.Current.AccentColorChanged += OnSystemAccentColorChanged;
            ColorsHelper.Current.Initialize();
            appResources.MergedDictionaries.Insert(0, ColorsHelper.Current.Colors);

            UpdateActualAccentColor();
            UpdateActualApplicationTheme();
        }

        private void OnSystemBackgroundColorChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (UsingSystemTheme)
                {
                    UpdateActualApplicationTheme();
                }
            });
        }

        private void OnSystemAccentColorChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (UsingSystemAccentColor)
                {
                    UpdateActualAccentColor();
                }
            });
        }

        private void OnSystemParametersChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SystemParameters.HighContrast))
            {
                ApplyApplicationTheme();
            }
        }
    }

    /// <summary>
    /// Declares the theme preference for an app.
    /// </summary>
    public enum ApplicationTheme
    {
        /// <summary>
        /// Use the **Light** default theme.
        /// </summary>
        Light = 0,
        /// <summary>
        /// Use the **Dark** default theme.
        /// </summary>
        Dark = 1
    }

    /// <summary>
    /// Specifies a UI theme that should be used for individual UIElement parts of an app UI.
    /// </summary>
    public enum ElementTheme
    {
        /// <summary>
        /// Use the Application.RequestedTheme value for the element. This is the default.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Use the **Light** default theme.
        /// </summary>
        Light = 1,
        /// <summary>
        /// Use the **Dark** default theme.
        /// </summary>
        Dark = 2
    }
}
