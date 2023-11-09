# Extract Azure EventHub Metrics from Azure Monitor

### Environment Variables

The following environment variables are required to successfully execute the application.

| Name| Value |
|-----------------------|-----------|
|DOTNET_ENVIRONMENT|Development|
|AZURE_TENANT_ID|The Azure Tenant ID|
|AZURE_CLIENT_ID|The Client ID of an Application registered in Microsoft Entra ID (formerly Azure AD)|
|AZURE_CLIENT_SECRET|The client secret of the Application|
|AZURE_SUBSCRIPTIONS|The subscriptions to be scanned|
|SQLDB_HostName|Microsoft SQL Server Host Name|
|SQLDB_DatabaseName|The database in the SQL Server|
|SQLDB_UserID|SQL Login ID|
|SQLDB_Password|Password for the SQL Login|
|SQLDB_SaveToTable|A boolean value to whether the extracted information to stored in a SQL table (table schema provided below) - Acceptable values are true or false"

````

CREATE TABLE [dbo].[EventHub_Metrics](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[eventhub_namespace_id] [varchar](1024) NULL,
	[eventhub_namespace_name] [varchar](256) NULL,
	[entity_name] [varchar](256) NULL,
	[time_stamp] [datetimeoffset](7) NULL,
	[metric_name] [varchar](128) NULL,
	[aggregation_type] [varchar](64) NULL,
	[metric_value] [int] NULL,
	[insert_time_stamp] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

````