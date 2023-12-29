namespace IGE.TankShooter.Entry.Graphics;

using System;

using GameObjects;

using Microsoft.Xna.Framework;

using MonoGame.Extended;

/// <summary>
/// Helper class to control camera movements.
/// Smoothly zooms in and out as the tank increases/decreases speed.
/// Track the position of the tank.
/// Stop panning when the edge of the world is reached.
/// </summary>
public class CameraOperator
{
  private GraphicsDeviceManager graphicsDevice;
  private OrthographicCamera camera;
  private RectangleF boundingBox;
  private Tank tank;

  private float desiredZoom;
 
  private const float BUFFER_ZOOM = 0.1f;

  private const float MIN_ZOOM = 0.5f;
  private const float MAX_ZOOM = 1.1f;

  /// <summary>
  /// This isn't in any particular unit. Just make it higher to make the zoom faster, and experiment
  /// with values that seem "nice".
  /// </summary>
  private const float ZOOM_SPEED = 1f;

  public CameraOperator(Tank tank, OrthographicCamera camera, RectangleF boundingBox, GraphicsDeviceManager graphicsDevice)
  {
    this.tank = tank;
    this.camera = camera;
    this.boundingBox = boundingBox;
    this.graphicsDevice = graphicsDevice;
  }

  public void CutTo(Vector2 position)
  {
    var target = this.OffsetByScreenBounds(position);
    this.camera.Position = target;
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
  
  public void Update(GameTime gameTime)
  {
    this.camera.Position = this.OffsetByScreenBounds(this.tank.CurrentPosition);

    var speedFactor = (Tank.MAX_SPEED - this.tank.CurrentSpeed) / Tank.MAX_SPEED;
    this.desiredZoom = MathHelper.Lerp(MIN_ZOOM, MAX_ZOOM, speedFactor);
    var zoomDelta = (this.desiredZoom - this.camera.Zoom);
    if (Math.Abs(zoomDelta) > BUFFER_ZOOM)
    {
      this.camera.Zoom += zoomDelta * gameTime.GetElapsedSeconds() * ZOOM_SPEED;
    }

    ClampToBoundingBox();
  }

  private void ClampToBoundingBox()
  {
    var topLeftWorld = this.camera.ScreenToWorld(Vector2.Zero);
    var bottomRightScreen = new Vector2(this.graphicsDevice.GraphicsDevice.Viewport.Bounds.Right, this.graphicsDevice.GraphicsDevice.Viewport.Bounds.Bottom);
    var bottomRightWorld = this.camera.ScreenToWorld(bottomRightScreen);

    var adjustment = new Vector2(0f, 0f);
    if (topLeftWorld.X < 0)
    {
      adjustment.X = -topLeftWorld.X;
    }
    else if (bottomRightWorld.X > boundingBox.Right)
    {
      adjustment.X = -(bottomRightWorld.X - boundingBox.Right);
    }
    
    if (topLeftWorld.Y < 0)
    {
      adjustment.Y = -topLeftWorld.Y;
    }
    else if (bottomRightWorld.Y > boundingBox.Bottom)
    {
      adjustment.Y = -(bottomRightWorld.Y - boundingBox.Bottom);
    }

    this.camera.Move(adjustment);
  }
}
