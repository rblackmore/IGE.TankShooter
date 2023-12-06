namespace IGE.TankShooter.Entry.Graphics;

using System;

using Core;

using GameObjects;

using Microsoft.Xna.Framework;

using MonoGame.Extended;

/// <summary>
/// Helper class to control camera movements.
/// Smoothly zooms in and out as the tank increases/decreases speed.
/// Track the position of the tank by smoothly panning to its location.
/// </summary>
public class CameraOperator
{
  private OrthographicCamera camera;
  private Tank tank;

  private MovementVelocity movement;

  private Vector2 desiredPosition;
  private float desiredZoom;
 
  /// <summary>
  /// Near enough is good enough, if we are within this many metres then don't try to get closer to the target
  /// any more. This helps smooth out the movement of the camera for little tank movements.
  /// </summary>
  private const float BUFFER_DISTANCE = 3f;
  private const float BUFFER_ZOOM = 0.1f;

  private const float MIN_ZOOM = 0.5f;
  private const float MAX_ZOOM = 1.1f;

  public CameraOperator(Tank tank, OrthographicCamera camera)
  {
    this.tank = tank;
    this.camera = camera;
    this.movement = new MovementVelocity(Vector2.Zero, 0f);
    this.movement.MaxVelocity = Tank.MAX_SPEED;
    this.movement.MinVelocity = Tank.MIN_SPEED;
    this.movement.Acceleration = Tank.ACCELERATION / 8;
    this.movement.Deceleration = Tank.DECELERATION / 8;
  }

  public void CutTo(Vector2 position)
  {
    this.movement.Clear();
    var target = this.OffsetByScreenBounds(position);
    this.camera.Position = target;
    this.desiredPosition = target;
  }

  private Vector2 OffsetByScreenBounds(Vector2 position)
  {
    // TODO: Find out how to do this via the Camera.Origin so we only need to set it up once, tried but with no success.
    var viewportBounds = this.camera.BoundingRectangle;
    return new Vector2(
      position.X - viewportBounds.Width * this.camera.Zoom / 2f,
      position.Y - viewportBounds.Height * this.camera.Zoom / 2f
    );
  }

  public void MoveTo(Vector2 position)
  {
    this.desiredPosition = this.OffsetByScreenBounds(position); // TODO: Let the player pan.
  }
  
  public void Update(GameTime gameTime)
  {
    this.MoveTo(this.tank.CurrentPosition);
    var positionDelta = (this.desiredPosition - this.camera.Position);
    var distance = positionDelta.Length();
    if (distance > BUFFER_DISTANCE)
    {
      this.movement.Direction = (this.desiredPosition - this.camera.Position);
      this.movement.IncreaseVelocity(gameTime);
      this.camera.Position += this.movement.GetScaler() * gameTime.GetElapsedSeconds();
    }

    var speedFactor = (Tank.MAX_SPEED - this.tank.CurrentSpeed) / Tank.MAX_SPEED;
    this.desiredZoom = MathHelper.Lerp(MIN_ZOOM, MAX_ZOOM, speedFactor);
    var zoomDelta = (this.desiredZoom - this.camera.Zoom);
    if (Math.Abs(zoomDelta) > BUFFER_ZOOM)
    {
      this.camera.Zoom += zoomDelta * gameTime.GetElapsedSeconds();
    }
  }
}
