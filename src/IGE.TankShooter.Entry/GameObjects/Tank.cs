namespace IGE.TankShooter.Entry.GameObjects;
using System;

using IGE.TankShooter.Entry.Core;
using IGE.TankShooter.Entry.Graphics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

  private Vector2 direction = Vector2.Zero;
  private float velocity = 1f;

  public Tank(Game1 tankGame)
  {
    this.tankGame = tankGame;
  }

  public override void Initialize()
  {
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

    this.BodyTransform = new Transform2(new Vector2(10, 20), 0.0f, Vector2.One);
    this.TurretTransform = new Transform2(new Vector2(10, 20), 0.0f, Vector2.One);

    //this.TurretTransform.Parent = this.BodyTransform;

  }

  public override void Update(GameTime gameTime)
  {
    //var deltaTime = gameTime.GetElapsedSeconds();

    //var kbState = KeyboardExtended.GetState();

    //if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
    //  this.direction.X -= 1;
    //if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
    //  this.direction.X += 1;
    //if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
    //  this.direction.Y -= 1;
    //if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
    //  this.direction.Y += 1;

    //var scaler = this.direction.NormalizedCopy();
    //this.BodyTransform.Position += scaler * deltaTime;

    //this.BodyTransform.Position += new Vector2(10);
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    spriteBatch.Draw(BodySprite, BodyTransform);
    spriteBatch.Draw(TurretSprite, TurretTransform);
  }
}
