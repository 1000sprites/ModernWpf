Thanks for installing the ModernWPF UI NuGet package!

Don't forget to add the theme resources to your Application resources in App.xaml:

    <Application
        ...
        xmlns:ui="http://schemas.modernwpf.com/2019">
        <Application.Resources>
            <ResourceDictionary>
                <ResourceDictionary.MergedDictionaries>
                    <ui:ThemeResources />
                    <ui:XamlControlsResources />
                    ...
                </ResourceDictionary.MergedDictionaries>
                ...
            </ResourceDictionary>
        </Application.Resources>
    </Application>

See https://github.com/Kinnara/ModernWpf for more information.
