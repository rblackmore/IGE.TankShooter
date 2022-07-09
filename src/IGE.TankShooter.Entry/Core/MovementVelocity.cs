namespace IGE.TankShooter.Entry.Core;

using Microsoft.Xna.Framework;
using MonoGame.Extended;

public class MovementVelocity
{
  public MovementVelocity(Vector2 direction, float velocity)
  {
    Direction = direction;
    Velocity = velocity;
  }

  public Vector2 Direction { get; set; }

  public float Velocity { get; set; }

  public Vector2 GetNormalizedDirection()
  {
    if (Direction == Vector2.Zero)
      return Vector2.Zero;

    return Direction.NormalizedCopy();
  }

  public Vector2 GetScaler()
  {
    return GetNormalizedDirection() * this.Velocity;
  }
}
