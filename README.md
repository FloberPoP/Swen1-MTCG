# Swen1-MTCG
Repository for Software Engineering 1 Labor BIF3-A1/A2 Project Monster Trading Cards Game (MTCG) 

#Docker Container DB User:
string host = "localhost";
string port = "5432";
string database = "mtcgdb";
string username = "postgres";
string password = "debian123";

Für Testing in Seed gibt es folgende Methoden:
ClearDatabase(); -> Cleared die DB
PrintTableContents(); -> GIbt alle Tabellen + inhalt aus
Vor und nach Unittests wird DB automatisch gecleared

Git:
https://github.com/FloberPoP/Swen1-MTCG.git