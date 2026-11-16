using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GISORA.Presentation.Shared.Maps.Components;

public sealed partial class MapView : IAsyncDisposable
{
    private const string ModulePath = "./_content/GISORA.Presentation.Shared/js/map.js?v=4";
    private const string OpenStreetMapTiles = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";

    private readonly string _elementId = $"gisora-map-{Guid.NewGuid():N}";
    private readonly List<MapLayer> _pendingLayers = [];

    private IJSObjectReference? _module;
    private DotNetObjectReference<MapView>? _reference;

    [Inject]
    private IJSRuntime Js { get; set; } = default!;

    [Parameter]
    public double CenterLatitude { get; set; } = -7.95;

    [Parameter]
    public double CenterLongitude { get; set; } = -35.25;

    [Parameter]
    public int Zoom { get; set; } = 13;

    [Parameter]
    public string? TileUrl { get; set; } = OpenStreetMapTiles;

    [Parameter]
    public string Attribution { get; set; } = "OpenStreetMap";

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string Height { get; set; } = "24rem";

    [Parameter]
    public bool AutoFit { get; set; } = true;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public EventCallback<MapFeature> OnFeatureSelected { get; set; }

    [Parameter]
    public EventCallback<MapPopupAction> OnPopupAction { get; set; }

    [Parameter]
    public EventCallback OnReady { get; set; }

    public event Action<MapCoordinate>? CoordinateChanged;

    public event Action<MapFeature>? FeatureSelected;

    public event Action<MapPopupAction>? PopupActioned;

    public event Action? DrawingStateChanged;

    public bool IsReady => _module is not null;

    public string ElementId => _elementId;

    public DrawingTool? DrawingTool { get; private set; }

    internal void AttachDrawingTool(DrawingTool tool)
    {
        DrawingTool = tool;
        DrawingStateChanged?.Invoke();
    }

    internal void DetachDrawingTool(DrawingTool tool)
    {
        if (ReferenceEquals(DrawingTool, tool) is false)
        {
            return;
        }

        DrawingTool = null;
        DrawingStateChanged?.Invoke();
    }

    internal void NotifyDrawingStateChanged() => DrawingStateChanged?.Invoke();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender is false)
        {
            return;
        }

        _module = await Js.InvokeAsync<IJSObjectReference>("import", ModulePath);
        _reference = DotNetObjectReference.Create(this);

        await _module.InvokeVoidAsync("create", _elementId, new
        {
            CenterLatitude,
            CenterLongitude,
            Zoom,
            TileUrl,
            Attribution,
        }, _reference);

        foreach (var layer in _pendingLayers.ToArray())
        {
            await SetLayerAsync(layer);
        }

        _pendingLayers.Clear();

        if (AutoFit)
        {
            await FitBoundsAsync();
        }

        await OnReady.InvokeAsync();
    }

    public async Task SetLayerAsync(MapLayer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);

        if (_module is null)
        {
            _pendingLayers.Add(layer);

            return;
        }

        await _module.InvokeVoidAsync("setLayer", _elementId, layer.Id, layer.GeoJson, layer.Color);
    }

    public async Task RemoveLayerAsync(string layerId)
    {
        if (_module is null)
        {
            _pendingLayers.RemoveAll(layer => layer.Id == layerId);

            return;
        }

        await _module.InvokeVoidAsync("removeLayer", _elementId, layerId);
    }

    public async Task<bool> ShowPopupAsync(string layerId, string featureId, MapPopupModel popup)
    {
        if (_module is null)
        {
            return false;
        }

        return await _module.InvokeAsync<bool>("showPopup", _elementId, layerId, featureId, popup);
    }

    public async Task HidePopupAsync()
    {
        if (_module is null)
        {
            return;
        }

        await _module.InvokeVoidAsync("hidePopup", _elementId);
    }

    public async Task ClearSelectionAsync()
    {
        if (_module is null)
        {
            return;
        }

        await _module.InvokeVoidAsync("clearSelection", _elementId);
    }

    public async Task FitBoundsAsync()
    {
        if (_module is null)
        {
            return;
        }

        await _module.InvokeVoidAsync("fitBounds", _elementId);
    }

    public async Task ZoomInAsync() => await ZoomAsync(1);

    public async Task ZoomOutAsync() => await ZoomAsync(-1);

    public async Task InvalidateSizeAsync()
    {
        if (_module is null)
        {
            return;
        }

        await _module.InvokeVoidAsync("invalidateSize", _elementId);
    }

    [JSInvokable]
    public async Task HandleFeatureSelected(MapFeature feature)
    {
        FeatureSelected?.Invoke(feature);
        await OnFeatureSelected.InvokeAsync(feature);
    }

    [JSInvokable]
    public async Task HandlePopupAction(string layerId, string featureId, string action)
    {
        var payload = new MapPopupAction(layerId, featureId, action);
        PopupActioned?.Invoke(payload);
        await OnPopupAction.InvokeAsync(payload);
    }

    [JSInvokable]
    public void HandleCoordinateChanged(double latitude, double longitude)
        => CoordinateChanged?.Invoke(new MapCoordinate(latitude, longitude));

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("destroy", _elementId);
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }

        _reference?.Dispose();
        _pendingLayers.Clear();
    }

    private async Task ZoomAsync(int delta)
    {
        if (_module is null)
        {
            return;
        }

        await _module.InvokeVoidAsync("zoom", _elementId, delta);
    }
}