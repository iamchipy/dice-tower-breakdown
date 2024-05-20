using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    
    internal class mainProg
    {
        // logging with Stack for speed instead of heap
        struct DiceRollLog
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
            int runningTotal = 0;

            // Check for "d" in string as we expect dice-format
            int indexOfDelimiter = diceFormatString.ToLower().IndexOf('d');  // TODO duplicate check of "d" in string remove to lower BigO
            // Deconstruct for validation
            int numberOfSides = Convert.ToInt32(diceFormatString.Substring(indexOfDelimiter + 1));
            // Check if we even have an index for D and if not assume 1
            int numberOfDice = indexOfDelimiter == 0 ? 1: Convert.ToInt32(diceFormatString.Substring(0, indexOfDelimiter));

            // validate input
            if (numberOfDice < 1 )
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
                Console.WriteLine("Roll[" +(i+1)+"] was: " + roll + "     >"+ runningTotal);
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
                Console.WriteLine("DiceString: " + individualDiceRolls[i] + " >> " + diceRollResult);
            }

            // drop the List<T> into an array as we are done with dynamics here
            int[] diceIndividualRollsCast = diceIndividualRolls.ToArray();

            // Be default we just return the value of the requested string
            return (diceRollResult, diceIndividualRollsCast, userInputString);
        }

        static void Main(string[] args)
        {
            do
            {
                // Get user's input string
                Console.Write("Dice Roll Input String: ");
                string usersRollRequest = Console.ReadLine();

                // Make the requested rolls
                var (a,b,c) = decodeDiceString(usersRollRequest);

                // Build logging entry instance
                DiceRollLog diceLog = new DiceRollLog();
                diceLog.result = a;
                diceLog.resultParts = b;
                diceLog.inputString = c;
                //debug Console.WriteLine(string.Join(",",b) + b.Length);

                // Report to the user
                Console.WriteLine("You rolled a " + diceLog.result + " [" + diceLog.inputString + " >> " + string.Join(",", diceLog.resultParts) + "]");

            } while(true);
        }
    }
}
