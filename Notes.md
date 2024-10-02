#Please change the following

1- Change Sql connection string
2- Update the OT.Assessment.Tester Api end-points accordingly
3- Run Docker-compose up to initiate RabbitMQ
4- Run dotnet clean,build and run on every project
5- Before running the solution please comment the databaseGenerate file and Notes to avoid build failed.
6- Packages to install: Microsoft.EntityFrameworkCore.SqlServer
                        Microsoft.EntityFrameworkCore.Tools
                        RabbitMQ.Client
                        Microsoft.Extensions.Configuration
                        Microsoft.Extensions.Hosting
                        Microsoft.Extensions.Http
                        Microsoft.Extensions.Logging.Console


Changes and Challenges

#Bonus Changes

1- Dead Letter Exchange (DLX): Implement a Dead Letter Queue (DLQ) for handling failed messages in       RabbitMQ.

2- Indexs, Adding these would improve query performance.

3- Player Existence Check: The solution handles the case where a player might not exist when receiving a wager.

4- Duplicate Message Prevention: The consumer checks for existing wagers before inserting them, which avoids duplicates.

5- Error Handling: In RabbitMQ consumer, implement a retry mechanism for failed message processing and consider adding a dead-letter queue to handle unprocessed messages.

6- Containerization: docker-compose.yml setup, enabling easier deployment.

7- Health Check, created to check the status connection on both RabbitMQ and SQL 


#Challenges

1- Firewall or Security Software
2- Update NBomber Client Configuration
3- Self-Signed SSL Certificate for Development
4- Connection not found for testing


#Architecture and project structure

OT.Assessment/
│
├── OT.Assessment.App/
│   ├── Controllers/
│   │   └── HealthController.cs
|   |   └── PlayerController.cs 
│   ├── Data/
│   │   └── ApplicationDbContext.cs
│   ├── Models/
│   │   ├── CasinoWager.cs
│   │   └── Player.cs
│   ├── Migrations/
│   ├── Program.cs
│   └── appsettings.json
│
├── OT.Assessment.Consumer/
│   ├── Services/
│   │   └── ConsumerService.cs
│   ├── Data/
│   │   └── ApplicationDbContext.cs
│   ├── Models/
│   │   └── CasinoWager.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── OT.Assessment.Tester/
│   └── Program.cs
│
├── OT.Assessment.Database/
│   └── DatabaseGenerate.sql
│
├── docker-compose.yml
├── README.md
└── .gitignore
