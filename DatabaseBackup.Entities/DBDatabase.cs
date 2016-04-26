using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseBackup.Entities
{
    public class DBDatabase
    {
        public string CollationName { get; set; }

        public byte CompatibilityLevel { get; set; }

        public IEnumerable<DBConstraint> Constraints { get; set; }

        public IEnumerable<DBFunction> Functions { get; set; }

        public bool IsAnsiNullDefaultOn { get; set; }

        public bool IsAnsiNullsOn { get; set; }

        public bool IsAnsiPaddingOn { get; set; }

        public bool IsAnsiWarningsOn { get; set; }

        public bool IsArithabortOn { get; set; }

        public bool IsAutoCloseOn { get; set; }

        public bool IsAutoCreateStatsOn { get; set; }

        public bool IsAutoShrinkOn { get; set; }

        public bool IsAutoUpdateStatsOn { get; set; }

        public bool IsBrokerEnabled { get; set; }

        public bool IsConcatNullYieldsNullOn { get; set; }

        public bool IsCursorCloseOnCommitOn { get; set; }

        public bool IsDateCorrelationOn { get; set; }

        public bool IsDbChainingOn { get; set; }

        public bool IsLocalCursorDefault { get; set; }

        public bool IsNumericRoundAbortOn { get; set; }

        public bool IsParameterizationForced { get; set; }

        public bool IsQuotedIdentifierOn { get; set; }

        public bool IsReadCommittedSnapshotOn { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsRecursiveTriggersOn { get; set; }

        public bool IsTrustworthyOn { get; set; }

        public string Name { get; set; }

        public string PageVerifyOptionDescription { get; set; }

        public IEnumerable<DBProcedure> Procedures { get; set; }

        public string RecoveryModelDescription { get; set; }

        public IEnumerable<DBSchema> Schemas { get; set; }

        public IEnumerable<DBSequence> Sequences { get; set; }

        public IEnumerable<DBSynonym> Synonyms { get; set; }

        public IEnumerable<DBTable> Tables { get; set; }

        public string UserAccessDescription { get; set; }

        public IEnumerable<DBView> Views { get; set; }

        public string GetCreationQuery()
        {
            var result = new StringBuilder();
            result.AppendLine("USE [master]");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"IF DB_ID (N'{this.Name}') IS NOT NULL DROP DATABASE [{this.Name}]");
            result.AppendLine("GO");

            result.AppendLine($"CREATE DATABASE [{this.Name}]");
            result.AppendLine($"COLLATE {this.CollationName}");
            result.AppendLine($"GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET COMPATIBILITY_LEVEL = {this.CompatibilityLevel.ToString()}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET AUTO_CLOSE {(this.IsAutoCloseOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET AUTO_CREATE_STATISTICS {(this.IsAutoCreateStatsOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET AUTO_UPDATE_STATISTICS {(this.IsAutoUpdateStatsOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET CURSOR_CLOSE_ON_COMMIT {(this.IsCursorCloseOnCommitOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET CURSOR_DEFAULT {(this.IsLocalCursorDefault ? "LOCAL" : "GLOBAL")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET CURSOR_CLOSE_ON_COMMIT {(this.IsCursorCloseOnCommitOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET {(this.IsReadOnly ? "READ_ONLY" : "READ_WRITE")}");
            result.AppendLine("GO");
            result.AppendLine();
            result.AppendLine($"ALTER DATABASE [{this.Name}] SET {this.UserAccessDescription}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET DATE_CORRELATION_OPTIMIZATION {(this.IsDateCorrelationOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET DB_CHAINING {(this.IsDbChainingOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET TRUSTWORTHY {(this.IsTrustworthyOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET PARAMETERIZATION {(this.IsParameterizationForced ? "FORCED" : "SIMPLE")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET RECOVERY {this.RecoveryModelDescription}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET PAGE_VERIFY {this.PageVerifyOptionDescription}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET {(this.IsBrokerEnabled ? "ENABLE_BROKER" : "DISABLE_BROKER")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET READ_COMMITTED_SNAPSHOT {(this.IsReadCommittedSnapshotOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET ANSI_NULL_DEFAULT {(this.IsAnsiNullDefaultOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET ANSI_NULLS {(this.IsAnsiNullsOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET ANSI_PADDING {(this.IsAnsiPaddingOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET ANSI_WARNINGS {(this.IsAnsiWarningsOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET ARITHABORT {(this.IsArithabortOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET CONCAT_NULL_YIELDS_NULL {(this.IsConcatNullYieldsNullOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET QUOTED_IDENTIFIER {(this.IsQuotedIdentifierOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET NUMERIC_ROUNDABORT {(this.IsNumericRoundAbortOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine($"ALTER DATABASE [{this.Name}] SET RECURSIVE_TRIGGERS {(this.IsRecursiveTriggersOn ? "ON" : "OFF")}");
            result.AppendLine("GO");
            result.AppendLine();

            return result.ToString();
        }
    }
}