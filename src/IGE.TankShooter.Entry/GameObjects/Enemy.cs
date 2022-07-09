namespace IGE.TankShooter.Entry.GameObjects;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;

public class Enemy : GameObject
{

  private Vector2 Position;
  private Tank Target;
  
  private const float SPEED = 10f;
  private const float SIZE = 2f;
  
  public Enemy(Vector2 initialPosition, Tank target)
  {
    Position = initialPosition;
    Target = target;
  }
  
  public override void Update(GameTime gameTime)
  {
    // Naively move toward the tank. In the future, be more intelligent.
    this.Position += (Target.CurrentPosition() - Position).NormalizedCopy() * gameTime.GetElapsedSeconds() * SPEED;
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    spriteBatch.DrawCircle(Position, SIZE, 10, Color.White);
  }
  
}
