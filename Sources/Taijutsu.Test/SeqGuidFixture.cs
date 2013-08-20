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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using SharpTestsEx;

namespace Taijutsu.Test
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SeqGuidFixture
    {
        private const int Count = 500;

        [TearDown]
        public void OnTearDown()
        {
            SystemTime.Reset();
        }

        [Test]
        public void ShouldBeUnique()
        {
            var generatedIds = new ConcurrentBag<Guid>();

            var tasks = new List<Task>();

            var i = Count;
            while (i-- > 0)
            {
                tasks.Add(
                    Task.Factory.StartNew(
                        () =>
                        {
                            for (var j = 0; j < 100; j++)
                            {
                                var id = SeqGuid.NewGuid();
                                generatedIds.Add(id);
                            }
                        }));
            }

            Task.WaitAll(tasks.ToArray());

            Assert.That(new HashSet<Guid>(generatedIds).Count, Is.EqualTo(Count * 100));
        }

        [Test]
        public virtual void ShouldBeCorrelatedWithGenerationTime()
        {
            SystemTime.TimeController.Customize(() => new DateTime(1983, 12, 19, 22, 10, 59));

            var dt1 = SystemTime.Now;

            var id1 = SeqGuid.NewGuid();

            Assert.That(dt1, Is.EqualTo(SeqGuid.ToDateTime(id1)).Within(1).Milliseconds);
        }

        [Test]
        [Explicit]
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed. Acceptable for tests.")]
        public void ShouldBeSequental()
        {
            var connectionString = DatabaseManager.PrepareDatabase("SeqGuidTest");
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "create table [SeqGuid] ([Key] uniqueidentifier not null primary key clustered ([Key] asc));";
                        command.ExecuteNonQuery();
                    }

                    var generatedIds = new List<Guid>();
                    var insertedIds = new List<Guid>();

                    using (var command = connection.CreateCommand())
                    {
                        var insertBuilder = new StringBuilder();
                        for (var i = 0; i < Count; i++)
                        {
                            var counter = i;
                            SystemTime.TimeController.Customize(() => new DateTime(2000, 1, 1, 1, 1, 1).AddMilliseconds(counter * 4));
                            var id = SeqGuid.NewGuid();
                            insertBuilder.AppendLine(string.Format("insert into [SeqGuid] ([Key]) values('{0}');", id));
                            generatedIds.Add(id);
                        }

                        command.CommandText = insertBuilder.ToString();
                        command.ExecuteNonQuery();
                    }

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "select [Key] from [SeqGuid]";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                insertedIds.Add(reader.GetGuid(0));
                            }
                        }
                    }

                    insertedIds.Should().Have.SameSequenceAs(generatedIds);
                }
            }
            finally
            {
                DatabaseManager.DropDatabase(connectionString: connectionString);
            }
        }
    }
}