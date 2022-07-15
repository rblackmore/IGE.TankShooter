namespace IGE.TankShooter.Entry.GameObjects;

using System;

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
  private Sprite Sprite;
  
  public Bullet(Game1 game, Texture2D texture, Vector2 direction, Vector2 initialPosition)
  {
    this.Bounds = new CircleF(initialPosition.ToPoint(), 0.15f);
    this.Velocity = direction.NormalizedCopy() * SPEED;
    this.tankGame = game;
    this.Sprite = new Sprite(texture);
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    Sprite.Draw(spriteBatch, this.Bounds.Position, Velocity.ToAngle(), new Vector2(0.05f));
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
  }
}
