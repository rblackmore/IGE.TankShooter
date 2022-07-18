namespace IGE.TankShooter.Entry.Core;

using System;

using Microsoft.Xna.Framework;

using MonoGame.Extended;

public class CountdownTimer
{
  private readonly float initialDelaySeconds;
  private readonly float MinDelaySeconds;
  private readonly float MaxDelaySeconds;

  private float Counter;

  public event EventHandler<EventArgs> CountdownTriggered;

  public CountdownTimer(float initialDelaySeconds, bool repeat = false)
  {
    this.Counter = this.initialDelaySeconds = initialDelaySeconds;
    this.IsRandomDelay = false;
    this.Repeat = repeat;
  }

  public CountdownTimer(float initialDelaySeconds, float minDelaySeconds, float maxDelaySeconds, bool repeat = false)
    : this(initialDelaySeconds, repeat)
  {
    this.MinDelaySeconds = minDelaySeconds;
    this.MaxDelaySeconds = maxDelaySeconds;
    this.IsRandomDelay = true;
  }

  public bool Repeat { get; set; }
  public bool IsRandomDelay { get; set; }

  private void OnCountdownTriggered()
  {
    this.CountdownTriggered?.Invoke(this, null);

    if (this.Repeat)
      this.Reset();
  }

  public bool Update(GameTime gameTime)
  {
    this.Counter -= gameTime.GetElapsedSeconds();

    if (this.Counter <= 0)
    {
      OnCountdownTriggered();

      return true;
    }

    return false;
  }

  public void Reset()
  {
    if (this.IsRandomDelay)
      this.Counter = new Random().NextSingle(MinDelaySeconds, MaxDelaySeconds);
    else
      this.Counter = this.initialDelaySeconds;
  }
}
