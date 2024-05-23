using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using Microsoft.Data.SqlClient;

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

        // basic load/save features I want for storing/fetch data from remote servers SQL etc
        public interface IRemoteLogging
        {
            // really just doing this to practice as in this case we aren't likely to change interfaces
            bool SaveToServer();
            bool LoadFromServer();
        }

        // Logging tool that assists with keeping track of rolls and outputing th data
        public class LoggingTool : IRemoteLogging
        {
            public int logThreshold = 1;  // The threshold for something to be logged
            public int reportThreshold = 1;  // The threshold for something to be reported to use
            public string dataPath = "rolls.csv"; // Export/Import filename assuming working dir
            public string logPath = "rolls.log"; // Logging filename assuming working dir
            public List<DiceRollEntry> rollHistory = new List<DiceRollEntry>();  // Create a running log of each roll

            // SQL connection setting
            // https://learn.microsoft.com/en-us/azure/azure-sql/database/azure-sql-dotnet-quickstart?view=azuresql&tabs=visual-studio%2Cpasswordless%2Cservice-connector%2Cportal
            readonly private string _connectionString = "Server=tcp:adftestingsql.database.windows.net,1433;Initial Catalog=ADFtestingSQL;Persist Security Info=False;User ID=adftestingsqlADMIN;Password=6IiW+x'?hb%T=KqbaU'-;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=10;";


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
                    // start timer
                    Stopwatch sw = Stopwatch.StartNew();
                    sw.Start();
                    int entryCount = 0;

                    using (StreamWriter writer = new StreamWriter(this.dataPath, append: true))
                    {
                        foreach (DiceRollEntry roll in this.rollHistory.ToArray())
                        {
                            // increment our line counter
                            entryCount++;
                            // Write each roll request as it's own line
                            writer.WriteLine($"{roll.inputString},{roll.result},'{string.Join(",", roll.resultParts)}'");
                        }
                    }
                    this.Report($"9: Dice history saved successfully to {Directory.GetCurrentDirectory()}\\{this.dataPath}");
                    this.Report($"3: Wrote {entryCount} lines in {sw.ElapsedMilliseconds:0,000}ms");
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
                    this.Report("8: Error ConvertingToIntArray - NullEmpty");
                    return Array.Empty<int>();
                }

                // clean the inputs
                commaSeparatedString = commaSeparatedString.Replace("\'", "");
                commaSeparatedString = commaSeparatedString.Replace("\"", "");
                commaSeparatedString = commaSeparatedString.Replace(" ",  "");

                // split string and build array of matching length
                string[] stringArray = commaSeparatedString.Split(',');
                this.Report($"2: ConvertToIntArray converting {stringArray.Length} items");
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
                        this.Report($"4: Failed to parse string2int [{stringArray[i]}] from >> {commaSeparatedString}");
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
                    // start timer
                    Stopwatch sw = Stopwatch.StartNew();
                    sw.Start();
                    int entryCount = 0;

                    // open datastream dreader
                    using (StreamReader reader = new StreamReader(this.dataPath))
                    {
                        // create the line var and then loop for each new line from the stream
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            // increment our line counter
                            entryCount++;
                            // split the CSV for read in
                            string[] parts = line.Split(new char[] {','}, 3);
                            // Add CSV data back into the List<DiceRollEntry> type
                            this.rollHistory.Add(new DiceRollEntry { inputString = parts[0], result = Convert.ToInt32(parts[1]), resultParts = this.ConvertToIntArray(parts[2])});
                        }
                    }
                    this.Report($"9: Dice history loaded successfully from {Directory.GetCurrentDirectory()}\\{this.dataPath}");
                    this.Report($"3: Reading in {entryCount} lines in {sw.ElapsedMilliseconds:0,000}ms");
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

            internal string ConnectAndExecuteQuery(string queryString, bool expectResponse = true)
            {
                string responseString = "";

                try
                {

                    // open a mew Microsoft.Data.SQLClient.SqlConnection object for use
                    using (SqlConnection connection = new SqlConnection(this._connectionString))
                    {
                        // init the connection
                        connection.Open();

                        // instantiate a command object and pass the opened connection with a string/query 
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            if (expectResponse)
                            {
                                // open a response reader to confirm
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    // Check if we have a row to read here
                                    if (reader.HasRows)
                                    {
                                        // loop while we have something to read
                                        while (reader.Read())
                                        {
                                            
                                            // Process each row of data here
                                            // Access data using reader.GetString(0), reader.GetInt32(1), etc. (based on column data types)
                                            responseString += $"{reader.GetString(1)},{Convert.ToInt32(reader.GetInt32(2))},{reader.GetString(3)}\n";  // TODO change output of function to handle columns as an option
                                        }

                                        // trim the last return back off the string
                                        responseString = responseString.Substring(0, responseString.Length - 1);
                                    }
                                    else
                                    {
                                        Console.WriteLine("No data found.");
                                    }
                                }
                            }
                        }
                    }

                    return responseString;
                }
                catch (Exception ex) 
                {
                    Report($"9:Error: {queryString}");
                    Report($"9:Error with SQL Command above: {ex.Message}");
                    return string.Empty;
                }
            }

            public bool SaveToServer()
            {
                // SQL query to build table if it doesn't exist
                string stringToBuildTable = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RollResults')" +
                    "BEGIN " +
                        "CREATE TABLE dbo.RollResults (" +
                        "ID INT PRIMARY KEY IDENTITY(1, 1)," + // Auto-incrementing primary key
                        "RollString VARCHAR(255) NOT NULL," + // String to represent the roll\r\n  
                        "RollResult INT NOT NULL," + //Integer result of the roll
                        "RollResultParts VARCHAR(MAX)" +  //String to store comma-separated roll parts (alternative to array)
                    ");" +
                    "END;";
                ConnectAndExecuteQuery(stringToBuildTable);

                // SQL query to add an entry into the table
                string stringToAddToTable = "INSERT INTO dbo.RollResults (RollString, RollResult, RollResultParts) " +
                    $"VALUES ";

                // update user screen to help with feeling responsive
                this.Report($"3: Building SQL string ...");

                // build all-in-one SQL command to save data with
                try
                {
                    // start timer
                    Stopwatch sw = Stopwatch.StartNew();
                    sw.Start();
                    int entryCount = 0;

                    foreach (DiceRollEntry roll in this.rollHistory.ToArray())
                    {
                        // increment our line counter
                        entryCount++;
                        // Write each roll request as it's own line to our string
                        // we add values in here FORMATTED:  VALUES ('2d6', 8, '4,4'), (a,b,c);    etc
                        stringToAddToTable += $"('{roll.inputString}',{roll.result},'{string.Join(",", roll.resultParts)}'), ";
                    }
                    // simply trim the last "," replacing it with terminal ";" 
                    stringToAddToTable = stringToAddToTable.Substring(0, stringToAddToTable.Length-2) + ";";

                    this.Report($"2: STRING: {stringToAddToTable}");
                    this.Report($"3: Submitting SQL save string...");
                    ConnectAndExecuteQuery(stringToAddToTable);

                    this.Report($"9: Dice history saved successfully to RemoteServer");
                    this.Report($"3: Wrote {entryCount} lines in {sw.ElapsedMilliseconds:0,000}ms");
                    return true;
                }
                catch (Exception ex)
                {
                    this.Report($"9: ERROR SAVING2Server: {ex.Message}");
                    return false;
                }
            }

            public bool LoadFromServer()
            {
                // Loads the history from removeServer
                try
                {
                    // start timer
                    Stopwatch sw = Stopwatch.StartNew();
                    sw.Start();
                    int entryCount = 0;

                    // SQL query to fetch entire table
                    this.Report($"3: Fetching full table from SQL ...");
                    string response = ConnectAndExecuteQuery(queryString: "SELECT* FROM dbo.RollResults;");
                    this.Report($"3: Reading in data ...");

                    // loop for entire response
                    foreach (string line in response.Split('\n'))
                    {
                        Report($"3: LINE: {line}");
                        // increment our line counter
                        entryCount++;
                        // split the CSV for read in
                        string[] column = line.Split(new char[] { ',' }, 3);
                        // Add CSV data back into the List<DiceRollEntry> type
                        this.rollHistory.Add(new DiceRollEntry { inputString = column[0], result = Convert.ToInt32(column[1]), resultParts = this.ConvertToIntArray(column[2]) });

                    }

                    this.Report($"9: Dice history loaded successfully from RemoveServer");
                    this.Report($"3: Reading in {entryCount} lines in {sw.ElapsedMilliseconds:0,000}ms");
                    return true;
                }
                catch (Exception ex)
                {
                    this.Report($"ERROR LoadingRemoveServer: {ex.Message}");
                    return false;
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
        static bool REPLPrompt(out int userChoice, LoggingTool log)
        {
            // set default per requirement of using "out" type
            userChoice = -1;

            // Display to the user the options
            Console.WriteLine($"REPL menu [verbosity:{log.reportThreshold}]");
            Console.WriteLine("1 - Roll Dice");
            Console.WriteLine("2 - Save History");
            Console.WriteLine("3 - Load History");
            Console.WriteLine("4 - Display History");
            Console.WriteLine("5 - SaveToServer History");
            Console.WriteLine("6 - LoadFromServer History");
            Console.WriteLine("0 - Exit");
            Console.Write("Select action[0-6]: ");
            // Capture user input
            userChoice = ReadIntInput();

            return (0 <= userChoice && userChoice <= 6);
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
                if (!REPLPrompt(out currentAction, log))
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
                    case 5:
                        log.SaveToServer();
                        break;
                    case 6:
                        log.LoadFromServer();
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
