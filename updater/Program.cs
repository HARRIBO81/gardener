﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace updater
{
    class Program
    {
        public Process ActiveProcess;

        public static string Repo = "https://github.com/encodeous/gardener.git";

        public static void Main(string[] args)
            => new Program().Execute();

        public void Execute()
        {
            Console.WriteLine("Starting Gardener Updater...");
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
            Run();
            Console.WriteLine("Updater Stopped.");
        }

        private void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            ActiveProcess.StandardInput.WriteLine("exit");
            e.Cancel = true;
        }

        public void RebuildSources()
        {
            Console.WriteLine("Cloning Git Repository...");
            Directory.CreateDirectory("update");
            RunWithRedirection("git", "clone " + Repo,
                Path.Combine(Environment.CurrentDirectory, "update"));
            Console.WriteLine("Building Sources");
            RunWithRedirection("dotnet", "build -o " + Path.Combine(Environment.CurrentDirectory, "binary"),
                Path.Combine(Environment.CurrentDirectory, "update", "gardener", "gardener"));

        }

        public void Update()
        {
            Console.WriteLine("Performing Self-Update...");
            Console.WriteLine("Gardener will automatically restart after completion.");
            RebuildSources();
            Run();
        }

        public void Run()
        {
            if (File.Exists("binary/gardener.dll"))
            {
                Console.WriteLine("Running Gardener...");
                var output = RunWithRedirection("dotnet", "binary/gardener.dll", Environment.CurrentDirectory);
                if (output == 0) return;
                if (output == -1)
                {
                    Update();
                    Run();
                }
            }
            else
            {
                Console.WriteLine("Could not locate gardener! Building Gardener from Sources.");
                RebuildSources();
                Run();
            }
        }

        public int RunWithRedirection(string file, string args, string workingDirectory)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = file,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                }
            };
            proc.OutputDataReceived += (sender, eventArgs) => Console.WriteLine(eventArgs.Data);
            proc.Start();
            ActiveProcess = proc;
            proc.BeginOutputReadLine();
            proc.WaitForExit();
            return proc.ExitCode;
        }
    }
}