using System.Diagnostics;
using UnityEngine;

namespace GUP.Core.Debug
{
    /// <summary>
    /// Log categories for filtering debug output.
    /// </summary>
    public enum LogCategory
    {
        State,       // State transitions
        Jump,        // Jump buffering/coyote
        Damage,      // Damage taken/dealt
        Magnet,      // Magnetic attach/detach
        Path,        // Path following
        ForceFields, // Force field enter/exit
        Respawn      // Death and respawn
    }

    /// <summary>
    /// Log severity levels.
    /// </summary>
    public enum LogLevel
    {
        Error,
        Warn,
        Info,
        Verbose
    }

    /// <summary>
    /// Debug logging facade with conditional compilation.
    /// All methods are no-ops unless GUP_DEBUG symbol is defined.
    /// </summary>
    public static class GupDebug
    {
        // Runtime toggle (can be set from GupDebugConfig or Inspector)
        private static bool _enabled = true;
        private static LogLevel _minLevel = LogLevel.Info;
        private static int _categoryMask = ~0; // All categories enabled by default

        /// <summary>Enable or disable all debug logging at runtime.</summary>
        public static bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        /// <summary>Minimum log level to display.</summary>
        public static LogLevel MinLevel
        {
            get => _minLevel;
            set => _minLevel = value;
        }

        /// <summary>Enable a specific category.</summary>
        [Conditional("GUP_DEBUG")]
        public static void EnableCategory(LogCategory category)
        {
            _categoryMask |= (1 << (int)category);
        }

        /// <summary>Disable a specific category.</summary>
        [Conditional("GUP_DEBUG")]
        public static void DisableCategory(LogCategory category)
        {
            _categoryMask &= ~(1 << (int)category);
        }

        /// <summary>Check if a category is enabled.</summary>
        public static bool IsCategoryEnabled(LogCategory category)
        {
            return (_categoryMask & (1 << (int)category)) != 0;
        }

        /// <summary>
        /// General purpose log with category and level.
        /// </summary>
        [Conditional("GUP_DEBUG")]
        public static void Log(LogCategory category, string message, LogLevel level = LogLevel.Info)
        {
            if (!_enabled) return;
            if (level < _minLevel) return;
            if (!IsCategoryEnabled(category)) return;

            string prefix = $"[{category}]";
            switch (level)
            {
                case LogLevel.Error:
                    UnityEngine.Debug.LogError($"{prefix} {message}");
                    break;
                case LogLevel.Warn:
                    UnityEngine.Debug.LogWarning($"{prefix} {message}");
                    break;
                default:
                    UnityEngine.Debug.Log($"{prefix} {message}");
                    break;
            }
        }

        // ==================== STATE TRANSITIONS ====================

        /// <summary>Log a state transition.</summary>
        [Conditional("GUP_DEBUG")]
        public static void LogStateTransition(string from, string to, string reason = null)
        {
            string msg = $"State: {from} → {to}";
            if (!string.IsNullOrEmpty(reason))
            {
                msg += $" (reason: {reason})";
            }
            Log(LogCategory.State, msg);
        }

        // ==================== JUMP EVENTS ====================

        /// <summary>Log jump request.</summary>
        [Conditional("GUP_DEBUG")]
        public static void LogJumpRequested(float timeSinceRequest, float coyoteTime, bool isGrounded, bool isStable)
        {
            Log(LogCategory.Jump, 
                $"Jump Requested | timeSinceRequest={timeSinceRequest:F3} | coyote={coyoteTime:F3} | grounded={isGrounded} | stable={isStable}");
        }

        /// <summary>Log jump consumed (successful jump).</summary>
        [Conditional("GUP_DEBUG")]
        public static void LogJumpConsumed(float jumpForce, Vector3 direction)
        {
            Log(LogCategory.Jump, $"Jump Consumed | force={jumpForce:F2} | dir={direction}");
        }

        /// <summary>Log jump rejected with reason.</summary>
        [Conditional("GUP_DEBUG")]
        public static void LogJumpRejected(string reason, float timeSinceRequest, float coyoteTime, bool isGrounded)
        {
            Log(LogCategory.Jump, 
                $"Jump Rejected | reason={reason} | timeSinceRequest={timeSinceRequest:F3} | coyote={coyoteTime:F3} | grounded={isGrounded}",
                LogLevel.Verbose);
        }

        // ==================== DAMAGE EVENTS ====================

        /// <summary>Log damage taken.</summary>
        [Conditional("GUP_DEBUG")]
        public static void LogDamageTaken(string target, float amount, float remainingHP, string source = null)
        {
            string msg = $"{target} took {amount:F1} damage | HP={remainingHP:F1}";
            if (!string.IsNullOrEmpty(source))
            {
                msg += $" | source={source}";
            }
            Log(LogCategory.Damage, msg);
        }

        /// <summary>Log entity death.</summary>
        [Conditional("GUP_DEBUG")]
        public static void LogDeath(string entity, string cause = null)
        {
            string msg = $"{entity} died";
            if (!string.IsNullOrEmpty(cause))
            {
                msg += $" | cause={cause}";
            }
            Log(LogCategory.Damage, msg, LogLevel.Warn);
        }

        // ==================== MAGNET EVENTS ====================

        /// <summary>Log magnet attach.</summary>
        [Conditional("GUP_DEBUG")]
        public static void LogMagnetAttach(string entity, Vector3 surfaceNormal)
        {
            Log(LogCategory.Magnet, $"{entity} attached to magnetic surface | normal={surfaceNormal}");
        }

        /// <summary>Log magnet detach.</summary>
        [Conditional("GUP_DEBUG")]
        public static void LogMagnetDetach(string entity, string reason = null)
        {
            string msg = $"{entity} detached from magnetic surface";
            if (!string.IsNullOrEmpty(reason))
            {
                msg += $" | reason={reason}";
            }
            Log(LogCategory.Magnet, msg);
        }

        // ==================== PATH EVENTS ====================

        /// <summary>Log path follow start.</summary>
        [Conditional("GUP_DEBUG")]
        public static void LogPathStart(string entity, string pathName)
        {
            Log(LogCategory.Path, $"{entity} started following path: {pathName}");
        }

        /// <summary>Log path follow progress (throttled, e.g., every 10%).</summary>
        [Conditional("GUP_DEBUG")]
        public static void LogPathProgress(string entity, float progress)
        {
            Log(LogCategory.Path, $"{entity} path progress: {progress:P0}", LogLevel.Verbose);
        }

        /// <summary>Log path follow complete.</summary>
        [Conditional("GUP_DEBUG")]
        public static void LogPathComplete(string entity)
        {
            Log(LogCategory.Path, $"{entity} completed path");
        }

        // ==================== RESPAWN EVENTS ====================

        /// <summary>Log respawn.</summary>
        [Conditional("GUP_DEBUG")]
        public static void LogRespawn(string entity, Vector3 position, string checkpointName = null)
        {
            string msg = $"{entity} respawned at {position}";
            if (!string.IsNullOrEmpty(checkpointName))
            {
                msg += $" | checkpoint={checkpointName}";
            }
            Log(LogCategory.Respawn, msg);
        }
    }
}
