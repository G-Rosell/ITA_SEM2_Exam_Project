﻿using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Concurrent;
using VagtplanApp.Shared.Model;

namespace VagtplanApp.Server.Repositories
{
    public class LogInRepository : ILogInRepository
    {
        private readonly IMongoCollection<LogIn> LogInCollection;

        public LogInRepository()
        {
            MongoClient client = new MongoClient(@"mongodb+srv://Adgang:ViSkalHaveAdgang123@cluster0.2szl4mg.mongodb.net/");
            IMongoDatabase database = client.GetDatabase("festival");
            LogInCollection = database.GetCollection<LogIn>("logins");
        }

        public List<LogIn> GetAll()
        {
            // Finder alle log-ins i MongoDB-samlingen og gemmer dem i en liste
            var LogInList = LogInCollection.Find(new BsonDocument()).ToList();

            // Returnere listen af log-ins.
            return LogInList;
        }

        // Tilføj her flere metoder hvis nødvendigt.
    }
}
