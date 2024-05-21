using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;  //stopwatch

namespace ConsoleApp1
{
    class Log
    {
        // Manage the reporting and ConsoleWriting
        // Accepts 
        //  String - value to log
        //  String - fully qualified path or simply file name 
        //  Int - level to be currently logging
        //  Int - level to be currently reporting
        public static void Report(string reportString, string logPath = "rolls.log", int logThreshold = 10, int reportThreshold = 5)
        {
            // base variables
            int instanceLevel = 10;
            DateTime timeStamp = DateTime.Now;
            string ts = DateTime.Now.ToString("HHmmss:fff");
            string[] reportStrings = reportString.Split(new char[] { ':' }, 2).ToArray();

            // try parse current instance level
            if (int.TryParse(reportStrings[0], out instanceLevel))
            {
                // here we know we were given a level for this instance so we compare and report if threshold met
                if (instanceLevel >= reportThreshold)
                {
                    Console.WriteLine($"{ts}: {reportStrings[1]}");
                }
            }
            else
            {   // since we don't know what to do with this we'll report it in console only to be sure it's not ignored
                Console.WriteLine($"{ts}:NoThreshold!! {reportStrings[1]}");
            }
        }
    }

    class mainProg
    {

        // logging with Stack for speed instead of heap
        struct DiceRollEntry
        {
            public string inputString;
            public int[] resultParts;
            public int result;
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
            return rng.Next(1, sides+1);
        }

        // Roll for dice format
        // Accepts input string in dice format #d##
        // Returns int value of roll 
        static public (int, int[]) diceRollString(string diceFormatString = "2d20")
        {
            int runningTotal = 0, numberOfSides, numberOfDice;

            // Check for "d" in string as we expect dice-format
            int indexOfDelimiter = diceFormatString.ToLower().IndexOf('d'); 
            // Handle case of missing 'd' element in string
            if (indexOfDelimiter < 0) 
            {
                numberOfSides = Convert.ToInt32(diceFormatString);
                return (numberOfSides, new int[] { numberOfSides});
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
                Log.Report("4:Roll[" +(i+1)+"] was: " + roll + "     >"+ runningTotal);
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
        static public (int, int[], string) decodeDiceString(string userInputString = "d20", bool showYourWork=false)
        {
            // Using a List<T> here to test it's performance as well as keeping the code simple
            List<int> diceIndividualRolls = new List<int>();
            int diceRollResult = 0;

            // deconstruct the string to determine how many dice to throw
            var individualDiceRolls = userInputString.ToLower().Split('+');

            // Loop for each entry in the input string
            for (int i = 0;i< individualDiceRolls.Length; i++)
            {
                // add the results and log
                var (a,b) = diceRollString(individualDiceRolls[i]);
                diceRollResult += a;
                diceIndividualRolls.AddRange(b);

                // display progress for reporting
                Log.Report($"6:DiceString: {individualDiceRolls[i]} >> {diceRollResult}");
            }

            // drop the List<T> into an array as we are done with dynamics here
            int[] diceIndividualRollsCast = diceIndividualRolls.ToArray();

            // Be default we just return the value of the requested string
            return (diceRollResult, diceIndividualRollsCast, userInputString);
        }
        
        // wrapper to read console input and try int32 convert
        // accepts string
        // returns -1 if fails or int value
        static public int ReadIntInput()
        {
            string inputString = Console.ReadLine();
            int parsedInt = -1;
            if(int.TryParse(inputString, out parsedInt))
            {
                return parsedInt;
            }
            return -1;
        }

        static string GetDiceInput()
        {
            // Get user's input string
            Console.Write("Dice Roll Input String: ");
            string usersRollRequest = Console.ReadLine().ToLower();

            // Check escape/cancel route
            if (string.IsNullOrEmpty(usersRollRequest)) {
                Log.Report("8: Exiting Dice Roll Mode");
                return usersRollRequest; 
            }

            // Validate input from user
            string pattern = @"^[\d|d|\+]{1,}$";  // TODO add catch for edge cases single letter entry "d" || numbers with "d" Sprint 7
            bool isValid = Regex.IsMatch(usersRollRequest, pattern);

            // if invalid we skip to the next loop
            if (!isValid)
            {
                Log.Report($"9:Invalid input [{usersRollRequest}] Please try again in FVTT dice format");
                return "invalid";
            }

            return usersRollRequest;
        }

        static bool GetReadEvaluateLoopInput(out int userChoice)
        {
            // set default per requirement of using "out" type
            userChoice = -1;

            // Display to the user the options
            Console.WriteLine();
            Console.WriteLine("1 - Roll Dice");
            Console.WriteLine("2 - Save History");
            Console.WriteLine("3 - Load History");
            Console.WriteLine("0 - Exit");
            Console.Write("Select action[0-3]: ");
            // Capture user input
            userChoice = ReadIntInput();

            return (0 <= userChoice && userChoice <= 3);
        }

        static void Main(string[] args)
        {
            int currentAction = -1;
            string diceRequestString;
            // Rename console window for QoL
            Console.Title = "Dice Tower v1";

            // Create a running log of each roll
            List<DiceRollEntry> diceRollLog = new List<DiceRollEntry>();

            do
            {
                // Get user input
                if(!GetReadEvaluateLoopInput(out currentAction))
                {
                    Log.Report("9: Invalid choice! (please try again)");
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
                            diceRequestString = GetDiceInput();
                            if (diceRequestString == "invalid") continue;
                            if (string.IsNullOrEmpty(diceRequestString)) break;

                            // Declare timer and start 
                            Stopwatch runTimer = new Stopwatch();  // TODO add separate stopwatches for sub-steps
                            runTimer.Start();

                            // Make the requested rolls
                            var (a, b, c) = decodeDiceString(diceRequestString);

                            // Build logging entry instance
                            diceRollLog.Add(new DiceRollEntry() { result = a, resultParts = b, inputString = c });

                            // Stop timer 
                            runTimer.Stop();

                            // Report to the user
                            Log.Report($"9:You rolled a {a} [{c} >> {string.Join(",", b)}]");
                            Log.Report($"5:runTimer: >> {runTimer.ElapsedMilliseconds:0,000}ms");
                            Log.Report($"9:LogLength {diceRollLog.Count}");
                            
                        } while (string.IsNullOrEmpty(diceRequestString));
                        // reset
                        currentAction = -1;
                        break;

                    default:
                        // Report that we received something unexpect
                        Log.Report($"9:UNEXPECTED currentAction: {currentAction}");
                        // reset
                        currentAction = -1;
                        break;

                }
            } while (currentAction != 0);

            // Goodbye confirmation
            Log.Report("9:REPL Complete, thanks for testing!");
            Console.ReadLine();
        }
    }
}
