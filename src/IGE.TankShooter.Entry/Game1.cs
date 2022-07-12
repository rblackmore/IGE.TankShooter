namespace IGE.TankShooter.Entry;

using System;
using System.Collections.Generic;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended.Input;
using GameObjects;

using Graphics;

using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

public class Game1 : Game
{
  private GraphicsDeviceManager graphics;
  public SpriteBatch spriteBatch;
  private Tank tank;
  public LinkedList<Bullet> Bullets = new();
  public LinkedList<Enemy> Enemies = new();
  private CountdownTimer EnemySpawnTimer = new(3, 3, 10);
  private Texture2D BulletTexture;
  private BackgroundMap Background;

  public OrthographicCamera Camera { get; set; }

  public Game1()
  {
    graphics = new GraphicsDeviceManager(this);
    Content.RootDirectory = "Content";
    IsMouseVisible = true;
  }

  protected override void Initialize()
  {
    Background = new BackgroundMap(200, 200);
    
    this.tank = new Tank(this, this.Background.GetBoundingBox().Center);
    this.tank.Initialize();

    var ratio = this.graphics.PreferredBackBufferWidth / 100;

    var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 100, this.graphics.PreferredBackBufferHeight / ratio);

    this.Camera = new OrthographicCamera(viewportAdapter);
    var cameraCenter = Background.GetBoundingBox().Center;
    this.Camera.Position = new Vector2(cameraCenter.X - 20f, cameraCenter.Y - 20f);

    this.Services.AddService(this.Camera);

    base.Initialize();
  }

  protected override void LoadContent()
  {
    spriteBatch = new SpriteBatch(GraphicsDevice);
    this.tank.LoadContent();
    this.BulletTexture = Content.Load<Texture2D>("bulletSand3_outline");
    Background.LoadContent(Content, GraphicsDevice);
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
      enemy.Update(gameTime);
    }

    CheckCollisions(gameTime);
    
    base.Update(gameTime);
  }

  /// <summary>
  /// This would be a lot simpler with simple foreach iteration of both bullets and enemies, however there are two
  /// reasons that we use more complex LinkedList node based iteration. The first is because we aim to remove items
  /// after collisions, and removing from a linked list is cheaper than removing from an array. Secondly, it is
  /// actually an Exception to remove from a list while iterating using foreach.
  /// </summary>
  /// <param name="gameTime"></param>
  private void CheckCollisions(GameTime gameTime)
  {
    var bulletNode = this.Bullets.First;
    while (bulletNode != null)
    {
      var bullet = bulletNode.Value;
      var nextBullet = bulletNode.Next;
      if (!this.Background.GetBoundingBox().Contains(bullet.Position.ToPoint()))
      {
        this.Bullets.Remove(bulletNode);
        bulletNode = nextBullet;
        continue;
      }
     
      var enemyNode = this.Enemies.First;
      while (enemyNode != null)
      {
        var nextEnemy = enemyNode.Next;
        if (bullet.IsColliding(enemyNode.Value))
        {
          this.Bullets.Remove(bulletNode);
          this.Enemies.Remove(enemyNode);
        }
        enemyNode = nextEnemy;
      }
      bulletNode = nextBullet;
    }
  }

  private void MaybeFireBullet()
  {
    if (MouseExtended.GetState().WasButtonJustDown(MouseButton.Left))
    {
      var targetScreen = Mouse.GetState().Position;
      var target = this.Camera.ScreenToWorld(targetScreen.X, targetScreen.Y);
      Bullets.AddFirst(new Bullet(this, BulletTexture, target, this.tank.CurrentPosition()));
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
      this.Camera.Zoom -= 0.1f;
    if (kbState.WasKeyJustDown(Keys.L))
      this.Camera.Zoom += 0.1f;

  }

  private void MaybeSpawnEnemy(GameTime gameTime)
  {
    if (this.EnemySpawnTimer.Update(gameTime))
    {
      // Project outward from the tank a distance of 50-75m and then rotate randomly in a 360 degree arc.
      var distanceFromTank = new Random().NextSingle(50f, 75f);
      var spawnPosition = this.tank.CurrentPosition() + (Vector2.One * distanceFromTank).Rotate((float)(new Random().NextDouble() * Math.PI));
      Enemies.AddFirst(new Enemy(spawnPosition, this.tank));
    }
  }

  protected override void Draw(GameTime gameTime)
  {
    GraphicsDevice.Clear(Color.Black);

    // See https://community.monogame.net/t/screen-tearing-with-monogame-extended-and-tiled/14757 for details of what
    // SamplerState.PointClamp does. It is to stop weird lines due to floating point weirdness between background tile
    // rows when zooming.
    this.spriteBatch.Begin(transformMatrix: this.Camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);
    
    this.Background.Draw(Camera);

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
