﻿using FluentCMS.Web.ApiClients.Services;
using FluentCMS.Web.UI.DynamicRendering;
using Microsoft.AspNetCore.Components.Routing;

namespace FluentCMS.Web.UI;

public partial class Default : IDisposable
{
    [Inject]
    private ILayoutProcessor LayoutProcessor { get; set; } = default!;

    [CascadingParameter]
    public ViewState ViewState { get; set; } = default!;

    [Parameter]
    public string? Route { get; set; }

    [Inject]
    public NavigationManager NavigationManager { set; get; } = default!;

    [Inject]
    public SetupManager SetupManager { set; get; } = default!;

    protected override async Task OnInitializedAsync()
    {
        NavigationManager.LocationChanged += LocationChanged;
        await Task.CompletedTask;
    }

    protected override async Task OnParametersSetAsync()
    {
        // check if setup is not done
        // if not it should be redirected to /setup route
        if (!await SetupManager.IsInitialized() && !NavigationManager.Uri.ToLower().EndsWith("/setup"))
        {
            NavigationManager.NavigateTo("/setup", true);
            return;
        }
    }

    void LocationChanged(object? sender, LocationChangedEventArgs e)
    {
        StateHasChanged();
    }

    void IDisposable.Dispose()
    {
        NavigationManager.LocationChanged -= LocationChanged;
    }

    protected RenderFragment RenderDynamicContent(string content) => builder =>
    {
        if (string.IsNullOrEmpty(content))
            return;

        var parameters = new Dictionary<string, object>
        {
            { "user", ViewState.User },
            { "site", ViewState.Site },
            { "page", ViewState.Page },
            { "plugins", ViewState.Plugins }
        };

        var segments = LayoutProcessor.ProcessSegments(content, parameters);
        var index = 0;
        foreach (var segment in segments)
        {
            if (segment.GetType() == typeof(HtmlSegment))
            {
                var htmlSegment = segment as HtmlSegment;
                builder.AddContent(index, (MarkupString)htmlSegment!.Content);
            }
            else if (segment.GetType() == typeof(ComponentSegment))
            {
                var componentSegment = segment as ComponentSegment;
                builder.OpenComponent(index, componentSegment!.Type);

                var attributeIndex = 0;
                foreach (var attribute in componentSegment.Attributes)
                {
                    builder.AddComponentParameter(attributeIndex, attribute.Key, attribute.Value);
                    attributeIndex++;
                }

                builder.CloseComponent();
            }
            index++;
        }
    };
}
