namespace IGE.TankShooter.Entry.Core;

public class Debug
{
  /// <summary>
  /// Helper to draw additional debug information over game objects.
  /// </summary>
  public static class DrawDebugLines {

    public static class Collisions {
      public static bool Bounds = false;
    }

    public static class Pathfinding {
      public static bool Grid = false;
      public static bool Results = false;
    }

    public static bool Movement = false;

  }
}
