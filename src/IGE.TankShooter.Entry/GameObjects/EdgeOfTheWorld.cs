namespace IGE.TankShooter.Entry.GameObjects;

using System;

using MonoGame.Extended;
using MonoGame.Extended.Collisions;

/// <summary>
/// Helper class to put barriers around the edge of the map for collision detection purposes.
/// Make it more than a few pixels wide to prevent items flying through the edge in a single frame.
/// </summary>
public class EdgeOfTheWorld: ICollisionActor
{
  
  private Game1 tankGame;

  public const float BufferSize = 10f;

  public enum Side
  {
    Top,
    Bottom,
    Right,
    Left,
  }

  public EdgeOfTheWorld(Game1 tankGame, Side side, RectangleF worldBounds)
  {
    this.tankGame = tankGame;
    Bounds = side switch
    {
      Side.Top => new RectangleF(worldBounds.Left - BufferSize, worldBounds.Top - BufferSize, worldBounds.Width + BufferSize * 2, BufferSize),
      Side.Bottom => new RectangleF(worldBounds.Left - BufferSize, worldBounds.Bottom, worldBounds.Width + BufferSize * 2, BufferSize),
      Side.Left => new RectangleF(worldBounds.Left - BufferSize, worldBounds.Top - BufferSize, BufferSize, worldBounds.Height + BufferSize * 2),
      Side.Right => new RectangleF(worldBounds.Right, worldBounds.Top - BufferSize, BufferSize, worldBounds.Height + BufferSize * 2),
      _ => throw new Exception("Unsupported side: " + side)
    };
  }
  
  public void OnCollision(CollisionEventArgs collisionInfo)
  {
  }

  public IShapeF Bounds { get; }
}
