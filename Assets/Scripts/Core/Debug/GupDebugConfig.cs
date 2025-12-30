using UnityEngine;

namespace GUP.Core.Debug
{
    /// <summary>
    /// ScriptableObject for runtime debug configuration.
    /// Create via Assets > Create > GUP > Debug Config.
    /// </summary>
    [CreateAssetMenu(fileName = "GupDebugConfig", menuName = "GUP/Debug Config")]
    public class GupDebugConfig : ScriptableObject
    {
        [Header("Master Toggle")]
        [Tooltip("Enable or disable all debug logging")]
        public bool enabled = true;

        [Header("Log Level")]
        [Tooltip("Minimum log level to display")]
        public LogLevel minLevel = LogLevel.Info;

        [Header("Categories")]
        public bool stateTransitions = true;
        public bool jumpEvents = true;
        public bool damageEvents = true;
        public bool magnetEvents = true;
        public bool pathEvents = true;
        public bool forceFieldEvents = true;
        public bool respawnEvents = true;

        /// <summary>
        /// Apply this config to the GupDebug facade.
        /// Call this on game start or when config changes.
        /// </summary>
        public void Apply()
        {
            GupDebug.Enabled = enabled;
            GupDebug.MinLevel = minLevel;

            SetCategory(LogCategory.State, stateTransitions);
            SetCategory(LogCategory.Jump, jumpEvents);
            SetCategory(LogCategory.Damage, damageEvents);
            SetCategory(LogCategory.Magnet, magnetEvents);
            SetCategory(LogCategory.Path, pathEvents);
            SetCategory(LogCategory.ForceFields, forceFieldEvents);
            SetCategory(LogCategory.Respawn, respawnEvents);
        }

        private void SetCategory(LogCategory category, bool enabled)
        {
            if (enabled)
                GupDebug.EnableCategory(category);
            else
                GupDebug.DisableCategory(category);
        }

        private void OnEnable()
        {
            // Auto-apply when loaded in editor
            #if UNITY_EDITOR
            Apply();
            #endif
        }

        private void OnValidate()
        {
            // Re-apply when values change in inspector
            Apply();
        }
    }
}
