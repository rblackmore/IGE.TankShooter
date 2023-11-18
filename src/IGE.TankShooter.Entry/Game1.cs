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
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.ViewportAdapters;

public class Game1 : Game
{
  private GraphicsDeviceManager graphics;
  private SpriteBatch spriteBatch;
  private Tank tank;
  private ISet<Bullet> Bullets = new HashSet<Bullet>();
  private ISet<Enemy> Enemies = new HashSet<Enemy>();
  private CountdownTimer EnemySpawnTimer = new(1, 1f, 3f);
  private Texture2D CrosshairTexture;
  private Sprite CrosshairSprite;
  private Texture2D[] EnemyPersonTextures;
  private BackgroundMap Background;
  private CollisionComponent CollisionComponent;
  private CameraOperator CameraOperator;

  public OrthographicCamera Camera { get; set; }

  public Game1()
  {
    graphics = new GraphicsDeviceManager(this);
    Content.RootDirectory = "Content";
    IsMouseVisible = true;
  }

  protected override void Initialize()
  {
    IsMouseVisible = false;
    Background = new BackgroundMap(200, 200);

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
    CameraOperator = new CameraOperator(this.tank, this.Camera);

    this.Services.AddService(this.Camera);

    base.Initialize();
  }

  protected override void LoadContent()
  {
    spriteBatch = new SpriteBatch(GraphicsDevice);
    this.tank.LoadContent(Content);
    this.CrosshairTexture = Content.Load<Texture2D>("crosshair061");
    this.CrosshairSprite = new Sprite(this.CrosshairTexture);
    this.EnemyPersonTextures = new Texture2D[]
    {
      Content.Load<Texture2D>("enemy_person_a"),
      Content.Load<Texture2D>("enemy_person_b"),
      Content.Load<Texture2D>("enemy_person_c"),
      Content.Load<Texture2D>("enemy_person_d"),
    };
    Background.LoadContent(Content, GraphicsDevice);
    
    // Has to wait for the tank to "LoadContent" (rather than Initialize()) because the tanks transformation can only
    // be calculated once we've loaded its textures and decided how much we need to scale them.
    CameraOperator.CutTo(this.tank.CurrentPosition);
  }

  protected override void Update(GameTime gameTime)
  {
    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
      Exit();

    MaybeFireBullet();
    MaybeSpawnEnemy(gameTime);
    
    this.tank.Update(gameTime);
    CameraOperator.Update(gameTime);

    
    foreach (var bullet in this.Bullets)
    {
      bullet.Update(gameTime);
    }
    
    foreach (var enemy in this.Enemies)
    {
      enemy.Update(gameTime);
    }
    
    CollisionComponent.Update(gameTime);

    base.Update(gameTime);
  }

  private void MaybeFireBullet()
  {
    if (MouseExtended.GetState().WasButtonJustDown(MouseButton.Left))
    {
      // If required, switch back to this to fire where the mouse is pointing, not where the turret is pushing.
      // var targetScreen = Mouse.GetState().Position;
      // var target = this.Camera.ScreenToWorld(targetScreen.X, targetScreen.Y);
      var bullet = this.tank.FireBullet();
      Bullets.Add(bullet);
      CollisionComponent.Insert(bullet);
    }
  }

  private void MaybeSpawnEnemy(GameTime gameTime)
  {
    if (this.EnemySpawnTimer.Update(gameTime))
    {
      var random = new Random();
      // Project outward from the tank a distance of 50-75m and then rotate randomly in a 360 degree arc.
      var distanceFromTank = random.NextSingle(50f, 75f);
      var spawnPosition = this.tank.CurrentPosition + (Vector2.One * distanceFromTank).Rotate((float)(new Random().NextDouble() * Math.PI));
      var enemy = new Enemy(spawnPosition, this.EnemyPersonTextures[random.Next(0, this.EnemyPersonTextures.Length)], this.tank);
      Enemies.Add(enemy);
      CollisionComponent.Insert(enemy);
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

    // Always draw last so as to not obscure.
    this.DrawCursor();
    this.spriteBatch.End();

    base.Draw(gameTime);
  }

  private void DrawCursor()
  {
    var mousePosition = this.Camera.ScreenToWorld(Mouse.GetState().Position.ToVector2());
    var scale = 2f / CrosshairTexture.Width;
    this.spriteBatch.Draw(CrosshairSprite, mousePosition, 0f, new Vector2(scale));
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
