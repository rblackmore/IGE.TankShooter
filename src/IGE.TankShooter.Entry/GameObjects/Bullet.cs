namespace IGE.TankShooter.Entry.GameObjects;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using MonoGame.Extended.Sprites;

using Physics;

public class Bullet : GameObject
{
  const float SPEED = 100f;

  private readonly Game1 tankGame;
  public Vector2 Position { get; set; }
  private Vector2 Velocity { get; }
  private Sprite Sprite;
  
  public Bullet(Game1 game, Texture2D texture, Vector2 targetPosition, Vector2 initialPosition)
  {
    this.Position = initialPosition;
    this.Velocity = (targetPosition - initialPosition).NormalizedCopy() * SPEED;
    this.tankGame = game;
    this.Sprite = new Sprite(texture);
  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    Sprite.Draw(spriteBatch, Position, Velocity.ToAngle(), new Vector2(0.05f));
  }

  public override void Update(GameTime gameTime)
  {
    this.Position += this.Velocity * gameTime.GetElapsedSeconds();
  }

  // TODO: Extract an appropriate interface from Enemy, e.g. "ICollidable"
  public bool IsColliding(IBoundingBox enemy)
  {
    return enemy.GetBoundingBox().Contains(this.Position);
  }
}
