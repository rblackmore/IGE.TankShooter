namespace IGE.TankShooter.Entry;

using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;
using MonoGame.Extended.SceneGraphs;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.ViewportAdapters;

using MonoGame.Extended.Input;
using IGE.TankShooter.Entry.Graphics;
using System;
using IGE.TankShooter.Entry.GameObjects;

public class Game1 : Game
{
  private GraphicsDeviceManager graphics;
  public SpriteBatch spriteBatch;
  private Tank tank;
  public ISet<Bullet> Bullets = new HashSet<Bullet>();

  public Game1()
  {
    graphics = new GraphicsDeviceManager(this);
    Content.RootDirectory = "Content";
    IsMouseVisible = true;
  }

  protected override void Initialize()
  {
    this.tank = new Tank(this);
    base.Initialize();
  }

  protected override void LoadContent()
  {
    spriteBatch = new SpriteBatch(GraphicsDevice);
    this.tank.LoadContent();
  }
  
  protected override void Update(GameTime gameTime)
  {
    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
      Exit();

    MaybeFireBullet();
    
    foreach (var bullet in this.Bullets)
    {
      bullet.Update(gameTime);
    }
    
    base.Update(gameTime);
  }
  
  private void MaybeFireBullet()
  {
    if (MouseExtended.GetState().WasButtonJustDown(MouseButton.Left))
    {
      var target = Mouse.GetState().Position;
      var initial = GraphicsDevice.Viewport.Bounds.Center;
      Bullets.Add(new Bullet(this, new Vector2(target.X, target.Y), new Vector2(initial.X, initial.Y)));
    }
  }

  protected override void Draw(GameTime gameTime)
  {
    GraphicsDevice.Clear(Color.CornflowerBlue);

    this.spriteBatch.Begin();

    this.tank.Draw(gameTime, spriteBatch);
    foreach (var bullet in this.Bullets)
    {
      bullet.Draw(gameTime, spriteBatch);
    }

    this.spriteBatch.End();

    base.Draw(gameTime);
  }
}
