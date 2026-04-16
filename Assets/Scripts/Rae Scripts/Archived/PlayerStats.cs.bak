
public static class PlayerStats
{
    private static float RELOAD_TIME_BASE = 5f; //seconds
    private static float MOVE_SPEED_BASE = 4f;
    private static int MAG_SIZE_BASE = 6;
    private static int HEALTH_BASE = 3; //currently health is a heart-based system

    public static float reloadTime => RELOAD_TIME_BASE - UpgradeManager.GetLevel("Reload Speed");
    public static float moveSpeed => MOVE_SPEED_BASE + UpgradeManager.GetLevel("Move Speed"); //TODO: this upgrade doesn't exist
    public static int magSize => MAG_SIZE_BASE + UpgradeManager.GetLevel("Magazine Size");
    public static int maxHealth => HEALTH_BASE + UpgradeManager.GetLevel("Max Health"); //TODO: this upgrade doesn't exist
}
