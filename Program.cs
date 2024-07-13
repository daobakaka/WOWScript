using System;
using System.Activities;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace WowMove
{
    internal class Program
    {
        // 导入ShowWindow函数
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        private const int SW_MINIMIZE = 6;
        static void Main(string[] args)
        {
            Activity workflow1 = new Workflow1();
            WorkflowInvoker.Invoke(workflow1);
            Console.WriteLine("start to script");
            IntPtr hConsole = GetConsoleWindow();//获取窗口控件
            PlayerInfo playerInfo = PlayerInfo.GetInstance();

            Console.WriteLine("请确认是否进行采集调试（默认0）：");
            playerInfo.ifDebug = Console.ReadLine() == "1";

            if (playerInfo.ifDebug)
            {
                Console.WriteLine("请输入你的采集坐标（输入采集位置(0到1)，顺序依次为 左 右 下 上）：");
                for (int i = 0; i < 4; i++)
                {
                    playerInfo.Screen4[i] = float.Parse(Console.ReadLine());
                }
            }

            Console.WriteLine("是否采用默认配置？（默认1）：");
            bool useDefault = Console.ReadLine() == "1";

            if (!useDefault)
            {
                Console.WriteLine("请输入你的生命值(默认10000)：");
                playerInfo.playerHealth = int.Parse(Console.ReadLine());

                Console.WriteLine("请输入你的魔法值(默认10000)：");
                playerInfo.playerMana = int.Parse(Console.ReadLine());

                //Console.WriteLine("请输入你的核心技能编号（默认116）：");
                //playerInfo.skillNum = int.TryParse(Console.ReadLine(), out int skillNum) ? skillNum : 116;
                Console.WriteLine("请输入你的辅助技能施放间隔（毫秒默认30000）：");
                playerInfo.assistSkillTime = int.Parse(Console.ReadLine());
                Console.WriteLine("请输入你的BUFF技能施放间隔（毫秒默认600000）：");
                playerInfo.buffSkillTime = int.Parse(Console.ReadLine());
                Console.WriteLine("请输入你的保命技能开启时生命值（默认3000）：");
                playerInfo.protectHealth = int.Parse(Console.ReadLine());

                Console.WriteLine("请输入你的战场名称(大写英文首字母如战歌：ZGXG)：");
                playerInfo.battleAreas = Enum.TryParse<BattleAreas>(Console.ReadLine(), out var battleArea) ? battleArea : BattleAreas.ZGXG;
            }

            Console.WriteLine("是否开启战场模式？（默认 1）：");
            bool enableBattleMode = Console.ReadLine() == "1";
            bool enableMovement = enableBattleMode; // 初始化参数

            Console.WriteLine("配置完成，5秒后继续...");
            for (int i = 5; i > 0; i--)
            {
                Console.WriteLine(i);
                Thread.Sleep(1000);
            }

            Console.WriteLine("配置结果：");
            Console.WriteLine($"Player Health: {playerInfo.playerHealth}");
            Console.WriteLine($"Player Mana: {playerInfo.playerMana}");
            //Console.WriteLine($"Skill Number: {playerInfo.skillNum}");
            Console.WriteLine($"Battle Area: {playerInfo.battleAreas}");
            Console.WriteLine($"Debug Mode: {playerInfo.ifDebug}");
            Console.WriteLine($"Screen Coordinates: {string.Join(", ", playerInfo.Screen4)}");
            Console.WriteLine($"Battle Mode: {enableBattleMode}");
            Thread.Sleep(3000);
            //--------------------------
            ShowWindow(hConsole, SW_MINIMIZE);

            bool enableLogger = true;
            bool enableReader = true;
            bool enablePtr = false;
            bool movementDebug = true;

            // 启动日志记录线程
            WowInfoLogger logger = new WowInfoLogger(playerInfo.Screen4[0], playerInfo.Screen4[1], playerInfo.Screen4[2], playerInfo.Screen4[3]);
            Task loggerTask = Task.Run(() =>
            {
                if (enableLogger)
                {
                    try
                    {
                        Console.WriteLine("Logger started.");
                        logger.StartLogging();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error in logger: " + ex.Message);
                    }
                }
            });

            CircleAreaLogger circleArea = new CircleAreaLogger(enablePtr);
            Task circleAreaTask = Task.Run(() =>
            {
                if (enablePtr)
                {
                    try
                    {
                        Console.WriteLine("ptr has started");
                        circleArea.StartLogging();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error in ptr" + ex.Message);
                    }
                }
            });

            // 启动文件读取线程
            RealTimeFileReader fileReader = new RealTimeFileReader(logger);
            Task readerTask = Task.Run(() =>
            {
                if (enableReader)
                {
                    try
                    {
                        Console.WriteLine("File reader started.");
                        fileReader.StartReading();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error in file reader: " + ex.Message);
                    }
                }
            });

            Thread.Sleep(3000); // 延迟三秒执行
            MovementController movementController = new MovementController(enableMovement);
            // 启动移动逻辑线程
            Task moveTask = Task.Run(() =>
            {
                if (movementDebug)
                {
                    try
                    {
                        Console.WriteLine("Movement started.");
                        movementController.StartMonitor(fileReader);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error in movement: " + ex.Message);
                    }
                }
            });

            // 等待所有任务完成
            Task.WaitAll(loggerTask, circleAreaTask, readerTask, moveTask);

            Console.WriteLine("All tasks completed.");
        }
    }
}
