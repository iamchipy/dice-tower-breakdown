using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;

namespace DiceTowerPractice
{
    public class Program
    {
        // For testing storing on Stack for speed instead of Heap
        public struct DiceRollEntry
        {
            public string inputString;
            public int[] resultParts;
            public int result;
        }

        // Logging tool that assists with keeping track of rolls and outputing th data
        public class LoggingTool
        {
            public int logThreshold = 1;  // The threshold for something to be logged
            public int reportThreshold = 5;  // The threshold for something to be reported to use
            public string dataPath = "rolls.csv"; // Export/Import filename assuming working dir
            public string logPath = "rolls.log"; // Logging filename assuming working dir
            public List<DiceRollEntry> rollHistory = new List<DiceRollEntry>();  // Create a running log of each roll

            // Manage the reporting and ConsoleWriting
            // Accepts 
            //  String - value to log
            //  String - fully qualified path or simply file name 
            //  Int - level to be currently logging
            //  Int - level to be currently reporting
            public void Report(string reportString)
            {
                // base variables
                int instanceLevel = 10;
                DateTime timeStamp = DateTime.Now;
                string ts = DateTime.Now.ToString("HHmmss:fff");
                string[] reportStrings = reportString.Split(new char[] { ':' }, 2).ToArray();

                // try parse current instance level
                if (int.TryParse(reportStrings[0], out instanceLevel))
                {
                    // here we know we were given a level for this instance so we compare and report/log if thresholds are met
                    string str = $"{ts}: {reportStrings[1]}";
                    if (instanceLevel >= this.reportThreshold) Console.WriteLine(str);
                    if (instanceLevel >= this.logThreshold) logToFile(str);
                }
                else
                {   // since we don't know what to do with this we'll report it in console only to be sure it's not ignored
                    string str = $"{ts}:NoThreshold!! {reportStrings[1]}";
                    Console.WriteLine(str);
                    logToFile(str);
                }

                void logToFile(string stringToLog)
                {
                    // Save string to the log file
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(this.logPath, append: true)) writer.WriteLine(stringToLog);
                    } // Report to user if there is some issue
                    catch (Exception ex)
                    {
                        this.Report($"ERROR LOGGING: {ex.Message}");
                    }
                }
            }

            public bool Save()
            {
                // Saves the history a datafile
                try
                {
                    using (StreamWriter writer = new StreamWriter(this.dataPath, append: true))
                    {
                        foreach (DiceRollEntry roll in this.rollHistory.ToArray())
                        {
                            writer.WriteLine($"{roll.inputString}, {roll.result}, '{string.Join(",", roll.resultParts)}'");
                        }
                    }
                    this.Report($"9: Dice history saved successfully to {Directory.GetCurrentDirectory()}\\{this.dataPath}");
                    return true;
                }catch (Exception ex)
                {
                    this.Report($"ERROR SAVING: {ex.Message}");
                    return false;
                }
            }

            internal int[] ConvertToIntArray(string commaSeparatedString)
            {
                // validate that the input isn't empty
                if (string.IsNullOrEmpty(commaSeparatedString))
                {
                    return Array.Empty<int>();
                }

                // split string and build array of matching length
                string[] stringArray = commaSeparatedString.Split(','); 
                int[] intArray = new int[stringArray.Length]; 

                // loop for array and try parse each one
                for (int i = 0; i < stringArray.Length; i++)
                {
                    if (int.TryParse(stringArray[i], out int num))
                    {
                        intArray[i] = num;
                    }
                    else
                    {
                        // Handle invalid integer conversion if we want to do something here later
                    }
                }

                return intArray;
            }

            public bool Load()
            {
                // Loads the history from datafile
                try
                {
                    using (StreamReader reader = new StreamReader(this.dataPath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string[] parts = line.Split(',');
                            this.rollHistory.Add(new DiceRollEntry { inputString = parts[0], result = Convert.ToInt32(parts[1]), resultParts = this.ConvertToIntArray(parts[2]) });
                        }
                    }
                    this.Report($"9: Dice history loaded successfully from {Directory.GetCurrentDirectory()}\\{this.dataPath}");
                    return true;
                }
                catch (Exception ex)
                {
                    this.Report($"ERROR SAVING: {ex.Message}");
                    return false;
                }
            }

            public void Display()
            {
                this.Report($"9:Reporting current DiceDATA: {this.rollHistory.Count} entries...");
                for (int i = 0; i < this.rollHistory.Count; i++)
                {
                    Console.WriteLine($"Roll: {this.rollHistory[i].inputString} \t=> {this.rollHistory[i].result} \t[{string.Join(",", this.rollHistory[i].resultParts)}]");
                }
            }
        }

        // Roll a single dice
        // Accepts int for number of dice sides
        // Returns int result
        static public int diceRollD(int sides = 20)
        {
            // validate input
            if (sides < 0 || sides > 1000 || sides < 1)
            {
                // TODO build exception possibly Sprint 7
                throw new Exception("InvalidNumberOfSides");
            }
            // Sleep here to allow the next dice roll to have sufficient time to use "DateTime.Now.Millisecond" as a seed
            Thread.Sleep(1);  // TODO suspect more than 1 MS sleep time due to being based on thread speed
            Random rng = new Random(DateTime.Now.Millisecond);
            return rng.Next(1, sides + 1);
        }

        // Roll for dice format
        // Accepts input string in dice format #d##
        // Returns int value of roll 
        static public (int, int[]) diceRollString(LoggingTool log, string diceFormatString = "2d20")
        {
            int runningTotal = 0, numberOfSides, numberOfDice;

            // Check for "d" in string as we expect dice-format
            int indexOfDelimiter = diceFormatString.ToLower().IndexOf('d');
            // Handle case of missing 'd' element in string
            if (indexOfDelimiter < 0)
            {
                numberOfSides = Convert.ToInt32(diceFormatString);
                return (numberOfSides, new int[] { numberOfSides });
            }
            else
            { // if it's not missing continue as before
                // Deconstruct for validation
                numberOfSides = Convert.ToInt32(diceFormatString.Substring(indexOfDelimiter + 1));
                // Check if we even have an index for 'd' and if not assume 1
                numberOfDice = indexOfDelimiter == 0 ? 1 : Convert.ToInt32(diceFormatString.Substring(0, indexOfDelimiter));
            }

            // validate input
            if (numberOfDice < 1)
            {
                // TODO build exception possibly Sprint 7
                throw new Exception("InvalidNumberOfDice");
            }

            // Create array to log each roll now that we know the number of rolls/dice
            int[] rolls = new int[numberOfDice];

            // now we call roll for each dice that needs rolled
            for (int i = 0; i < numberOfDice; i++)
            {
                // TODO build in logging Sprint 3
                int roll = diceRollD(numberOfSides);
                log.Report("4:Roll[" + (i + 1) + "] was: " + roll + "     >" + runningTotal);
                runningTotal += roll;
                rolls[i] = roll;
            }

            return (runningTotal, rolls);
        }

        // Decodes user input
        // Accepts string formatted like 2d20+d6+3 (Foundry Virtual Table Top format)
        // Returns a touple with the data
        //  Int - total result for requested role
        //  Int[] - array to represent the individual rolls that were created
        //  String - copy of the initial input value for later cross checking
        static public (int, int[], string) decodeDiceString(LoggingTool log, string userInputString = "d20", bool showYourWork = false)
        {
            // Using a List<T> here to test it's performance as well as keeping the code simple
            List<int> diceIndividualRolls = new List<int>();
            int diceRollResult = 0;

            // deconstruct the string to determine how many dice to throw
            var individualDiceRolls = userInputString.ToLower().Split('+');

            // Loop for each entry in the input string
            for (int i = 0; i < individualDiceRolls.Length; i++)
            {
                // add the results and log
                var (a, b) = diceRollString(log, individualDiceRolls[i]);
                diceRollResult += a;
                diceIndividualRolls.AddRange(b);

                // display progress for reporting
                log.Report($"4:DiceString: {individualDiceRolls[i]} >> {diceRollResult}");
            }

            // drop the List<T> into an array as we are done with dynamics here
            int[] diceIndividualRollsCast = diceIndividualRolls.ToArray();

            // Be default we just return the value of the requested string
            return (diceRollResult, diceIndividualRollsCast, userInputString);
        }

        // wrapper to read console input and try int32 convert
        // returns -1 if fails or int value
        static public int ReadIntInput()
        {
            string inputString = Console.ReadLine();
            int parsedInt = -1;
            if (int.TryParse(inputString, out parsedInt))
            {
                return parsedInt;
            }
            return -1;
        }

        // Prmpts user for a dice format string
        // returns String validated to be diceFormat or "invalid"
        static string GetDiceInput(LoggingTool log)
        {
            // Get user's input string
            Console.Write("Dice Roll Input String (blank to exit): ");
            string usersRollRequest = Console.ReadLine().ToLower();

            // Check escape/cancel route
            if (string.IsNullOrEmpty(usersRollRequest))
            {
                log.Report("8: Exiting Dice Roll Mode");
                return usersRollRequest;
            }

            // Validate input from user
            string pattern = @"^[\d|d|\+]{1,}$";  // TODO add catch for edge cases single letter entry "d" || numbers with "d" Sprint 7
            bool isValid = Regex.IsMatch(usersRollRequest, pattern);

            // if invalid we skip to the next loop
            if (!isValid)
            {
                log.Report($"9:Invalid input [{usersRollRequest}] Please try again in FVTT dice format");
                return "invalid";
            }

            return usersRollRequest;
        }

        // Prompts user to select from a list of modes/functions for REPL loop
        // Accepts Int var to store user's choice
        // Returns Bool on success/valid option
        static bool REPLPrompt(out int userChoice)
        {
            // set default per requirement of using "out" type
            userChoice = -1;

            // Display to the user the options
            Console.WriteLine();
            Console.WriteLine("1 - Roll Dice");
            Console.WriteLine("2 - Save History");
            Console.WriteLine("3 - Load History");
            Console.WriteLine("4 - Display History");
            Console.WriteLine("0 - Exit");
            Console.Write("Select action[0-4]: ");
            // Capture user input
            userChoice = ReadIntInput();

            return (0 <= userChoice && userChoice <= 4);
        }



        static void Main(string[] args)
        {
            var log = new LoggingTool();
            int currentAction = -1;
            string diceRequestString;

            // Rename console window for QoL
            Console.Title = "Dice Tower v1.7";

            // Create a running log of each roll
            List<DiceRollEntry> diceRollLog = new List<DiceRollEntry>();

            do
            {
                // Get user input
                if (!REPLPrompt(out currentAction))
                {
                    log.Report("9: Invalid choice! (please try again)");
                    continue;
                }


                // select action
                switch (currentAction)
                {
                    case 0:
                        // let script end
                        break;
                    case 1:
                        do
                        {
                            // ask for dice string
                            diceRequestString = GetDiceInput(log);
                            if (diceRequestString == "invalid") continue;
                            if (string.IsNullOrEmpty(diceRequestString)) break;

                            // Declare timer and start 
                            Stopwatch runTimer = new Stopwatch();  // TODO add separate stopwatches for sub-steps
                            runTimer.Start();

                            // Make the requested rolls
                            var (a, b, c) = decodeDiceString(log, diceRequestString);

                            // Build logging entry instance
                            log.rollHistory.Add(new DiceRollEntry() { result = a, resultParts = b, inputString = c });

                            // Stop timer 
                            runTimer.Stop();

                            // Report to the user
                            log.Report($"9:You rolled a {a} [{c} >> {string.Join(",", b)}]");
                            log.Report($"5:runTimer: >> {runTimer.ElapsedMilliseconds:0,000}ms");

                        } while (true);
                        // reset
                        currentAction = -1;
                        break;
                    case 2:
                        log.Save();
                        break;
                    case 3:
                        log.Load();
                        break;
                    case 4:
                        log.Display();
                        break;

                    default:
                        // Report that we received something unexpect
                        log.Report($"9:UNEXPECTED currentAction: {currentAction}");
                        // reset
                        currentAction = -1;
                        break;

                }
            } while (currentAction != 0);

            // Goodbye confirmation
            log.Report("9:REPL Complete, thanks for testing!");
            Console.ReadLine();
        }
    }
}
