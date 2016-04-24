using DatabaseBackup.ContractsDAL;
using DatabaseBackup.Entities;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseBackup.DAL
{
    public class DBDao : IDao
    {
        private string rootDir = Path.Combine(Path.GetTempPath(), "DatabaseBackups");

        public void Backup(string conString, string pathToFile)
        {
            DBDatabase database;
            string databaseName = Regex.Match(conString, @"\b(Database|Initial Catalog)=""(.+?)"";").Groups[2].Value;
            using (var connection = new SqlConnection(conString))
            {
                connection.Open();

                database = this.GetDatabase(connection, databaseName);

                var constraints = new List<DBConstraint>(this.GetPrimaryKeyConstraints(connection));
                constraints.AddRange(this.GetForeignKeyConstraints(connection));
                constraints.AddRange(this.GetUniqueConstraints(connection));
                constraints.AddRange(this.GetCheckedConstraints(connection));

                database.Constraints = constraints;

                database.Schemas = this.GetSchemas(connection);

                database.Tables = this.GetTables(connection);

                database.Procedures = this.GetStoredProcedures(connection);

                database.Synonyms = this.GetSynonyms(connection);

                database.Views = this.GetViews(connection);

                database.Functions = this.GetFunctions(connection);

                database.Sequences = this.GetSequences(connection);
            }

            this.CreateBackupFile(database, pathToFile);
        }

        public void Restore(string pathToFile, string conString)
        {
            var script = File.ReadAllLines(pathToFile);
            var comString = new StringBuilder();
            using (var connection = new SqlConnection(conString))
            {
                connection.Open();
                foreach (var line in script)
                {
                    if (Regex.IsMatch(line, @".*GO.*"))
                    {
                        try
                        {
                            using (var command = new SqlCommand(comString.ToString(), connection))
                            {
                                command.ExecuteNonQuery();
                                Console.WriteLine(comString);
                            }

                            comString.Clear();
                            continue;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(comString);
                            Console.WriteLine(e.Message);
                            if (Console.ReadLine().ToLower() == "g")
                            {
                                comString.Clear();
                                continue;
                            }
                            else
                            {
                                connection.Close();
                                return;
                            }
                        }
                    }

                    comString.AppendLine(line);
                }
            }
        }

        public IEnumerable<string> ShowDatabases(string conString)
        {
            using (var connection = new SqlConnection(conString))
            {
                connection.Open();
                using (var command = new SqlCommand($"SELECT name from sys.databases", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return reader.GetString(0);
                    }
                }
            }
        }

        private void CreateBackupFile(DBDatabase database, string pathToFile)
        {
            var curDate = DateTime.Now;
            if (!Directory.Exists(this.rootDir))
            {
                Directory.CreateDirectory(this.rootDir);
            }
            using (var sqlFile = new StreamWriter(pathToFile))
            {
                sqlFile.WriteLine(database.GetCreationQuery());
                sqlFile.WriteLine();
                sqlFile.WriteLine($"USE {database.Name}");
                sqlFile.WriteLine("GO");
                sqlFile.WriteLine();

                this.WriteSchemas(database.Schemas, sqlFile);
                sqlFile.WriteLine();

                this.WriteTablesCreation(database.Tables, sqlFile);
                sqlFile.WriteLine();

                this.WriteViews(database.Views, sqlFile);
                sqlFile.WriteLine();

                this.WriteSynonyms(database.Synonyms, sqlFile);
                sqlFile.WriteLine();

                this.WriteProcedures(database.Procedures, sqlFile);
                sqlFile.WriteLine();

                this.WriteFunctions(database.Functions, sqlFile);
                sqlFile.WriteLine();

                this.WriteSequences(database.Sequences, sqlFile);
                sqlFile.WriteLine();

                this.WriteTableData(database.Tables, sqlFile);
                sqlFile.WriteLine();

                this.WriteConstraints(database.Constraints, sqlFile);
                sqlFile.WriteLine();

                this.WriteTriggers(database.Tables, sqlFile);
                sqlFile.WriteLine();
            }
        }

        #region getters

        private IEnumerable<DBCheckConstraint> GetCheckedConstraints(SqlConnection connection)
        {
            var checkedConstraints = new List<DBCheckConstraint>();
            string sqlCommandStr = @"select tab.TABLE_SCHEMA, tab.TABLE_NAME, scc.name, scc.definition from sys.check_constraints as scc inner join (select st.object_id, ist.TABLE_NAME, ist.TABLE_SCHEMA from INFORMATION_SCHEMA.TABLES as ist inner join sys.tables as st on ist.TABLE_NAME = st.name WHERE ist.TABLE_TYPE = 'BASE TABLE') as tab
on scc.parent_object_id = tab.object_id";

            using (SqlCommand command = new SqlCommand(sqlCommandStr, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        checkedConstraints.Add(new DBCheckConstraint
                        {
                            TableSchema = reader.GetString(0),
                            TableName = reader.GetString(1),
                            Name = reader.GetString(2),
                            CheckClause = reader.GetString(3),
                        });
                    }
                }
            }

            return checkedConstraints;
        }

        private IEnumerable<DBColumn> GetColumns(SqlConnection connection, DBTable table)
        {
            string sqlCommandStr = @"SELECT COLUMN_NAME, COLUMN_DEFAULT, IS_NULLABLE, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, COLLATION_NAME
                                            FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @tableSchema AND TABLE_NAME = @tableName";
            var columns = new List<DBColumn>();

            using (SqlCommand command = new SqlCommand(sqlCommandStr, connection))
            {
                command.Parameters.AddWithValue("@tableSchema", table.Schema);
                command.Parameters.AddWithValue("@tableName", table.Name);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columns.Add(new DBColumn
                        {
                            Name = reader.GetString(0),
                            Default = (reader.IsDBNull(1)) ? null : reader.GetString(1),
                            IsNullable = reader.GetString(2) == "YES" ? true : false,
                            DataType = reader.GetString(3),
                            CharactersMaxLength = (reader.IsDBNull(4) || reader.GetString(3) == "hierarchyid") ? -1 : reader.GetInt32(4),
                            CollationName = (reader.IsDBNull(5)) ? null : reader.GetString(5),
                        });
                    }
                }
            }

            return columns;
        }

        private IEnumerable<DBData> GetData(SqlConnection connection, DBTable table)
        {
            var sqlCommandStr = $"SELECT {string.Join(", ", table.Columns.Select(x => string.Format($"[{x.Name}]")))} FROM {table.Schema}.{table.Name}";
            var data = new List<DBData>();
            using (SqlCommand command = new SqlCommand(sqlCommandStr, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tempData = new DBData();
                        tempData.NameValue = new Dictionary<string, string>();
                        tempData.TableName = table.Name;
                        tempData.TableSchema = table.Schema;
                        int counter = 0;
                        foreach (var column in table.Columns)
                        {
                            if (reader[counter] is DBNull)
                            {
                                tempData.NameValue.Add(column.Name, "NULL");
                                continue;
                            }

                            switch (column.DataType)
                            {
                                case "binary":
                                case "varbinary":
                                case "image":
                                case "rowversion":
                                case "timestamp":
                                    tempData.NameValue.Add(column.Name, $"'0x{BitConverter.ToString((byte[])reader[counter]).Replace("-", string.Empty)}'");
                                    break;

                                case "bigint":
                                case "bit":
                                case "decimal":
                                case "float":
                                case "int":
                                case "money":
                                case "numeric":
                                case "real":
                                case "smallint":
                                case "smallmoney":
                                case "tinyint":
                                    tempData.NameValue.Add(column.Name, reader[counter].ToString());
                                    break;

                                case "nchar":
                                case "ntext":
                                case "nvarchar":
                                    tempData.NameValue.Add(column.Name, $"N'{reader[counter].ToString()}'");
                                    break;

                                case "char":
                                case "text":
                                case "varchar":
                                    tempData.NameValue.Add(column.Name, $"'{reader[counter].ToString()}'");
                                    break;

                                case "date":
                                    tempData.NameValue.Add(column.Name, $"'{reader.GetDateTime(counter).ToShortDateString()}'");
                                    break;

                                case "datetime":
                                    tempData.NameValue.Add(column.Name, $"'{reader.GetDateTime(counter).ToString("yyyy-MM-dd HH:mm:ss")}'");
                                    break;

                                case "datetime2":
                                    tempData.NameValue.Add(column.Name, $"'{reader.GetDateTime(counter).ToString("yyyy-MM-dd-HH:mm:ss.fffffff")}'");
                                    break;

                                case "datetimeoffset":
                                    tempData.NameValue.Add(column.Name, $"'{reader.GetDateTime(counter).ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz")}'");
                                    break;

                                case "time":
                                    tempData.NameValue.Add(column.Name, $"'{reader.GetTimeSpan(counter)}'");
                                    break;

                                case "uniqueidentifier":
                                    tempData.NameValue.Add(column.Name, $"'{reader.GetGuid(counter).ToString()}'");
                                    break;

                                default:
                                    break;
                            }
                            counter++;
                        }

                        data.Add(tempData);
                    }
                }
            }

            return data;
        }

        private DBDatabase GetDatabase(SqlConnection connection, string databaseName)
        {
            var sqlStrCommand = @"SELECT
                                name,
                                compatibility_level,
                                collation_name,
                                user_access_desc,
                                is_read_only,
                                is_auto_close_on,
                                is_auto_shrink_on,
                                is_read_committed_snapshot_on,
                                recovery_model_desc,
                                page_verify_option_desc,
                                is_auto_create_stats_on,
                                is_auto_update_stats_on,
                                is_ansi_null_default_on,
                                is_ansi_nulls_on,
                                is_ansi_padding_on,
                                is_ansi_warnings_on,
                                is_arithabort_on,
                                is_concat_null_yields_null_on,
                                is_quoted_identifier_on,
                                is_numeric_roundabort_on,
                                is_recursive_triggers_on,
                                is_cursor_close_on_commit_on,
                                is_date_correlation_on,
                                is_db_chaining_on,
                                is_trustworthy_on,
                                is_parameterization_forced,
                                is_broker_enabled
                                FROM sys.databases WHERE name = @dbName";
            using (SqlCommand command = new SqlCommand(sqlStrCommand, connection))
            {
                command.Parameters.AddWithValue("@dbName", databaseName);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var Name = reader.GetString(0);
                        var CompatibilityLevel = reader.GetByte(1);
                        var CollationName = reader.GetString(2);
                        var UserAccessDescription = reader.GetString(3);
                        var IsReadOnly = reader.GetBoolean(4);
                        var IsAutoCloseOn = reader.GetBoolean(5);
                        var IsAutoShrinkOn = reader.GetBoolean(6);
                        var IsReadCommittedSnapshotOn = reader.GetBoolean(7);
                        var RecoveryModelDescription = reader.GetString(8);
                        var PageVerifyOptionDescription = reader.GetString(9);
                        var IsAutoCreateStatsOn = reader.GetBoolean(10);
                        var IsAutoUpdateStatsOn = reader.GetBoolean(11);
                        var IsAnsiNullDefaultOn = reader.GetBoolean(12);
                        var IsAnsiNullsOn = reader.GetBoolean(13);
                        var IsAnsiPaddingOn = reader.GetBoolean(14);
                        var IsAnsiWarningsOn = reader.GetBoolean(15);
                        var IsArithabortOn = reader.GetBoolean(16);
                        var IsConcatNullYieldsNullOn = reader.GetBoolean(17);
                        var IsQuotedIdentifierOn = reader.GetBoolean(18);
                        var IsNumericRoundAbortOn = reader.GetBoolean(19);
                        var IsRecursiveTriggersOn = reader.GetBoolean(20);
                        var IsCursorCloseOnCommitOn = reader.GetBoolean(21);
                        var IsDateCorrelationOn = reader.GetBoolean(22);
                        var IsDbChainingOn = reader.GetBoolean(23);
                        var IsTrustworthyOn = reader.GetBoolean(24);
                        var IsParameterizationForced = reader.GetBoolean(25);
                        var IsBrokerEnabled = reader.GetBoolean(26);
                        return new DBDatabase
                        {
                            Name = reader.GetString(0),
                            CompatibilityLevel = reader.GetByte(1),
                            CollationName = reader.GetString(2),
                            UserAccessDescription = reader.GetString(3),
                            IsReadOnly = reader.GetBoolean(4),
                            IsAutoCloseOn = reader.GetBoolean(5),
                            IsAutoShrinkOn = reader.GetBoolean(6),
                            IsReadCommittedSnapshotOn = reader.GetBoolean(7),
                            RecoveryModelDescription = reader.GetString(8),
                            PageVerifyOptionDescription = reader.GetString(9),
                            IsAutoCreateStatsOn = reader.GetBoolean(10),
                            IsAutoUpdateStatsOn = reader.GetBoolean(11),
                            IsAnsiNullDefaultOn = reader.GetBoolean(12),
                            IsAnsiNullsOn = reader.GetBoolean(13),
                            IsAnsiPaddingOn = reader.GetBoolean(14),
                            IsAnsiWarningsOn = reader.GetBoolean(15),
                            IsArithabortOn = reader.GetBoolean(16),
                            IsConcatNullYieldsNullOn = reader.GetBoolean(17),
                            IsQuotedIdentifierOn = reader.GetBoolean(18),
                            IsNumericRoundAbortOn = reader.GetBoolean(19),
                            IsRecursiveTriggersOn = reader.GetBoolean(20),
                            IsCursorCloseOnCommitOn = reader.GetBoolean(21),
                            IsDateCorrelationOn = reader.GetBoolean(22),
                            IsDbChainingOn = reader.GetBoolean(23),
                            IsTrustworthyOn = reader.GetBoolean(24),
                            IsParameterizationForced = reader.GetBoolean(25),
                            IsBrokerEnabled = reader.GetBoolean(26),
                        };
                    }
                }
            }

            return null;
        }

        private IEnumerable<DBForeignKeyConstraint> GetForeignKeyConstraints(SqlConnection connection)
        {
            var foreignKeyConstraints = new List<DBForeignKeyConstraint>();
            string sqlCommandStr = @"SELECT
	                                    FK_Schema = FK.TABLE_SCHEMA,
                                        FK_Table = FK.TABLE_NAME,
                                        FK_Column = CU.COLUMN_NAME,
                                        PK_Schema = PK.TABLE_SCHEMA,
	                                    PK_Table = PK.TABLE_NAME,
                                        PK_Column = PT.COLUMN_NAME,
                                        Constraint_Name = C.CONSTRAINT_NAME,
		                                On_Delete = C.DELETE_RULE,
		                                On_Update = C.UPDATE_RULE
                                    FROM
                                        INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
                                    INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK
                                        ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
                                    INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK
                                        ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
                                    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU
                                        ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
                                    INNER JOIN (
                                                SELECT
                                                    i1.TABLE_NAME,
                                                    i2.COLUMN_NAME
                                                FROM
                                                    INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
                                                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2
                                                    ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
                                                WHERE
                                                    i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
                                                ) PT
                                        ON PT.TABLE_NAME = PK.TABLE_NAME";

            using (SqlCommand command = new SqlCommand(sqlCommandStr, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var constraintName = reader["Constraint_Name"] as string;
                        var primaryTableColumnName = reader["PK_Column"] as string;
                        var foreignTableColumnName = reader["FK_Column"] as string;
                        var element = foreignKeyConstraints.FirstOrDefault(x => x.Name == constraintName);

                        if (element != null)
                        {
                            element.PrimaryTableColumns.Add(primaryTableColumnName);
                            element.Columns.Add(foreignTableColumnName);
                            continue;
                        }

                        var foreignTableName = reader["FK_Table"] as string;
                        var foreignTableSchema = reader["FK_Schema"] as string;
                        var primaryTableSchema = reader["PK_Schema"] as string;
                        var primaryTableName = reader["PK_Table"] as string;
                        var onDeleteRule = reader["On_Delete"] as string;
                        var onUpdateRule = reader["On_Update"] as string;

                        foreignKeyConstraints.Add(new DBForeignKeyConstraint
                        {
                            Name = constraintName,
                            PrimaryTableColumns = new List<string> { primaryTableColumnName },
                            PrimaryTableName = primaryTableName,
                            PrimaryTableSchema = primaryTableSchema,
                            TableName = foreignTableName,
                            TableSchema = foreignTableSchema,
                            Columns = new List<string> { foreignTableColumnName },
                            OnDeleteRule = onDeleteRule,
                            OnUpdateRule = onUpdateRule,
                        });
                    }
                }
            }

            return foreignKeyConstraints;
        }

        private IEnumerable<DBFunction> GetFunctions(SqlConnection connection)
        {
            var functions = new List<DBFunction>();
            string sqlCommandStr = @"SELECT ROUTINE_DEFINITION FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'FUNCTION'";

            using (SqlCommand command = new SqlCommand(sqlCommandStr, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        functions.Add(new DBFunction
                        {
                            Definition = reader.GetString(0),
                        });
                    }
                }
            }
            return functions;
        }

        private IEnumerable<DBPrimaryKeyConstraint> GetPrimaryKeyConstraints(SqlConnection connection)
        {
            var primaryKeyConstraints = new List<DBPrimaryKeyConstraint>();
            string sqlCommandStr = @"SELECT  tc.CONSTRAINT_NAME, tc.TABLE_SCHEMA, tc.TABLE_NAME, cu.COLUMN_NAME
	                                                                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
		                                                                        INNER JOIN(SELECT * FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE) AS cu
		                                                                        ON tc.CONSTRAINT_NAME = cu.CONSTRAINT_NAME
                                                                        WHERE CONSTRAINT_TYPE = 'PRIMARY KEY'";

            using (SqlCommand command = new SqlCommand(sqlCommandStr, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var constraintName = reader["CONSTRAINT_NAME"] as string;
                        var primaryTableSchema = reader["TABLE_SCHEMA"] as string;
                        var primaryTableName = reader["TABLE_NAME"] as string;
                        var primaryTableColumnName = reader["COLUMN_NAME"] as string;

                        var element = primaryKeyConstraints.FirstOrDefault(x => x.Name == constraintName);

                        if (element != null)
                        {
                            element.Columns.Add(primaryTableColumnName);
                            continue;
                        }

                        primaryKeyConstraints.Add(new DBPrimaryKeyConstraint
                        {
                            Name = constraintName,
                            Columns = new List<string> { primaryTableColumnName },
                            TableName = primaryTableName,
                            TableSchema = primaryTableSchema,
                        });
                    }
                }
            }

            return primaryKeyConstraints;
        }

        private IEnumerable<DBSchema> GetSchemas(SqlConnection connection)
        {
            var schemas = new List<DBSchema>();
            string sqlCommandStr = @"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME NOT IN
(
'dbo',
'guest',
'INFORMATION_SCHEMA',
'sys',
'db_owner',
'db_accessadmin',
'db_securityadmin',
'db_ddladmin',
'db_backupoperator',
'db_datareader',
'db_datawriter',
'db_denydatareader',
'db_denydatawriter')";

            using (SqlCommand command = new SqlCommand(sqlCommandStr, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        schemas.Add(new DBSchema
                        {
                            Name = reader.GetString(0),
                        });
                    }
                }
            }
            return schemas;
        }

        private IEnumerable<DBSequence> GetSequences(SqlConnection connection)
        {
            string sqlCommandStr = @"SELECT infS.SEQUENCE_SCHEMA, infS.SEQUENCE_NAME, infS.DATA_TYPE, infS.START_VALUE, infS.INCREMENT, infS.MINIMUM_VALUE, infS.MAXIMUM_VALUE, ss.is_cached FROM INFORMATION_SCHEMA.SEQUENCES as infS
INNER JOIN (SELECT name, is_cached FROM sys.sequences) as ss
ON ss.name = infS.SEQUENCE_NAME";

            var sequences = new List<DBSequence>();

            using (SqlCommand command = new SqlCommand(sqlCommandStr, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sequences.Add(new DBSequence
                        {
                            Schema = reader.GetString(0),
                            Name = reader.GetString(1),
                            DataType = reader.GetString(2),
                            StartValue = reader.GetInt64(3),
                            Increment = reader.GetInt64(4),
                            MinValue = reader.GetInt64(5),
                            MaxValue = reader.GetInt64(6),
                            IsCached = reader.GetBoolean(7),
                        });
                    }
                }
            }

            return sequences;
        }

        private IEnumerable<DBProcedure> GetStoredProcedures(SqlConnection connection)
        {
            var procedures = new List<DBProcedure>();
            string sqlCommandStr = @"SELECT ROUTINE_DEFINITION FROM INFORMATION_SCHEMA.ROUTINES
                                        WHERE ROUTINE_TYPE = 'PROCEDURE'";

            using (SqlCommand command = new SqlCommand(sqlCommandStr, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        procedures.Add(new DBProcedure
                        {
                            Definition = reader.GetString(0),
                        });
                    }
                }
            }

            return procedures;
        }

        private IEnumerable<DBSynonym> GetSynonyms(SqlConnection connection)
        {
            var synonyms = new List<DBSynonym>();
            string sqlCommandStr = "SELECT name, base_object_name FROM sys.synonyms";
            char[] trimChars = new char[] { '[', ']' };
            using (SqlCommand command = new SqlCommand(sqlCommandStr, connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var catalogueSchemaObject = reader.GetString(1).Replace("[", string.Empty).Replace("]", string.Empty).Split('.');

                    synonyms.Add(new DBSynonym
                    {
                        Name = reader.GetString(0),
                        Catalogue = catalogueSchemaObject[0],
                        ObjectName = catalogueSchemaObject[2],
                        Schema = catalogueSchemaObject[1],
                    });
                }
            }

            return synonyms;
        }

        private IEnumerable<DBTable> GetTables(SqlConnection connection)
        {
            var tables = new List<DBTable>();
            var sqlStrCommand = "SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
            using (SqlCommand command = new SqlCommand(sqlStrCommand, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tables.Add(new DBTable
                        {
                            Schema = reader.GetString(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            foreach (var table in tables)
            {
                table.Columns = this.GetColumns(connection, table);
                table.Data = this.GetData(connection, table);
                table.Triggers = this.GetTableTriggers(connection, table);
            }

            return tables;
        }

        private IEnumerable<DBTrigger> GetTableTriggers(SqlConnection connection, DBTable table)
        {
            var triggers = new List<DBTrigger>();

            // Dictionary<Name, Schema>
            var nameSchemaPairs = new Dictionary<string, string>();

            StringBuilder definition = new StringBuilder();
            string sqlCommandStr = @"SELECT sys.objects.name AS [trigger], sys.tables.name AS [table], sys.schemas.name AS [schema] FROM  sys.schemas
                                    RIGHT JOIN sys.tables ON sys.schemas.schema_id = sys.tables.schema_id
                                    RIGHT JOIN sys.objects ON sys.tables.object_id = sys.objects.parent_object_id
                                    WHERE sys.objects.type = 'tr' AND sys.tables.name = @tableName AND sys.schemas.name = @tableSchema";

            using (SqlCommand command = new SqlCommand(sqlCommandStr, connection))
            {
                command.Parameters.AddWithValue("@tableName", table.Name);
                command.Parameters.AddWithValue("@tableSchema", table.Schema);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        nameSchemaPairs.Add(reader.GetString(0), reader.GetString(2));
                    }
                }
            }

            foreach (var pair in nameSchemaPairs)
            {
                using (SqlCommand command = new SqlCommand("sp_helptext", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@objname", $"{pair.Value}.{pair.Key}"));

                    using (SqlDataReader reader2 = command.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            definition.Append(reader2.GetString(0));
                        }
                    }

                    triggers.Add(new DBTrigger
                    {
                        Name = pair.Key,
                        Definition = definition.ToString(),
                    });

                    definition.Clear();
                }
            }

            return triggers;
        }

        private IEnumerable<DBUniqueConstraint> GetUniqueConstraints(SqlConnection connection)
        {
            var uniqueConstraints = new List<DBUniqueConstraint>();
            string sqlCommandStr = @"SELECT tc.CONSTRAINT_SCHEMA, tc.CONSTRAINT_NAME,  tc.TABLE_SCHEMA, tc.TABLE_NAME, cu.COLUMN_NAME
	        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
	        INNER JOIN(SELECT * FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE) AS cu
	        ON tc.CONSTRAINT_NAME = cu.CONSTRAINT_NAME
	        WHERE tc.CONSTRAINT_TYPE = 'UNIQUE'";

            using (SqlCommand command = new SqlCommand(sqlCommandStr, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var constraintName = reader["CONSTRAINT_NAME"] as string;
                        var primaryTableSchema = reader["TABLE_SCHEMA"] as string;
                        var primaryTableName = reader["TABLE_NAME"] as string;
                        var primaryTableColumnName = reader["COLUMN_NAME"] as string;

                        var element = uniqueConstraints.FirstOrDefault(x => x.Name == constraintName);

                        if (element != null)
                        {
                            element.Columns.Add(primaryTableColumnName);
                            continue;
                        }

                        uniqueConstraints.Add(new DBUniqueConstraint
                        {
                            Name = constraintName,
                            Columns = new List<string> { primaryTableColumnName },
                            TableName = primaryTableName,
                            TableSchema = primaryTableSchema,
                        });
                    }
                }
            }

            return uniqueConstraints;
        }

        private IEnumerable<DBView> GetViews(SqlConnection connection)
        {
            var views = new List<DBView>();
            string sqlCommandStr = @"SELECT TABLE_SCHEMA, TABLE_NAME, VIEW_DEFINITION FROM INFORMATION_SCHEMA.Views ";

            using (SqlCommand command = new SqlCommand(sqlCommandStr, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        views.Add(new DBView
                        {
                            Schema = reader.GetString(0),
                            Name = reader.GetString(1),
                            Definition = reader.GetString(2),
                        });
                    }
                }
            }

            foreach (var view in views)
            {
                view.Triggers = this.GetViewTriggers(connection, view);
            }

            return views;
        }

        private IEnumerable<DBTrigger> GetViewTriggers(SqlConnection connection, DBView view)
        {
            var triggers = new List<DBTrigger>();
            var names = new List<string>();
            StringBuilder definition = new StringBuilder();
            string sqlCommandStr = "SELECT name FROM sys.triggers AS st WHERE st.parent_id = (SELECT object_id FROM sys.tables WHERE name = @viewName AND schema_id = (SELECT schema_id FROM sys.schemas WHERE name = @viewSchema))";
            string sqlCommandStr2 = "exec sp_helptext @name";

            using (SqlCommand command = new SqlCommand(sqlCommandStr, connection))
            {
                command.Parameters.AddWithValue("@viewName", view.Name);
                command.Parameters.AddWithValue("@viewSchema", view.Schema);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        names.Add(reader.GetString(0));
                    }
                }
            }

            foreach (var name in names)
            {
                using (SqlCommand command = new SqlCommand(sqlCommandStr2, connection))
                {
                    command.Parameters.AddWithValue("@name", $"{name}");
                    using (SqlDataReader reader2 = command.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            definition.Append(reader2.GetString(0));
                        }
                    }

                    triggers.Add(new DBTrigger
                    {
                        Name = name,
                        Definition = definition.ToString(),
                    });

                    definition.Clear();
                }
            }

            return triggers;
        }

        #endregion getters

        #region WriteMethods

        private void WriteConstraints(IEnumerable<DBConstraint> constraints, StreamWriter sqlFile)
        {
            foreach (var constraint in constraints)
            {
                sqlFile.WriteLine(constraint.GetCreationQuery());
                sqlFile.WriteLine("GO");
            }
        }

        private void WriteFunctions(IEnumerable<DBFunction> functions, StreamWriter sqlFile)
        {
            sqlFile.WriteLine("/* Functions */");
            foreach (var function in functions)
            {
                sqlFile.WriteLine(function.GetCreationQuery());
                sqlFile.WriteLine("GO");
            }
        }

        private void WriteProcedures(IEnumerable<DBProcedure> procedures, StreamWriter sqlFile)
        {
            sqlFile.WriteLine("/* Stored procedures */");

            foreach (var procedure in procedures)
            {
                sqlFile.WriteLine(procedure.GetCreationQuery());
                sqlFile.WriteLine("GO");
            }
        }

        private void WriteSchemas(IEnumerable<DBSchema> schemas, StreamWriter sqlFile)
        {
            foreach (var schema in schemas)
            {
                sqlFile.WriteLine(schema.GetCreationQuery());
                sqlFile.WriteLine("GO");
            }
        }

        private void WriteSequences(IEnumerable<DBSequence> sequences, StreamWriter sqlFile)
        {
            foreach (var sequence in sequences)
            {
                sqlFile.WriteLine(sequence.GetCreationQuery());
                sqlFile.WriteLine("GO");
            }
        }

        private void WriteSynonyms(IEnumerable<DBSynonym> synonyms, StreamWriter sqlFile)
        {
            sqlFile.WriteLine("/* Synonyms */");
            foreach (var synonym in synonyms)
            {
                sqlFile.WriteLine(synonym.GetCreationQuery());
                sqlFile.WriteLine("GO");
            }
        }

        private void WriteTableData(IEnumerable<DBTable> tables, StreamWriter sqlFile)
        {
            sqlFile.WriteLine("/* Data */");

            foreach (var table in tables)
            {
                foreach (var dataPiece in table.Data)
                {
                    sqlFile.WriteLine(dataPiece.GetCreationQuery());
                    sqlFile.WriteLine("GO");
                }
            }
        }

        private void WriteTablesCreation(IEnumerable<DBTable> tables, StreamWriter sqlFile)
        {
            sqlFile.WriteLine("/* Tables */");

            foreach (var table in tables)
            {
                sqlFile.WriteLine(table.GetCreationQuery());
                sqlFile.WriteLine("GO");
            }
        }

        private void WriteTriggers(IEnumerable<DBTable> tables, StreamWriter sqlFile)
        {
            foreach (var table in tables)
            {
                foreach (var trigger in table.Triggers)
                {
                    sqlFile.WriteLine(trigger.GetCreationQuery());
                    sqlFile.WriteLine("GO");
                    sqlFile.WriteLine();
                }
            }
        }

        private void WriteViews(IEnumerable<DBView> views, StreamWriter sqlFile)
        {
            foreach (var view in views)
            {
                sqlFile.WriteLine(view.GetCreationQuery());
                sqlFile.WriteLine("GO");
            }
        }

        #endregion WriteMethods
    }
}