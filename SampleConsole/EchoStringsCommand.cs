﻿using System;
using ManyConsole;

namespace SampleConsole
{
    public class EchoStringsCommand : ConsoleCommand
    {
        public string comments;

        public EchoStringsCommand()
        {
            IsCommand("echo");

            HasOption("c|comment=",
                           "enter a string, maybe even delimited by double quotes, i.e. - \"See Matt's poorly written code\"",
                           v => comments = v);

            AllowsAnyAdditionalArguments("<foo1> <foo2> <fooN> where N is a word");
        }

        public override DefaultCommandResult Run(string[] remainingArguments, ref DefaultCommandSettings settings) {
            if (string.IsNullOrWhiteSpace(comments))
            {
                Console.WriteLine("You made no comment");
            }
            else
            {
                Console.WriteLine("Your comment is: " + comments);
            }

            Console.WriteLine("The remaining arguments were " + Newtonsoft.Json.JsonConvert.SerializeObject(remainingArguments));

            return new DefaultCommandResult();
        }
    }
}
