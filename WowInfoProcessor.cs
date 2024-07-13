using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WowMove
{
    public class WowInfoProcessor
    {
        public int Level { get; private set; }
        public int Health { get; private set; }
        public int Mana { get; private set; }
        public (float X, float Y, float Z) Coordinates { get; private set; }
        public int TargetLevel { get; private set; }
        public int TargetHealth { get; private set; }
        public string TargetGUID { get; private set; }
        public string TargetAttackable { get; private set; }
        public string TargetDistance { get; private set; }
        public string TargetCanLoot { get; private set; }
        public DateTime Time { get; private set; }

        public void ProcessBuffer(string buffer)
        {
            var lines = buffer.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (line.Contains("Level: "))
                {
                    Level = ParseIntValue(line, "Level: ");
                }
                else if (line.Contains("Health: "))
                {
                    Health = ParseIntValue(line, "Health: ");
                }
                else if (line.Contains("Mana: "))
                {
                    Mana = ParseIntValue(line, "Mana: ");
                }
                else if (line.Contains("Coord: "))
                {
                    Coordinates = ParseCoordinates(line);
                }
                else if (line.Contains("Target Level: "))
                {
                    TargetLevel = ParseIntValue(line, "Target Level: ");
                }
                else if (line.Contains("Target Health: "))
                {
                    TargetHealth = ParseIntValue(line, "Target Health: ");
                }
                else if (line.Contains("Target GUID: "))
                {
                    TargetGUID = ParseStringValue(line, "Target GUID: ");
                }
                else if (line.Contains("Target Attackable: "))
                {
                    TargetAttackable = ParseStringValue(line, "Target Attackable: ");
                }
                else if (line.Contains("Target Distance: "))
                {
                    TargetDistance = ParseStringValue(line, "Target Distance: ");
                }
                else if (line.Contains("Target Can Loot: "))
                {
                    TargetCanLoot = ParseStringValue(line, "Target Can Loot: ");
                }
                else if (line.Contains("Time: "))
                {
                    Time = ParseDateTimeValue(line, "Time: ");
                }
            }
        }

        private int ParseIntValue(string line, string prefix)
        {
            var valueStr = line.Substring(line.IndexOf(prefix) + prefix.Length).Trim();
            return int.TryParse(valueStr, out int value) ? value : 0;
        }

        private (float X, float Y, float Z) ParseCoordinates(string line)
        {
            var coordsStr = line.Substring(line.IndexOf("Coord: ") + "Coord: ".Length).Trim();
            var coords = coordsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (coords.Length == 3 &&
                float.TryParse(coords[0], out float x) &&
                float.TryParse(coords[1], out float y) &&
                float.TryParse(coords[2], out float z))
            {
                return (x, y, z);
            }
            return (0, 0, 0);
        }

        private string ParseStringValue(string line, string prefix)
        {
            return line.Substring(line.IndexOf(prefix) + prefix.Length).Trim();
        }

        private DateTime ParseDateTimeValue(string line, string prefix)
        {
            var dateTimeStr = line.Substring(line.IndexOf(prefix) + prefix.Length).Trim();
            return DateTime.TryParse(dateTimeStr, out DateTime dateTime) ? dateTime : DateTime.MinValue;
        }
    }
}
