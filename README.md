# DeveloperTest181217

Scenario:  
We have 2 directories which contain CSV files.  
We need to be able to determine which CSV files in the 1st directory are similar to CSV files in the 2nd directory.  

Definitions:  
Two directories, each containing CSV files.  
Each CSV files contains a sorted (Ascending) set of integer numbers (1 row only).  
A CSV file is considered “similar” to another CSV file if both files contain at least X identical numbers.  

Desired function:  
We want to copy all CSV files from directory "A", which are similar to at least 1 csv file in directory "B", to directory "C".  

Instruction:  
Write a class which handles, in multi-threading, the loading, "similarity" testing and copying of files  
The class should have a method which returns VOID and receives 3 strings (A,B & C, each a directory path) and an int (X, minimal amount of identical numbers).  
This method should also write a "scores.txt" file in directory C which contain a line for every copied file from directory A. The line should look like: [name of file] [tab] [maximum intersection score]
