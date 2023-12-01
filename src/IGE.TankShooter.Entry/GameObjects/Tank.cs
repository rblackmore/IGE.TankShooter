namespace IGE.TankShooter.Entry.GameObjects;
using System;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Input;
using MonoGame.Extended.Sprites;

public class Tank : GameObject, ICollisionActor
{
  private Game1 tankGame;
  private Turret Turret;
  
  private Sprite Sprite;
  private Transform2 Transform;

  private const float MAX_DIRECTION_CHANGE_RATE = MathF.PI; // Radians per second.
  public const float ACCELERATION = 10.0f; // Units per second.
  public const float DECELERATION = 20.0f; // Units per second.
  public const float MIN_SPEED = 0.0f; // Units per second.
  public const float MAX_SPEED = 10.0f; // Units per second.

  private MovementVelocity velocity;
  private readonly Point2 initialPosition;
  private CircleF bounds;

  public Tank(Game1 tankGame, Point2 initialPosition)
  {
    this.tankGame = tankGame;
    this.initialPosition = initialPosition;
    this.Turret = new Turret(tankGame, this, new Vector2(0f, 0.75f));
    bounds = new CircleF(initialPosition, 0.5f);
  }

  public Vector2 CurrentPosition => Transform.Position;
  public float CurrentRotation => Transform.Rotation;
  public float CurrentSpeed => this.velocity.GetScaler().Length();
  public float CurrentTurretAngle => this.Turret.CurrentAngle;
  public Vector2 CurrentBarrelTipPosition => this.Turret.CurrentBarrelTipPosition;

  public override void Initialize()
  {
    this.velocity = new MovementVelocity(Vector2.UnitX, 0f);
    this.velocity.MaxVelocity = 10.0f;
    this.velocity.MinVelocity = -10.0f;
    this.velocity.Acceleration = ACCELERATION;
    this.velocity.Deceleration = DECELERATION;
    base.Initialize();
  }

  public override void LoadContent(ContentManager content)
  {
    var texture = content.Load<Texture2D>("tankBody_red_outline");

    // Calculate scale based on a desired width of 3m.
    // That same scale factor will be used for the height too but we don't specify a desired height, rather just take
    // the height of the texture and scale it using the same ratio we used to get to 3m width.
    var spriteScale = 3f / texture.Width;

    this.Sprite = new Sprite(texture);
    this.Transform = new Transform2(new Vector2(initialPosition.X, initialPosition.Y), 0.0f, new Vector2(spriteScale));
    this.bounds.Radius = 1.5f;
   
    this.Turret.LoadContent(content, this.Transform.Position, this.Transform.Rotation, spriteScale);
  }

  public override void Update(GameTime gameTime)
  {
    this.MoveTank(gameTime);
    this.Turret.Update(gameTime);
    this.bounds.Position = this.Transform.Position;
  }

  private void RotateTankBodyTo(GameTime gameTime)
  {
    this.Transform.Rotation = this.velocity.Direction.ToAngle();
  }

  private void UpdateTankDirection(GameTime gameTime)
  {
    var deltaTime = gameTime.GetElapsedSeconds();

    // Make sure to normalize this desired direction, otherwise if we interpolate between
    // "almost up but a bit to the right" and "directly down", then while the vertical component
    // of the vector will smoothly interpolate from up to down, the horizontal component
    // is only minuscule, meaning the tank will instantly flip from one direction to the other
    // during the interpolation. This normalisation results in the tank having to travel via a wide arc.
    if (this.velocity.Velocity > 1f) {
      this.velocity.Direction =
        Vector2.Lerp(this.velocity.Direction, this.velocity.TargetDirection, deltaTime * MAX_DIRECTION_CHANGE_RATE).NormalizedCopy();
    }
  }

  private void MoveTank(GameTime gameTime)
  {
    var deltaTime = gameTime.GetElapsedSeconds();

    var kbState = KeyboardExtended.GetState();

    var targetDirection = Vector2.Zero;

    if (kbState.IsKeyDown(Keys.A))
      targetDirection -= Vector2.UnitX;
    if (kbState.IsKeyDown(Keys.D))
      targetDirection += Vector2.UnitX;
    if (kbState.IsKeyDown(Keys.W))
      targetDirection -= Vector2.UnitY;
    if (kbState.IsKeyDown(Keys.S))
      targetDirection += Vector2.UnitY;

    if (targetDirection != Vector2.Zero)
    {
      // While I don't believe this normalisation is strictly necessary, it makes the debug
      // drawing look better, because the line pointing "up and right" is now the same length
      // as the line pointing "right" which feels better.
      targetDirection.Normalize();
      
      this.velocity.IncreaseVelocity(gameTime);
      this.velocity.TargetDirection = targetDirection;
    }
    else
    {
      this.velocity.ReturnToZero(gameTime);
    }

    UpdateTankDirection(gameTime);
    RotateTankBodyTo(gameTime);
    var scaler = this.velocity.GetScaler();

    this.Transform.Position += scaler * deltaTime;
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    spriteBatch.Draw(Sprite, Transform);

    if (Debug.DrawDebugLines)
    {
      spriteBatch.DrawCircle(this.bounds, 15, Color.Cyan, 0.1f);
      spriteBatch.DrawLine(CurrentPosition, CurrentPosition + velocity.TargetDirection * 10, Color.Cyan, 0.2f);
      spriteBatch.DrawLine(CurrentPosition, CurrentPosition + (velocity.Direction).NormalizedCopy() * 10, Color.LightCyan, 0.2f);
    }
    
    this.Turret.Draw(gameTime, spriteBatch);
  }

  public Bullet FireBullet()
  {
    return this.Turret.FireBullet();
  }

  public void OnCollision(CollisionEventArgs collisionInfo)
  {
    if (collisionInfo.Other is Enemy enemy)
    {
      this.tankGame.OnPlayerHit(enemy);
    }
  }

  public IShapeF Bounds { get { return bounds; } }
}

class Turret : GameObject
{
  
  private const float MAX_ROTATION_SPEED = 5f; // degrees per seconds
 
  private Game1 tankGame;
  private Tank tank;
  private Sprite Sprite;
  private Sprite BulletSprite;
  private Transform2 Transform;
  private Vector2 OffsetFromTankCentre;

  /// <summary>
  /// 
  /// </summary>
  /// <param name="tankGame"></param>
  /// <param name="tank"></param>
  /// <param name="offsetFromTankCentre">
  /// The tank body will have a point that the turret is mounted. This will depend on the tank body and therefore
  /// we request that the tank pass this in as we have no real knowledge of where on the tank we should sit. When
  /// updating our position as the tank moves, make sure to sit ourselves this far off the centre of the tank, taking
  /// into account the rotation of the tank body).
  /// </param>
  public Turret(Game1 tankGame, Tank tank, Vector2 offsetFromTankCentre)
  {
    this.tankGame = tankGame;
    this.tank = tank;
    this.OffsetFromTankCentre = offsetFromTankCentre;
  }

  public float CurrentAngle => Transform.Rotation;

  /// <summary>
  /// Request the tankPosition and tankRotation be passed in rather than querying this.Tank, because it ensures that
  /// the calling class must have this information prior to invoking this method. If we query this.tank from within
  /// this method, we don't really know whether it has been initialised or not yet.
  /// </summary>
  public void LoadContent(ContentManager content, Vector2 tankPosition, float tankRotation, float spriteScale)
  {
    var texture = content.Load<Texture2D>("tankRed_barrel1_outline");

    this.Sprite = new Sprite(texture);
    this.Sprite.Origin = new Vector2(texture.Width / 2f, texture.Height / 4f);
    this.Transform = new Transform2(CalculatePosition(tankPosition, tankRotation), 0.0f, new Vector2(spriteScale, spriteScale));
    
    var bulletTexture = content.Load<Texture2D>("bulletSand3_outline");
    this.BulletSprite = new Sprite(bulletTexture);
    this.BulletSprite.Origin = new Vector2(bulletTexture.Width / 2f, bulletTexture.Height / 2f);
  }

  /// <summary>
  /// Take into account the tank position, the rotation of the tank, and our own understanding of how far offset from
  /// the centre of the tank we should be.
  /// </summary>
  /// <returns></returns>
  private Vector2 CalculatePosition(Vector2 tankPosition, float rotation)
  {
    return tankPosition + OffsetFromTankCentre.Rotate(rotation);
  }

  /// <summary>
  /// When bullets leave the cannon, they should exit from this position (the tip of the barrel).
  /// </summary>
  public Vector2 CurrentBarrelTipPosition => this.Transform.Position +
      new Vector2(0f, (this.Sprite.TextureRegion.Texture.Height - this.Sprite.Origin.Y) * this.Transform.Scale.Y).Rotate(this.Transform.Rotation);

  public override void Update(GameTime gameTime)
  {
    var target = tankGame.Camera.ScreenToWorld(new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
    this.RotateTurretTo(target, gameTime);
    this.Transform.Position = CalculatePosition(this.tank.CurrentPosition, this.tank.CurrentRotation);
  }

  private void RotateTurretTo(Vector2 target, GameTime gameTime)
  {
    var currentAngle = this.Transform.Rotation * 180 / (float)Math.PI;
    while (currentAngle < 0)
    {
      currentAngle += 360;
    }
    while (currentAngle > 360)
    {
      currentAngle -= 360;
    }

    // Not sure what math is doing such that we need to rotate a further 180 degrees, but possibly something like
    // the world coordinate system being upside down compared to what the intuitive way of calculating angles would
    // expect?
    var targetAngle = (target - this.tank.CurrentPosition).ToAngle() * 180 / (float)Math.PI + 180;
    while (targetAngle < 0)
    {
      targetAngle += 360;
    }
    while (targetAngle > 360)
    {
      targetAngle -= 360;
    }

    float toRotate = targetAngle - currentAngle;
    if (toRotate > 180)
    {
      toRotate = -(360 - toRotate);
    }

    if (toRotate < -180)
    {
      toRotate = 360 + toRotate;
    }

    this.Transform.Rotation = (currentAngle + toRotate * MAX_ROTATION_SPEED * gameTime.GetElapsedSeconds()) * ((float)Math.PI / 180);
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    spriteBatch.Draw(Sprite, Transform);

    if (Debug.DrawDebugLines)
    {
      spriteBatch.DrawCircle(this.CurrentBarrelTipPosition, 0.5f, 8, Color.Cyan, 0.1f);
    }
  }

  public Bullet FireBullet()
  {
    return new Bullet(
      this.tankGame,
      this.BulletSprite,
      this.Transform.Scale, 
      Vector2.UnitY.Rotate(this.Transform.Rotation),
      this.CurrentBarrelTipPosition
    );
  }
}
