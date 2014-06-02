using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Reflection;
using System.Text;

namespace Dal
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class OleDBPersistenceManager : IDisposable
    {

        #region Privates Members

        string _connectStringName = null;
        string _commandText = null;


        #endregion Private Member

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public OleDBPersistenceManager()
            : this("", "") { }

        /// <summary>
        /// Constructor with ConnectionStringName initialisation
        /// </summary>
        /// <param name="connectionStringName"></param>
        public OleDBPersistenceManager(string connectionStringName)
            : this(connectionStringName, "") { }

        /// <summary>
        /// Constructor with ConnectionStringName and CommandText initialisation
        /// </summary>
        /// <param name="connectionStringName"></param>
        /// <param name="commandText"></param>
        public OleDBPersistenceManager(string connectionStringName, string commandText)
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
                OleDbDataReader dataReader = null;
                OleDbConnection objConn = new OleDbConnection(ConnectString);
                objConn.Open();

                //string str = "alter session set optimizer_index_caching=90";
                //using (OleDbCommand objCmd = new OleDbCommand(str))
                //{
                //    objCmd.Connection = objConn;
                //    objCmd.ExecuteNonQuery();
                //}

                //string str2 = "alter session set optimizer_index_cost_adj=15";
                //using (OleDbCommand objCmd = new OleDbCommand(str2))
                //{
                //    objCmd.Connection = objConn;
                //    objCmd.ExecuteNonQuery();
                //}

//DEV
//alter session set optimizer_index_caching=90
//alter session set optimizer_index_cost_adj=15

//PROD
//alter session set optimizer_index_caching=100
//alter session set optimizer_index_cost_adj=0

                using (OleDbCommand objCmd = new OleDbCommand(CommandText))
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
                using (OleDbConnection objConn = new OleDbConnection(ConnectString))
                {
                    // build command
                    using (OleDbCommand objCmd = new OleDbCommand(CommandText, objConn))
                    {
                        if (parameters != null)
                        {
                            objCmd.Parameters.AddRange(parameters);
                        }
                        //return objCmd.ExecuteReader(CommandBehavior.CloseConnection);
                        using (OleDbDataAdapter oda = new OleDbDataAdapter(objCmd))
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
                using (OleDbConnection objConn = new OleDbConnection(ConnectString))
                {
                    objConn.Open();
                    using (OleDbCommand objCmd = new OleDbCommand(CommandText, objConn))
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
        /// Execute une requete et retourne le nombre d'enregistrement affecté
        /// </summary>
        /// <returns></returns>
        public int ExecuteNonQuery()
        {
            return ExecuteNonQuery(null);
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
            OleDbType dbType;
            switch (type)
            {
                case DbType.String:
                    dbType = OleDbType.VarChar;
                    break;
                case DbType.Date:
                    dbType = OleDbType.DBDate;
                    break;
                case DbType.DateTime:
                    dbType = OleDbType.DBTimeStamp;
                    break;
                //case DbType.DateTime:
                //    dbType = OleDbType.;
                //    break;

                default:
                    dbType = (OleDbType)type;
                    break;
            }

            OleDbParameter param = new OleDbParameter(parameterName, dbType, size);
            param.Value = value;
            param.Direction = paramDirection;

            return param;
        }

        #region Pour génération Générique
        ///// <summary>
        ///// Obtient les données pour le rapport/Entity recu lorsque le type est connu
        ///// </summary>
        ///// <param name="entityType">Doit etre dérivé de EntityBase</param>
        ///// <returns></returns>
        //public OleDbDataReader ObtenirDataReader(System.Type entityType)
        //{
        //    OleDbDataReader dr = null;
        //    object dataReport = null;

        //    //== Créé une instance de l'entité à récupérer
        //    dataReport = Activator.CreateInstance(entityType);

        //    //== Comme la fonction ObtenirDataReader est Abstract dans la classe de base 
        //    //== on peut l'appeler ainsi (plutot que la méthode par réflexion) car elle est
        //    //== définit dans la classe appelante.
        //    dr = ((EntityBase)dataReport).ObtenirDataReader();

        //    //==  Si on doit absolument passé par la classe appelante on utilise la reflection pour l'invoquer
        //    //== à partir de sont type
        //    //System.Reflection.MethodInfo mi = entityType.GetMethod("ObtenirDataReader");
        //    //mi.Invoke(dataReport, null);

        //    return dr;
        //}

        ///// <summary>
        ///// Obtient les données pour le rapport/Entity recu en chaine de caractères. Les paramètres sont CASE SENSITIVE.
        ///// </summary>
        ///// <param name="assemblyName">Nom comple du DLL/Assembly EX: GZM.RptApp.Entities</param>
        ///// <param name="entityName">Nom de la classe correspondant au rapport (doit etre dérivé de EntityBase)</param>
        ///// <returns></returns>
        //public OleDbDataReader ObtenirDataReader(string assemblyName, string entityName)
        //{
        //    //== Charge l'Assembly pour accèder à ses propriétés
        //    Assembly assembly = Assembly.Load(assemblyName);

        //    //== Recupere le nom complet de l'assembly
        //    //AssemblyName assemblyName = assembly.GetName();

        //    //== Construit le nom complet de la classe à accèder
        //    string fullClassName = assemblyName + "." + entityName;

        //    //== Récupere le TYPE De la classe (transforme la string en OBJET)
        //    Type rptType = assembly.GetType(fullClassName);

        //    //=== Appel de la fonction de base qui retourne les données
        //    return ObtenirDataReader(rptType);
        //}

        #endregion
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
