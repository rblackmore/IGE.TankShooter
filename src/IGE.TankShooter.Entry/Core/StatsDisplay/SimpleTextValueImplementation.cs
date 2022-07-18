namespace IGE.TankShooter.Entry.Core.StatsDisplay;

using IGE.TankShooter.Entry.Stats;

public class SimpleTextValueImplementation : ITextValue
{
  public string Text { get; private set; }

  public SimpleTextValueImplementation(string value)
  {
    Text = value;
  }
}
