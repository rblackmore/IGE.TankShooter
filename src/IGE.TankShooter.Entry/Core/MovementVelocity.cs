namespace IGE.TankShooter.Entry.Core;

using Microsoft.Xna.Framework;

using MonoGame.Extended;

public class MovementVelocity
{
  private float velocity;

  public MovementVelocity(float direction, float velocity)
  {
    Direction = new Angle(direction);
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
  public Angle Direction { get; set; }

  /// <summary>
  /// Current Velocity.
  /// </summary>
  public float Velocity
  {
    get => this.velocity;
    set
    {
      this.velocity = value;
      this.velocity = (this.velocity >= this.MaxVelocity) ? this.MaxVelocity
        : (this.velocity <= this.MinVelocity) ? this.MinVelocity
        : this.velocity;
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
  public void Return(GameTime gameTime)
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

  public Vector2 GetNormalizedDirection()
  {
    if (this.Velocity == 0)
      return Vector2.Zero;

    return this.Direction.ToUnitVector();
  }

  public Vector2 GetScaler()
  {
    return GetNormalizedDirection() * this.Velocity;
  }
}
