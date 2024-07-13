using System;
using System.Runtime.InteropServices;
using System.Threading;

public class MouseMover
{
    // 引入 user32.dll 中的 SetCursorPos 函数
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);

    // 引入 user32.dll 中的 GetSystemMetrics 函数
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    // 常量，用于 GetSystemMetrics 函数
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    public void MoveMouseInSpiral(double speed, double amplitude, double scaleFactor, CancellationToken token)
    {
        // 获取屏幕宽度和高度
        int screenWidth = GetSystemMetrics(SM_CXSCREEN);
        int screenHeight = GetSystemMetrics(SM_CYSCREEN);

        // 计算屏幕中心点
        int centerX = screenWidth / 2;
        int centerY = screenHeight / 2;

        double angle = 0;
        double radius = 0;

        while (!token.IsCancellationRequested)
        {
            while (radius < Math.Max(screenWidth, screenHeight) / 2)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("Mouse movement cancelled.");
                    return;
                }

                // 计算螺旋线的当前位置
                int x = centerX + (int)(radius * Math.Cos(angle) * scaleFactor);
                int y = centerY + (int)(radius * Math.Sin(angle) * scaleFactor * 0.6);

                // 如果鼠标超出屏幕范围，重置到中心点
                if (x < 0.1* screenWidth || x >=0.9* screenWidth || y < 0.2* screenHeight || y >=0.8* screenHeight)
                {
                    radius = 0;
                    angle = 0;
                    break;
                }

                // 设置鼠标位置
                SetCursorPos(x, y);

                // 更新角度和半径
                angle += speed;
                radius += amplitude;

                Thread.Sleep(10); // 控制移动速度
            }

            // 重置到中心点
            radius = 0;
            angle = 0;
        }
    }
}
