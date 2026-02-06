# CorsacCosmetics

CorsacCosmetics is a client-side BepInEx mod for Among Us that adds support for user-provided cosmetic images. This fork updates the original CorsacHats to support newer Among Us versions and expands the available cosmetic types to include hats, visors, and nameplates.

### Supported cosmetic types and locations
- Hats: CorsacCosmetics/Hats
- Visors: CorsacCosmetics/Visors
- Nameplates: CorsacCosmetics/Nameplates

### Making your own cosmetics
You can see the example cosmetics included in the repository for reference. Some tips for creating your own:
- Dimensions matter! Use the example images as a guide for sizing your cosmetics appropriately.
- Transparent backgrounds are recommended for hats and visors.
- Resolution doesn't matter on nameplates because they are scaled, but keep them small for performance.

Notes:
- Right now, only PNG format is supported. This will be expanded in future updates.
- Only static images are supported; animated cosmetics are not currently implemented.
- The mod doesn't support setting specific cosmetic sprites (eg. climbing sprite) yet.
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

### Visibility notes
- Any custom cosmetic will be visible to you and to other players who have the same mod and the same cosmetic files installed.
- Players who do not have the mod (or the same files) will see a pseudo-random cosmetic chosen by the original game.

### Installation

1. Make sure you have BepInEx and Reactor installed for Among Us. This plugin depends on Reactor.
2. Copy the mod DLL (CorsacCosmetics.dll) into `BepInEx\plugins`.
3. Start Among Us. The mod will create the `CorsacCosmetics` folder and subfolders for `Hats`, `Visors`, and `Nameplates` if they don't already exist.
4. Place your custom PNG files into the appropriate subfolder (Hats, Visors, Nameplates).
5. (Optional) Create corresponding JSON metadata files for each cosmetic as described above.
6. Restart Among Us to load the new cosmetics.
7. Enjoy your custom cosmetics in-game!

### Notable changes in this fork
- Updated to support more recent Among Us versions.
- Added Visors and Nameplates as cosmetic types.

> This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.

### Attributions and license
- This mod is not affiliated with Among Us or Innersloth LLC.
- Original CorsacHats project and contributors for inspiration.
- See the LICENSE file in this repository for license information.
