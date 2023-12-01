namespace IGE.TankShooter.Entry.GameObjects;

using System;

using Microsoft.Xna.Framework;

using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Tiled;

/// <summary>
/// Helper class to put barriers around the edge of the map for collision detection purposes.
/// Make it more than a few pixels wide to prevent items flying through the edge in a single frame.
/// </summary>
public class MapObject: ICollisionActor
{
  
  private readonly TiledMapTilesetTile tilesetTile;
  private readonly TiledMapTile tile;

  public MapObject(TiledMap map, TiledMapTile tile, TiledMapTilesetTile tilesetTile, Vector2 scale)
  {
    this.tile = tile;
    this.tilesetTile = tilesetTile;
    
    var collisionBounds = tilesetTile.Objects[0];
    this.Bounds = new RectangleF(
      (new Point2(tile.X * map.TileWidth, tile.Y * map.TileHeight) + collisionBounds.Position) * scale,
      collisionBounds.Size * scale
    );
  }
  
  public void OnCollision(CollisionEventArgs collisionInfo)
  {
    // Let the tank handle collisions with this rather than vice verca.
  }

  public IShapeF Bounds { get; }
}
