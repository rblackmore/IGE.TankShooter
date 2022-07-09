﻿namespace IGE.TankShooter.Entry.GameObjects;
using System;

using IGE.TankShooter.Entry.Core;
using IGE.TankShooter.Entry.Graphics;

using Microsoft.Xna.Framework;
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
  
  private const float MAX_TURRET_ROTATION_SPEED = 6f; // degrees per seconds

  private MovementVelocity velocity;

  public Tank(Game1 tankGame)
  {
    this.tankGame = tankGame;
  }

  public Vector2 CurrentPosition() => BodyTransform.Position;

  public override void Initialize()
  {
    this.velocity = new MovementVelocity(Vector2.Zero, 10f);
    base.Initialize();
  }

  public override void LoadContent()
  {
    // TODO: Load better textures.
    var bodyTexture =
      GraphicsHelpers.CreateColouredRectangle(tankGame.GraphicsDevice, 6, 3, Color.DarkBlue);

    BodySprite = new Sprite(bodyTexture);

    var turretTexture =
      GraphicsHelpers.CreateColouredRectangle(tankGame.GraphicsDevice, 6, 1, Color.DarkRed);

    TurretSprite = new Sprite(turretTexture);

    this.TurretTransform = new Transform2(new Vector2(10,9), 0.0f, Vector2.One);
    this.BodyTransform = new Transform2(new Vector2(10, 10), 0.0f, Vector2.One);

    this.BodyTransform.TranformUpdated += BodyTransform_TranformUpdated;

    this.TurretTransform.Parent = this.BodyTransform;

  }

  private void BodyTransform_TranformUpdated()
  {
    this.TurretTransform.Position = this.BodyTransform.Position -= Vector2.UnitY * 10;
  }

  public override void Update(GameTime gameTime)
  {
    this.MoveTank(gameTime);
    RotateTurretTo(new Vector2(Mouse.GetState().X, Mouse.GetState().Y), gameTime);
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
    
    var targetAngle = (target - CurrentPosition()).ToAngle() * 180 / (float)Math.PI;
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

  private void MoveTank(GameTime gameTime)
  {
    var deltaTime = gameTime.GetElapsedSeconds();

    var kbState = KeyboardExtended.GetState();

    var direction = Vector2.Zero;

    if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
      direction -= Vector2.UnitX;
    if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
      direction += Vector2.UnitX;
    if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
      direction -= Vector2.UnitY;
    if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
      direction += Vector2.UnitY;

    this.velocity.Direction = direction;

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
