namespace IGE.TankShooter.Entry.Graphics;

using System;

using Core;

using GameObjects;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;

/// <summary>
/// Helper class to control camera movements.
/// Smoothly zooms in and out as the tank increases/decreases speed.
/// Track the position of the tank by smoothly panning to its location.
/// Stop panning when the edge of the world is reached.
/// </summary>
public class CameraOperator
{
  private GraphicsDeviceManager graphicsDevice;
  private OrthographicCamera camera;
  private RectangleF boundingBox;
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

  public CameraOperator(Tank tank, OrthographicCamera camera, RectangleF boundingBox, GraphicsDeviceManager graphicsDevice)
  {
    this.tank = tank;
    this.camera = camera;
    this.boundingBox = boundingBox;
    this.graphicsDevice = graphicsDevice;
    this.movement = new MovementVelocity(Vector2.Zero, 0f)
    {
      MaxVelocity = Tank.MAX_SPEED,
      MinVelocity = Tank.MIN_SPEED,
      Acceleration = Tank.ACCELERATION / 8,
      Deceleration = Tank.DECELERATION / 8
    };
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

    ClampToBoundingBox();
  }

  private void ClampToBoundingBox()
  {
    var topLeftWorld = this.camera.ScreenToWorld(Vector2.Zero);
    var bottomRightScreen = new Vector2(this.graphicsDevice.GraphicsDevice.Viewport.Bounds.Right, this.graphicsDevice.GraphicsDevice.Viewport.Bounds.Bottom);
    var bottomRightWorld = this.camera.ScreenToWorld(bottomRightScreen);

    var adjustment = new Vector2(0f, 0f);
    var needsAdjustment = false;
    if (topLeftWorld.X < 0)
    {
      adjustment.X = -topLeftWorld.X;
      Console.Write($"Left of world. Camera: {topLeftWorld.X}. ");
      needsAdjustment = true;
    }
    else if (bottomRightWorld.X > boundingBox.Right)
    {
      adjustment.X = -(bottomRightWorld.X - boundingBox.Right);
      Console.Write($"Right of world. World right: {boundingBox.Right}. Camera: {bottomRightWorld.X}. ");
      needsAdjustment = true;
    }
    
    if (topLeftWorld.Y < 0)
    {
      adjustment.Y = -topLeftWorld.Y;
      Console.Write($"Above world. Camera: {topLeftWorld.Y}. ");
      needsAdjustment = true;
    }
    else if (bottomRightWorld.Y > boundingBox.Bottom)
    {
      adjustment.Y = -(bottomRightWorld.Y - boundingBox.Bottom);
      Console.Write($"Below world. World bottom: {boundingBox.Bottom}. Camera: {bottomRightWorld.Y}. ");
      needsAdjustment = true;
    }

    if (needsAdjustment)
    {
      Console.WriteLine($"Adjusting: {adjustment}");
      this.camera.Move(adjustment);
    }
  }
}
