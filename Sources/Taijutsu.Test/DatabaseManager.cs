// Copyright 2009-2013 Nikita Govorov
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

namespace Taijutsu.Test
{
    public static class DatabaseManager
    {
        // ReSharper disable ConvertToConstant.Global
        // ReSharper disable MemberCanBePrivate.Global

        public static readonly string SlqServer = @"(localdb)\v11.0";

        public static readonly string EmptyDatabase = "EmptyDatabase";

        public static readonly string TestDatabase = "TestDatabase";

        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore ConvertToConstant.Global

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

                var workingFolderPath = AppDomain.CurrentDomain.BaseDirectory; // Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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

            return new SqlConnectionStringBuilder(modifiedConnectionString) { AttachDBFilename = string.Empty }.ToString();
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
                    sqlTemplate =
                        "if exists(select * from sys.databases where name = '{0}')  begin; alter database {0} set offline with rollback immediate; alter database {0} set online; drop database {0}; end;";
                }
                else
                {
                    sqlTemplate =
                        "if exists(select * from sys.databases where name = '{0}')  begin; alter database {0} set offline with rollback immediate; alter database {0} set online; exec sp_detach_db '{0}'; end;";
                }

                // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
                using (var command = new SqlCommand(string.Format(sqlTemplate, database), connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private static string ExtractInitialCatalog(string database = "", string connectionString = "")
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = connectionStringBuilder.InitialCatalog ?? string.Empty;
            return database == string.Empty ? (string.IsNullOrWhiteSpace(databaseName) ? TestDatabase : databaseName) : database;
        }

        private static string BuildConnectionString(string database = "", string server = "", string file = "", string connectionString = "")
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

            var serverName = connectionStringBuilder.DataSource ?? string.Empty;
            connectionStringBuilder.DataSource = server == string.Empty ? (string.IsNullOrWhiteSpace(serverName) ? SlqServer : serverName) : server;

            connectionStringBuilder.InitialCatalog = ExtractInitialCatalog(database: database, connectionString: connectionString);

            connectionStringBuilder.IntegratedSecurity = true;

            if (file == string.Empty || file != null)
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
                connectionStringBuilder.AttachDBFilename = string.Empty;
            }

            return connectionStringBuilder.ToString();
        }
    }
}