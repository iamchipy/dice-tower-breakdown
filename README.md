# Dice Tower Breakdown
This is a dice rolling tool to created large tables of data 
for testing C# structure storage speeds and Random's seed options

## Features
- REPL menu to help navigate a CLI/ConsoleApp:
- ![image](https://github.com/iamchipy/dice-tower-breakdown/assets/1663877/16f743d1-bcfb-416e-8773-412af4def3a7)
- Primary function (rolling any number of dice in FVTT format [2d20+5d5+d3+1]):
- ![image](https://github.com/iamchipy/dice-tower-breakdown/assets/1663877/1ae07133-4291-4f8b-8709-d1cccefc7070)
- Display History of dice strings that were completed:
- ![image](https://github.com/iamchipy/dice-tower-breakdown/assets/1663877/ce7cdf6d-6cee-4c5f-a35a-3f43b6f42efb)
- Also includes ability to save and load dice rolled history both to local file or remove SQL server (currenly Azure SQL)




# Roadmap 
### Sprint 1
- [x] Create roadmap
- - [X] Sprint out lines
- [x] Create repository 
- [x] Build solution folder structure 

### Sprint 2
- [x] Create initial Roll function
- - [x] Create output for logging

### Sprint 3
- [x] Create storage collections
- [x] Create roll logging feature

### Sprint 4
- [x] Primary input validation
- - [x] Handle flat modifiers to the value
- [x] Create timing benchmark system

### Sprint 5
- [x] Create REPL for other options
- [x] Add verbosity levels to increase speed
- - [x] Refactor report to be class 
- [x] Correct project structure

### Sprint 6
- [x] Create display feature
- [x] Create output/save feature
- [x] Create input/load feature

### Sprint 7
- [x] Compare collections types
- - [x] Construct timers for each step in the process
- [x] SQL bridge interface

### Sprint 8
- [ ] Add custom errors for dice exceptions
- [ ] Add edge case validations for RegEx


# Benchmarks

### 10x d20 List<DiceRollEntry>
Wrote 10 lines in 0,003ms
Reading in 10 lines in 0,011ms

### 10x 10d20 List<DiceRollEntry>
runTimer: >> 146ms-255ms
Wrote 10 lines in 0,003ms
Wrote 10 lines in 0,004ms
Reading in 10 lines in 0,026ms
Reading in 10 lines in 0,017ms
Reading in 10 lines in 0,016ms

### 100d20 List<DiceRollEntry>
runTimer: >> 1,564ms
Wrote 1 lines in 0,002ms
Reading in 1 lines in 0,003ms
Reading in 1 lines in 0,003ms
Reading in 1 lines in 0,013ms

### 10x 100d20 List<DiceRollEntry>
runTimer: >> 1,525ms-1,508ms
Wrote 10 lines in 0,003ms (3x)
Reading in 10 lines in 0,017ms
Reading in 10 lines in 0,018ms
Reading in 10 lines in 0,107ms

### 1000d20 List<DiceRollEntry>
runTimer: >> 15,398ms  (with display)
runTimer: >> 15,209ms  (without display)
