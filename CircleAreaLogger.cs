using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace WowMove
{
    public class CircleAreaLogger
    {
        private InputSimulator inputSimulator;
        private bool printCoordinates;

        public CircleAreaLogger(bool printCoordinates = false)
        {
            this.printCoordinates = printCoordinates;
            inputSimulator = new InputSimulator();
        }

        public void StartLogging()
        {
            while (true)
            {
                try
                {
                    if (printCoordinates)
                    {
                        // 获取当前鼠标位置
                        Point cursorPosition = Cursor.Position;

                        // 计算比例坐标
                        float screenWidth = Screen.PrimaryScreen.Bounds.Width;
                        float screenHeight = Screen.PrimaryScreen.Bounds.Height;
                        float normalizedX = cursorPosition.X / screenWidth;
                        float normalizedY = cursorPosition.Y / screenHeight;

                        // 打印比例坐标
                        Console.WriteLine($"Mouse Position: X = {normalizedX:F2}, Y = {normalizedY:F2}");
                    }

                    Thread.Sleep(10); // 更新间隔时间为10毫秒
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error during logging: " + ex.Message);
                }
            }
        }
        public void SimulateMouseClick(float normalizedX, float normalizedY)
        {
            if (normalizedX < 0 || normalizedX > 1 || normalizedY < 0 || normalizedY > 1)
            {
                throw new ArgumentException("Normalized coordinates must be between 0 and 1.");
            }

            float screenWidth = Screen.PrimaryScreen.Bounds.Width;
            float screenHeight = Screen.PrimaryScreen.Bounds.Height;
            int clickX = (int)(normalizedX * screenWidth);
            int clickY = (int)(normalizedY * screenHeight);

            inputSimulator.Mouse.MoveMouseTo(clickX, clickY);
            inputSimulator.Mouse.LeftButtonClick();

            Console.WriteLine($"Mouse clicked at: X = {normalizedX:F2}, Y = {normalizedY:F2}");
        }
    }
}
