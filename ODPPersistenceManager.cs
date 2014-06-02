using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
//using System.Data.OleDb;
using System.Data.Common;
using System.Reflection;
using System.Text;

using Oracle.DataAccess.Client;

namespace Dal
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ODPPersistenceManager
    {

        #region Privates Members

        string _connectStringName = null;
        string _commandText = null;


        #endregion Private Member

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public ODPPersistenceManager()
            : this("", "") { }

        /// <summary>
        /// Constructor with ConnectionStringName initialisation
        /// </summary>
        /// <param name="connectionStringName"></param>
        public ODPPersistenceManager(string connectionStringName)
            : this(connectionStringName, "") { }

        /// <summary>
        /// Constructor with ConnectionStringName and CommandText initialisation
        /// </summary>
        /// <param name="connectionStringName"></param>
        /// <param name="commandText"></param>
        public ODPPersistenceManager(string connectionStringName, string commandText)
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
            get
            {
                return _connectStringName;
            }
            set
            {
                _connectStringName = value;
            }
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
            set
            {
                _commandText = value;
            }
        }


        #endregion Properties

        #region Public Method
        /// <summary>
        /// Execute une requete sur la BD
        /// </summary>
        /// <returns></returns>
        public DbDataReader ExecuteQueryDataReader()
        {
            OracleCommand objCmd = new OracleCommand();
            OracleDataReader dataReader = null;

            try
            {
                OracleConnection objConn = new OracleConnection(ConnectString);
                objConn.ConnectionString = ConnectString;
                //objConn.Open();
                //execute queries
                //objConn.Close();

                objConn.Open();
                objCmd.Connection = objConn;
                objCmd.CommandText = CommandText;
                dataReader = objCmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch
            {
                throw;
            }
            finally
            {
                objCmd.Dispose();
                //objConn.Dispose();
            }

            return dataReader;
        }

        /// <summary>
        /// Execute une requete sur la BD
        /// </summary>
        /// <returns></returns>
        public DataSet ExecuteQueryDataSet()
        {
            DataSet dataSet = null;
            OracleConnection objConn = new OracleConnection(ConnectString);
            OracleCommand objCmd = new OracleCommand();
            OracleDataAdapter oda = new OracleDataAdapter(objCmd);

            try
            {

                objConn.Open();
                objCmd.Connection = objConn;
                objCmd.CommandText = CommandText;
                dataSet = new DataSet();
                oda.Fill(dataSet);
            }
            catch
            {
                throw;
            }
            finally
            {
                oda.Dispose();
                objCmd.Dispose();
                objConn.Close();
                objConn.Dispose();
            }

            return dataSet;
        }


        /// <summary>
        /// Execute une stored proc
        /// </summary>
        /// <param name="parameters"></param>
        public void ExecuteStoredProc(DbParameter[] parameters)
        {
            OracleConnection objConn = new OracleConnection(ConnectString);
            OracleCommand objCmd = new OracleCommand();

            try
            {

                objConn.Open();
                objCmd.Connection = objConn;
                objCmd.CommandText = CommandText;
                objCmd.Parameters.AddRange(parameters);

            }
            catch
            {
                throw;
            }
            finally
            {
                objCmd.Dispose();
                objConn.Close();
                objConn.Dispose();
            }
        }
        /// <summary>
        /// Execute une requete et retourne le nombre d'enregistrement affecté
        /// </summary>
        /// <returns></returns>
        public int ExecuteNonQuery()
        {
            OracleConnection objConn = new OracleConnection(ConnectString);
            OracleCommand objCmd = new OracleCommand();
            int nbEnregAffecte = 0;

            try
            {

                objConn.Open();
                objCmd.Connection = objConn;
                objCmd.CommandText = CommandText;
                nbEnregAffecte = objCmd.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
            finally
            {
                objCmd.Dispose();
                objConn.Close();
                objConn.Dispose();
            }

            return nbEnregAffecte;
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
            OracleDbType dbType;
            if (type == DbType.String)
            {
                dbType = OracleDbType.Varchar2;
            }
            else
            {
                dbType = (OracleDbType)type;
            }
            //OracleParameter param = new OracleParameter(parameterName, (OracleDbType)type, size);
            OracleParameter param = new OracleParameter(parameterName, dbType, size);
            param.Value = value;

            return param;
        }


        #endregion
    }
}
