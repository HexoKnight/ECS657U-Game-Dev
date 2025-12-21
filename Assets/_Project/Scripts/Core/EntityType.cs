/// <summary>
/// Identifies the type of entity for damage system and event routing.
/// Used by IDamageable to enable decoupled entity type detection.
/// </summary>
public enum EntityType
{
    /// <summary>Unknown or generic entity</summary>
    Unknown = 0,
    
    /// <summary>Player character</summary>
    Player = 1,
    
    /// <summary>Enemy NPC</summary>
    Enemy = 2,
    
    /// <summary>Destructible environment object</summary>
    Destructible = 3,
    
    /// <summary>Environmental hazard</summary>
    Hazard = 4
}
