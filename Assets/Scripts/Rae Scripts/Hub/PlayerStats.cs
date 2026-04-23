
public static class PlayerStats
{
    private static float RELOAD_TIME_BASE = 5f; //seconds
    private static float MOVE_SPEED_BASE = 4f;
    private static int MAG_SIZE_BASE = 6;
    private static int HEALTH_BASE = 3; //currently health is a heart-based system
    private static float DASH_COOLDOWN_BASE = 2;
    private static float DASH_DISTANCE_BASE = 5;
    private static int MAX_JUMP_BASE = 1;
    private static float CURRENCY_RATE_BASE = 1;

    public static float reloadTime => RELOAD_TIME_BASE - UpgradeManager.GetLevel("Reload Speed");
    public static float moveSpeed => MOVE_SPEED_BASE + UpgradeManager.GetLevel("Move Speed"); //TODO: this upgrade doesn't exist
    public static int magSize => MAG_SIZE_BASE + UpgradeManager.GetLevel("Magazine Size");
    public static int maxHealth => HEALTH_BASE + UpgradeManager.GetLevel("Max Health"); //TODO: this upgrade doesn't exist
    public static float dashCooldown => DASH_COOLDOWN_BASE - UpgradeManager.GetLevel("Dash Cooldown"); //TODO: create upgrade
    public static float dashDistance => DASH_DISTANCE_BASE + UpgradeManager.GetLevel("Dash Distance"); //TODO: create upgrade
    public static int maxJumps => MAX_JUMP_BASE + UpgradeManager.GetLevel("Max Jumps"); //TODO: create upgrade
    public static float currencyRate => CURRENCY_RATE_BASE + UpgradeManager.GetLevel("Conversion Rate"); //TODO: create upgrade
}
