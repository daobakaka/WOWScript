using System;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace WowMove
{public enum SkillAssist 
    {assit,buff,protect
    
    
    }
    public enum BattleStatus
    {
        none, queued, ready, active, complete, NA,ohher,death


    }
    public enum BattleAreas {ZGXG,ALXPD,FBZY,ATLKSG,YGHT,NA }
    public class MovementController
    {
        private InputSimulator inputSimulator;
        private RealTimeFileReader fileReader;
        private Random random;
        private CancellationTokenSource movementCts;
        private CancellationTokenSource attackCts;
        private CancellationTokenSource rotateAttackCts;
        private bool ifrotate;
        private CancellationTokenSource monitorCts;
        private CancellationTokenSource recoverCts;
        private CancellationTokenSource xrecoverCts;
        private bool ifrecover;
        private bool ifXrecover;
        private bool ifBuffRecover;
        private bool ifProtectRecover;
        //module of monitor
        private int health;
        private int myMana;
        private bool startBattleMode;
        private bool battleNA;
        private bool battleQueued;
        private bool battleReady;
        private bool ifDead;
        private bool ifXR;
        //judge to quei war
        private int judgeNumForQuit;
        private int judgeNumMana;
        private int judgeNumManaADD;
        private bool ifjudge;
        //info par pass

        public MovementController(bool startBattleMode)
        {
            inputSimulator = new InputSimulator();
            random = new Random();
            movementCts = new CancellationTokenSource();
            attackCts = new CancellationTokenSource();
            rotateAttackCts = new CancellationTokenSource();
            recoverCts = new CancellationTokenSource();
            xrecoverCts = new CancellationTokenSource();
            ifrotate = true;
            ifrecover = true;
            ifXrecover = true;
            ifBuffRecover = true;
            ifDead = false;
            ifXR=false;
            ifjudge = true;
            ifProtectRecover=true;
            this.startBattleMode = startBattleMode;
            battleNA = true;
            //monitorCts = new CancellationTokenSource();
        }

        public void StartMonitor(RealTimeFileReader fileReader)
        {
            this.fileReader = fileReader;
            StopAllTasks();
            rotateAttackCts = new CancellationTokenSource();
            recoverCts=new CancellationTokenSource();
            xrecoverCts=new CancellationTokenSource();
            health = fileReader.Health;
            myMana = fileReader.MyMana;
        
                health = PlayerInfo.GetInstance().playerHealth;
        
                myMana = PlayerInfo.GetInstance().playerMana;
            Console.WriteLine("start to monitor"+health+"---"+myMana);
            monitorCts = new CancellationTokenSource();
            _ = MonitorConditionsAsync(monitorCts.Token);
      
        }

        public void StopAllTasks()
        {
            monitorCts?.Cancel();
            movementCts?.Cancel();
            attackCts?.Cancel();
            rotateAttackCts?.Cancel();
            recoverCts?.Cancel();
            xrecoverCts?.Cancel();
            Console.WriteLine("All tasks stopped.");
        }

        private async Task MonitorConditionsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!fileReader.InCombat && ifrecover && (fileReader.Health < health * 0.7f || fileReader.MyMana < myMana * 0.7f))
                {
                   Console.WriteLine("enter recover");
                    StartRecover();
                    ifrecover = false;
                }

                else if (fileReader.TargetAttackable && fileReader.TargetHealth > 0 && fileReader.Distance <= 35&& !fileReader.IsDead)
                {
                    //Console.WriteLine("enter attack");
                    if (attackCts.IsCancellationRequested)
                            StartAttack();
                }
             else
            {
                   // Console.WriteLine("enter move");
                    if (movementCts.IsCancellationRequested&&!fileReader.IsDead)
                    StartMovement();

            }
            await Task.Delay(500, token);
                if (ifXrecover)
                {
                    ifXrecover=false;
                    _ = OtherProtectKey(PlayerInfo.GetInstance().assistSkillTime, SkillAssist.assit);//辅助技能
                }
                if (ifBuffRecover)
                {
                    ifBuffRecover=false;
                    _ = OtherProtectKey(PlayerInfo.GetInstance().buffSkillTime, SkillAssist.buff);//BUFF技能
                }

                if (ifProtectRecover && (fileReader.Health > 1 && fileReader.Health <= PlayerInfo.GetInstance().protectHealth))
                {
                    ifProtectRecover=false;
                    _ = OtherProtectKey(10000, SkillAssist.protect);//保命技能，默认10秒间隔
                }
                //-----battle mode
                if (startBattleMode)
                { if (fileReader.BattlefieldStatus == "NA"&&battleNA==true&&battleQueued==false&&!fileReader.IsDead)
                        _ = PerformEnterWar(BattleStatus.NA,PlayerInfo.GetInstance().battleAreas);//选择战场
                  if(fileReader.BattlefieldStatus== "queued")
                        battleQueued = true;
                  else
                        battleQueued = false;
                  if(fileReader.BattlefieldStatus=="ready")
                        _=PerformEnterWar(BattleStatus.ready,BattleAreas.NA);
                    if (fileReader.BattlefieldStatus == "complete")
                        _ = PerformEnterWar(BattleStatus.complete, BattleAreas.NA);
                    if (fileReader.BattlefieldStatus == "active" && !fileReader.IsDead)
                    {
                        //if (ifjudge)
                        //{ judgeNumMana = fileReader.MyMana;
                        //    ifjudge = false;
                        //}
                        //judgeNumManaADD++;
                        //if (judgeNumManaADD > 8)
                        //{   ifjudge = true;
                        //    judgeNumManaADD = 0;
                        //    if (judgeNumMana == fileReader.MyMana)
                        //        judgeNumForQuit++;
                        //    else
                        //        judgeNumForQuit = 0;

                        //}
                        if (ifjudge)
                        {
                            _ = PerformEnterWar(BattleStatus.active, BattleAreas.NA);
                            ifjudge= false;
                        }
                    }
                }
                if (fileReader.IsDead && !ifDead&&fileReader.Health==0)
                {
                    _ = PerformEnterWar(BattleStatus.death, BattleAreas.NA);
                    ifDead = true;
                }
                if (fileReader.IsDead && !ifXR && fileReader.BattlefieldStatus != "active"&&fileReader.Health==1)//虚弱复活逻辑
                {
                    _ = PerformEnterWar(BattleStatus.ohher, BattleAreas.NA);
                    ifXR = true;
                }
            }
        }
        private void StartMovement()
        {
           
            attackCts?.Cancel(); // 取消攻击任务
            movementCts = new CancellationTokenSource();
            _ = PerformMovementLoop(movementCts.Token);
        }

        private void StartAttack()  
        {
            
            movementCts?.Cancel(); // 取消移动任务
            attackCts = new CancellationTokenSource();
            _ = StopMovementAndAttackTarget(attackCts.Token);
        }

        private void StartRotateAttack()
        {
              movementCts?.Cancel();
            _ = PerformRotateAndAttack(rotateAttackCts.Token,false);
        }

        private async Task OtherProtectKey(int time, SkillAssist skillAssist)
        {

            switch (skillAssist)
            {
                case SkillAssist.assit:
                   
                        PressKey(VirtualKeyCode.VK_2);
                        await Task.Delay(3000);
                        PressKey(VirtualKeyCode.VK_3);
                        await Task.Delay(3000);
                    
                    await Task.Delay(time);
                    ifXrecover = true;
                    break;
            
             case SkillAssist.buff:
                    {
                        
                           
                            PressKey(VirtualKeyCode.VK_4);
                            await Task.Delay(1800);
                            PressKey(VirtualKeyCode.VK_4);
                            await Task.Delay(1800);
                            PressKey(VirtualKeyCode.VK_4);
                        
                        await Task.Delay(time);
                        ifBuffRecover = true;
                        break;
                }
                case SkillAssist.protect:
                    {
                        
                          
                            PressKey(VirtualKeyCode.VK_6);
                            await Task.Delay(2000);
                            PressKey(VirtualKeyCode.VK_7);
                            await Task.Delay(2000);
                            PressKey(VirtualKeyCode.VK_8);
                        
                        await Task.Delay(time);
                        ifProtectRecover = true;
                        break;
                    }


            }
        }
        private void StartRecover()
        {
            Console.WriteLine("health:" + fileReader.Health + "..myMana:" + fileReader.MyMana);

           _= Recocer(recoverCts.Token);
        
        }
        private void StartXRecover()
        {
            Console.WriteLine("health:" + fileReader.Health + "..myMana:" + fileReader.MyMana);

            _ = XRecocer(xrecoverCts.Token);

        }

        private async Task PerformMovementLoop(CancellationToken token)
        {
            int loopCount = 0;
            Console.WriteLine("start to move");
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (ifrecover)
                    {
                      //  Console.WriteLine("innermove");
                        var randomfix =0.6+ 0.4*random.NextDouble();
                        if (random.NextDouble() > 0.8d)
                            PressKey(VirtualKeyCode.SPACE);
                        //  PressKey(VirtualKeyCode.END);
                        await Task.Delay(1000, token);
                        if(fileReader.TargetHealth==0||fileReader.Distance>35)//在没有目标的情况下使用搜寻
                        PressKey(VirtualKeyCode.TAB);
                        await Task.Delay(1000, token);
                        if (fileReader.Distance > 35 && fileReader.TargetHealth > 0 && fileReader.TargetAttackable)
                            StartRotateAttack();
                        await Task.Delay(4000, token);
                       // await HoldKeyAsync(VirtualKeyCode.VK_A, (int)Math.Floor(randomfix*500), token);
                        await HoldKeyAsync(VirtualKeyCode.VK_A,  490, token);
                        loopCount++;
                        await Task.Delay(500, token);
                        if (loopCount % 4 == 0)
                        {  if (fileReader.BattlefieldStatus != "active")
                                await HoldKeyAsync(VirtualKeyCode.VK_W, 500, token);
                            else
                                await HoldKeyAsync(VirtualKeyCode.VK_W, 10000, token);                
                        }
                    }
                    await Task.Delay(1000, token);
                    Console.WriteLine("move..." + ifrecover);
                }
            }
            catch (OperationCanceledException)
            {
                // Handle the cancellation if needed
                Console.WriteLine("Movement loop cancelled");
            }
        }
        private async Task Recocer(CancellationToken token)
        {
            
            Console.WriteLine("start to recover");
            try
            {
                
                    PressKey(VirtualKeyCode.VK_X);
                    PressKey(VirtualKeyCode.VK_9);
                    PressKey(VirtualKeyCode.VK_0);
                    await Task.Delay(15000, token);
                    ifrecover = true;   
                
            }
            catch (OperationCanceledException)
            {
                // Handle the cancellation if needed
                Console.WriteLine("Recover loop cancelled");
            }
        }
        private async Task XRecocer(CancellationToken token)
        {

            Console.WriteLine("start to Xrecover");
            try
            {

                PressKey(VirtualKeyCode.VK_X);
                await Task.Delay(10000, token);
                ifrecover = true;

            }
            catch (OperationCanceledException)
            {
                // Handle the cancellation if needed
                Console.WriteLine("Xrecover loop cancelled");
            }
        }
        private async Task StopMovementAndAttackTarget(CancellationToken token)
        {
            Console.WriteLine("start to attack");
            try
            {
                int attackNum = 0;
                while (!token.IsCancellationRequested&&fileReader.TargetHealth>0&&fileReader.TargetAttackable&&!fileReader.IsDead)
                {
                    attackNum++;
                    PressKey(VirtualKeyCode.VK_1);
                    if ((!fileReader.FacingTarget && ifrotate&&!fileReader.IsCasting)||fileReader.Distance>35)
                    {
                        StartRotateAttack();
                        ifrotate = false;

                    }
                    await Task.Delay(1000, token);
                   // Console.WriteLine("has attack num:!!!!!!!!!!!!!!!" + attackNum);
                }
            }
            catch (OperationCanceledException)
            {
                // Handle the cancellation if needed
                Console.WriteLine("Attack loop cancelled");
            }
        }

        private async Task PerformRotateAndAttack(CancellationToken token,bool useMouse)
        {
            Console.WriteLine("start to rotate attack");
            if (useMouse)
            {
                try
                {
                    int screenWidth = Screen.PrimaryScreen.Bounds.Width;
                    int screenHeight = Screen.PrimaryScreen.Bounds.Height;

                    double randomFactor = random.NextDouble() * 0.05 ; // 随机偏移系数在0.1到0.2之间
                    int offsetX = (int)(screenWidth * randomFactor); // X轴偏移
                    int offsetY = (int)(screenHeight * randomFactor); // Y轴偏移

                    int targetX = (int)(screenWidth * 0.5) + offsetX; // 目标X坐标，假设点击区域中心在屏幕中间
                    int targetY = (int)(screenHeight * 0.7) + offsetY; // 目标Y坐标，假设点击区域中心在屏幕中间

                    // 将鼠标移动到目标位置并点击
                    inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop((double)targetX / screenWidth * 65535, (double)targetY / screenHeight * 65535);
                    inputSimulator.Mouse.RightButtonClick();
                    Console.WriteLine($"Clicked at position ({targetX}, {targetY})");
                    await Task.Delay(1000, token);
                    ifrotate = true;
                }
                catch (OperationCanceledException)
                {
                    // Handle the cancellation if needed
                    Console.WriteLine("Rotate attack loop cancelled");
                }
            }
            else
            {
                try
                {

                    PressKey(VirtualKeyCode.VK_O);
                    await Task.Delay(500, token);
                    PressKey(VirtualKeyCode.VK_S);
                    await Task.Delay(4000, token);
                    ifrotate = true;
                }
                catch (OperationCanceledException)
                {
                    // Handle the cancellation if needed
                    Console.WriteLine("Rotate attack loop cancelled");
                }
            }
        }
        private async Task PerformEnterWar(BattleStatus status,BattleAreas areas)
        {
            switch (status)
            {
                case BattleStatus.NA:
                    {
                        if (areas ==BattleAreas.ZGXG)
                        {
                            battleNA = false;
                            PressKey(VirtualKeyCode.VK_5);
                            await Task.Delay(2000);
                            await Task.Delay(1000);
                            PressKey((VirtualKeyCode.VK_H));
                            await Task.Delay(1000);
                            SimulateMouseClick(0.80f, 0.84f, true);
                            await Task.Delay(2000);
                            SimulateMouseClick(0.85f, 0.435f, true);
                            await Task.Delay(2000);
                            SimulateMouseClick(0.85f, 0.435f, true);
                            await Task.Delay(3000);
                            SimulateMouseClick(0.86f, 0.81f, true);
                            await Task.Delay(1000);
                            PressKey((VirtualKeyCode.VK_H));
                            await Task.Delay(10000);
                            battleNA = true;
                        }
                      else  if (areas == BattleAreas.ALXPD)
                        {
                            battleNA = false;
                            PressKey(VirtualKeyCode.VK_5);
                            await Task.Delay(2000);
                            await Task.Delay(1000);
                            PressKey((VirtualKeyCode.VK_H));
                            await Task.Delay(1000);
                            SimulateMouseClick(0.80f, 0.84f, true);
                            await Task.Delay(2000);
                            SimulateMouseClick(0.85f, 0.45f, true);
                            await Task.Delay(2000);
                            SimulateMouseClick(0.85f, 0.45f, true);
                            await Task.Delay(3000);
                            SimulateMouseClick(0.86f, 0.81f, true);
                            await Task.Delay(1000);
                            PressKey((VirtualKeyCode.VK_H));
                            await Task.Delay(10000);
                            battleNA = true;

                        }
                        else if (areas == BattleAreas.ATLKSG)
                        {
                            battleNA = false;
                            PressKey(VirtualKeyCode.VK_5);
                            await Task.Delay(2000);
                            await Task.Delay(1000);
                            PressKey((VirtualKeyCode.VK_H));
                            await Task.Delay(1000);
                            SimulateMouseClick(0.80f, 0.84f, true);
                            await Task.Delay(2000);
                            SimulateMouseClick(0.85f, 0.47f, true);
                            await Task.Delay(2000);
                            SimulateMouseClick(0.85f, 0.47f, true);
                            await Task.Delay(3000);
                            SimulateMouseClick(0.86f, 0.81f, true);
                            await Task.Delay(1000);
                            PressKey((VirtualKeyCode.VK_H));
                            await Task.Delay(10000);
                            battleNA = true;

                        }
                        else if (areas == BattleAreas.FBZY)
                        {
                            battleNA = false;
                            PressKey(VirtualKeyCode.VK_5);
                            await Task.Delay(2000);
                            await Task.Delay(1000);
                            PressKey((VirtualKeyCode.VK_H));
                            await Task.Delay(1000);
                            SimulateMouseClick(0.80f, 0.84f, true);
                            await Task.Delay(2000);
                            SimulateMouseClick(0.85f, 0.49f, true);
                            await Task.Delay(2000);
                            SimulateMouseClick(0.85f, 0.49f, true);
                            await Task.Delay(3000);
                            SimulateMouseClick(0.86f, 0.81f, true);
                            await Task.Delay(1000);
                            PressKey((VirtualKeyCode.VK_H));
                            await Task.Delay(10000);
                            battleNA = true;

                        }
                        else if (areas == BattleAreas.YGHT)
                        {
                            battleNA = false;
                            PressKey(VirtualKeyCode.VK_5);
                            await Task.Delay(2000);
                            await Task.Delay(1000);
                            PressKey((VirtualKeyCode.VK_H));
                            await Task.Delay(1000);
                            SimulateMouseClick(0.80f, 0.84f, true);
                            await Task.Delay(2000);
                            SimulateMouseClick(0.85f, 0.51f, true);
                            await Task.Delay(2000);
                            SimulateMouseClick(0.85f, 0.51f, true);
                            await Task.Delay(3000);
                            SimulateMouseClick(0.86f, 0.81f, true);
                            await Task.Delay(1000);
                            PressKey((VirtualKeyCode.VK_H));
                            await Task.Delay(10000);
                            battleNA = true;

                        }
                        break;
                    } 
                case BattleStatus.ready:
                    {
                        await Task.Delay(1000);
                        SimulateMouseClick(0.45f, 0.23f, true);
                        movementCts?.Cancel();
                        break;
                    }
                case BattleStatus.complete:
                    {
                        await Task.Delay(1000);
                        SimulateMouseClick(0.50f, 0.68f, true);
                        break;
                    }
                case BattleStatus.active://退出战场
                    {
                        await Task.Delay(3000);
                        
                        if (fileReader.BattlefieldStatus == "active" )
                            SimulateMouseClick(0.50f, 0.68f, true);
                        await Task.Delay(30000);
                        ifjudge=true;
                        break;
                    }
                case BattleStatus.death:
                    {
                        await Task.Delay(10000);
                        if(fileReader.Health==0)//死亡状态才释放
                             SimulateMouseClick(0.50f, 0.22f, true);
                        if (fileReader.BattlefieldStatus != "active")
                        {
                           
                            await Task.Delay(10000);
                            ifDead = false;
                        }
                        else
                        {
                            await Task.Delay(35000);
                            ifDead = false;

                        }
                        break;
                    }
                case BattleStatus.ohher:
                    {
                        await Task.Delay(5000);
                        SimulateMouseClick(0.50f, 0.25f, false);
                        await Task.Delay(2000);
                        SimulateMouseClick(0.50f, 0.25f, false);
                        await Task.Delay(2000);
                        SimulateMouseClick(0.10f, 0.30f, true);
                        await Task.Delay(1000);
                        SimulateMouseClick(0.46f, 0.29f, true);
                        await Task.Delay(1000);
                        SimulateMouseClick(0.46f, 0.29f,true);
                        ifXR = false;
                        break;
                    
                    }
            }
        } 
        private void SimulateMouseClick(float normalizedX, float normalizedY,bool ifleft)
        {
            if (normalizedX < 0 || normalizedX > 1 || normalizedY < 0 || normalizedY > 1)
            {
                throw new ArgumentException("Normalized coordinates must be between 0 and 1.");
            }


            //float screenWidth = Screen.PrimaryScreen.Bounds.Width;
            //float screenHeight = Screen.PrimaryScreen.Bounds.Height;
            float screenWidth = 65335;
            float screenHeight = 65335;
            float clickX = (float)(normalizedX * screenWidth);
            float clickY = (float)(normalizedY * screenHeight);

            inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(clickX, clickY);
            if(ifleft)
            inputSimulator.Mouse.LeftButtonClick();
            else
                inputSimulator.Mouse.RightButtonClick();
            Console.WriteLine($"Mouse clicked at: X = {normalizedX:F2}a{clickX}b{screenWidth}, Y = {normalizedY:F2}a{clickY}b{screenHeight}");
        }

        private void PressKey(VirtualKeyCode key)
        {
            inputSimulator.Keyboard.KeyPress(key);
            Console.WriteLine("Pressed key: " + key);
        }

        private async Task HoldKeyAsync(VirtualKeyCode key, int duration, CancellationToken token)
        {
            inputSimulator.Keyboard.KeyDown(key);
            try
            {
                await Task.Delay(duration, token);
            }
            catch (OperationCanceledException)
            {
                // Handle the cancellation if needed
            }
            inputSimulator.Keyboard.KeyUp(key);
            Console.WriteLine("Held key: " + key + " for " + duration + "ms");
        }
    }
}