namespace IGE.TankShooter.Entry.GameObjects;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;

public class Bullet : GameObject
{
  private const float SIZE = 0.1f;
  const float SPEED = 200f;

  private readonly Game1 tankGame;
  private Vector2 Position { get; set; }
  private Vector2 Velocity { get; set; }

  public Bullet(Game1 game, Vector2 targetPosition, Vector2 initialPosition)
  {
    this.Position = initialPosition;
    this.Velocity = (targetPosition - initialPosition).NormalizedCopy() * SPEED;
    this.tankGame = game;
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    spriteBatch.FillRectangle(new RectangleF(Position.X, Position.Y, SIZE, SIZE), Color.White);
  }

  public override void Update(GameTime gameTime)
  {
    this.Position += this.Velocity * gameTime.GetElapsedSeconds();

    // TODO: Use world coordinates.
    if (!this.tankGame.GraphicsDevice.Viewport.Bounds.Contains(this.Position))
    {
      this.tankGame.Bullets.Remove(this);
    }
  }
}
