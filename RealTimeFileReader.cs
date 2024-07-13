using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WowMove
{
    public class RealTimeFileReader
    {
        private WowInfoLogger logger;
        private bool iflog = false   ;

        public int Health { get; private set; }
        public int MyMana { get; private set; }
        public (float X, float Y, float Z) Coordinates { get; private set; }
        public int TargetHealth { get; private set; }
        public bool TargetAttackable { get; private set; }
        public int Distance { get; private set; }
        public bool TargetCanLoot { get; private set; }
        public DateTime Time { get; private set; }
        public bool InCombat { get; private set; }
        public bool FacingTarget { get; set; }
        public bool IsCasting { get; private set; }
        public bool IsDead { get; private set; } // 新增的属性
        public string BattlefieldStatus { get; private set; } // 新增的属性
        public string Debuff { get; private set; }//手动新增属性

        public string CusorStr { get; set; }

        public RealTimeFileReader(WowInfoLogger logger)
        {
            this.logger = logger;
            iflog=PlayerInfo.GetInstance().ifDebug;
        }

        public void StartReading()
        {
            while (true)
            {
                string content = logger.ReadBuffer();
                if (!string.IsNullOrEmpty(content))
                {
                    ProcessBuffer(content);
                    PrintProcessedInfo();
                }
                Thread.Sleep(100);
            }
        }

        private void ProcessBuffer(string buffer)
        {
            var lines = buffer.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (line.Contains("Health: ") && !line.Contains("Target Health:"))
                {
                    Health = ParseIntValue(line, "Health: ");
                }
                else if (line.Contains("MyMana: "))
                {
                    MyMana = ParseIntValue(line, "MyMana: ");
                }
                else if (line.Contains("Coord: "))
                {
                    Coordinates = ParseCoordinates(line);
                }
                else if (line.Contains("Target Health: "))
                {
                    TargetHealth = ParseIntValue(line, "Target Health: ");
                }
                else if (line.Contains("Target Attackable: "))
                {
                    TargetAttackable = ParseBoolValue(line, "Target Attackable: ");
                }
                else if (line.Contains("Distance: "))
                {
                    Distance = ParseIntValue(line, "Distance: ");
                }
                else if (line.Contains("Target Can Loot: "))
                {
                    TargetCanLoot = ParseBoolValue(line, "Target Can Loot: ");
                }
                else if (line.Contains("In Combat: "))
                {
                    InCombat = ParseBoolValue(line, "In Combat: ");
                }
                else if (line.Contains("Facing Target: "))
                {
                    FacingTarget = ParseBoolValue(line, "Facing Target: ");
                }
                else if (line.Contains("Is Casting: "))
                {
                    IsCasting = ParseBoolValue(line, "Is Casting: ");
                }
                else if (line.Contains("Is Dead: "))
                {
                    IsDead = ParseBoolValue(line, "Is Dead: ");
                }
                else if (line.Contains("Battle: "))
                {
                    BattlefieldStatus = ParseStringValue(line, "Battle: ");
                }
                else if (line.Contains("Time: "))
                {
                    Time = ParseDateTimeValue(line, "Time: ");
                }
                else if (line.Contains("Debuffs: "))
                {

                    Debuff = ParseStringValue(line, "Debuffs: ");
                }
                else
                {
                    CusorStr = line;
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
            var valueStr = line.Substring(line.IndexOf(prefix) + prefix.Length).Trim();
            return valueStr;
        }

        private bool ParseBoolValue(string line, string prefix)
        {
            var valueStr = line.Substring(line.IndexOf(prefix) + prefix.Length).Trim();
            return valueStr.Equals("true", StringComparison.OrdinalIgnoreCase) || valueStr.Equals("Yes", StringComparison.OrdinalIgnoreCase);
        }

        private DateTime ParseDateTimeValue(string line, string prefix)
        {
            var dateTimeStr = line.Substring(line.IndexOf(prefix) + prefix.Length).Trim();
            return DateTime.TryParse(dateTimeStr, out DateTime dateTime) ? dateTime : DateTime.MinValue;
        }

        private void PrintProcessedInfo()
        {
            if (iflog)
            {
                Console.WriteLine("read Health: " + Health);
                Console.WriteLine("read MyMana: " + MyMana);
                Console.WriteLine("read Coordinates: " + Coordinates.X + ", " + Coordinates.Y + ", " + Coordinates.Z);
                Console.WriteLine("read Target Health: " + TargetHealth);
                Console.WriteLine("read Target Attackable: " + TargetAttackable);
                Console.WriteLine("read Distance: " + Distance);
                Console.WriteLine("read Target Can Loot: " + TargetCanLoot);
                Console.WriteLine("read In Combat: " + InCombat);
                Console.WriteLine("read Facing Target: " + FacingTarget);  // 输出是否面向目标
                Console.WriteLine("read Is Casting: " + IsCasting);  // 输出是否在施法
                Console.WriteLine("read Is Dead: " + IsDead);  // 输出玩家是否死亡
                Console.WriteLine("read Battlefield Status: " + BattlefieldStatus);  // 输出战场状态
                Console.WriteLine("read Debuffs: " + Debuff);  // 输出Debuffs
                Console.WriteLine("read Time: " + Time);
            }
        }
    }
}
