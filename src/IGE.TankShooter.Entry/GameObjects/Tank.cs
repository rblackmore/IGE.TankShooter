﻿namespace IGE.TankShooter.Entry.GameObjects;
using System;

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
  private Turret Turret;
  
  private Sprite Sprite;
  private Transform2 Transform;

  private const float MAX_DIRECTION_CHANGE_RATE = MathF.PI / 3; // Radians per second.
  public const float ACCELERATION = 5.0f; // Units per second.
  public const float MIN_SPEED = 0.0f; // Units per second.
  public const float MAX_SPEED = 10.0f; // Units per second.

  private MovementVelocity velocity;
  private readonly Point2 initialPosition;

  public Tank(Game1 tankGame, Point2 initialPosition)
  {
    this.initialPosition = initialPosition;
    this.Turret = new Turret(tankGame, this);
  }

  public Vector2 CurrentPosition => Transform.Position;
  public float CurrentSpeed => this.velocity.GetScaler().Length();
  public float CurrentTurretAngle => this.Turret.CurrentAngle;

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
    var texture = content.Load<Texture2D>("tankBody_red_outline");

    // Calculate scale based on a desired width of 3m.
    // That same scale factor will be used for the height too but we don't specify a desired height, rather just take
    // the height of the texture and scale it using the same ratio we used to get to 3m width.
    var spriteScale = 3f / texture.Width;

    this.Sprite = new Sprite(texture);
    this.Transform = new Transform2(new Vector2(initialPosition.X, initialPosition.Y), 0.0f, new Vector2(spriteScale));
   
    this.Turret.LoadContent(content, initialPosition, spriteScale);
  }

  public override void Update(GameTime gameTime)
  {
    this.MoveTank(gameTime);
    this.Turret.Update(gameTime);
  }

  private void RotateTankBodyTo(GameTime gameTime)
  {
    this.Transform.Rotation = this.velocity.Direction.ToAngle();
  }

  private void UpdateTankDirection(GameTime gameTime)
  {
    var deltaTime = gameTime.GetElapsedSeconds();

    this.velocity.Direction =
      Vector2.Lerp(this.velocity.Direction, this.velocity.TargetDirection, deltaTime * MAX_DIRECTION_CHANGE_RATE);
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

    this.Transform.Position += scaler * deltaTime;
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    spriteBatch.Draw(Sprite, Transform);
    this.Turret.Draw(gameTime, spriteBatch);
  }
}

class Turret : GameObject
{
  
  private const float MAX_ROTATION_SPEED = 5f; // degrees per seconds
 
  private Game1 tankGame;
  private Tank tank;
  private Sprite Sprite;
  private Transform2 Transform;

  public Turret(Game1 tankGame, Tank tank)
  {
    this.tankGame = tankGame;
    this.tank = tank;
  }

  public float CurrentAngle => Transform.Rotation;

  public void LoadContent(ContentManager content, Point2 initialPosition, float spriteScale)
  {
    var texture = content.Load<Texture2D>("tankRed_barrel1_outline");

    this.Sprite = new Sprite(texture);
    this.Sprite.Origin = new Vector2(texture.Width / 2f, texture.Height / 4f);
    this.Transform = new Transform2(new Vector2(initialPosition.X, initialPosition.Y), 0.0f, new Vector2(spriteScale, spriteScale));
  }

  public override void Update(GameTime gameTime)
  {
    var target = tankGame.Camera.ScreenToWorld(new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
    this.RotateTurretTo(target, gameTime);
    this.Transform.Position = tank.CurrentPosition;
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
  }
}
