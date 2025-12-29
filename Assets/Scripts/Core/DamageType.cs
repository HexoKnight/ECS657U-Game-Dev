namespace GUP.Core
{
    /// <summary>
    /// Enumeration of all damage types in the game.
    /// Used by DamageData and visual/audio feedback systems.
    /// </summary>
    public enum DamageType
    {
        /// <summary>Contact damage from touching an enemy or hazard</summary>
        Contact,
        
        /// <summary>Melee attack damage (crab claws, etc.)</summary>
        Melee,
        
        /// <summary>Projectile damage (spines, etc.)</summary>
        Projectile,
        
        /// <summary>Explosion damage (exploding fish)</summary>
        Explosion,
        
        /// <summary>Environmental damage (spikes, falling)</summary>
        Environmental,
        
        /// <summary>Electric damage (jellyfish)</summary>
        Electric
    }
}
