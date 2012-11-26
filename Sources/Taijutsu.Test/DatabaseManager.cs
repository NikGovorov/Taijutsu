using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

namespace Taijutsu.Test
{
    public class DatabaseManager
    {
        // ReSharper disable ConvertToConstant.Global
        public static string SlqServer = @"(localdb)\v11.0";
        public static string EmptyDatabase = "EmptyDatabase";
        public static string TestDatabase = "TestDatabase";
        // ReSharper restore ConvertToConstant.Global

        private static string ExtractInitialCatalog(string database = "", string connectionString = "")
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = connectionStringBuilder.InitialCatalog ?? string.Empty;
            return database == "" ? (string.IsNullOrWhiteSpace(databaseName) ? TestDatabase : databaseName) : database;
        }

        private static string BuildConnectionString(string database = "", string server = "", string file = "", string connectionString = "")
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

            var serverName = connectionStringBuilder.DataSource ?? string.Empty;
            connectionStringBuilder.DataSource = server == "" ? (string.IsNullOrWhiteSpace(serverName) ? SlqServer : serverName) : server;

            connectionStringBuilder.InitialCatalog = ExtractInitialCatalog(database: database, connectionString: connectionString);

            connectionStringBuilder.IntegratedSecurity = true;

            if (file == "" || file != null)
            {
                string databaseFilePath;

                if (file.IndexOf(".", StringComparison.Ordinal) > 0)
                {
                    databaseFilePath = file;
                }
                else
                {
                    var workingFolderPath = AppDomain.CurrentDomain.BaseDirectory;
                    // ReSharper disable AssignNullToNotNullAttribute
                    databaseFilePath = Path.Combine(workingFolderPath, (string.IsNullOrEmpty(file) ? EmptyDatabase : file) + ".mdf");
                    // ReSharper restore AssignNullToNotNullAttribute    
                }

                connectionStringBuilder.AttachDBFilename = databaseFilePath;
            }
            else
            {
                connectionStringBuilder.AttachDBFilename = "";
            }

            return connectionStringBuilder.ToString();
        }

        public static string PrepareDatabase(string database = "", string connectionString = "", string file = "", bool dynamicFile = true, bool dynamicDatabase = true)
        {
            var newDataFilePath = string.Empty;
            var random = "_" + DateTime.Now.Ticks;
            if (dynamicFile)
            {
                if (file == string.Empty)
                {
                    file = EmptyDatabase;
                }

                file += random;


                var workingFolderPath = AppDomain.CurrentDomain.BaseDirectory;// Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                // ReSharper disable AssignNullToNotNullAttribute
                var originalDataFilePath = Path.Combine(workingFolderPath, EmptyDatabase + ".mdf");
                newDataFilePath = Path.Combine(Path.GetTempPath(), file + ".mdf");
                // ReSharper restore AssignNullToNotNullAttribute
                File.Copy(originalDataFilePath, newDataFilePath);
                file = newDataFilePath;
            }

            if (dynamicDatabase)
            {
                database = ExtractInitialCatalog(database: database, connectionString: connectionString);
                database += random;
            }

            var modifiedConnectionString = BuildConnectionString(database: database, file: file, connectionString: connectionString);

            try
            {
                using (var connection = new SqlConnection(modifiedConnectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("select * from master.sys.tables", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                try
                {
                    File.Delete(newDataFilePath);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                }
                throw;
            }
            return new SqlConnectionStringBuilder(modifiedConnectionString) { AttachDBFilename = "" }.ToString();
        }

        public static void DropDatabase(string database = "", string connectionString = "", bool dropFiles = true)
        {
            using (var connection = new SqlConnection(BuildConnectionString(file: null, database: "master", connectionString: connectionString)))
            {
                connection.Open();

                database = ExtractInitialCatalog(database: database, connectionString: connectionString);

                string sqlTemplate;

                // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
                if (dropFiles)
                {
                    sqlTemplate = "if exists(select * from sys.databases where name = '{0}')  begin; alter database {0} set offline with rollback immediate; alter database {0} set online; drop database {0}; end;";
                }
                else
                {
                    sqlTemplate = "if exists(select * from sys.databases where name = '{0}')  begin; alter database {0} set offline with rollback immediate; alter database {0} set online; exec sp_detach_db '{0}'; end;";
                }
                // ReSharper restore ConvertIfStatementToConditionalTernaryExpression

                using (var command = new SqlCommand(string.Format(sqlTemplate, database), connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void CreateSchema(string connectionString, string schema)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(string.Format("if not exists(select 1 from INFORMATION_SCHEMA.SCHEMATA where SCHEMA_NAME='{0}') exec ('create schema [{0}]')", schema), connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}