namespace GISORA.Web.E2ETests;

[Collection(WebAppGroup.Name)]
public class MapModuleTests(WebAppFixture fixture)
{
    private const string ModulePath = "/_content/GISORA.Shared.Maps/js/map.js";

    [Fact]
    public async Task RendersLayerSelectsFeatureAndDestroys()
    {
        var result = await EvaluateAsync<MapProbe>("""
            async () => {
              const module = await import('MODULE');
              const host = document.createElement('div');
              host.id = 'e2e-map';
              host.style.width = '400px';
              host.style.height = '300px';
              host.style.position = 'absolute';
              host.style.top = '0';
              host.style.left = '0';
              document.body.appendChild(host);

              let selected = null;
              const dotNetRef = {
                invokeMethodAsync: (name, payload) => {
                  if (name === 'HandleFeatureSelected') {
                    selected = payload;
                  }
                  return Promise.resolve(true);
                }
              };

              await module.create('e2e-map', {
                centerLatitude: 0,
                centerLongitude: 0,
                zoom: 2,
                tileUrl: null,
                attribution: ''
              }, dotNetRef);

              await module.setLayer('e2e-map', 'e2e', JSON.stringify({
                type: 'FeatureCollection',
                features: [{
                  type: 'Feature',
                  id: 'property-1',
                  properties: { registration: '001.002.003', area: 500 },
                  geometry: {
                    type: 'Polygon',
                    coordinates: [[[0, 0], [10, 0], [10, 10], [0, 10], [0, 0]]]
                  }
                }]
              }));

              const paths = document.querySelectorAll('#e2e-map path.leaflet-interactive').length;
              const fitted = await module.fitBounds('e2e-map');

              const path = document.querySelector('#e2e-map path.leaflet-interactive');
              if (path) {
                path.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
              }

              await new Promise((resolve) => setTimeout(resolve, 150));

              await module.destroy('e2e-map');
              const remaining = document.querySelectorAll('#e2e-map .leaflet-pane').length;
              host.remove();

              return {
                paths,
                fitted,
                selectedId: selected === null ? null : selected.id,
                selectedRegistration: selected === null ? null : selected.properties.registration,
                remaining
              };
            }
            """);

        Assert.Equal(1, result.Paths);
        Assert.True(result.Fitted);
        Assert.Equal("property-1", result.SelectedId);
        Assert.Equal("001.002.003", result.SelectedRegistration);
        Assert.Equal(0, result.Remaining);
    }

    [Fact]
    public async Task ZoomAndInvalidateWorkOnExistingMap()
    {
        var result = await EvaluateAsync<MapProbe>("""
            async () => {
              const module = await import('MODULE');
              const host = document.createElement('div');
              host.id = 'e2e-map-2';
              host.style.width = '300px';
              host.style.height = '200px';
              document.body.appendChild(host);

              await module.create('e2e-map-2', {
                centerLatitude: -7.95,
                centerLongitude: -35.25,
                zoom: 10,
                tileUrl: null,
                attribution: ''
              }, { invokeMethodAsync: () => Promise.resolve(true) });

              const zoomed = await module.zoom('e2e-map-2', 2);
              const invalidated = await module.invalidateSize('e2e-map-2');
              await module.destroy('e2e-map-2');
              host.remove();

              return { zoomed, invalidated };
            }
            """);

        Assert.True(result.Zoomed);
        Assert.True(result.Invalidated);
    }

    private async Task<T> EvaluateAsync<T>(string script)
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(fixture.BaseUrl);

        return await page.EvaluateAsync<T>(script.Replace("MODULE", ModulePath, StringComparison.Ordinal));
    }

    private sealed class MapProbe
    {
        public int Paths { get; set; }

        public bool Fitted { get; set; }

        public string? SelectedId { get; set; }

        public string? SelectedRegistration { get; set; }

        public int Remaining { get; set; }

        public bool Zoomed { get; set; }

        public bool Invalidated { get; set; }
    }
}