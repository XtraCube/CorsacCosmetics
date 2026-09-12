# CorsacCosmetics

CorsacCosmetics is a client-side BepInEx mod for Among Us that adds support for user-provided cosmetic images. This fork updates the original CorsacHats to support newer Among Us versions and expands the available cosmetic types to include hats, visors, and nameplates.

### Supported cosmetic types and locations
- Hats: `CorsacCosmetics/Hats`
- Visors: `CorsacCosmetics/Visors`
- Nameplates: `CorsacCosmetics/Nameplates`
- Bundles: `CorsacCosmetics/Bundles`

### Cosmetic bundles

Cosmetic bundles (`.ccb`) allow you to package multiple cosmetics together and set specific cosmetic sprites (eg. preview, climb). Bundles should be placed in the `CorsacCosmetics/Bundles` folder.

You can create cosmetic bundles with the [online editor](https://allofus.dev/cosmetics/).


### Making your own cosmetics
You can use the `Crewmate_Base.png` file in the Templates folder as a base layer for cosmetics.
Player cosmetics should be 270x428 pixels in size. Nameplates can be any size as they are scaled,
but try to stick to the same aspect ratio as the original nameplate (around 3:1) for best results.

Notes:
- Right now, only PNG format is supported. This will be expanded in future updates.
- Only static images are supported; animated cosmetics are not currently implemented.
- There aren't any templates besides the examples yet.

### File format and naming
- The loaders currently look for PNG files only ("*.png").
- Use a unique filename for each cosmetic, for example: my_cool_hat.png
- Each cosmetic type has an associated metadata you can set by making a JSON file with the same name as the PNG file. For example, for `my_cool_hat.png`, create a `my_cool_hat.json` file in the same folder with the following structure:

```json
{
   "Name": "MyHat",
   "MatchPlayerColor": true,
   "BlocksVisors": false,
   "InFront": true,
   "NoBounce": false
}
```

### For Developers

CorsacCosmetics provides a `PluginCompat` static class that allows other mods to add cosmetics programmatically. This is designed to work with soft-dependencies, so your mod doesn't need to reference CorsacCosmetics directly.

#### Downloading resources at runtime

If you are downloading resources at runtime, you should do it before Corsac discovers and installs cosmetics. Use `QueueDiscoveryTask` to register an async task that Corsac will wait for before proceeding:

```csharp
async Task DownloadCosmetics()
{
    using var httpClient = new HttpClient();
    var bytes = await httpClient.GetByteArrayAsync("https://example.com/my_cosmetics.ccb");
    var outputPath = Path.Combine(CosmeticPaths.BundlePath, "my_cosmetics.ccb");
    await File.WriteAllBytesAsync(outputPath, bytes);
}

// Queue the task before Corsac runs discovery (for example, in Plugin Load)
PluginCompat.QueueDiscoveryTask(DownloadCosmetics());
```

This ensures your resources are fully downloaded and saved before Corsac attempts to load cosmetics. Multiple tasks can be queued and they will be executed in order.

##### Downloading bundles

For convenience, you can use `QueueBundleDownload` to download a `.ccb` bundle from a URL. The bundle will be automatically saved to the `CorsacCosmetics/Bundles` folder with the filename from the URL:

```csharp
// Download to default Bundles folder
PluginCompat.QueueBundleDownload("https://example.com/my_bundle.ccb");

// Or specify a custom output folder
PluginCompat.QueueBundleDownload("https://example.com/my_bundle.ccb", @"path\to\custom\folder");
```

##### Adding bundles from bytes

If you already have bundle data in memory (e.g., from an embedded resource or network stream), you can use `AddBundleBytes` to register it directly:

```csharp
byte[] bundleData = GetBundleBytesFromSomewhere();
PluginCompat.AddBundleBytes(bundleData);
```

#### Adding a folder source

You can register a folder for CorsacCosmetics to load cosmetics from. The directory structure must match Corsac's default structure (with `Hats`, `Visors`, `Nameplates` subfolders). The `Bundles` subfolder is not included in the folder source:

```csharp
PluginCompat.AddFolderSource(@"path\to\your\cosmetics\folder", "your_group_name");
```

The `groupName` parameter is optional and defaults to `"default"`. This can be used to organize cosmetics from different sources.

#### Adding a bundle source

You can also load cosmetics from a `.ccb` bundle file:

```csharp
PluginCompat.AddBundleSource(@"path\to\your\bundle.ccb");
```

#### Custom sources and asset readers

For deeper integration, you can implement the `ICosmeticSource` and `ICosmeticAssetReader` interfaces to create fully custom cosmetic loading pipelines.

##### ICosmeticSource

Implement this interface to create a custom source for discovering cosmetics:

```csharp
public interface ICosmeticSource
{
    string SourceId { get; }
    Task<IEnumerable<CosmeticDescriptor>> DiscoverAsync();
}
```

- `SourceId`: A unique identifier for your source
- `DiscoverAsync()`: Returns a collection of `CosmeticDescriptor` objects representing the cosmetics your source provides

When creating `CosmeticDescriptor`, try to pass a unique value into the `sourceId` parameter to ensure proper namespacing.
For example, the built-in `LocalBundleSource` passes a hash of the bundle manifest, guaranteeing that different bundles will
have different group IDs and cosmetic IDs.

##### ICosmeticAssetReader

Implement this interface to handle loading sprite assets for your cosmetics:

```csharp
public interface ICosmeticAssetReader
{
    Task<Sprite?> LoadSpriteAsync(string spriteKey);
}
```

- `LoadSpriteAsync(string spriteKey)`: Loads and returns a `Sprite` for the given key. The spriteKey can be used to request different sprite variants (e.g., "preview", "climb").

It is highly recommended not to keep a cache of sprites for your `ICosmeticAssetReader`. The purpose of this system is to create
a sprite when it is needed, and to unload the sprite when it is no longer being used. If you keep a sprite cache, you are 
defeating the purpose of the system.

##### CosmeticDescriptor

When implementing `ICosmeticSource.DiscoverAsync()`, you'll need to create `CosmeticDescriptor` instances:

```csharp
var descriptor = new CosmeticDescriptor(
    sourceId: "MyCustomSource",
    group: "my_group",
    name: "My Custom Hat",
    type: CosmeticType.Hat,
    metadata: new HatMetadata { Name = "MyHat", MatchPlayerColor = true },
    reader: new MyCustomAssetReader()
);
```

The `CosmeticDescriptor` class has the following properties:
- `Id`: Auto-generated unique identifier
- `DisplayName`: The display name of the cosmetic
- `Group`: The group this cosmetic belongs to
- `Type`: The cosmetic type (`Hat`, `Visor`, or `NamePlate`)
- `SourceId`: The source that provided this cosmetic
- `Metadata`: The cosmetic metadata (implements `ICosmeticMetadata`)
- `AssetReader`: The reader used to load sprites (implements `ICosmeticAssetReader`)

##### Registering a custom source

Register your custom source with the `SourceRegistry`:

```csharp
var mySource = new MyCustomSource();
SourceRegistry.Instance.RegisterSource(mySource);
```

### Visibility notes
- Any custom cosmetic will be visible to you and to other players who have the same mod and the same cosmetic files installed.
- Players who do not have the mod (or have different files) will likely see nothing.

### Installation

1. Make sure you have BepInEx and Reactor installed for Among Us. This plugin depends on Reactor.
2. Copy the mod DLL (CorsacCosmetics.dll) into `BepInEx\plugins`.
3. Start Among Us. The mod will create the `CorsacCosmetics` folder and subfolders for `Bundles`, Hats`, `Visors`, and `Nameplates` if they don't already exist.
4. Place your `.ccb` bundles and custom PNG files into the appropriate subfolder (Bundles, Hats, Visors, Nameplates).
5. (Optional) Create corresponding JSON metadata files for each PNG cosmetic as described above.
6. Restart Among Us to load the new cosmetics.
7. Enjoy your custom cosmetics in-game!

### Notable changes in this fork
- Updated to support more recent Among Us versions.
- Added Visors and Nameplates as cosmetic types.
- Added custom bundle format (`.ccb`)

> This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.

### Attributions and license
- This mod is not affiliated with Among Us or Innersloth LLC.
- Original CorsacHats project and contributors for inspiration.
- See the LICENSE file in this repository for license information.
