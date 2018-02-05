using System.Collections.Generic;
using NLog.Layouts;

namespace NLog.MongoDB.NetCore
{
    /// <summary>
    /// NLog message target for MongoDB.
    /// </summary>
    public interface IMongoTarget
    {
        /// <summary>
        /// Gets or sets the connection string name string.
        /// </summary>
        /// <value>
        /// The connection name string.
        /// </value>
        string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the connection.
        /// </summary>
        /// <value>
        /// The name of the connection.
        /// </value>
        string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the default document format.
        /// </summary>
        /// <value>
        ///   <c>true</c> to use the default document format; otherwise, <c>false</c>.
        /// </value>
        bool IncludeDefaults { get; set; }

        /// <summary>
        /// Gets or sets the name of the database.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        Layout DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        Layout CollectionName { get; set; }

        /// <summary>
        /// Gets or sets the size in bytes of the capped collection.
        /// </summary>
        /// <value>
        /// The size of the capped collection.
        /// </value>
        long? CappedCollectionSize { get; set; }

        /// <summary>
        /// Gets or sets the capped collection max items.
        /// </summary>
        /// <value>
        /// The capped collection max items.
        /// </value>
        long? CappedCollectionMaxItems { get; set; }

        /// <summary>
        /// Gets the fields collection.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        IList<MongoField> Fields { get; }

        /// <summary>
        /// Gets the properties collection.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        IList<MongoField> Properties { get; }
    }
}
