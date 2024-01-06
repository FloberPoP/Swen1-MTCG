# Swen1-MTCG
Repository for Software Engineering 1 Labor BIF3-A1/A2 Project Monster Trading Cards Game (MTCG) 

#Docker Container DB User:
docker run -d --name Test_Container -p 5432:5432 -e POSTGRES_DB=mtcgdb -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=debian123 postgres:latest

host = "localhost";
port = "5432";
database = "mtcgdb";
username = "postgres";
password = "debian123";

Für Testing in Seed gibt es folgende Methoden:
ClearDatabase(); -> Cleared die DB
PrintTableContents(); -> GIbt alle Tabellen + inhalt aus
Vor und nach Unittests wird DB automatisch gecleared

Git:
https://github.com/FloberPoP/Swen1-MTCG.git