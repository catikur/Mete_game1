using MeteGame.Core;
using UnityEngine;

namespace MeteGame.Missions
{
    /// <summary>
    /// Görev süreleri: mesafe + tür + zorluk. Süre bitince görev BATMAZ;
    /// sadece o bacak için zamanında bonusu kaçar.
    /// </summary>
    public static class MissionClock
    {
        public static Color DifficultyColor(MissionDifficulty difficulty) =>
            difficulty switch
            {
                MissionDifficulty.Easy => new Color(0.45f, 0.90f, 0.50f),
                MissionDifficulty.Hard => new Color(1f, 0.48f, 0.38f),
                _ => GameConfig.Gold
            };

        public static MissionDifficulty RollDifficulty(System.Random rng, MissionType type, float totalDistance)
        {
            int easyMax;
            int mediumMax;
            switch (type)
            {
                case MissionType.TimedDelivery:
                    easyMax = 18;
                    mediumMax = 58;
                    break;
                case MissionType.SchoolRun:
                    easyMax = 26;
                    mediumMax = 72;
                    break;
                case MissionType.AnimalRescue:
                    easyMax = 55;
                    mediumMax = 88;
                    break;
                default:
                    easyMax = 42;
                    mediumMax = 82;
                    break;
            }

            int roll = rng.Next(100);
            if (totalDistance > 240f)
                roll += 16;
            else if (totalDistance < 130f)
                roll -= 12;
            roll = Mathf.Clamp(roll, 0, 99);

            if (roll < easyMax)
                return MissionDifficulty.Easy;
            if (roll < mediumMax)
                return MissionDifficulty.Medium;
            return MissionDifficulty.Hard;
        }

        public static int SecondsForLeg(float distance, MissionType type, MissionDifficulty difficulty)
        {
            float raw = distance / GameConfig.TimerCruiseSpeed + GameConfig.TimerLegBuffer;
            raw *= TypeMultiplier(type) * DifficultyMultiplier(difficulty);
            int rounded = Mathf.RoundToInt(raw / 5f) * 5;
            return Mathf.Max(GameConfig.MinLegSeconds, rounded);
        }

        public static string Format(float remainingSeconds)
        {
            int seconds = Mathf.Max(0, Mathf.CeilToInt(remainingSeconds));
            int minutes = seconds / 60;
            int part = seconds % 60;
            return minutes + ":" + part.ToString("00");
        }

        public static Color Tint(float remaining, float duration)
        {
            if (remaining <= 0f)
                return new Color(1f, 0.28f, 0.28f);

            float ratio = duration <= 0.01f ? 1f : remaining / duration;
            if (ratio > GameConfig.TimerSoonRatio)
                return new Color(0.42f, 0.92f, 0.52f);
            if (ratio > GameConfig.TimerUrgentRatio)
                return new Color(1f, 0.86f, 0.22f);
            return new Color(1f, 0.42f, 0.32f);
        }

        public static bool IsUrgent(float remaining, float duration)
        {
            if (remaining <= 0f)
                return true;
            float ratio = duration <= 0.01f ? 1f : remaining / duration;
            return ratio <= GameConfig.TimerUrgentRatio;
        }

        static float TypeMultiplier(MissionType type) =>
            type switch
            {
                MissionType.AnimalRescue => 1.10f,
                MissionType.SchoolRun => 0.92f,
                MissionType.TimedDelivery => 0.88f,
                _ => 1f
            };

        static float DifficultyMultiplier(MissionDifficulty difficulty) =>
            difficulty switch
            {
                MissionDifficulty.Easy => 1.25f,
                MissionDifficulty.Hard => 0.90f,
                _ => 1f
            };
    }
}
