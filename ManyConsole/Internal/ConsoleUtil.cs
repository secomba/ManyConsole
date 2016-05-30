using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ManyConsole.Internal
{
    public abstract class ConsoleUtil
    {
        public const int ConsoleWidth = 80;
        public static void VerifyNumberOfArguments(string[] args, int expectedArgumentCount)
        {
            if (args.Count() < expectedArgumentCount)
                throw new ConsoleHelpAsException(
                    String.Format("Invalid number of arguments-- expected {0} more.", expectedArgumentCount - args.Count()));
            
            if (args.Count() > expectedArgumentCount)
                throw new ConsoleHelpAsException("Extra parameters specified: " + String.Join(", ", args.Skip(expectedArgumentCount).ToArray()));
        }

        public static bool DoesArgMatchCommand(string argument, IConsoleCommand command)
        {
            if (argument == null || command == null)
            {
                return false;
            }
            return
                command.Command.ToLower()
                    .Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries).Select(it => it.Trim()).ToArray()
                    .Contains(argument.ToLower());
        }

        public static string FormatCommandName(string commandName)
        {
            if (!commandName.Contains("|"))
            {
                return commandName.Trim();
            }
            return String.Join(", ", commandName.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries).Select(it => it.Trim()).ToArray());
        }

        /// <summary>
        /// Splits a string human readable.
        /// Splits up strings separating them using punctiations provided by the splitpattern without exceeding the given maximum maxSubstringLength border.<br/>
        /// If no punctuation was found, the string is splitted at the nearest whitespace character to the given border.<br/>
        /// If no whitespace character is found, the string is "hard-splitted" to ensure not to exceed the maximum border.<br/>
        /// Pagination be set to a certain number of splits, adding pagination to the beginning of each substring <example>(e.g. "[1/12] this is a test string.")</example>
        /// </summary>
        /// <param name="s"></param>
        /// <param name="maxSubstringLength"></param>
        /// <param name="splitpattern"></param>
        /// <param name="paginateAfterNSplits">The number of splits when pagination should be enabled (default: -1 ^= no pagination) </param>
        /// <returns></returns>
        public static List<string> SplitStringHumanReadable(string s, int maxSubstringLength, string splitpattern = @"\!|\?|\,|\.|\-|\:|\;", int paginateAfterNSplits = -1) {
            const int paginationStringLength = 10;

            List<string> messageList = new List<string>();

            if (string.IsNullOrEmpty(s) || maxSubstringLength < 1) {
                return messageList;
            }

            string str = "";
            str += s;

            str = str.Trim();

            try {

                if (paginateAfterNSplits >= 0 && str.Length > maxSubstringLength) {
                    if (maxSubstringLength - paginationStringLength > 0) {
                        maxSubstringLength -= paginationStringLength; // reduce maxSubstringLength to reserve space for pagination
                    } else {
                        paginateAfterNSplits = -1; // disable pagination when remaining substring would be too short
                    }
                }

                while (str.Length > maxSubstringLength) {

                    string tmp = str.Substring(0, maxSubstringLength);

                    int lastIdx = -1;

                    if (!string.IsNullOrEmpty(splitpattern)) {
                        lastIdx = Regex.Match(tmp, splitpattern, RegexOptions.RightToLeft).Index;
                    }

                    if (lastIdx > 0) {
                        lastIdx++;
                    } else {
                        lastIdx = tmp.LastIndexOf(' '); // use whitespace to seperate strings when no punctiations where found

                        if (lastIdx <= 0) {
                            lastIdx = maxSubstringLength; // simply cut words when no whitespace was found
                        }
                    }

                    tmp = tmp.Substring(0, lastIdx);
                    messageList.Add(tmp);
                    str = str.Substring(lastIdx).Trim();
                }

                messageList.Add(str);

                // add pagination
                if (paginateAfterNSplits >= 0 && messageList.Count > paginateAfterNSplits) {
                    for (int i = 0; i < messageList.Count; i++) {
                        string paginationString = "[" + (i + 1) + "/" + messageList.Count + "] ";
                        if (paginationString.Length > paginationStringLength) { // ensure pagination str is not longer than paginationStringLength
                            paginationString = paginationString.Substring(0, paginationStringLength - 3);
                            paginationString += ".. ";
                        }

                        messageList[i] = paginationString + messageList[i];
                    }
                }

            } catch (Exception) { // "hard-split" when error occurred
                messageList.Clear();
                while (s.Length > maxSubstringLength) {
                    messageList.Add(s.Substring(0, maxSubstringLength));
                    s = s.Substring(maxSubstringLength);
                }
                messageList.Add(s);
            }

            return messageList;
        }
    }
}