namespace IGE.TankShooter.Entry.GameObjects;
using System;
using System.Diagnostics;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;
using MonoGame.Extended.Input;
using MonoGame.Extended.Sprites;

public class Tank : GameObject
{
  private Sprite BodySprite;
  private Sprite TurretSprite;

  private Transform2 BodyTransform;
  private Transform2 TurretTransform;

  private Game1 tankGame;

  private const float MAX_TANK_DIRECTION_CHANGE_RATE = MathF.PI / 3; // Radians per second.
  private const float MAX_TURRET_ROTATION_SPEED = 5f; // degrees per seconds
  public const float ACCELERATION = 5.0f; // Units per second.
  public const float MIN_SPEED = 0.0f; // Units per second.
  public const float MAX_SPEED = 10.0f; // Units per second.

  private MovementVelocity velocity;
  private readonly Point2 initialPosition;

  public Tank(Game1 tankGame, Point2 initialPosition)
  {
    this.tankGame = tankGame;
    this.initialPosition = initialPosition;
  }

  public Vector2 CurrentPosition => BodyTransform.Position;
  public float CurrentSpeed => this.velocity.GetScaler().Length();
  public float CurrentTurretAngle => this.TurretTransform.Rotation;

  public override void Initialize()
  {
    this.velocity = new MovementVelocity(Vector2.UnitX, 0f);
    this.velocity.MaxVelocity = 10.0f;
    this.velocity.MinVelocity = -10.0f;
    this.velocity.Acceleration = ACCELERATION;
    base.Initialize();
  }

  public override void LoadContent(ContentManager content)
  {
    var bodyTexture = content.Load<Texture2D>("tankBody_red_outline");
    var turretTexture = content.Load<Texture2D>("tankRed_barrel1_outline");

    // Calculate scale based on a desired width of 3m.
    // That same scale factor will be used for the height too but we don't specify a desired height, rather just take
    // the height of the texture and scale it using the same ratio we used to get to 3m width.
    var spriteScale = 3f / bodyTexture.Width;

    this.BodySprite = new Sprite(bodyTexture);
    this.BodyTransform = new Transform2(new Vector2(initialPosition.X, initialPosition.Y), 0.0f, new Vector2(spriteScale));
    this.BodyTransform.TranformUpdated += BodyTransform_TranformUpdated;

    this.TurretSprite = new Sprite(turretTexture);
    this.TurretTransform = new Transform2(new Vector2(initialPosition.X, initialPosition.Y), 0.0f, new Vector2(spriteScale));
    this.TurretTransform.Parent = this.BodyTransform;

  }

  private void BodyTransform_TranformUpdated()
  {
    this.TurretTransform.Position = this.BodyTransform.Position -= Vector2.UnitY * 10;
  }

  public override void Update(GameTime gameTime)
  {
    this.MoveTank(gameTime);
    RotateTurretTo(tankGame.Camera.ScreenToWorld(new Vector2(Mouse.GetState().X, Mouse.GetState().Y)), gameTime);
  }

  private void RotateTurretTo(Vector2 target, GameTime gameTime)
  {
    var currentAngle = this.TurretTransform.Rotation * 180 / (float)Math.PI;
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
    var targetAngle = (target - CurrentPosition).ToAngle() * 180 / (float)Math.PI + 180;
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

    this.TurretTransform.Rotation = (currentAngle + toRotate * MAX_TURRET_ROTATION_SPEED * gameTime.GetElapsedSeconds()) * ((float)Math.PI / 180);
  }

  private void RotateTankBodyTo(GameTime gameTime)
  {
    this.BodyTransform.Rotation = this.velocity.Direction.ToAngle();
  }

  private void UpdateTankDirection(GameTime gameTime)
  {
    var deltaTime = gameTime.GetElapsedSeconds();

    this.velocity.Direction =
      Vector2.Lerp(this.velocity.Direction, this.velocity.TargetDirection, deltaTime * MAX_TANK_DIRECTION_CHANGE_RATE);
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
      this.velocity.IncreaseVelocity(gameTime);
      this.velocity.TargetDirection = targetDirection; // TODO: Direction should slowly rotate toward target.
    }
    else
    {
      this.velocity.ReturnToZero(gameTime);
    }

    UpdateTankDirection(gameTime);
    RotateTankBodyTo(gameTime);
    var scaler = this.velocity.GetScaler();

    this.BodyTransform.Position += scaler * deltaTime;
    this.TurretTransform.Position += scaler * deltaTime;
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    spriteBatch.Draw(BodySprite, BodyTransform);
    spriteBatch.Draw(TurretSprite, TurretTransform);
  }
}
