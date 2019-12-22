//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : Firebird Connection Class
//------------------------------------------------------------------------------
using System;
using System.Data;
using System.Threading;

using FirebirdSql.Data.FirebirdClient;

//------------------------------------------------------------------------------
namespace Ulee.Database.Firebird
{
    //--------------------------------------------------------------------------
    public abstract class UlFirebird
    {
        protected FbConnection connect;
		protected FbConnectionStringBuilder connectStrings;
		protected FbCommand command;
		protected FbDataAdapter dataAdapter;
        protected FbTransaction trans;

		public string DataSource
		{
			get { return connectStrings.DataSource; }
			set { connectStrings.DataSource = value; }
		}

		public string Database
		{
			get { return connectStrings.Database; }
			set { connectStrings.Database = value; }
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

		public int Port
		{
			get { return connectStrings.Port; }
			set { connectStrings.Port = value; }
		}

		public string Charset
		{
			get { return connectStrings.Charset; }
			set { connectStrings.Charset = value; }
		}

		public bool Pooling
		{
			get { return connectStrings.Pooling; }
			set { connectStrings.Pooling = value; }
		}

		public int Dialect
		{
			get { return connectStrings.Dialect; }
			set { connectStrings.Dialect = value; }
		}

        public FbTransaction Trans
        { get { return trans; } }

		//----------------------------------------------------------------------
        public UlFirebird(FbServerType type = FbServerType.Default)
        {
			connect = new FbConnection();
			connectStrings = new FbConnectionStringBuilder();
			command = new FbCommand("", connect);
            command.Transaction = null;
			dataAdapter = new FbDataAdapter("", connect);
            trans = null;
			
			connectStrings.Clear();
			connectStrings.DataSource = "";
			connectStrings.Database = "";
			connectStrings.UserID = "SYSDBA";
			connectStrings.Password = "masterkey";
			connectStrings.Charset = "NONE";
			connectStrings.Dialect = 3;
			connectStrings.Port = 3050;
			connectStrings.Pooling = false;
            connectStrings.ServerType = type;
        }

		//----------------------------------------------------------------------
		public void Open()
		{
			if (connect.State != ConnectionState.Closed)
			{
				throw new Exception("Occurred Firebird database is not closed error in UlFirebird.Open");
			}

			try
			{
				connect.ConnectionString = connectStrings.ToString();
				connect.Open();
			}
			catch (Exception e)
			{
                string msg = string.Format("{0}\r\nOccurred Firebird database opening error in UlFirebird.Open", e.ToString());
                throw new Exception(msg);
            }
        }

		//----------------------------------------------------------------------
		public void Close()
		{
			if (connect.State == ConnectionState.Closed)
			{
				throw new Exception("Occurred Firebird database is not Opened error in UlFirebird.Close");
			}

			try
			{
				connect.Close();
			}
			catch (Exception e)
			{
                string msg = string.Format("{0}\r\nOccurred Firebird database closing error in UlFirebird.Close", e.ToString());
                throw new Exception(msg);
			}
		}

		//----------------------------------------------------------------------
		public FbTransaction BeginTrans()
		{
			try
			{
				trans = connect.BeginTransaction();
                command.Transaction = trans;
			}
			catch (Exception e)
			{
                string msg = string.Format("{0}\r\nOccurred beginning transaction exception in UlFirebird.BeginTrans", e.ToString());
                throw new Exception(msg);
			}

            return trans;
		}

		//----------------------------------------------------------------------
		public void SaveTrans(string aTag)
		{
			if (trans == null)
			{
				throw new Exception("Occurred transaction doesn't begin error in UlFirebird.SaveTrans");
			}

			try
			{
				trans.Save(aTag);
			}
			catch (Exception e)
			{
                string msg = string.Format("{0}\r\nOccurred begin transaction exception in UlFirebird.SaveTrans", e.ToString());
                throw new Exception(msg);
			}
		}

		//----------------------------------------------------------------------
		public void CommitTrans(string aTag ="")
		{
			if (trans == null)
			{
				throw new Exception("Occurred transaction doesn't begin error in UlFirebird.CommitTrans");
			}

			try
			{
				if (aTag == "")
				{
					trans.Commit();
				}
				else
				{
					trans.Commit(aTag);
				}
			}
			catch (Exception e)
			{
                string msg = string.Format("{0}\r\nOccurred commit transaction exception in UlFirebird.BeginTrans", e.ToString());
                throw new Exception(msg);
			}
		}

		//----------------------------------------------------------------------
		public void RollbackTrans(string aTag ="")
		{
			if (trans == null)
			{
				throw new Exception("Occurred transaction doesn't begin error in UlFirebird.RollbackTrans");
			}

			try
			{
				if (aTag == "")
				{
					trans.Rollback();
				}
				else
				{
					trans.Rollback(aTag);
				}
			}
			catch (Exception e)
			{
                string msg = string.Format("{0}\r\nOccurred commit transaction exception in UlFirebird.RollbackTrans", e.ToString());
                throw new Exception(msg);
			}
		}

		//----------------------------------------------------------------------
		public void CommitRetainingTrans()
		{
            if (trans == null)
            {
                throw new Exception("Occurred transaction doesn't begin error in UlFirebird.RollbackTrans");
            }

            try
            {
				trans.CommitRetaining();
			}
			catch (Exception e)
			{
                string msg = string.Format("{0}\r\nOccurred commit transaction exception in UlFirebird.CommitRetainingTrans", e.ToString());
                throw new Exception(msg);
			}
		}

		//----------------------------------------------------------------------
		public void RollbackRetainingTrans()
		{
            if (trans == null)
            {
                throw new Exception("Occurred transaction doesn't begin error in UlFirebird.RollbackTrans");
            }

            try
            {
				trans.RollbackRetaining();
			}
			catch (Exception e)
			{
                string msg = string.Format("{0}\r\nOccurred commit transaction exception in UlFirebird.RollbackRetainingTrans", e.ToString());
                throw new Exception(msg);
			}
		}

		//----------------------------------------------------------------------
		public Int64 GetGenNo(string aName, int aInc=1)
		{
            string sSQL = string.Format(
				"select first 1 gen_id({0}, {1}) from RDB$DATABASE",
				aName, aInc);

			command.CommandText = sSQL;

			return (Int64) command.ExecuteScalar();
		}

		//----------------------------------------------------------------------
		public DateTime CurrentDateTime()
		{
            if (trans == null)
            {
                throw new Exception("Occurred transaction doesn't begin error in UlFirebird.RollbackTrans");
            }

            command.CommandText = "select CURRENT_TIMESTAMP from RDB$DATABASE";

			return (DateTime) command.ExecuteScalar();
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

    //--------------------------------------------------------------------------
    public abstract class UlDataSet
	{
		protected FbConnection connect;
		protected FbTransaction trans;
		protected FbDataAdapter dataAdapter;
		protected FbCommand command;
		protected DataSet dataSet;

		public DataSet DataSet { get { return dataSet; } }

        public int RowCount { get { return GetRowCount(); } }

        public bool Empty { get { return IsEmpty(); } }

        //----------------------------------------------------------------------
        public UlDataSet(FbConnection aConnect, FbCommand aCommand=null, FbDataAdapter aAdapter=null)
		{
			connect = aConnect;

            if (aCommand == null)
            {
                command = new FbCommand("", connect);
                command.Transaction = null;
            }
            else
            {
                command = aCommand;
            }

            if (dataAdapter == null)
                dataAdapter = new FbDataAdapter("", connect);
            else
                dataAdapter = aAdapter;

            dataAdapter.SelectCommand = command;

			dataSet = new DataSet();

			trans = null;
		}

		//----------------------------------------------------------------------
		~UlDataSet()
		{
		}

		//----------------------------------------------------------------------
		protected void SetTrans(FbTransaction aTrans)
		{
			trans = aTrans;
			command.Transaction = trans;
		}

		//----------------------------------------------------------------------
		protected void BeginTrans(FbTransaction aTrans = null)
		{
			if (aTrans != null) return;

			trans = connect.BeginTransaction();
			command.Transaction = trans;
		}

		//----------------------------------------------------------------------
		protected void CommitTrans(FbTransaction aTrans = null)
		{
			if (aTrans != null) return;

			if (trans == null)
			{
				throw new Exception("Occurred transaction doesn't begin error in UlDataSet.CommitTrans");
			}

			try
			{
				trans.Commit();
			}
            catch (Exception e)
            {
                string msg = string.Format("{0}\r\nOccurred commit transaction exception in UlFirebird.CommitTrans", e.ToString());
                throw new Exception(msg);
            }
            finally
            {
				SetTrans(null);
			}
		}

        //----------------------------------------------------------------------
        protected void RollbackTrans(FbTransaction aTrans = null, Exception exp = null)
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
                throw new Exception("Occurred transaction doesn't begin error in OxDataSet.RollbackTrans");
            }

            try
            {
                trans.Rollback();
            }
            catch (Exception e)
            {
                string msg = string.Format("{0}\r\nOccurred rollback transaction exception in OxFirebird.RollbackTrans", e.ToString());
                throw new Exception(msg);
            }
            finally
            {
                SetTrans(null);
            }
        }

		//----------------------------------------------------------------------
		protected void CommitRetainingTrans(FbTransaction aTrans = null)
		{
			if (aTrans != null) return;

			if (trans == null)
			{
				throw new Exception("Occurred transaction doesn't begin error in UlDataSet.CommitRetainingTrans");
			}

            try
            {
                trans.CommitRetaining();
            }
            catch (Exception e)
            {
                string msg = string.Format("{0}\r\nOccurred commit retaining transaction exception in UlFirebird.CommitRetainingTrans", e.ToString());
                throw new Exception(msg);
            }
        }

        //----------------------------------------------------------------------
        protected void RollbackRetainingTrans(FbTransaction aTrans = null, Exception exp = null)
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
                throw new Exception("Occurred transaction doesn't begin error in OxDataSet.RollbackRetainingTrans");
            }

            try
            {
                trans.RollbackRetaining();
            }
            catch (Exception e)
            {
                string msg = string.Format(
                    "{0}\r\nOccurred rollback retaining transaction exception in OxFirebird.RollbackRetainingTrans", 
                    e.ToString());
                throw new Exception(msg);
            }
        }

        //----------------------------------------------------------------------
        public int GetRowCount(int index=0)
        {
            if (index >= dataSet.Tables.Count) return 0;

            return dataSet.Tables[index].Rows.Count;
        }

        public bool IsEmpty(int index=0)
        {
            if (index >= dataSet.Tables.Count) return true;

            return (dataSet.Tables[index].Rows.Count == 0) ? true : false;
        }
    }
}
//------------------------------------------------------------------------------
