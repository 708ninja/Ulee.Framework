//------------------------------------------------------------------------------
// Copyright (C) 2020 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2020/08/19
// Language    : Visual Studio 2019 C#
// Description : MS-SQL Server Connection Class
//------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace Ulee.Database.SqlServer
{
	public enum ESqlServerException
    {
		Default = 0,
		Open = -100,
		Close = -101,
		BeginTransaction = -102,
		SaveTransaction = -103,
		CommitTransaction = -104,
		RollbackTransaction = -105
	}

	public class SqlServerException : ApplicationException
	{
		public ESqlServerException Code { get; private set; }

		public SqlServerException(string msg = "Occurred SQL Server Exception!", 
			ESqlServerException code = ESqlServerException.Default) : base(msg)
		{
			Code = code;
		}
	}

	public abstract class UlSqlServer
    {
        protected SqlConnection connect;
        protected SqlConnectionStringBuilder connectStrings;
        protected SqlCommand command;
        protected SqlDataAdapter dataAdapter;
        protected SqlTransaction trans;

        public string DataSource
        {
            get { return connectStrings.DataSource; }
            set { connectStrings.DataSource = value; }
        }

        public string InitialCatalog
        {
            get { return connectStrings.InitialCatalog; }
            set { connectStrings.InitialCatalog = value; }
        }

		public bool IntegratedSecurity
        {
			get { return connectStrings.IntegratedSecurity; }
			set { connectStrings.IntegratedSecurity = value; }
        }

		public bool TrustServerCertificate
		{
			get { return connectStrings.TrustServerCertificate; }
			set { connectStrings.TrustServerCertificate = value; }
		}

		public string UserID
        {
            get { return connectStrings.UserID; }
            set { connectStrings.UserID = value; }
        }

        public string Password
        {
            get { return connectStrings.Password; }
            set { connectStrings.Password = value; }
        }

        public bool Pooling
        {
            get { return connectStrings.Pooling; }
            set { connectStrings.Pooling = value; }
        }

		public string ConnectString
        {
			get { return connectStrings.ToString(); }
        }

        public SqlTransaction Trans
        { get { return trans; } }

        public UlSqlServer(string connectString = null)
        {
            connect = new SqlConnection();
            connectStrings = new SqlConnectionStringBuilder(connectString);
            command = new SqlCommand("", connect);
            command.Transaction = null;
            dataAdapter = new SqlDataAdapter("", connect);
            trans = null;

            //connectStrings.Clear();
            //connectStrings.TrustServerCertificate = false;
            //connectStrings.DataSource = "";
            //connectStrings.InitialCatalog = "";
            //connectStrings.IntegratedSecurity = false;
            //connectStrings.UserID = "";
            //connectStrings.Password = "";
            //connectStrings.Pooling = false;
        }
	
		public void Open()
		{
			if (connect.State != ConnectionState.Closed)
			{
				throw new SqlServerException(
					"Occurred SQL Server connection is not closed error in UlSqlServer.Open", 
					ESqlServerException.Open);
			}

			try
			{
				connect.ConnectionString = connectStrings.ToString();
				connect.Open();
			}
			catch (Exception e)
			{
				string msg = $"{e.ToString()}\r\nOccurred SQL Server connection opening error in UlSqlServer.Open";
				throw new SqlServerException(msg, ESqlServerException.Open);
			}
		}

		public void Close()
		{
			if (connect.State == ConnectionState.Closed)
			{
				throw new SqlServerException(
					"Occurred SQL Server connection is not opened error in UlSqlServer.Close", 
					ESqlServerException.Close);
			}

			try
			{
				connect.Close();
			}
			catch (Exception e)
			{
				string msg = $"{e.ToString()}\r\nOccurred SQL Server connection closing error in UlSqlServer.Close";
				throw new SqlServerException(msg, ESqlServerException.Close);
			}
		}

		public SqlTransaction BeginTrans()
		{
			try
			{
				trans = connect.BeginTransaction();
				command.Transaction = trans;
			}
			catch (Exception e)
			{
				string msg = $"{e.ToString()}\r\nOccurred beginning transaction exception in UlSqlServer.BeginTrans";
				throw new SqlServerException(msg, ESqlServerException.BeginTransaction);
			}

			return trans;
		}

		public void SaveTrans(string tag)
		{
			if (trans == null)
			{
				throw new SqlServerException(
					"Occurred transaction doesn't begin error in UlSqlServer.SaveTrans", 
					ESqlServerException.SaveTransaction);
			}

			try
			{
				trans.Save(tag);
			}
			catch (Exception e)
			{
				string msg = $"{e.ToString()}\r\nOccurred begin transaction exception in UlSqlServer.SaveTrans";
				throw new SqlServerException(msg, ESqlServerException.SaveTransaction);
			}
		}

		public void CommitTrans()
		{
			if (trans == null)
			{
				throw new SqlServerException(
					"Occurred transaction doesn't begin error in UlSqlServer.CommitTrans",
					ESqlServerException.CommitTransaction);
			}

			try
			{
                trans.Commit();
			}
			catch (Exception e)
			{
				string msg = $"{e.ToString()}\r\nOccurred commit transaction exception in UlSqlServer.CommitTrans";
				throw new SqlServerException(msg, ESqlServerException.CommitTransaction);
			}
		}

		public void RollbackTrans(string tag = "")
		{
			if (trans == null)
			{
				throw new SqlServerException(
					"Occurred transaction doesn't begin error in UlSqlServer.RollbackTrans",
					ESqlServerException.RollbackTransaction);
			}

			try
			{
				if (tag == "")
				{
					trans.Rollback();
				}
				else
				{
					trans.Rollback(tag);
				}
			}
			catch (Exception e)
			{
				string msg = $"{e.ToString()}\r\nOccurred commit transaction exception in UlSqlServer.RollbackTrans";
				throw new SqlServerException(msg, ESqlServerException.RollbackTransaction);
			}
		}

		public void Lock()
		{
			Monitor.Enter(this);
		}

		public void Unlock()
		{
			Monitor.Exit(this);
		}
	}

	public abstract class UlSqlDataSet
	{
		protected SqlConnection connect;
		protected SqlTransaction trans;
		protected SqlDataAdapter dataAdapter;
		protected SqlCommand command;
		protected DataSet dataSet;

		public SqlTransaction Trans { set { SetTrans(value); } }

		public DataSet DataSet { get { return dataSet; } }

		//public Int64 Identity { get { return GetIdentity(); } }

		public int RowCount { get { return GetRowCount(); } }

		public bool Empty { get { return IsEmpty(); } }

		public UlSqlDataSet(SqlConnection aConnect, SqlCommand aCommand = null, SqlDataAdapter aAdapter = null)
		{
			connect = aConnect;

			if (aCommand == null)
			{
				command = new SqlCommand("", connect);
				command.Transaction = null;
			}
			else
			{
				command = aCommand;
			}

			if (dataAdapter == null)
				dataAdapter = new SqlDataAdapter("", connect);
			else
				dataAdapter = aAdapter;

			dataAdapter.SelectCommand = command;
			dataSet = new DataSet();
			trans = null;
		}

		~UlSqlDataSet()
		{
		}

		protected void SetTrans(SqlTransaction aTrans)
		{
			trans = aTrans;
			command.Transaction = trans;
		}

		protected void BeginTrans(SqlTransaction aTrans = null)
		{
			if (aTrans != null) return;

			trans = connect.BeginTransaction();
			command.Transaction = trans;
		}

		protected void CommitTrans(SqlTransaction aTrans = null)
		{
			if (aTrans != null) return;

			if (trans == null)
			{
				throw new SqlServerException(
					"Occurred transaction doesn't begin error in UlDataSet.CommitTrans", 
					ESqlServerException.CommitTransaction);
			}

			try
			{
				trans.Commit();
			}
			catch (Exception e)
			{
				string msg = $"{e.ToString()}\r\nOccurred commit transaction exception in UlFirebird.CommitTrans";
				throw new SqlServerException(msg, ESqlServerException.CommitTransaction);
			}
			finally
			{
				SetTrans(null);
			}
		}

		protected void RollbackTrans(SqlTransaction aTrans = null, Exception exp = null)
		{
			if (aTrans != null)
			{
				if (exp != null)
				{
					throw exp;
				}

				return;
			}

			if (trans == null)
			{
				throw new SqlServerException(
					"Occurred transaction doesn't begin error in UlDataSet.RollbackTrans",
					ESqlServerException.RollbackTransaction);
			}

			try
			{
				trans.Rollback();
			}
			catch (Exception e)
			{
				string msg = $"{e.ToString()}\r\nOccurred rollback transaction exception in OxFirebird.RollbackTrans";
				throw new SqlServerException(msg, ESqlServerException.RollbackTransaction);
			}
			finally
			{
				SetTrans(null);
			}
		}

		public Int64 GetIdentity()
		{
			command.CommandText = " select cast(scope_identity() as bigint) as seq ";
			dataSet.Clear();
			dataAdapter.Fill(dataSet);

			return Convert.ToInt64(dataSet.Tables[0].Rows[0]["seq"]);
		}

		public int GetRowCount(int index = 0)
		{
			if (index >= dataSet.Tables.Count) return 0;

			return dataSet.Tables[index].Rows.Count;
		}

		public bool IsEmpty(int index = 0)
		{
			if (index >= dataSet.Tables.Count) return true;

			return (dataSet.Tables[index].Rows.Count == 0) ? true : false;
		}
	}
}
