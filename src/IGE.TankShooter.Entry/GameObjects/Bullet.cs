namespace IGE.TankShooter.Entry.GameObjects;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;

public class Bullet : GameObject, ICollisionActor
{
  const float SPEED = 100f;

  private readonly Game1 tankGame;
  public IShapeF Bounds { get; }
  private Vector2 Velocity { get; }
  private readonly Sprite Sprite;
  private readonly Vector2 SpriteScale;
  
  // Only kept for help with debugging.
  private readonly Vector2 initialPosition;
  
  public Bullet(Game1 game, Sprite sprite, Vector2 spriteScale, Vector2 direction, Vector2 initialPosition)
  {
    this.initialPosition = initialPosition;
    this.Bounds = new CircleF(initialPosition.ToPoint(), 0.15f);
    this.Velocity = direction.NormalizedCopy() * SPEED;
    this.tankGame = game;
    this.Sprite = sprite;
    this.SpriteScale = spriteScale;
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    Sprite.Draw(spriteBatch, this.Bounds.Position, Velocity.ToAngle(), SpriteScale);

    if (Debug.DrawDebugLines)
    {
      spriteBatch.DrawCircle((CircleF)this.Bounds, 10, Color.Black, 0.1f);
      spriteBatch.DrawLine(this.initialPosition, this.initialPosition + this.Velocity, Color.Yellow, 0.1f);
    }
  }

  public override void Update(GameTime gameTime)
  {
    this.Bounds.Position += this.Velocity * gameTime.GetElapsedSeconds();
  }

  public void OnCollision(CollisionEventArgs collisionInfo)
  {
    if (collisionInfo.Other is Enemy enemy)
    {
      this.tankGame.OnEnemyHit(this, enemy);
    }
    else if (collisionInfo.Other is MapObject or EdgeOfTheWorld)
    {
      this.tankGame.RemoveBullet(this);
    }
  }
}
