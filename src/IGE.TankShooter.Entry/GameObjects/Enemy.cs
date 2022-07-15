namespace IGE.TankShooter.Entry.GameObjects;

using System;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;

public class Enemy : GameObject, ICollisionActor
{

  private Tank Target;
  private Texture2D Texture;
  private Sprite Sprite;
  private Vector2 Direction;
  
  private const float SPEED = 5f;
  private const float SIZE = 1f;
  
  public Enemy(Vector2 initialPosition, Texture2D texture, Tank target)
  {
    Target = target;
    Bounds = new CircleF(initialPosition.ToPoint(), SIZE);
    Texture = texture;
    Sprite = new Sprite(Texture);
  }

  public override void LoadContent(ContentManager content)
  {
    
  }

  public override void Update(GameTime gameTime)
  {
    // Naively move toward the tank. In the future, be more intelligent.
    this.Direction = (Target.CurrentPosition() - new Vector2(this.Bounds.Position.X, this.Bounds.Position.Y)).NormalizedCopy();
    this.Bounds.Position += this.Direction * gameTime.GetElapsedSeconds() * SPEED;
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    spriteBatch.Draw(Sprite, this.Bounds.Position, this.Direction.ToAngle() + (float)Math.PI / 2f, new Vector2(SIZE / Texture.Width));
  }

  public void OnCollision(CollisionEventArgs collisionInfo)
  {
    
  }

  public IShapeF Bounds { get; }
}
