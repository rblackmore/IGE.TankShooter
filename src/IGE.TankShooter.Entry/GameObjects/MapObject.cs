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

  public MapObject(TiledMap map, TiledMapTile tile, TiledMapTilesetTile tilesetTile, Vector2 scale)
  {
    var collisionBounds = tilesetTile.Objects[0];
    this.Bounds = new RectangleF(
      (new Point2(tile.X * map.TileWidth, tile.Y * map.TileHeight) + collisionBounds.Position) * scale,
      collisionBounds.Size * scale
    );
  }

  public MapObject(RectangleF r1, RectangleF r2)
  {
    var x = Math.Min(r1.Position.X, r2.Position.X);
    var y = Math.Min(r1.Position.Y, r2.Position.Y);
    
    this.Bounds = new RectangleF(
      x,
      y, 
      Math.Max(r1.Right - x, r2.Right - x), 
      Math.Max(r1.Bottom - y, r2.Bottom - y)
    );
  }
  
  public void OnCollision(CollisionEventArgs collisionInfo)
  {
    // Let the tank handle collisions with this rather than vice verca.
  }

  public IShapeF Bounds { get; }

  public override string ToString() => this.Bounds.ToString();
}
