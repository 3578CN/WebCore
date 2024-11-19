using System.Data;
using MySql.Data.MySqlClient;

namespace CBC.WebCore.Common.DataHelpers
{
    /// <summary>
    /// MySQL 数据库操作帮助类。
    /// </summary>
    public class MySqlHelper : IDisposable
    {
        private MySqlConnection _connection;
        private string _connectionString;

        #region 属性

        /// <summary>
        /// 设置或获取数据库连接字符串。
        /// </summary>
        public string ConnectionString
        {
            get => _connectionString;
            set => _connectionString = value;
        }

        /// <summary>
        /// 获取或设置参数集合。
        /// 这些参数可以用于查询、命令等操作。
        /// </summary>
        public IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        #endregion

        #region 构造函数

        /// <summary>
        /// 带连接字符串的构造函数。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串。</param>
        public MySqlHelper(string connectionString = null)
        {
            if (connectionString == null)
            {
                _connection = new MySqlConnection();
            }
            else
            {
                _connectionString = connectionString;
                _connection = new MySqlConnection(connectionString);
            }
        }

        #endregion

        #region 打开数据库连接

        /// <summary>
        /// 打开数据库连接。
        /// </summary>
        private void Open()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                try
                {
                    _connection.ConnectionString = _connectionString;
                    _connection.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception("打开数据库连接时发生错误：" + ex.Message);
                }
            }
        }

        #endregion

        #region 参数化查询支持

        /// <summary>
        /// 向 MySqlCommand 添加参数。
        /// </summary>
        /// <param name="command">MySqlCommand 对象。</param>
        private void AddParameters(MySqlCommand command)
        {
            foreach (var param in Parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }

        #endregion

        #region 数据库连接测试

        /// <summary>
        /// 测试数据库连接。
        /// </summary>
        /// <returns>返回打开数据库连接是否成功。</returns>
        public bool TestConnection()
        {
            try
            {
                Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 执行 SQL 命令

        /// <summary>
        /// 执行 SQL 命令并返回数据表。
        /// </summary>
        /// <param name="sql">SQL 命令。</param>
        /// <param name="index">表索引。</param>
        /// <returns>返回数据表。</returns>
        public DataTable GetDataTable(string sql, int index = 0)
        {
            Open();

            using var command = new MySqlCommand(sql, _connection);
            AddParameters(command);
            using var dataAdapter = new MySqlDataAdapter(command);
            var dataSet = new DataSet();
            dataAdapter.Fill(dataSet);

            return dataSet.Tables[index];
        }

        /// <summary>
        /// 执行 SQL 命令并返回数据读取器。
        /// </summary>
        /// <param name="sql">SQL 命令。</param>
        /// <returns>返回数据读取器。</returns>
        public MySqlDataReader ExecuteReader(string sql)
        {
            Open();

            var command = new MySqlCommand(sql, _connection);
            AddParameters(command);
            return command.ExecuteReader();
        }

        /// <summary>
        /// 执行 SQL 命令并返回单一结果。
        /// </summary>
        /// <param name="sql">SQL 命令。</param>
        /// <returns>返回单一结果。</returns>
        public object ExecuteScalar(string sql)
        {
            Open();

            using var command = new MySqlCommand(sql, _connection);
            AddParameters(command);
            return command.ExecuteScalar();
        }

        /// <summary>
        /// 执行 SQL 命令并返回受影响的行数。
        /// </summary>
        /// <param name="sql">SQL 命令。</param>
        /// <returns>返回受影响的行数。</returns>
        public int ExecuteNonQuery(string sql)
        {
            Open();

            using var command = new MySqlCommand(sql, _connection);
            AddParameters(command);
            return command.ExecuteNonQuery();
        }

        #endregion

        #region 执行存储过程

        /// <summary>
        /// 执行存储过程并返回数据表。
        /// </summary>
        /// <param name="procName">存储过程名称。</param>
        /// <param name="index">表索引。</param>
        /// <returns>返回数据表。</returns>
        public DataTable SPGetDataTable(string procName, int index = 0)
        {
            Open();

            using var command = new MySqlCommand(procName, _connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            AddParameters(command);

            using var dataAdapter = new MySqlDataAdapter(command);
            var dataSet = new DataSet();
            dataAdapter.Fill(dataSet);

            return dataSet.Tables[index];
        }

        /// <summary>
        /// 执行存储过程并返回数据读取器。
        /// </summary>
        /// <param name="procName">存储过程名称。</param>
        /// <returns>返回数据读取器。</returns>
        public MySqlDataReader SPExecuteReader(string procName)
        {
            Open();

            var command = new MySqlCommand(procName, _connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            AddParameters(command);

            return command.ExecuteReader();
        }

        /// <summary>
        /// 执行存储过程并返回单一结果。
        /// </summary>
        /// <param name="procName">存储过程名称。</param>
        /// <returns>返回单一结果。</returns>
        public object SPExecuteScalar(string procName)
        {
            Open();

            using var command = new MySqlCommand(procName, _connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            AddParameters(command);

            return command.ExecuteScalar();
        }

        /// <summary>
        /// 执行存储过程并返回受影响的行数。
        /// </summary>
        /// <param name="procName">存储过程名称。</param>
        /// <returns>返回受影响的行数。</returns>
        public int SPExecuteNonQuery(string procName)
        {
            Open();

            using var command = new MySqlCommand(procName, _connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            AddParameters(command);

            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行存储过程并返回参数输出值。
        /// </summary>
        /// <param name="procName">存储过程名称。</param>
        /// <param name="outParams">输出参数名称。</param>
        /// <returns>返回参数输出值的字典。</returns>
        public IDictionary<string, string> SPGetOutputParams(string procName, string[] outParams)
        {
            Open();

            using var command = new MySqlCommand(procName, _connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            var output = new Dictionary<string, string>();

            foreach (var param in outParams)
            {
                command.Parameters.Add(param, MySqlDbType.VarChar).Direction = ParameterDirection.Output;
                output.Add(param, command.Parameters[param].Value?.ToString() ?? string.Empty);
            }

            return output;
        }

        #endregion

        #region IDisposable 实现

        private bool _disposed = false;

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        /// <param name="disposing">是否释放托管资源。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Close();
                    _connection?.Dispose();
                    _connectionString = null;
                    Parameters = null;
                }
                _disposed = true;
            }
        }

        #endregion
    }
}