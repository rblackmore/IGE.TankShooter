namespace IGE.TankShooter.Entry.Core;

using System;

using Microsoft.Xna.Framework;

using MonoGame.Extended;

public class CountdownTimer
{

  private readonly float MinDelaySeconds;
  private readonly float MaxDelaySeconds;

  private float Counter;
  
  public CountdownTimer(float initialDelaySeconds, float minDelaySeconds, float maxDelaySeconds)
  {
    this.Counter = initialDelaySeconds;
    this.MinDelaySeconds = minDelaySeconds;
    this.MaxDelaySeconds = maxDelaySeconds;
  }

  public bool Update(GameTime gameTime)
  {
    this.Counter -= gameTime.GetElapsedSeconds();

    if (this.Counter < 0)
    {
      this.Counter = new Random().NextSingle(MinDelaySeconds, MaxDelaySeconds);
      return true;
    }

    return false;
  }

}
