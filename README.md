NLog.MongoDB.NetCore
====================
[![NuGet](https://img.shields.io/nuget/v/NLog.MongoDB.NetCore.svg)](https://www.nuget.org/packages/NLog.MongoDB.NetCore/)
[![Build status](https://ci.appveyor.com/api/projects/status/p56vnc4gkiroydgx?svg=true)](https://ci.appveyor.com/project/ByNeo/nlog-mongodb-netcore)
[![Build Status](https://travis-ci.org/ByNeo/NLog.MongoDB.NetCore.svg?branch=master)](https://travis-ci.org/ByNeo/NLog.MongoDB.NetCore)
[![Join the chat at https://gitter.im/NLogMongoDBNetCore/NLogMongoDBNetCore](https://img.shields.io/gitter/room/nlogmongodbnetcore/nlogmongodbnetcore.svg?colorB=46BC99)](https://gitter.im/NLogMongoDBNetCore/NLogMongoDBNetCore)

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

#### NLog.config target

```xml
<target xsi:type="Mongo"
        name="mongoDefault"
        connectionString="mongodb://localhost/Logging"
        collectionName="DefaultLog"
        cappedCollectionSize="26214400">
  <property name="ThreadID" layout="${threadid}" bsonType="Int32" />
  <property name="ThreadName" layout="${threadname}" />
  <property name="ProcessID" layout="${processid}" bsonType="Int32" />
  <property name="ProcessName" layout="${processname:fullName=true}" />
  <property name="UserName" layout="${windows-identity}" />
</target>
```

### Complete Custom Document

#### NLog.config target

```xml
<target xsi:type="Mongo"
        name="mongoCustom"
        includeDefaults="false"
        connectionString="mongodb://localhost"
        collectionName="CustomLog"
        databaseName="Logging"
        cappedCollectionSize="26214400">
  <field name="Date" layout="${date}" bsonType="DateTime" />
  <field name="Level" layout="${level}"/>
  <field name="Message" layout="${message}" />
  <field name="Logger" layout="${logger}"/>
  <field name="Exception" layout="${exception:format=tostring}" />
  <field name="ThreadID" layout="${threadid}" bsonType="Int32" />
  <field name="ThreadName" layout="${threadname}" />
  <field name="ProcessID" layout="${processid}" bsonType="Int32" />
  <field name="ProcessName" layout="${processname:fullName=true}" />
  <field name="UserName" layout="${windows-identity}" />
</target>
```

#### Custom Output JSON

```JSON
{
    "_id": {
        "$oid": "594b8db3b65a8d0db4204123"
    },
    "Date": {
        "$date": "2017-06-22T09:28:19.863Z"
    },
    "Level": "Info",
    "Message": "LogLevel.Info: , k=1071, l=1299",
    "Logger": "NLog.MongoDB.NetCore.ConsoleTest.Program",
    "ThreadID": 1,
    "ProcessID": 3508,
    "ProcessName": "D:\\SPECIALS\\PROJECTS\\NLog.MongoDB.NetCore\\tests\\NLog.MongoDB.NetCore.ConsoleTest\\bin\\Debug\\netcoreapp1.0\\win10-x64\\NLog.MongoDB.NetCore.ConsoleTest.exe",
    "UserName": "kaan.has"
}
```
