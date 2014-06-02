using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace Dal
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SqlClientPersistenceManager : IDisposable
    {

        #region Privates Members

        string _connectStringName = null;
        string _commandText = null;


        #endregion Private Member

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public SqlClientPersistenceManager()
            : this("", "") { }

        /// <summary>
        /// Constructor with ConnectionStringName initialisation
        /// </summary>
        /// <param name="connectionStringName"></param>
        public SqlClientPersistenceManager(string connectionStringName)
            : this(connectionStringName, "") { }

        /// <summary>
        /// Constructor with ConnectionStringName and CommandText initialisation
        /// </summary>
        /// <param name="connectionStringName"></param>
        /// <param name="commandText"></param>
        public SqlClientPersistenceManager(string connectionStringName, string commandText)
            : base()
        {
            _connectStringName = connectionStringName;
            _commandText = commandText;
        }
        #endregion Constructor
        
        #region Properties

        /// <summary>
        /// Nom du parametre pour Chaine de connexion à la BD
        /// </summary>
        public string ConnectStringName
        {
            get { return _connectStringName; }
            set { _connectStringName = value; }
        }

        /// <summary>
        /// ConnectionString pour la BD (la propriété ConnectStringName doit etre déclarer)
        /// </summary>
        private string ConnectString
        {
            get
            {
                //== Vérifie la définition du nom de paramètres
                if (String.IsNullOrEmpty(_connectStringName))
                {
                    throw new ApplicationException("ConnectStringName : Must be defined");
                }

                return ConfigurationManager.ConnectionStrings[_connectStringName].ToString();
            }
        }

        /// <summary>
        /// SQL pour l'éxécution 
        /// </summary>
        public string CommandText
        {
            get
            {
                //== Vérifie la définition du nom de paramètres
                if (String.IsNullOrEmpty(_commandText))
                {
                    throw new ApplicationException("Command Text (SQL) : Must be defined");
                }
                return _commandText;
            }
            set { _commandText = value; }
        }


        #endregion Properties

        #region Public Method
        /// <summary>
        /// Execute une requete sur la BD
        /// </summary>
        /// <returns></returns>
        public DbDataReader ExecuteQueryDataReader()
        {
            return ExecuteQueryDataReader(null);
        }

        /// <summary>
        /// Execute une requete sur la BD
        /// </summary>
        /// <param name="parameters">Si c'est pour un cursor omettre le ?</param>
        /// <returns></returns>
        public DbDataReader ExecuteQueryDataReader(DbParameter[] parameters)
        {
            try
            {
                SqlDataReader dataReader = null;
                SqlConnection objConn = new SqlConnection(ConnectString);
                objConn.Open();

                using (SqlCommand objCmd = new SqlCommand(CommandText))
                {
                    if (parameters != null)
                    {
                        objCmd.Parameters.AddRange(parameters);
                    }

                    objCmd.Connection = objConn;
                    dataReader = objCmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                return dataReader;
            }
            catch { throw; }
        }

        /// <summary>
        /// Execute une requete sur la BD 
        /// </summary>
        /// <returns></returns>        
        public DataSet ExecuteQueryDataSet()
        {
            return ExecuteQueryDataSet(null);
        }

        /// <summary>
        /// Requete avec Parametres
        /// </summary>
        /// <param name="parameters">Si c'est pour un cursor omettre le ?</param>
        /// <returns></returns>
        public DataSet ExecuteQueryDataSet(DbParameter[] parameters)
        {
            try
            {
                using (SqlConnection objConn = new SqlConnection(ConnectString))
                {
                    // build command
                    using (SqlCommand objCmd = new SqlCommand(CommandText, objConn))
                    {
                        if (parameters != null)
                        {
                            objCmd.Parameters.AddRange(parameters);
                        }
                        //return objCmd.ExecuteReader(CommandBehavior.CloseConnection);
                        using (SqlDataAdapter oda = new SqlDataAdapter(objCmd))
                        {
                            using (DataSet ds = new DataSet())
                            {
                                oda.Fill(ds);
                                return ds;
                            }
                        }
                    }
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Execute une requete et retourne le nombre d'enregistrement affecté
        /// </summary>
        /// <returns></returns>
        public int ExecuteNonQuery()
        {
            return ExecuteNonQuery(null);
        }

        /// <summary>
        /// Execute une stored proc
        /// </summary>
        /// <param name="parameters">Si c'est pour un cursor omettre le ?</param>
        /// <remarks> 
        /// Pour les stored Proc FORMAT : {CALL someProcedure(?)}
        /// </remarks>
        public int ExecuteNonQuery(DbParameter[] parameters)
        {
            try
            {
                int nbEnregAffecte = 0;
                using (SqlConnection objConn = new SqlConnection(ConnectString))
                {
                    objConn.Open();
                    using (SqlCommand objCmd = new SqlCommand(CommandText, objConn))
                    {
                        if (parameters != null)
                        {
                            objCmd.Parameters.AddRange(parameters);
                        }
                        nbEnregAffecte = objCmd.ExecuteNonQuery();
                    }
                }
                return nbEnregAffecte;
            }
            catch { throw; }
        }

        /// <summary>
        /// Creation de parametre pour la base de donnée correspondante
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public DbParameter CreateParameter(string parameterName, DbType type, int size, object value)
        {
            return CreateParameter(parameterName, type, size, value, ParameterDirection.Input);
        }

        /// <summary>
        /// Creation de parametre pour la base de donnée correspondante
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <param name="value"></param>
        /// <param name="paramDirection">Direction du parametre : Input, Output etc.</param>
        /// <returns></returns>
        public DbParameter CreateParameter(string parameterName, DbType type, int size, object value, ParameterDirection paramDirection)
        {
            SqlDbType dbType;
            switch (type)
            {
                case DbType.String:
                    dbType = SqlDbType.NVarChar;
                    break;
                case DbType.Date:
                    dbType = SqlDbType.Date;
                    break;
                case DbType.DateTime:
                    dbType = SqlDbType.DateTime;
                    break;
                //case DbType.DateTime:
                //    dbType = OleDbType.;
                //    break;

                default:
                    dbType = (SqlDbType)type;
                    break;
            }

            SqlParameter param = new SqlParameter(parameterName, dbType, size);
            param.Value = value;
            param.Direction = paramDirection;

            return param;
        }
        #endregion

        #region IDisposable Interface
        // Track whether Dispose has been called.
        private bool disposed = false;

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    //component.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                disposed = true;
            }
        }
        #endregion IDisposable Interface
    }
}
