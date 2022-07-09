namespace IGE.TankShooter.Entry.GameObjects;
using System;

using IGE.TankShooter.Entry.Core;
using IGE.TankShooter.Entry.Graphics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using MonoGame.Extended.Sprites;

public class Tank : GameObject
{
  private Sprite BodySprite;
  private Sprite TurretSprite;

  private Transform2 BodyTransform;
  private Transform2 TurretTransform;

  private Game1 tankGame;

  public Tank(Game1 tankGame)
  {
    this.tankGame = tankGame;
  }

  public override void Initialize()
  {
    // Initilize the things.
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

    this.BodyTransform = new Transform2(new Vector2(50), 0.0f, new Vector2(30));
    this.TurretTransform = new Transform2(new Vector2(50), 0.0f, new Vector2(30));

    this.TurretTransform.Parent = this.BodyTransform;

  }
  public override void UnloadContent()
  {
    // Unload Things.
  }
   
  public override void Update(GameTime gameTime)
  {

  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    spriteBatch.Draw(BodySprite, BodyTransform);
    spriteBatch.Draw(TurretSprite, TurretTransform);
  }
}
