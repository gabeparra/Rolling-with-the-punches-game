
public static class PlayerStats
{
    private static float RELOAD_TIME_BASE = 2f; //seconds
    private static float MOVE_SPEED_BASE = 5f;
    private static int MAG_SIZE_BASE = 6;
    private static int HEALTH_BASE = 3; //currently health is a heart-based system
    private static float DASH_COOLDOWN_BASE = 1f; //seconds
    private static float DASH_TIME_BASE = 0.12f; //seconds. How long player spends dashing while doing so. effectively controls the dash distance
    private static int MAX_JUMP_BASE = 1;
    private static float CURRENCY_RATE_BASE = 1f;
    private static int GOLD_PER_KILL_BASE = 7;

    public static float reloadTime => RELOAD_TIME_BASE - (0.25f *UpgradeManager.GetLevel("Reload Speed")); //each upgrade takes of a quarter second
    public static float moveSpeed => MOVE_SPEED_BASE + UpgradeManager.GetLevel("Move Speed");
    public static int magSize => MAG_SIZE_BASE + UpgradeManager.GetLevel("Magazine Size");
    public static int maxHealth => HEALTH_BASE + UpgradeManager.GetLevel("Max Health");
    public static float dashCooldown => DASH_COOLDOWN_BASE - UpgradeManager.GetLevel("Dash Cooldown");
    public static float dashTime => DASH_TIME_BASE + (0.01f * UpgradeManager.GetLevel("Dash Distance"));
    public static int maxJumps => MAX_JUMP_BASE + UpgradeManager.GetLevel("Max Jumps");
    public static float currencyRate => CURRENCY_RATE_BASE + UpgradeManager.GetLevel("Gold-Cash Conversion Rate");
    public static int goldPerKill => GOLD_PER_KILL_BASE + UpgradeManager.GetLevel("Gold Per Kill");
}
