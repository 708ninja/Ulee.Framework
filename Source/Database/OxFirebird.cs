
using System;
using System.Data;

using FirebirdSql.Data.FirebirdClient;

//------------------------------------------------------------------------------
namespace OxLib.Database
{
    //--------------------------------------------------------------------------
    public abstract class OxFirebird
    {
        protected FbConnection connect;
		protected FbConnectionStringBuilder connectStrings;
		protected FbTransaction trans;
		protected FbCommand command;
		protected FbDataAdapter dataAdapter;

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
		{
			get { return trans; }
		}

		//----------------------------------------------------------------------
        public OxFirebird()
        {
			connect = new FbConnection();
			connectStrings = new FbConnectionStringBuilder();
			command = new FbCommand("", connect);
			dataAdapter = new FbDataAdapter("", connect);
			
			trans = null;

			connectStrings.Clear();
			connectStrings.DataSource = "localhost";
			connectStrings.Database = "";
			connectStrings.UserID = "SYSDBA";
			connectStrings.Password = "masterkey";
			connectStrings.Charset = "NONE";
			connectStrings.Dialect = 3;
			connectStrings.Port = 3050;
			connectStrings.Pooling = false;
		}

		//----------------------------------------------------------------------
		public void Open()
		{
			if (connect.State != ConnectionState.Closed)
			{
				throw new Exception("Occurred Firebird database is not closed error in OxFirebird.Open");
			}

			try
			{
				connect.ConnectionString = connectStrings.ToString();
				connect.Open();
			}
			catch (Exception)
			{
				throw new Exception("Occurred Firebird database opening error in OxFirebird.Open");
			}
		}

		//----------------------------------------------------------------------
		public void Close()
		{
			if (connect.State == ConnectionState.Closed)
			{
				throw new Exception("Occurred Firebird database is not Opened error in OxFirebird.Close");
			}

			try
			{
				connect.Close();
			}
			catch (Exception)
			{
				throw new Exception("Occurred Firebird database closing error in OxFirebird.Close");
			}
		}

		//----------------------------------------------------------------------
		public void BeginTrans()
		{
			try
			{
				if (trans != null)
				{
					trans.Commit();
				}

				trans = connect.BeginTransaction();
			}
			catch (Exception)
			{
				throw new Exception("Occurred beginning transaction exception in OxFirebird.BeginTrans");
			}
		}

		//----------------------------------------------------------------------
		public void SaveTrans(string aTag)
		{
			if (trans == null)
			{
				throw new Exception("Occurred transaction doesn't begin error in OxFirebird.SaveTrans");
			}

			try
			{
				trans.Save(aTag);
			}
			catch (Exception)
			{
				throw new Exception("Occurred begin transaction exception in OxFirebird.SaveTrans");
			}
		}

		//----------------------------------------------------------------------
		public void CommitTrans(string aTag="")
		{
			if (trans == null)
			{
				throw new Exception("Occurred transaction doesn't begin error in OxFirebird.CommitTrans");
			}

			try
			{
				if (aTag == "")
				{
					trans.Commit();
					trans = null;
				}
				else
				{
					trans.Commit(aTag);
				}
			}
			catch (Exception)
			{
				throw new Exception("Occurred commit transaction exception in OxFirebird.BeginTrans");
			}
		}

		//----------------------------------------------------------------------
		public void RollbackTrans(string aTag="")
		{
			if (trans == null)
			{
				throw new Exception("Occurred transaction doesn't begin error in OxFirebird.RollbackTrans");
			}

			try
			{
				if (aTag == "")
				{
					trans.Rollback();
					trans = null;
				}
				else
				{
					trans.Rollback(aTag);
				}
			}
			catch (Exception)
			{
				throw new Exception("Occurred commit transaction exception in OxFirebird.RollbackTrans");
			}
		}

		//----------------------------------------------------------------------
		public void CommitRetainingTrans()
		{
			try
			{
				trans.CommitRetaining();
			}
			catch (Exception)
			{
				throw new Exception("Occurred commit transaction exception in OxFirebird.BeginTrans");
			}
		}

		//----------------------------------------------------------------------
		public void RollbackRetainingTrans()
		{
			try
			{
				trans.RollbackRetaining();
			}
			catch (Exception)
			{
				throw new Exception("Occurred commit transaction exception in OxFirebird.RollbackRetainingTrans");
			}
		}

		//----------------------------------------------------------------------
		public Int64 GetGenNo(string aName, int aInc=1)
		{
			string sSQL = string.Format(
				"select first 1 gen_id({0}, {1}) from RDB$DATABASE",
				aName, aInc);

			command.Transaction = trans;
			command.CommandText = sSQL;

			return (Int64) command.ExecuteScalar();
		}

		//----------------------------------------------------------------------
		public DateTime CurrentDateTime()
		{
			command.Transaction = trans;
			command.CommandText = "select CURRENT_TIMESTAMP from RDB$DATABASE";

			return (DateTime) command.ExecuteScalar();
		}
	}

	//--------------------------------------------------------------------------
	public abstract class OxDataSet
	{
		protected FbConnection connect;
		protected FbTransaction trans;
		protected FbDataAdapter dataAdapter;
		protected FbCommand command;
		protected DataSet dataSet;

		public DataSet DataSet { get { return dataSet; } }

		//----------------------------------------------------------------------
		public OxDataSet(FbConnection aConnect, FbCommand aCommand, FbDataAdapter aAdapter)
		{
			connect = aConnect;
			command = aCommand;
			dataAdapter = aAdapter;
			dataAdapter.SelectCommand = command;

			dataSet = new DataSet();

			trans = null;
		}

		//----------------------------------------------------------------------
		~OxDataSet()
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
				throw new Exception("Occurred transaction doesn't begin error in OxDataSet.CommitTrans");
			}

			try
			{
				trans.Commit();
			}
			finally
			{
				trans = null;
			}
		}

		//----------------------------------------------------------------------
		protected void RollbackTrans(FbTransaction aTrans = null)
		{
			if (aTrans != null) return;

			if (trans == null)
			{
				throw new Exception("Occurred transaction doesn't begin error in OxDataSet.RollbackTrans");
			}

			try
			{
				trans.Rollback();
			}
			finally
			{
				trans = null;
			}
		}

		//----------------------------------------------------------------------
		protected void CommitRetainingTrans(FbTransaction aTrans = null)
		{
			if (aTrans != null) return;

			if (trans == null)
			{
				throw new Exception("Occurred transaction doesn't begin error in OxDataSet.CommitRetainingTrans");
			}

			trans.CommitRetaining();
		}

		//----------------------------------------------------------------------
		protected void RollbackRetainingTrans(FbTransaction aTrans = null)
		{
			if (aTrans != null) return;

			if (trans == null)
			{
				throw new Exception("Occurred transaction doesn't begin error in OxDataSet.RollbackRetainingTrans");
			}

			trans.RollbackRetaining();
		}
	}
}
//------------------------------------------------------------------------------
