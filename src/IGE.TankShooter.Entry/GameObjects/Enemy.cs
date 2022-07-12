namespace IGE.TankShooter.Entry.GameObjects;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;

using Physics;

public class Enemy : GameObject, IBoundingBox
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

  public RectangleF GetBoundingBox()
  {
    return new RectangleF(Position.X - SIZE / 2, Position.Y - SIZE / 2, SIZE, SIZE);
  }
}
