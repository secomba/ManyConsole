﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace ManyConsole.Internal
{
    public class ConsoleHelp
    {
        
        public static void ShowSummaryOfCommands(IEnumerable<IConsoleCommand> commands, TextWriter console, string summaryTitle = null)
        {
            console.WriteLine();
            if (summaryTitle != null)
            {
                console.WriteLine(summaryTitle);
                console.WriteLine();
            }

            console.WriteLine("Available commands are:");
            console.WriteLine();

            string helpCommand = "help <name>";

            var commandList = commands.Where(it => !it.IsHidden).ToList();
            var n = commandList.Select(c => ConsoleUtil.FormatCommandName(c.Command)).Concat(new [] { helpCommand}).Max(c => c.Length) + 1;
            foreach (var command in commandList)
            {
                // don't exceed console window with
                PrintCommandConsoleFriendly(console, ConsoleUtil.FormatCommandName(command.Command), command.OneLineDescription, n);
            }
            console.WriteLine();
            PrintCommandConsoleFriendly(console, helpCommand, "For help with one of the above commands", n);
            console.WriteLine();
        }


        private static void PrintCommandConsoleFriendly(TextWriter console, string commandName, string oneLineDescription, int offset, Func<int, string> getFormatString = null)
        {
            var intendation = offset + 3;
            var minWordLength = 6;
            var commandFormatString = getFormatString?.Invoke(offset) ?? "    {0,-" + offset + "}- {1}";
            var prefix = new String(' ', intendation);
            var commandStrings = ConsoleUtil.SplitStringHumanReadable(string.Format(commandFormatString, commandName,
               oneLineDescription), ConsoleUtil.ConsoleWidth, splitpattern: null);

            console.WriteLine(commandStrings.First());

            foreach (var commandString in commandStrings.Skip(1)) {

                if (ConsoleUtil.ConsoleWidth - intendation > minWordLength)
                {
                    var secondLineCommandStrings = ConsoleUtil.SplitStringHumanReadable(commandString, ConsoleUtil.ConsoleWidth - intendation, splitpattern: null);
                    secondLineCommandStrings.ForEach(it => console.WriteLine(prefix + it));
                }
                else
                {
                    console.WriteLine(prefix + commandString);
                }
            }
        }
        public static void ShowCommandHelp(IConsoleCommand selectedCommand, TextWriter console, bool skipExeInExpectedUsage = false)
        {
            var haveOptions = selectedCommand.GetActualOptions().Count > 0;

            console.WriteLine();
            console.WriteLine("'" + ConsoleUtil.FormatCommandName(selectedCommand.Command) + "' - " + selectedCommand.OneLineDescription);
            console.WriteLine();
            console.Write("Expected usage:");

            if (!skipExeInExpectedUsage)
            {
                console.Write(" " + AppDomain.CurrentDomain.FriendlyName);
            }

            console.Write(" " + selectedCommand.Command);

            if (haveOptions)
                console.Write(" <options> ");

            console.WriteLine((haveOptions? "":" ") + selectedCommand.RemainingArgumentsHelpText);

            if (haveOptions)
            {
                console.WriteLine("<options> available:");
                selectedCommand.GetActualOptions().WriteOptionDescriptions(console);
            }
            console.WriteLine();
        }

        public static void ShowParsedCommand(IConsoleCommand consoleCommand, TextWriter consoleOut)
        {

            if (!consoleCommand.TraceCommandAfterParse || consoleCommand.IsHidden)
            {
                return;
            }

            string[] skippedProperties = {
                "IsHidden",
                "Command",
                "OneLineDescription",
                "Options",
                "Console",
                "TraceCommandAfterParse",
                "RemainingArgumentsCount",
                "RemainingArgumentsHelpText",
                "ShowHelpWithoutFurtherArgs",
                "RequiredOptions"
            };

            var properties = consoleCommand.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !skippedProperties.Contains(p.Name));

            var fields = consoleCommand.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !skippedProperties.Contains(p.Name));

            Dictionary<string,string> allValuesToTrace = new Dictionary<string, string>();

            foreach (var property in properties)
            {
                object value = property.GetValue(consoleCommand, new object[0]);
                allValuesToTrace[property.Name] = value != null ? value.ToString() : "null";
            }

            foreach (var field in fields)
            {
                allValuesToTrace[field.Name] = MakeObjectReadable(field.GetValue(consoleCommand));
            }

            consoleOut.WriteLine();

            string introLine = String.Format("Executing {0}", ConsoleUtil.FormatCommandName(consoleCommand.Command));

            if (string.IsNullOrEmpty(consoleCommand.OneLineDescription))
            {
                introLine = introLine + ":";
                Console.WriteLine(introLine);
            }
            else
            {
                var description =  consoleCommand.OneLineDescription;
                PrintCommandConsoleFriendly(consoleOut, introLine, description, introLine.Length, offset => "{0} ({1}):");
            }

            foreach(var value in allValuesToTrace.OrderBy(k => k.Key))
                consoleOut.WriteLine("    " + value.Key + " : " + value.Value);

            consoleOut.WriteLine();
        }

        static string MakeObjectReadable(object value)
        {
            string readable;

            if (value is System.Collections.IEnumerable && !(value is string))
            {
                readable = "";
                var separator = "";

                foreach (var member in (IEnumerable) value)
                {
                    readable += separator + MakeObjectReadable(member);
                    separator = ", ";
                }
            }
            else if (value != null)
                readable = value.ToString();
            else
                readable = "null";
            return readable;
        }
    }
}
