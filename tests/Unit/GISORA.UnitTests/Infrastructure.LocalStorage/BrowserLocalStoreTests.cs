using GISORA.Infrastructure.Persistence;

using GISORA.UnitTests.Infrastructure.LocalStorage.Fakes;

using Microsoft.JSInterop;

namespace GISORA.UnitTests.Infrastructure.LocalStorage;

public class BrowserLocalStoreTests
{
    private enum SampleKind
    {
        First,
        Second,
    }

    private sealed record Sample(string Name, SampleKind Kind, int Count);

    private static BrowserLocalStore CreateStore(FakeJsStorage storage) => new(storage);

    [Fact]
    public async Task MissingKeyReturnsDefault()
    {
        var store = CreateStore(new FakeJsStorage());

        var value = await store.GetAsync<Sample>("properties", "missing");

        Assert.Null(value);
    }

    [Fact]
    public async Task RoundTripsValueWithEnums()
    {
        var store = CreateStore(new FakeJsStorage());
        var sample = new Sample("Passira", SampleKind.Second, 7);

        await store.SetAsync("properties", "sample", sample);
        var loaded = await store.GetAsync<Sample>("properties", "sample");

        Assert.Equal(sample, loaded);
    }

    [Fact]
    public async Task ExistsReflectsStoredValue()
    {
        var store = CreateStore(new FakeJsStorage());

        await store.SetAsync("properties", "sample", new Sample("A", SampleKind.First, 1));

        Assert.True(await store.ExistsAsync("properties", "sample"));
        Assert.False(await store.ExistsAsync("properties", "other"));
    }

    [Fact]
    public async Task RemoveDeletesValue()
    {
        var store = CreateStore(new FakeJsStorage());
        await store.SetAsync("properties", "sample", new Sample("A", SampleKind.First, 1));

        await store.RemoveAsync("properties", "sample");

        Assert.False(await store.ExistsAsync("properties", "sample"));
    }

    [Fact]
    public async Task ClearNamespaceDoesNotAffectOtherNamespaces()
    {
        var store = CreateStore(new FakeJsStorage());
        await store.SetAsync("properties", "sample", new Sample("A", SampleKind.First, 1));
        await store.SetAsync("territory", "sample", new Sample("B", SampleKind.First, 1));

        await store.ClearNamespaceAsync("properties");

        Assert.False(await store.ExistsAsync("properties", "sample"));
        Assert.True(await store.ExistsAsync("territory", "sample"));
    }

    [Fact]
    public async Task QuotaFailureIsTranslated()
    {
        var js = new FakeJsStorage { ThrowOnSet = new JSException("QuotaExceededError: limit reached") };
        var store = CreateStore(js);

        await Assert.ThrowsAsync<StorageQuotaExceededException>(() =>
            store.SetAsync("properties", "sample", new Sample("A", SampleKind.First, 1)));
    }

    [Fact]
    public async Task GenericJsFailureIsTranslated()
    {
        var js = new FakeJsStorage { ThrowOnSet = new JSException("unexpected") };
        var store = CreateStore(js);

        var exception = await Assert.ThrowsAsync<StorageException>(() =>
            store.SetAsync("properties", "sample", new Sample("A", SampleKind.First, 1)));

        Assert.IsNotType<StorageQuotaExceededException>(exception);
    }

    [Fact]
    public async Task UnavailableStorageReturnsFalseInsteadOfThrowing()
    {
        var js = new FakeJsStorage { ThrowOnAvailability = new JSException("SecurityError") };
        var store = CreateStore(js);

        Assert.False(await store.IsAvailableAsync());
    }

    [Fact]
    public async Task InvalidJsonThrowsStorageException()
    {
        var js = new FakeJsStorage();
        await js.SetItemAsync("gisora:properties:broken", "not-json", CancellationToken.None);
        var store = CreateStore(js);

        await Assert.ThrowsAsync<StorageException>(() => store.GetAsync<Sample>("properties", "broken"));
    }
}