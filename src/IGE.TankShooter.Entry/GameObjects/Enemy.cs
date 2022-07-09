namespace IGE.TankShooter.Entry.GameObjects;

using System;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;

public class Enemy : GameObject
{

  private Vector2 Position;
  private Tank Target;
  
  private const float SPEED = 100f;
  private const float SIZE = 100f;
  
  public Enemy(Vector2 initialPosition, Tank target)
  {
    Position = initialPosition;
    Target = target;
  }
  
  public override void Update(GameTime gameTime)
  {
    // Naively move toward the tank. In the future, be more intelligent.
    this.Position += (Target.CurrentPosition() - Position).NormalizedCopy() * gameTime.GetElapsedSeconds() * SPEED;
    Console.WriteLine("Moved enemy to: " + this.Position);
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    spriteBatch.DrawCircle(Position, SIZE, 20, Color.White);
  }
  
}
