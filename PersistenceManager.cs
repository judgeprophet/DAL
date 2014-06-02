using System;
using System.Collections.Generic;
using System.Text;

namespace Dal
{
    /// <summary>
    /// Classe Principal de connexion BD à dériver
    /// Pour changer de Driver de connection BD simplement changer la classe dérivé
    /// </summary>
    public class PersistenceManager : SqlClientPersistenceManager
    //public class PersistenceManager : ODPPersistenceManager
    {
        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public PersistenceManager()
            : this ("", "") { }

        /// <summary>
        /// Constructor with ConnectionStringName initialisation
        /// </summary>
        /// <param name="connectionStringName"></param>
        public PersistenceManager(string connectionStringName)
            : this(connectionStringName, "") { }

        /// <summary>
        /// Constructor with ConnectionStringName and CommandText initialisation
        /// </summary>
        /// <param name="connectionStringName"></param>
        /// <param name="commandText"></param>
        public PersistenceManager(string connectionStringName, string commandText)
            : base(connectionStringName, commandText) { }

        #endregion Constructor
    }
}
