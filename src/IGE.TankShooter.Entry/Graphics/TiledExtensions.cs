namespace IGE.TankShooter.Entry.Graphics;

using System;
using System.Linq;

using MonoGame.Extended.Tiled;

public static class TiledExtensions
{

  public static void ForEachTile(this TiledMap map, Action<TiledMapTile, TiledMapTilesetTile> callback)
  {
    foreach (var layer in map.TileLayers)
    {
      foreach (var tile in layer.Tiles)
      {
        var tilesetTile = tile.GetTilesetTile(map);
        if (tilesetTile != null)
        {
          callback(tile, tilesetTile);
        }
      }
    }
  }
  
  public static TiledMapTilesetTile GetTilesetTile(this TiledMapTile tile, TiledMap map)
  {
    if (!tile.IsBlank)
    {
      // Oh wow, this is all a little crazy.
      // Despite the weirdness regarding global + local identifiers, there is one other quirk which is that not
      // all tiles are included in the exported tileset XML file. Specifically, if there is no custom property
      // or collision set on the tile, then it is not included. Thus, the tileset will report that it has a large
      // number of tiles, even though the underlying tileset.Tiles array  here only has a subset of those tiles.
      // End result: We can't reliably index into the tileset.Tiles array, but rather need to loop over each of
      // them to find out what their index is.
      var tileset = map.GetTilesetByTileGlobalIdentifier(tile.GlobalIdentifier);
      var tilesetFirstIdentifier = map.GetTilesetFirstGlobalIdentifier(tileset);
      var tilesetTileLocalIdentifier = tile.GlobalIdentifier - tilesetFirstIdentifier;
      var tilesetTile = tileset.Tiles.FirstOrDefault(t => t.LocalTileIdentifier == tilesetTileLocalIdentifier);
      
      if (tilesetTile != null)
      {
        return tilesetTile;
      }
    }

    return null;
  }
  
}
