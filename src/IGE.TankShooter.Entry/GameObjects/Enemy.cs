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

  private NavigationPath _navigationPath;
  private Pathfinder _pathfinder;
  
  public Enemy(Vector2 initialPosition, Texture2D texture, Tank target, Pathfinder pathfinder)
  {
    Target = target;
    Bounds = new CircleF(initialPosition.ToPoint(), SIZE);
    Texture = texture;
    Sprite = new Sprite(Texture);
    _pathfinder = pathfinder;
  }

  public override void LoadContent(ContentManager content)
  {
    
  }

  public override void Update(GameTime gameTime)
  {
    if (_navigationPath == null || !_navigationPath.Matches(Target.CurrentPosition, this.Bounds.Position)) {
      _navigationPath = _pathfinder.FindPath(Target.CurrentPosition, this.Bounds.Position);
    }

    if (_navigationPath == null)
    {
      // Can't move if we don't know where to go yet. Maybe we can do something naive while we are
      // finding a path in the background the first time we come into existence for an optimisation,
      // to avoid enemies that just stand still while they think.
      this.Direction = Vector2.Zero;
      return;
    }

    // Find out which part of the navigation path we are currently on, and head towards the next
    // node in the path.
    var nextPos = this._navigationPath.NextPosition(this.Bounds.Position);
    if (nextPos is Vector2 nextPosVector)
    {
      this.Direction = (nextPosVector - new Vector2(this.Bounds.Position.X, this.Bounds.Position.Y)).NormalizedCopy();
      this.Bounds.Position += this.Direction * gameTime.GetElapsedSeconds() * SPEED;
    }

  }

  public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
  {
    _navigationPath?.Draw(spriteBatch);

    spriteBatch.Draw(Sprite, this.Bounds.Position, this.Direction.ToAngle() + (float)Math.PI / 2f, new Vector2(SIZE / Texture.Width));

    if (Debug.DrawDebugLines.Collisions.Bounds)
    {
      spriteBatch.DrawCircle((CircleF)this.Bounds, 10, Color.Red, 0.2f);
      spriteBatch.DrawLine(this.Bounds.Position, this.Bounds.Position + (this.Direction * 5), Color.Red, 0.2f);
    }
  }

  public void OnCollision(CollisionEventArgs collisionInfo)
  {
  }

  public IShapeF Bounds { get; }
}
