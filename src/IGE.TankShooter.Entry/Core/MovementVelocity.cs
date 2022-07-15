namespace IGE.TankShooter.Entry.Core;

using System;

using Microsoft.Xna.Framework;

using MonoGame.Extended;

public class MovementVelocity
{
  private float velocity;

  public MovementVelocity(Vector2 direction, float velocity)
  {
    Direction = direction;
    Velocity = velocity;
  }

  /// <summary>
  /// Maximum Velocity.
  /// </summary>
  public float MaxVelocity { get; set; }

  public float MinVelocity { get; set; }

  /// <summary>
  /// Acceleration per second.
  /// </summary>
  public float Acceleration { get; set; }

  /// <summary>
  /// Current Direction.
  /// </summary>
  public Vector2 Direction { get; set; }

  public Vector2 TargetDirection { get; set; }

  /// <summary>
  /// Current Velocity.
  /// </summary>
  public float Velocity
  {
    get => this.velocity;
    set
    {
      this.velocity = MathHelper.Clamp(value, this.MinVelocity, this.MaxVelocity);
    }
  }

  /// <summary>
  /// Increases the Velocity by the Acceleration per second.
  /// </summary>
  /// <param name="gameTime">Current Gametime.</param>
  public void IncreaseVelocity(GameTime gameTime)
  {
    var deltaTime = gameTime.GetElapsedSeconds();
    this.Velocity += this.Acceleration * deltaTime;
  }

  /// <summary>
  /// Decreased the Velocity by the Acceleration per second.
  /// </summary>
  /// <param name="gameTime">Current Gametime.</param>
  public void DecreaseVelocity(GameTime gameTime)
  {
    var deltaTime = gameTime.GetElapsedSeconds();
    this.Velocity -= this.Acceleration * deltaTime;
  }

  /// <summary>
  /// Returns the <see cref="Velocity"/> back to 0.
  /// </summary>
  /// <param name="gameTime">Current Gametime.</param>
  public void ReturnToZero(GameTime gameTime)
  {

    if (this.Velocity == 0)
    {
      return;
    }

    if (this.Velocity < 0)
    {
      this.IncreaseVelocity(gameTime);
    }

    if (this.Velocity > 0)
    {
      this.DecreaseVelocity(gameTime);
    }
  }

  /// <summary>
  /// Force clear the movement velocity associated with this without any animation.
  /// </summary>
  public void Clear()
  {
    this.Velocity = 0f;
  }

  public Vector2 GetNormalizedDirection()
  {
    if (this.Velocity == 0)
      return Vector2.Zero;

    return this.Direction.NormalizedCopy();

  }

  public Vector2 GetScaler()
  {
    return GetNormalizedDirection() * this.Velocity;
  }
}
