namespace IGE.TankShooter.Entry;

using System;
using System.Collections.Generic;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended.Input;
using GameObjects;

public class Game1 : Game
{
  private GraphicsDeviceManager graphics;
  public SpriteBatch spriteBatch;
  private Tank tank;
  public ISet<Bullet> Bullets = new HashSet<Bullet>();
  public ISet<Enemy> Enemies = new HashSet<Enemy>();
  private CountdownTimer EnemySpawnTimer = new CountdownTimer(3, 3, 10);

  public OrthographicCamera Camera { get; set; }

  public Game1()
  {
    graphics = new GraphicsDeviceManager(this);
    Content.RootDirectory = "Content";
    IsMouseVisible = true;
  }

  protected override void Initialize()
  {
    this.tank = new Tank(this);
    this.tank.Initialize();

    var ratio = this.graphics.PreferredBackBufferWidth / 100;

    var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 100, this.graphics.PreferredBackBufferHeight / ratio);

    this.Camera = new OrthographicCamera(viewportAdapter);

    this.Services.AddService(this.Camera);

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
    MaybeSpawnEnemy(gameTime);
    TranslateCamera();

    this.tank.Update(gameTime);
    
    foreach (var bullet in this.Bullets)
    {
      bullet.Update(gameTime);
    }
    
    foreach (var enemy in this.Enemies)
    {
      Console.WriteLine("Updating enemy: " + enemy);
      enemy.Update(gameTime);
    }
    
    base.Update(gameTime);
  }

  private void MaybeFireBullet()
  {
    if (MouseExtended.GetState().WasButtonJustDown(MouseButton.Left))
    {
      var target = Mouse.GetState().Position;
      Bullets.Add(new Bullet(this, new Vector2(target.X, target.Y), this.tank.CurrentPosition()));
    }
  }

  private void TranslateCamera()
  {
    //TODO: Bad code is bad.
    var direction = Vector2.Zero;

    var kbState = KeyboardExtended.GetState();

    if (kbState.IsKeyDown(Keys.Up))
      direction -= Vector2.UnitY;
    if (kbState.IsKeyDown(Keys.Down))
      direction += Vector2.UnitY;
    if (kbState.IsKeyDown(Keys.Left))
      direction -= Vector2.UnitX;
    if (kbState.IsKeyDown(Keys.Right))
      direction += Vector2.UnitX;

    this.Camera.Move(direction);

    if (kbState.WasKeyJustDown(Keys.O))
      this.Camera.Zoom -= 1;
    if (kbState.WasKeyJustDown(Keys.L))
      this.Camera.Zoom += 1;

  }

  private void MaybeSpawnEnemy(GameTime gameTime)
  {
    if (this.EnemySpawnTimer.Update(gameTime))
    {
      var height = new Random().Next(0, GraphicsDevice.Viewport.Height);
      Enemies.Add(new Enemy(new Vector2(GraphicsDevice.Viewport.Width, height), this.tank));
    }
  }

  protected override void Draw(GameTime gameTime)
  {
    GraphicsDevice.Clear(Color.Black);

    this.spriteBatch.Begin(transformMatrix: this.Camera.GetViewMatrix());

    this.tank.Draw(gameTime, spriteBatch);

    foreach (var bullet in this.Bullets)
    {
      bullet.Draw(gameTime, spriteBatch);
    }

    foreach (var enemy in this.Enemies)
    {
      enemy.Draw(gameTime, spriteBatch);
    }

    this.spriteBatch.End();

    base.Draw(gameTime);
  }
}
