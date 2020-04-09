# CovidCore

# Software Requirements to run locally -

1. Visual Studio 2017 - I used Community Edition v15.9.21 - https://visualstudio.microsoft.com/vs/older-downloads/
2. .Net Core SDK 2.2.110-win-x64.exe - goto https://dotnet.microsoft.com/download/dotnet-core/2.2 search for 2.2.110 and download the Windows x64 version and install it after visual studio 
3. RabbitMQ - https://www.rabbitmq.com/install-windows.html

# Initial Config
1. Covid.Api\appsettings.json 
	- api requires access to a SqlServer db, the connection string needs inserting into this file
2. Covid.UserService\appsettings.json 
	- needs to be configured for the api's endpoint - ensure it has a trailing / and uses http protocol
	- needs to be configured for RabbitMq

# Database requirements

If running in Visual Studio - go to View -> SQL Server Object Explorer.  There should be something like (localdb)\MSSQLLocalDb.....  
Use this instance and put the connection string in the api config above

Run the following script in the Sql Server -

CREATE TABLE [dbo].[User] (
    [Id]        BIGINT        IDENTITY (1, 1) NOT NULL,
    [Firstname] NVARCHAR (50) NOT NULL,
    [Surname]   NVARCHAR (50) NOT NULL,
    [Dob]       DATETIME      NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_User_Id]
    ON [dbo].[User]([Id] ASC);

# Rabbit requirements

Set the following up in Rabbit

Queue: CreateUser
Exchange: User
RoutingKey: CreateUser

Queue: User
Exchange: User
RoutingKey: User

# Scenario

1. Publish a message with the following specification on to the "CreateUser" queue -

{
"Firstname":"Joe",
"Surname":"Bloggs",
"DateOfBirth":"2018/06/05T13:14:15",
}

2. The UserService reads this message, alls the api/users/create endpoint to persist the user in the database returning the id in the response

3. The UserService re-queries the api on api/users/{id} to re-retrieve the user details

4. It then publishes a message similar to above but including the record id on to the "User" queue.

5. To verify this has happened view the "User" queue in RabbitMq, can also check the User table in the database

** repeated publishing of messages will continually trigger the logic below

