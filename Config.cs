using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace TeleportProtection;
public class TeleportProtectionConfig : BasePluginConfig
{
    public override int Version { get; set; } = 1;

    [JsonPropertyName("RoundStartDelay")]
    public float RoundStartDelay { get; set; } = 5.0f;

    [JsonPropertyName("ProtectionTime")]
    public float ProtectionTime { get; set; } = 1.0f;

    [JsonPropertyName("RemoveProtectionOnWeaponFire")]
    public bool RemoveProtectionOnWeaponFire { get; set; } = true;

    [JsonPropertyName("TeleportProtectionMessage")]
    public string TeleportProtectionMessage { get; set; } = $"[{{lightred}}SpawnProtection{{default}}] You are protected for {{green}}{{Delay}}{{default}} second(s)!";

    [JsonPropertyName("TeleportProtectionRemoveMessage")]
    public string TeleportProtectionRemoveMessage { get; set; } = $"[{{lightred}}SpawnProtection{{default}}] You are no longer {{green}}protected{{default}} from any damage!";
}