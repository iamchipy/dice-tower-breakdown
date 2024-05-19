using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    
    internal class mainProg
    {

        

  
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
        static public int diceRollString(string diceFormatString = "2d20")
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

            // now we call roll for each dice that needs rolled
            for (int i = 0; i < numberOfDice; i++)
            {
                // TODO build in logging Sprint 3
                int roll = diceRollD(numberOfSides);
                Console.WriteLine("Roll[" +(i+1)+"] was: " + roll + "     >"+ runningTotal);
                runningTotal += roll;
            }

            return runningTotal;
        }

        // Decodes user input
        // Accepts string formatted like 2d20+d6+3 (Foundry Virtual Table Top format)
        // Returns total result for requested role
        static public int decodeDiceString(string userInputString = "d20", bool showYourWork=false)
        {
            int diceRollResult = 0;
            //int parsedInt;

            // deconstruct the string to determine how many dice to throw
            var individualDiceRolls = userInputString.ToLower().Split('+');

            // Loop for each entry in the input string
            for (int i = 0;i< individualDiceRolls.Length; i++)
            {
                Console.WriteLine("DiceString: " + individualDiceRolls[i]);
                Console.WriteLine(diceRollString(individualDiceRolls[i]));
            }

            // Be default we just return the value of the requested string
            return diceRollResult;
        }

        static void Main(string[] args)
        {
            do
            {
                // Get user's input string
                Console.Write("Dice Roll Input String: ");
                string usersRollRequest = Console.ReadLine();

                // Make the requested rolls
                int finalRoll = decodeDiceString(usersRollRequest);

                // Report to the user
                Console.WriteLine("You rolled a " + finalRoll + "[" + "PARTS" + "]");

            } while(true);
        }
    }
}
