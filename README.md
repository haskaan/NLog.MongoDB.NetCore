NLog.MongoDB.NetCore
====================
An NLog target for MongoDB persistance on the .NET Core platform.


Download
----------
The NLog.MongoDB.NetCore library is available on nuget.org via package name `NLog.MongoDB.NetCore`.

To install NLog.MongoDB.NetCore, run the following command in the Package Manager Console

    PM> Install-Package NLog.MongoDB.NetCore
    
More information about NuGet package avaliable at
<https://nuget.org/packages/NLog.MongoDB.NetCore>


Configuration Syntax
--------------------
```xml
<extensions>
  <add assembly="NLog.MongoDB.NetCore"/>
</extensions>

<targets>
  <target xsi:type="Mongo"
          name="String"
          connectionName="String"
          connectionString="String"
          collectionName="String"
          cappedCollectionSize="Long"
          cappedCollectionMaxItems="Long"
          databaseName="String"
          includeDefaults="Boolean">
    
    <!-- repeated --> 
    <field name="String" layout="Layout" bsonType="Boolean|DateTime|Double|Int32|Int64|String"  />
    
    <!-- repeated --> 
    <property name="String" layout="Layout" bsonType="Boolean|DateTime|Double|Int32|Int64|String"  />
  </target>
</targets>
```


Parameters
----------
### General Options

_name_ - Name of the target.

### Connection Options

_connectionName_ - The name of the connection string to get from the config file. 

_connectionString_ - Connection string. When provided, it overrides the values specified in connectionName. 

_databaseName_ - The name of the database, overrides connection string database.

### Collection Options
_collectionName_ - The name of the MongoDB collection to write logs to.  

_cappedCollectionSize_ - If the collection doesn't exist, it will be create as a capped collection with this max size.

_cappedCollectionMaxItems_ - If the collection doesn't exist, it will be create as a capped collection with this max number of items.  _cappedCollectionSize_ must also be set when using this setting.

### Document Options

_includeDefaults_ - Specifies if the default document is created when writing to the collection.  Defaults to true.

_field_ - Specifies a root level document field. There can be multiple fields specified.

_property_ - Specifies a dictionary property on the Properties field. There can be multiple properties specified.


Examples
--------
### Default Configuration with Extra Properties
