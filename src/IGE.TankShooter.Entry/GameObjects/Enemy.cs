namespace IGE.TankShooter.Entry.GameObjects;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using MonoGame.Extended.Collisions;

public class Enemy : GameObject, ICollisionActor
{

  private Tank Target;
  
  private const float SPEED = 10f;
  private const float SIZE = 2f;
  
  public Enemy(Vector2 initialPosition, Tank target)
  {
    Target = target;
    Bounds = new CircleF(initialPosition.ToPoint(), SIZE);
  }
  
  public override void Update(GameTime gameTime)
  {
    // Naively move toward the tank. In the future, be more intelligent.
    this.Bounds.Position += (Target.CurrentPosition() - new Vector2(this.Bounds.Position.X, this.Bounds.Position.Y)).NormalizedCopy() * gameTime.GetElapsedSeconds() * SPEED;
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    spriteBatch.DrawCircle((CircleF)this.Bounds, 10, Color.White);
  }

  public void OnCollision(CollisionEventArgs collisionInfo)
  {
    
  }

  public IShapeF Bounds { get; }
}
