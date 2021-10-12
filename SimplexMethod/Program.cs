﻿using System;
using System.IO;

namespace SimplexMethod
{
    class Program
    {
        private static Lpp task;
        
        static void Main(string[] args)
        {
            task = new Lpp();
            
            //Console.Write("Введите путь к файлу: ");
            string path = "task2.txt";
            //path = Console.ReadLine();
            
            StreamReader fstream = null;
            try
            {
                fstream = new StreamReader(path);
                fstream = new StreamReader(path);
                
                task.SetTargetFunction(fstream.ReadLine());
                task.SetTarget(fstream.ReadLine());
                while (!fstream.EndOfStream)
                {
                    task.AddLimitation(fstream.ReadLine());
                }
            }
            catch (Exception ex)
            {
                Console.Write("Ошибка при открытии файла: " + ex.Message);
                return;
            }
            finally
            {
                if (fstream != null)
                    fstream.Close();
            }
            
            Console.WriteLine("Входные данные: ");
            task.PrintLpp();
            Console.WriteLine();

            try
            {
                task.Solve();
            }
            catch (Exception ex)
            {
                Console.Write("Ошибка решения: " + ex.Message);
            }
        }
    }
}