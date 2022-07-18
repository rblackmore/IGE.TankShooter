namespace IGE.TankShooter.Entry;

using System;
using System.Collections.Generic;

using Core;

using GameObjects;

using Graphics;

using IGE.TankShooter.Entry.Core.StatsDisplay;
using IGE.TankShooter.Entry.Core.StatsDisplay.Data;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Input;
using MonoGame.Extended.ViewportAdapters;

public class Game1 : Game
{
  private float targetFrameRate = 200f;

  private GraphicsDeviceManager graphics;
  private SpriteBatch spriteBatch;
  private Tank tank;
  private ISet<Bullet> Bullets = new HashSet<Bullet>();
  private ISet<Enemy> Enemies = new HashSet<Enemy>();
  private CountdownTimer EnemySpawnTimer = new(3, 3, 10, true);
  private Texture2D BulletTexture;
  private BackgroundMap Background;
  private CollisionComponent CollisionComponent;

  private TextOverlay textOverlay;
  private FrameRateCounter fpsCounter;

  public OrthographicCamera Camera { get; set; }

  public Game1()
  {
    graphics = new GraphicsDeviceManager(this);
    Content.RootDirectory = "Content";
    IsMouseVisible = true;
  }

  protected override void Initialize()
  {
    this.IsFixedTimeStep = true;
    this.TargetElapsedTime = TimeSpan.FromMilliseconds(1000f / targetFrameRate);
    this.graphics.SynchronizeWithVerticalRetrace = false;
    this.graphics.ApplyChanges();
    Background = new BackgroundMap(200, 200);
    this.EnemySpawnTimer.CountdownTriggered += MaybeSpawnEnemy;

    var collisionBounds = Background.BoundingBox;
    collisionBounds.Inflate(EdgeOfTheWorld.BufferSize, EdgeOfTheWorld.BufferSize);
    CollisionComponent = new CollisionComponent(collisionBounds);
    
    CollisionComponent.Insert(new EdgeOfTheWorld(this, EdgeOfTheWorld.Side.Bottom, Background.BoundingBox));
    CollisionComponent.Insert(new EdgeOfTheWorld(this, EdgeOfTheWorld.Side.Top, Background.BoundingBox));
    CollisionComponent.Insert(new EdgeOfTheWorld(this, EdgeOfTheWorld.Side.Left, Background.BoundingBox));
    CollisionComponent.Insert(new EdgeOfTheWorld(this, EdgeOfTheWorld.Side.Right, Background.BoundingBox));
    
    this.tank = new Tank(this, this.Background.BoundingBox.Center);
    this.tank.Initialize();

    var ratio = this.graphics.PreferredBackBufferWidth / 100;

    var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 100, this.graphics.PreferredBackBufferHeight / ratio);

    this.Camera = new OrthographicCamera(viewportAdapter);
    var cameraCenter = Background.BoundingBox.Center;
    this.Camera.Position = new Vector2(cameraCenter.X - 20f, cameraCenter.Y - 20f);

    this.Services.AddService(this.Camera);

    this.textOverlay = new TextOverlay(this);
    this.textOverlay.Initialize();

    this.fpsCounter = new FrameRateCounter();
    this.fpsCounter.Initialize();

    this.textOverlay.Add(fpsCounter);

    base.Initialize();
  }

  protected override void LoadContent()
  {
    spriteBatch = new SpriteBatch(GraphicsDevice);
    this.tank.LoadContent();
    this.BulletTexture = Content.Load<Texture2D>("bulletSand3_outline");
    Background.LoadContent(Content, GraphicsDevice);
    this.textOverlay.LoadContent();
  }

  protected override void Update(GameTime gameTime)
  {
    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
      Exit();

    this.EnemySpawnTimer.Update(gameTime);
    MaybeFireBullet();
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
    
    CollisionComponent.Update(gameTime);

    this.textOverlay.Update(gameTime);

    base.Update(gameTime);
  }

  private void MaybeFireBullet()
  {
    if (MouseExtended.GetState().WasButtonJustDown(MouseButton.Left))
    {
      var targetScreen = Mouse.GetState().Position;
      var target = this.Camera.ScreenToWorld(targetScreen.X, targetScreen.Y);
      var bullet = new Bullet(this, BulletTexture, target, this.tank.CurrentPosition());
      Bullets.Add(bullet);
      CollisionComponent.Insert(bullet);

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

  private void MaybeSpawnEnemy(object sender, EventArgs args)
  {
    // Project outward from the tank a distance of 50-75m and then rotate randomly in a 360 degree arc.
    var distanceFromTank = new Random().NextSingle(50f, 75f);
    var spawnPosition = this.tank.CurrentPosition() + (Vector2.One * distanceFromTank).Rotate((float)(new Random().NextDouble() * Math.PI));
    var enemy = new Enemy(spawnPosition, this.tank);
    Enemies.Add(enemy);
    CollisionComponent.Insert(enemy);
  }

  protected override void Draw(GameTime gameTime)
  {
    GraphicsDevice.Clear(Color.Black);
    this.fpsCounter.Update(gameTime);

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

    this.spriteBatch.Begin();
    this.textOverlay.Draw(gameTime, this.spriteBatch);
    this.spriteBatch.End();


    base.Draw(gameTime);
  }

  public void RemoveBullet(Bullet bullet)
  {
    Bullets.Remove(bullet);
    CollisionComponent.Remove(bullet);
  }

  public void OnEnemyHit(Bullet bullet, Enemy enemy)
  {
    // TODO: Queue explosion, damage, points, etc.
    Enemies.Remove(enemy);
    CollisionComponent.Remove(enemy);
    
    RemoveBullet(bullet);
  }
}
