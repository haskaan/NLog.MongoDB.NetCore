using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NLog.Layouts;

namespace NLog.MongoDB.NetCore
{
    /// <summary>
    /// NLog message target for MongoDB.
    /// </summary>
    [Target("Mongo")]
    public class MongoTarget : Target, IMongoTarget
    {
        static MongoTarget()
        {
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
        }

        #region Fields

        private static readonly ConcurrentDictionary<string, IMongoCollection<BsonDocument>> _collectionCache = new ConcurrentDictionary<string, IMongoCollection<BsonDocument>>();

        /// <summary>
        /// Gets the fields collection.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        [ArrayParameter(typeof(MongoField), "field")]
        public IList<MongoField> Fields { get; private set; }

        /// <summary>
        /// Gets the properties collection.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        [ArrayParameter(typeof(MongoField), "property")]
        public IList<MongoField> Properties { get; private set; }

        /// <summary>
        /// Gets or sets the connection string name string.
        /// </summary>
        /// <value>
        /// The connection name string.
        /// </value>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the connection.
        /// </summary>
        /// <value>
        /// The name of the connection.
        /// </value>
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the default document format.
        /// </summary>
        /// <value>
        ///   <c>true</c> to use the default document format; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeDefaults { get; set; }

        /// <summary>
        /// Gets or sets the name of the database.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        public Layout DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        public Layout CollectionName { get; set; }

        /// <summary>
        /// Gets or sets the size in bytes of the capped collection.
        /// </summary>
        /// <value>
        /// The size of the capped collection.
        /// </value>
        public long? CappedCollectionSize { get; set; }

        /// <summary>
        /// Gets or sets the capped collection max items.
        /// </summary>
        /// <value>
        /// The capped collection max items.
        /// </value>
        public long? CappedCollectionMaxItems { get; set; }

        #endregion


        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoTarget"/> class.
        /// </summary>
        public MongoTarget()
        {
            Fields = new List<MongoField>();
            Properties = new List<MongoField>();
            IncludeDefaults = true;
        }

        #endregion


        #region Overrides

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        /// <exception cref="NLog.NLogConfigurationException">Can not resolve MongoDB ConnectionString. Please make sure the ConnectionString property is set.</exception>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (!string.IsNullOrEmpty(ConnectionName))
                ConnectionString = GetConnectionString(ConnectionName);

            if (string.IsNullOrEmpty(ConnectionString))
                throw new NLogConfigurationException("Can not resolve MongoDB ConnectionString. Please make sure the ConnectionString property is set.");

        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            if (logEvents.Length == 0)
                return;

            try
            {
                var groupings = logEvents
                    .Select(e => Tuple.Create(CreateDocument(e.LogEvent),
                        GetCollection(e.LogEvent)))
                    .GroupBy(tuple => tuple.Item2);

                foreach (var grouping in groupings)
                    grouping.Key.InsertMany(grouping.Select(g => g.Item1));

                foreach (var ev in logEvents)
                    ev.Continuation(null);

            }
            catch (Exception ex)
            {
                if (ex is OutOfMemoryException || ex is NLogConfigurationException)
                    throw;

                InternalLogger.Error("Error when writing to MongoDB {0}", ex);

                foreach (var ev in logEvents)
                    ev.Continuation(ex);

            }
        }

        /// <summary>
        /// Writes logging event to the log target.
        /// classes.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                var document = CreateDocument(logEvent);
                var collection = GetCollection(logEvent);
                collection.InsertOne(document);
            }
            catch (Exception ex)
            {
                if (ex is OutOfMemoryException || ex is NLogConfigurationException)
                    throw;

                InternalLogger.Error("Error when writing to MongoDB {0}", ex);
            }
        }

        #endregion


        #region Private Methods

        private BsonDocument CreateDocument(LogEventInfo logEvent)
        {
            var document = new BsonDocument();
            if (IncludeDefaults || Fields.Count == 0)
                AddDefaults(document, logEvent);

            // extra fields
            foreach (var field in Fields)
            {
                var value = GetValue(field, logEvent);
                if (value != null)
                    document[field.Name] = value;
            }

            AddProperties(document, logEvent);

            return document;
        }

        private void AddDefaults(BsonDocument document, LogEventInfo logEvent)
        {
            document.Add("Date", new BsonDateTime(logEvent.TimeStamp));

            if (logEvent.Level != null)
                document.Add("Level", new BsonString(logEvent.Level.Name));

            if (logEvent.LoggerName != null)
                document.Add("Logger", new BsonString(logEvent.LoggerName));

            if (logEvent.FormattedMessage != null)
                document.Add("Message", new BsonString(logEvent.FormattedMessage));

            if (logEvent.Exception != null)
                document.Add("Exception", CreateException(logEvent.Exception));


        }

        private void AddProperties(BsonDocument document, LogEventInfo logEvent)
        {
            var propertiesDocument = new BsonDocument();
            foreach (var field in Properties)
            {
                string key = field.Name;
                var value = GetValue(field, logEvent);

                if (value != null)
                    propertiesDocument[key] = value;
            }

            var properties = logEvent.Properties ?? Enumerable.Empty<KeyValuePair<object, object>>();
            foreach (var property in properties)
            {
                if (property.Key == null || property.Value == null)
                    continue;

                string key = Convert.ToString(property.Key, CultureInfo.InvariantCulture);
                string value = Convert.ToString(property.Value, CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(value))
                    propertiesDocument[key] = new BsonString(value);
            }

            if (propertiesDocument.ElementCount > 0)
                document.Add("Properties", propertiesDocument);

        }

        private BsonValue CreateException(Exception exception)
        {
            if (exception == null)
                return BsonNull.Value;

            var document = new BsonDocument();
            document.Add("Message", new BsonString(exception.Message));
            document.Add("BaseMessage", new BsonString(exception.GetBaseException().Message));
            document.Add("Text", new BsonString(exception.ToString()));
            document.Add("Type", new BsonString(exception.GetType().ToString()));

            return document;
        }


        private BsonValue GetValue(MongoField field, LogEventInfo logEvent)
        {
            var value = field.Layout.Render(logEvent);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = value.Trim();

            if (string.IsNullOrEmpty(field.BsonType)
                || string.Equals(field.BsonType, "String", StringComparison.OrdinalIgnoreCase))
                return new BsonString(value);


            BsonValue bsonValue;
            if (string.Equals(field.BsonType, "Boolean", StringComparison.OrdinalIgnoreCase)
                && MongoConvert.TryBoolean(value, out bsonValue))
                return bsonValue;

            if (string.Equals(field.BsonType, "DateTime", StringComparison.OrdinalIgnoreCase)
                && MongoConvert.TryDateTime(value, out bsonValue))
                return bsonValue;

            if (string.Equals(field.BsonType, "Double", StringComparison.OrdinalIgnoreCase)
                && MongoConvert.TryDouble(value, out bsonValue))
                return bsonValue;

            if (string.Equals(field.BsonType, "Int32", StringComparison.OrdinalIgnoreCase)
                && MongoConvert.TryInt32(value, out bsonValue))
                return bsonValue;

            if (string.Equals(field.BsonType, "Int64", StringComparison.OrdinalIgnoreCase)
                && MongoConvert.TryInt64(value, out bsonValue))
                return bsonValue;

            return new BsonString(value);
        }

        private IMongoCollection<BsonDocument> GetCollection(LogEventInfo logEvent)
        {
            string connectionName = ConnectionName ?? string.Empty;
            string connectionString = ConnectionString ?? string.Empty;

            var mongoUrl = new MongoUrl(connectionString);
            string databaseName = DatabaseName.Render(logEvent) ?? mongoUrl.DatabaseName ?? "NLog";
            string collectionName = CollectionName.Render(logEvent) ?? "Log";

            // cache mongo collection based on target name.
            string key = $"k|{connectionName}|{connectionString}|{databaseName}|{collectionName}";

            return _collectionCache.GetOrAdd(key, k =>
            {
                // create collection
                var client = new MongoClient(mongoUrl);

                // Database name overrides connection string
                var database = client.GetDatabase(databaseName);

                if (!CappedCollectionSize.HasValue || CollectionExists(database, collectionName))
                    return database.GetCollection<BsonDocument>(collectionName);

                // create capped
                var options = new CreateCollectionOptions
                {
                    Capped = true,
                    MaxSize = CappedCollectionSize,
                    MaxDocuments = CappedCollectionMaxItems
                };

                database.CreateCollection(collectionName, options);

                return database.GetCollection<BsonDocument>(collectionName);
            });
        }

        private static bool CollectionExists(IMongoDatabase database, string collectionName)
        {
            var options = new ListCollectionsOptions
            {
                Filter = Builders<BsonDocument>.Filter.Eq("name", collectionName)
            };

            return database.ListCollections(options).ToEnumerable().Any();
        }

        private static string GetConnectionString(string name)
        {
            var value = GetEnvironmentVariable(name);
            if (!string.IsNullOrEmpty(value))
                return value;
#if NET45
            var connectionString = ConfigurationManager.ConnectionStrings[name];
            return connectionString?.ConnectionString;
#else
            IConfigurationRoot configuration;
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            configuration = builder.Build();
            return configuration[name];
#endif

        }

        private static string GetEnvironmentVariable(string name)
        {
            return string.IsNullOrEmpty(name) ? null : Environment.GetEnvironmentVariable(name);
        }

        #endregion
    }
}
