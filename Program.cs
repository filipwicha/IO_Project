using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace IO_Project
{
    class Program
    {
        static void Main(string[] args)
        {
            int imageSize = 1024;
            int iterationCounter = 200;
            int tasks = 0;
            try
            {
                Console.WriteLine("Enter number of tasks:");
                tasks = Convert.ToInt32(Console.ReadLine());
                if (imageSize % tasks != 0) throw new Exception("(imageSize % tasks) must be egual to 0, because of the need of equal number of lines for every task");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            Console.WriteLine("-----------------------------------\n");
            int[,] image;

            int[,] imageSync;
            int[,] imageASync;

            /*choose one of two: */
            image = CreateImage(imageSize);             
            //image = CreateImageHalf(imageSize);  

            ToImage(image, "new.jpg");
            //sync
            Stopwatch stopWatchSync = new Stopwatch();
            stopWatchSync.Start();
            imageSync = Sync(image, iterationCounter);
            stopWatchSync.Stop();
            //async
            Stopwatch stopWatchAsync = new Stopwatch();
            stopWatchAsync.Start();
            imageASync = Async(image, tasks, iterationCounter);
            stopWatchAsync.Stop();

            Console.WriteLine("Statistic for: \n1. " + iterationCounter + " iterations\n2. " + tasks + " tasks\n3. " + imageSize + " px x " + imageSize + " px image. \n");
            Console.WriteLine("Time of executing sync operations:   " + stopWatchSync.Elapsed + " s");
            Console.WriteLine("Time of executing async operations:  " + stopWatchAsync.Elapsed + " s");

            ToImage(imageSync, "sync.jpg");
            ToImage(imageSync, "async.jpg");
            Console.WriteLine("\nBoth images saved to: " + Environment.CurrentDirectory + " as sync.jpg and async.jpg");
            
            Console.ReadKey();
        }

        static int[,] CalculateSync(int[,] before)
        {
            int[,] after = before;
            int size = before.GetLength(0) - 2; //padding
            for (int i = 1; i < size - 1; i++)
            {
                for (int j = 1; j < size - 1; j++)
                {
                    after[i, j] = (int)(before[i, j] * 0.6 + (before[i, j - 1] + before[i, j + 1] + before[i - 1, j] + before[i + 1, j]) * 0.1);
                }
            }
            return after;
        }

        static int[,] Sync(int[,] before, int iterationCounter)
        {
            int[,] after = before;

            for (int i = 0; i < iterationCounter; i++)
            {
                after = CalculateSync(after);
            }

            return after;
        }

        static void CalculateAsync(int[,] before, int[,] after, int tasks, int task)
        {
            int size = before.GetLength(0) - 2; //padding
            int numberOfLinesPerThread = size / tasks;
            int numberOfLine = task * numberOfLinesPerThread;

            for (int i = numberOfLine + 1; i < size; i++)
            {
                for (int j = 1; j < size - 1; j++)
                {
                    after[i, j] = (int)(before[i, j] * 0.6 + (before[i, j - 1] + before[i, j + 1] + before[i - 1, j] + before[i + 1, j]) * 0.1);
                }
            }
        }

        static int[,] Async(int[,] before, int threadCounter, int iterationCounter)
        {
            int[,] after = before;
            for (int i = 0; i < iterationCounter; i++)
            {
                Task[] tasks = new Task[threadCounter];
                for (int j = 0; j < threadCounter; j++)
                {
                    tasks[j] = Task.Run(() => CalculateAsync(before, after, threadCounter, j));
                }
                Task.WaitAll(tasks);
            }

            return after;
        }

        static void ToImage(int[,] image, string fileName)
        {
            int size = image.GetLength(0);
            byte[,] byteImage = new byte[size - 2, size - 2]; //padding
            for (int i = 1; i < size - 1; i++)
            {
                for (int j = 1; j < size - 1; j++)
                {
                    //new image without padding
                    byteImage[i - 1, j - 1] = (byte)(((uint)image[i, j]) & 0xFF);
                }
            }

            byte[] OneDByteImage = byteImage.Cast<byte>().Select(c => c).ToArray();

            Bitmap bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            for (int h = 0; h < size - 2; h++)
            {
                for (int w = 0; w < size - 2; w++)
                {
                    //Average out the RGB components to find the Gray Color
                    int red = OneDByteImage[w + (h * (size - 2))]; //(pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    int green = OneDByteImage[w + (h * (size - 2))]; //(pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    int blue = OneDByteImage[w + (h * (size - 2))]; //(pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    Color grayColor = Color.FromArgb(red, green, blue); //(red+green+blue)/3);
                    bmp.SetPixel(w, h, grayColor);
                }
            }

            bmp.Save(fileName, ImageFormat.Jpeg);
        }

        static int[,] CreateImage(int size)
        {
            int[,] image = new int[1 + size + 1, 1 + size + 1]; //padding

            Random ran = new Random();
            for (int i = 0; i < size + 2; i++)
            {
                for (int j = 0; j < size + 2; j++)
                {
                    image[i, j] = ran.Next(0,255);
                }
            }

            return image;
        }

        static int[,] CreateImageHalf(int size)
        {
            int[,] image = new int[1 + size + 1, 1 + size + 1]; //padding
            
            for (int i = 0; i < size + 2; i++)
            {
                for (int j = 0; j < size + 2; j++)
                {
                    if (i>size/2)
                    {
                        image[i, j] = 0;
                    }
                    else
                    {
                        image[i, j] = 255;
                    }
                }
            }

            return image;
        }
    }
}
