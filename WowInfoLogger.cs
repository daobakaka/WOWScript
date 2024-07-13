using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Tesseract;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WowMove
{
    public class WowInfoLogger
    {
        private Rectangle bounds;
        private TesseractEngine ocrEngine;
        private StringBuilder logBuffer;
        private int lastReadPosition;
        private bool drawingBounds;

        public WowInfoLogger(float x1, float x2, float y1, float y2)
        {
            Console.WriteLine("Initializing WowInfoLogger...");
            SetBounds(x1, x2, y1, y2);

            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string tessdataPath = Path.Combine(currentDirectory, "tessdata");
                Console.WriteLine($"Current Directory: {currentDirectory}");
                Console.WriteLine($"Tessdata Path: {tessdataPath}");

                if (!Directory.Exists(tessdataPath))
                {
                    throw new DirectoryNotFoundException($"tessdata directory not found at: {tessdataPath}");
                }
                else
                {
                    Console.WriteLine($"tessdata directory exists at: {tessdataPath}");
                }

                // 检查语言数据文件是否存在
                string engDataPath = Path.Combine(tessdataPath, "eng.traineddata");
                if (!File.Exists(engDataPath))
                {
                    throw new FileNotFoundException("Language data file not found at: " + engDataPath);
                }

                // 设置 TESSDATA_PREFIX 环境变量
                Environment.SetEnvironmentVariable("TESSDATA_PREFIX", tessdataPath);

                ocrEngine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default);
                Console.WriteLine("TesseractEngine initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error initializing TesseractEngine: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
            }

            logBuffer = new StringBuilder();
            lastReadPosition = 0;

            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WowInfoLog.txt");
            try
            {
                if (File.Exists(logFilePath))
                {
                    File.Delete(logFilePath);
                    Console.WriteLine("Deleted existing log file.");
                }
                File.Create(logFilePath).Close();
                Console.WriteLine("Created new log file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error handling log file: " + ex.Message);
            }

            drawingBounds = true;
            _ = DrawRecognitionBoundsContinuously(); //绘制边框
        }


        public void SetBounds(float x1, float x2, float y1, float y2)
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            // 确保截图区域在屏幕范围内
            bounds = new Rectangle(
                Math.Max(0, (int)(screenWidth * x1)),
                Math.Max(0, (int)(screenHeight * (1 - y2))),
                Math.Min(screenWidth, (int)(screenWidth * (x2 - x1))),
                Math.Min(screenHeight, (int)(screenHeight * (y2 - y1)))
            );

            Console.WriteLine($"Bounds set to: {bounds}");
        }

        public void StartLogging()
        {
            while (true)
            {
                try
                {
                    // 截图
                    Bitmap screenshot = CaptureScreen(bounds);
                    if (screenshot == null)
                    {
                        Console.WriteLine("Failed to capture screenshot.");
                        continue;
                    }
                  //  Console.WriteLine("Screenshot captured.");

                    // 预处理截图
                    Bitmap processedScreenshot = PreprocessImage(screenshot);

                    // OCR 识别
                    string text = RecognizeText(processedScreenshot);
                   if(PlayerInfo.GetInstance().ifDebug)
                    Console.WriteLine(text);
                    text = ProcessRecognizedText(text);
                    if (PlayerInfo.GetInstance().ifDebug)
                        Console.WriteLine(text);
                    WriteToBuffer(text);

                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error during logging: " + ex.Message);
                }
            }
        }
        private Bitmap CaptureScreen(Rectangle bounds)
        {
            try
            {
                Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
                }
                // Console.WriteLine("++++Health: " + fileReader.Health+!token.IsCancellationRequested);  Console.WriteLine("Screen captured.");
                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error capturing screen: " + ex.Message);
                return null;
            }
        }

        private Bitmap PreprocessImage(Bitmap bitmap)
        {
            Bitmap grayBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            using (Graphics g = Graphics.FromImage(grayBitmap))
            {
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] {0.3f, 0.3f, 0.3f, 0, 0},
                    new float[] {0.59f, 0.59f, 0.59f, 0, 0},
                    new float[] {0.11f, 0.11f, 0.11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);
                g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, attributes);
            }

            BitmapData data = grayBitmap.LockBits(new Rectangle(0, 0, grayBitmap.Width, grayBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int bytes = Math.Abs(data.Stride) * grayBitmap.Height;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(data.Scan0, rgbValues, 0, bytes);

            for (int y = 0; y < grayBitmap.Height; y++)
            {
                for (int x = 0; x < grayBitmap.Width; x++)
                {
                    int index = y * data.Stride + x * 4;
                    byte r = rgbValues[index];
                    byte g = rgbValues[index + 1];
                    byte b = rgbValues[index + 2];
                    int gray = (r + g + b) / 3;
                    byte binaryColor = (byte)(gray > 128 ? 255 : 0);
                    rgbValues[index] = binaryColor;
                    rgbValues[index + 1] = binaryColor;
                    rgbValues[index + 2] = binaryColor;
                }
            }

            Marshal.Copy(rgbValues, 0, data.Scan0, bytes);
            grayBitmap.UnlockBits(data);

            return grayBitmap;
        }

        private string RecognizeText(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return "Bitmap is null.";
            }

            try
            {
                using (var img = PixConverter.ToPix(bitmap))
                {
                    if (ocrEngine == null)
                    {
                        Console.WriteLine("ocrEngine is null.");
                        return "OCR engine is not initialized.";
                    }

                    using (var page = ocrEngine.Process(img))
                    {
                        string text = page.GetText();
                      //  Console.WriteLine("Text has been recognized.");
                        return text;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error recognizing text: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                return "Error recognizing text.";
            }
        }

        private string ProcessRecognizedText(string text)
        {
            // 将文本按遇到的 "--" 符号分割
            var lines = text.Split(new[] { "--" }, StringSplitOptions.None);
            // 创建一个列表用于存储处理后的行
            var processedLines = new List<string>();

            // 处理每一行
            foreach (var line in lines)
            {
                var cleanedLine = line.Trim();

                // 如果没有 `+` 符号，则跳过该行
                if (!cleanedLine.Contains("+"))
                {
                    continue;
                }

                int plusIndex = cleanedLine.IndexOf('+');
                if (plusIndex >= 0)
                {
                    // 截取 `+` 号后的字符串
                    var substringAfterPlus = cleanedLine.Substring(plusIndex + 1).Trim();
                    // 查找第一个字母或数字
                    var firstLetterMatch = Regex.Match(substringAfterPlus, @"[a-zA-Z0-9]");
                    if (firstLetterMatch.Success)
                    {
                        // 保留第一个字母或数字后面的内容，去掉前面的字符
                        cleanedLine = firstLetterMatch.Value + substringAfterPlus.Substring(firstLetterMatch.Index + 1);
                    }
                    else
                    {
                        cleanedLine = substringAfterPlus;
                    }
                }

                // 将处理后的行添加到列表中
                if (!string.IsNullOrEmpty(cleanedLine))
                {
                    processedLines.Add(cleanedLine);
                }
            }
            // 将处理后的所有行合并为一个字符串，行与行之间用换行符分隔
            return string.Join(Environment.NewLine, processedLines);
        }


        private void WriteToBuffer(string text)
        {
            lock (logBuffer)
            {
                logBuffer.AppendLine(text);
            }
        }
        
        public string ReadBuffer()
        {
            lock (logBuffer)
            {
                if (lastReadPosition >= logBuffer.Length)
                {
                    return string.Empty;
                }

                string newContent = logBuffer.ToString(lastReadPosition, logBuffer.Length - lastReadPosition);
                lastReadPosition = logBuffer.Length;
                return newContent;
            }
        }

        private void DrawRecognitionBounds()
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                ControlPaint.DrawReversibleFrame(bounds, Color.Red, FrameStyle.Thick);
            }
        }

        private async Task DrawRecognitionBoundsContinuously()
        {
            while (drawingBounds)
            {
                DrawRecognitionBounds();
                await Task.Delay(500);
            }
        }
    }
}
