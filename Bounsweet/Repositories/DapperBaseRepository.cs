using Bounsweet.Models;
using Bounsweet.Repositories.Adapters;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bounsweet.Repositories
{
    public abstract class DapperBaseRepository<TConn> where TConn : IDbConnection, new()
    {
        private bool _disposed;
        protected TConn _conn;
        protected IDbTransaction _transaction;
        private DbAdapter<TConn> _adapter;

        protected void BeginConnection(string connectionString)
        {
            _adapter = new DbAdapter<TConn>();
            _conn = new TConn();
            _conn.ConnectionString = connectionString;
            _conn.Open();
        }

        protected void BeginTrans()
        {
            _transaction = _conn.BeginTransaction();
        }


        protected void Commit()
        {
            try
            {
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
            }
            finally
            {
                _transaction.Dispose();
            }
        }
        public void Rollback()
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }

        ~DapperBaseRepository()
        {
            dispose(false);
        }

        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        private void dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                        _transaction = null;
                    }
                    if (_conn != null)
                    {
                        _conn.Dispose();
                        _conn = default;
                    }
                }
                _disposed = true;
            }
        }


        /// <summary>
        /// 取得第一筆資料
        /// </summary>
        /// <typeparam name="T">表單類型</typeparam>
        /// <param name="queryCondition">
        /// 查詢條件
        /// 使用範例: 
        /// new ColValue<T> 
        /// {
        ///    { c => c.Col1, value},
        ///    { c => c.Col2, value}
        /// }
        /// </param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual T GetFirstOrDefault<T>(ColSet<T> queryCondition, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (queryCondition == null)
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }

            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            StringBuilder sqlBuilder = new StringBuilder($"SELECT * FROM {tableName}");
            DynamicParameters queryData = new DynamicParameters();
            if (queryCondition != null)
            {
                sqlBuilder.Append(" WHERE ");

                var filters = queryCondition.Select(p => $"{GetMemberName(p.Key.Body)} = {_adapter.DbParamTag}{GetMemberName(p.Key.Body)}");
                foreach (var data in queryCondition)
                {
                    queryData.Add(GetMemberName(data.Key.Body), data.Value);
                }
                sqlBuilder.AppendJoin(" AND ", filters);
            }
            T result = _conn.QueryFirstOrDefault<T>(sqlBuilder.ToString(), queryData, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 取得第一筆資料
        /// </summary>
        /// <typeparam name="T">表單類型</typeparam>
        /// <param name="queryCondition">
        /// 查詢條件
        /// 使用範例: 
        /// new ColValue<T> 
        /// {
        ///    { c => c.Col1, value},
        ///    { c => c.Col2, value}
        /// }
        /// </param>
        /// <param name="queryParam">查詢參數</param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual T GetFirstOrDefault<T>(string queryCondition, DynamicParameters queryParam, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (string.IsNullOrEmpty(queryCondition))
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }

            if (queryParam == null)
            {
                throw new ArgumentNullException(nameof(queryParam));
            }
            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            StringBuilder sqlBuilder = new StringBuilder($"SELECT * FROM {tableName}");
            sqlBuilder.Append($" WHERE {queryCondition}");
            T result = _conn.QueryFirstOrDefault<T>(sqlBuilder.ToString(), queryParam, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 取得多筆資料
        /// </summary>
        /// <typeparam name="T">表單類型</typeparam>
        /// <param name="queryCondition">
        /// 查詢條件
        /// 使用範例: 
        /// new ColValue<T> 
        /// {
        ///    { c => c.Col1, value},
        ///    { c => c.Col2, value}
        /// }
        /// </param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual IEnumerable<T> Get<T>(ColSet<T>? queryCondition = null, string tableName = "", bool buffer = true, int? commandTimeout = null) where T : class
        {
            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            StringBuilder sqlBuilder = new StringBuilder($"SELECT * FROM {tableName}");
            DynamicParameters queryData = new DynamicParameters();
            if (queryCondition != null)
            {
                sqlBuilder.Append(" WHERE ");

                var filters = queryCondition.Select(p => $"{GetMemberName(p.Key.Body)} = {_adapter.DbParamTag}{GetMemberName(p.Key.Body)}");
                foreach (var data in queryCondition)
                {
                    queryData.Add(GetMemberName(data.Key.Body), data.Value);
                }
                sqlBuilder.AppendJoin(" AND ", filters);
            }
            IEnumerable<T> result = _conn.Query<T>(sqlBuilder.ToString(), queryData, _transaction, buffer, commandTimeout);
            return result;
        }

        /// <summary>
        /// 取得多筆資料
        /// </summary>
        /// <typeparam name="T">表單類型</typeparam>
        /// <param name="queryCondition">查詢條件</param>
        /// <param name="queryParam">查詢參數</param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual IEnumerable<T> Get<T>(string queryCondition, DynamicParameters queryParam, string tableName = "", bool buffer = true, int? commandTimeout = null) where T : class
        {
            if (string.IsNullOrEmpty(queryCondition))
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }

            if (queryParam == null)
            {
                throw new ArgumentNullException(nameof(queryParam));
            }

            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            StringBuilder sqlBuilder = new StringBuilder($"SELECT * FROM {tableName}");
            if (queryParam != null)
            {
                sqlBuilder.Append(" WHERE ");
                if (!string.IsNullOrEmpty(queryCondition))
                {
                    sqlBuilder.Append(queryCondition);
                }
            }
            IEnumerable<T> result = _conn.Query<T>(sqlBuilder.ToString(), queryParam, _transaction, buffer, commandTimeout);
            return result;
        }

        /// <summary>
        /// 新增資料
        /// </summary>
        /// <typeparam name="T">表單類型</typeparam>
        /// <param name="newData">新資料</param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual int InsertAll<T>(T newData, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (newData == null)
            {
                throw new ArgumentNullException(nameof(newData));
            }

            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            var allProps = typeof(T).GetProperties().Select(p => p.Name);
            StringBuilder sqlBuilder = new StringBuilder($"INSERT INTO {tableName}( ");
            sqlBuilder.AppendJoin(',', allProps);
            sqlBuilder.Append(") VALUES (");
            allProps = allProps.Select(p => $"{_adapter.DbParamTag}{p}");
            sqlBuilder.AppendJoin(',', allProps);
            sqlBuilder.Append(")");

            var result = _conn.Execute(sqlBuilder.ToString(), newData, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 新增資料
        /// </summary>
        /// <typeparam name="T">表單類型</typeparam>
        /// <param name="newData">新資料</param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual int Insert<T>(DynamicParameters newData, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (newData == null)
            {
                throw new ArgumentNullException(nameof(newData));
            }
            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            var allProps = typeof(T).GetProperties().Select(p => p.Name);
            StringBuilder sqlBuilder = new StringBuilder($"INSERT INTO {tableName}( ");
            sqlBuilder.AppendJoin(',', allProps);
            sqlBuilder.Append(") VALUES (");
            allProps = allProps.Select(p => $"{_adapter.DbParamTag}{p}");
            sqlBuilder.AppendJoin(',', allProps);
            sqlBuilder.Append(")");

            var result = _conn.Execute(sqlBuilder.ToString(), newData, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 新增資料
        /// </summary>
        /// <typeparam name="T">表單類型</typeparam>
        /// <param name="newData">新資料</param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual int Insert<T>(ColSet<T> newData, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (newData == null)
            {
                throw new ArgumentNullException(nameof(newData));
            }
            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            var newDataDict = newData.ToDictionary(n => GetMemberName(n.Key.Body), n => n.Value);

            StringBuilder sqlBuilder = new StringBuilder($"INSERT INTO {tableName}( ");
            sqlBuilder.AppendJoin(',', newDataDict.Keys);
            sqlBuilder.Append(") VALUES (");
            var newDataVal = newDataDict.Keys.Select(p => $"{_adapter.DbParamTag}{p}");
            sqlBuilder.AppendJoin(',', newDataVal);
            sqlBuilder.Append(")");

            DynamicParameters newDataParam = new DynamicParameters();
            foreach (var data in newDataDict)
            {
                newDataParam.Add(data.Key, data.Value);
            }
            var result = _conn.Execute(sqlBuilder.ToString(), newDataParam, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 批次新增資料
        /// </summary>
        /// <typeparam name="T">表單類型</typeparam>
        /// <param name="newData">新資料</param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns>更新數量</returns>
        protected virtual int Insert<T>(IEnumerable<ColSet<T>> newDatas, string tableName = "", int? commandTimeout = null) where T : class
        {
            BeginTrans();
            var updateTotalCount = 0;
            foreach (var data in newDatas)
            {
                updateTotalCount += Insert<T>(data, tableName, commandTimeout);
            }
            Commit();
            return updateTotalCount;
        }

        /// <summary>
        /// 更新單筆資料
        /// </summary>
        /// <typeparam name="T">更新表單類型</typeparam>
        /// <param name="updateData">更新資料</param>
        /// <param name="queryCondition">
        /// 查詢條件
        /// 使用範例: 
        /// new ColValue<T> 
        /// {
        ///    { c => c.Col1, value},
        ///    { c => c.Col2, value}
        /// }
        /// </param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual int Update<T>(ColSet<T> updateData, ColSet<T> queryCondition, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (updateData == null)
            {
                throw new ArgumentNullException(nameof(updateData));
            }
            if (queryCondition == null)
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }
            DynamicParameters paramData = new DynamicParameters();

            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;

            var needUpdateCols = updateData.Select(p => $"{GetMemberName(p.Key.Body)} = {_adapter.DbParamTag}{GetMemberName(p.Key.Body)}");
            // 開始組sql字串
            StringBuilder sqlBuilder = new StringBuilder($"UPDATE {tableName} SET ");
            sqlBuilder.AppendJoin(',', needUpdateCols);
            sqlBuilder.Append(" WHERE ");
            // 查詢條件的標記
            var queryTag = "query_tag_";
            var filters = queryCondition.Select(p => $"{GetMemberName(p.Key.Body)} = {_adapter.DbParamTag}{queryTag}{GetMemberName(p.Key.Body)}");
            sqlBuilder.AppendJoin(" AND ", filters);

            foreach (var col in updateData)
            {
                paramData.Add($"{GetMemberName(col.Key.Body)}", col.Value);
            }
            foreach (var col in queryCondition)
            {
                paramData.Add($"{queryTag}{GetMemberName(col.Key.Body)}", col.Value);
            }

            var result = _conn.Execute(sqlBuilder.ToString(), paramData, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 更新單筆資料
        /// </summary>
        /// <typeparam name="T">更新表單類型</typeparam>
        /// <param name="updateData">更新資料</param>
        /// <param name="queryCondition">更新查詢條件</param>
        /// <param name="paramData">查詢條件資料</param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual int Update<T>(ColSet<T> updateData, string queryCondition, DynamicParameters paramData, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (updateData == null)
            {
                throw new ArgumentNullException(nameof(updateData));
            }
            if (string.IsNullOrEmpty(queryCondition))
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }

            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            var needUpdateCols = updateData.Select(p => $"{GetMemberName(p.Key.Body)} = {_adapter.DbParamTag}{GetMemberName(p.Key.Body)}");
            // 開始組sql字串
            StringBuilder sqlBuilder = new StringBuilder($"UPDATE {tableName} SET ");
            sqlBuilder.AppendJoin(',', needUpdateCols);
            sqlBuilder.Append($" WHERE {queryCondition}");

            foreach (var col in updateData)
            {
                paramData.Add(GetMemberName(col.Key.Body), col.Value);
            }

            var result = _conn.Execute(sqlBuilder.ToString(), paramData, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 更新單筆資料
        /// </summary>
        /// <typeparam name="T">更新表單類型</typeparam>
        /// <param name="updateData">更新資料</param>
        /// <param name="queryCondition">
        /// 查詢條件
        /// 使用範例: 
        /// new ColValue<T> 
        /// {
        ///    { c => c.Col1, value},
        ///    { c => c.Col2, value}
        /// }
        /// </param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual int UpdateAll<T>(T updateData, ColSet<T> queryCondition, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (updateData == null)
            {
                throw new ArgumentNullException(nameof(updateData));
            }
            if (queryCondition == null)
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }
            DynamicParameters paramData = new DynamicParameters();

            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;

            var updateDataDict = typeof(T).GetProperties().ToDictionary(t => t.Name, t => t.GetValue(updateData, null));
            var needUpdateCols = updateDataDict.Keys.Select(c => $"{c} = {_adapter.DbParamTag}{c}");
            // 開始組sql字串
            StringBuilder sqlBuilder = new StringBuilder($"UPDATE {tableName} SET ");
            sqlBuilder.AppendJoin(',', needUpdateCols);
            sqlBuilder.Append(" WHERE ");
            // 查詢條件的標記
            var queryTag = "query_tag_";
            var filters = queryCondition.Select(p => $"{GetMemberName(p.Key.Body)} = {_adapter.DbParamTag}{queryTag}{GetMemberName(p.Key.Body)}");
            sqlBuilder.AppendJoin(" AND ", filters);

            foreach (var item in updateDataDict)
            {
                paramData.Add(item.Key, item.Value);
            }
            foreach (var col in queryCondition)
            {
                paramData.Add($"{queryTag}{GetMemberName(col.Key.Body)}", col.Value);
            }

            var result = _conn.Execute(sqlBuilder.ToString(), paramData, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 多筆批次資料更新
        /// </summary>
        /// <typeparam name="T">更新表單類型</typeparam>
        /// <param name="updateData">更新資料</param>
        /// <param name="queryCondition">
        /// 查詢條件
        /// 使用範例: 
        /// new ColValue<T> 
        /// {
        ///    { c => c.Col1, value},
        ///    { c => c.Col2, value}
        /// }
        /// </param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns>更新數量</returns>
        protected virtual int Update<T>(IEnumerable<ColSet<T>> updateDatas, ColSet<T> queryCondition, string tableName = "", int? commandTimeout = null) where T : class
        {
            BeginTrans();
            var updateTotalCount = 0;
            foreach (var data in updateDatas)
            {
                updateTotalCount += Update<T>(data, queryCondition, tableName, commandTimeout);
            }
            Commit();
            return updateTotalCount;
        }

        /// <summary>
        /// 刪除多筆資料
        /// </summary>
        /// <typeparam name="T">更新表單類型</typeparam>
        /// <param name="queryCondition">
        /// 查詢條件
        /// 使用範例: 
        /// new ColValue<T> 
        /// {
        ///    { c => c.Col1, value},
        ///    { c => c.Col2, value}
        /// }
        /// </param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        protected virtual int Delete<T>(ColSet<T> queryCondition, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (queryCondition == null)
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }
            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            DynamicParameters paramData = new DynamicParameters();
            var filters = queryCondition.Select(p => $"{GetMemberName(p.Key.Body)} = {_adapter.DbParamTag}{GetMemberName(p.Key.Body)}");
            StringBuilder sqlBuilder = new StringBuilder($"DELETE FROM {tableName} WHERE ");
            sqlBuilder.AppendJoin(" AND ", filters);
            foreach (var col in queryCondition)
            {
                paramData.Add($"{GetMemberName(col.Key.Body)}", col.Value);
            }
            var result = _conn.Execute(sqlBuilder.ToString(), paramData, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 刪除多筆資料
        /// </summary>
        /// <typeparam name="T">更新表單類型</typeparam>
        /// <param name="queryCondition">查詢條件</param>
        /// <param name="queryParam">查詢條件資料</param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        protected virtual int Delete<T>(string queryCondition, DynamicParameters queryParam, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (queryCondition == null)
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }
            if (queryParam == null)
            {
                throw new ArgumentNullException(nameof(queryParam));
            }
            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            StringBuilder sqlBuilder = new StringBuilder($"DELETE FROM {tableName} WHERE {queryCondition}");
            var result = _conn.Execute(sqlBuilder.ToString(), queryParam, _transaction, commandTimeout);
            return result;
        }





        /// <summary>
        /// 取得第一筆資料
        /// </summary>
        /// <typeparam name="T">表單類型</typeparam>
        /// <param name="queryCondition">
        /// 查詢條件
        /// 使用範例: 
        /// new ColValue<T> 
        /// {
        ///    { c => c.Col1, value},
        ///    { c => c.Col2, value}
        /// }
        /// </param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual async Task<T> GetFirstOrDefaultAsync<T>(ColSet<T> queryCondition, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (queryCondition == null)
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }

            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            StringBuilder sqlBuilder = new StringBuilder($"SELECT * FROM {tableName}");
            DynamicParameters queryData = new DynamicParameters();
            if (queryCondition != null)
            {
                sqlBuilder.Append(" WHERE ");

                if (queryCondition == null)
                {
                    throw new ArgumentNullException(nameof(queryCondition));
                }

                var filters = queryCondition.Select(p => $"{GetMemberName(p.Key.Body)} = {_adapter.DbParamTag}{GetMemberName(p.Key.Body)}");
                foreach (var data in queryCondition)
                {
                    queryData.Add(GetMemberName(data.Key.Body), data.Value);
                }
                sqlBuilder.AppendJoin(" AND ", filters);
            }
            T result = await _conn.QueryFirstOrDefaultAsync<T>(sqlBuilder.ToString(), queryData, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 取得第一筆資料
        /// </summary>
        /// <typeparam name="T">表單類型</typeparam>
        /// <param name="queryCondition">
        /// 查詢條件
        /// 使用範例: 
        /// new ColValue<T> 
        /// {
        ///    { c => c.Col1, value},
        ///    { c => c.Col2, value}
        /// }
        /// </param>
        /// <param name="queryParam">查詢參數</param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual async Task<T> GetFirstOrDefaultAsync<T>(string queryCondition, DynamicParameters queryParam, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (string.IsNullOrEmpty(queryCondition))
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }

            if (queryParam == null)
            {
                throw new ArgumentNullException(nameof(queryParam));
            }
            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            StringBuilder sqlBuilder = new StringBuilder($"SELECT * FROM {tableName}");
            sqlBuilder.Append($" WHERE {queryCondition}");
            T result = await _conn.QueryFirstOrDefaultAsync<T>(sqlBuilder.ToString(), queryParam, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 取得多筆資料
        /// </summary>
        /// <typeparam name="T">表單類型</typeparam>
        /// <param name="queryCondition">
        /// 查詢條件
        /// 使用範例: 
        /// new ColValue<T> 
        /// {
        ///    { c => c.Col1, value},
        ///    { c => c.Col2, value}
        /// }
        /// </param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual async Task<IEnumerable<T>> GetAsync<T>(ColSet<T>? queryCondition = null, string tableName = "", int? commandTimeout = null) where T : class
        {
            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            StringBuilder sqlBuilder = new StringBuilder($"SELECT * FROM {tableName}");
            DynamicParameters queryData = new DynamicParameters();
            if (queryCondition != null)
            {
                sqlBuilder.Append(" WHERE ");

                var filters = queryCondition.Select(p => $"{GetMemberName(p.Key.Body)} = {_adapter.DbParamTag}{GetMemberName(p.Key.Body)}");
                foreach (var data in queryCondition)
                {
                    queryData.Add(GetMemberName(data.Key.Body), data.Value);
                }
                sqlBuilder.AppendJoin(" AND ", filters);
            }
            IEnumerable<T> result = await _conn.QueryAsync<T>(sqlBuilder.ToString(), queryData, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 取得多筆資料
        /// </summary>
        /// <typeparam name="T">表單類型</typeparam>
        /// <param name="queryCondition">查詢條件</param>
        /// <param name="queryParam">查詢參數</param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual async Task<IEnumerable<T>> GetAsync<T>(string queryCondition, DynamicParameters queryParam, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (string.IsNullOrEmpty(queryCondition))
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }

            if (queryParam == null)
            {
                throw new ArgumentNullException(nameof(queryParam));
            }
            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            StringBuilder sqlBuilder = new StringBuilder($"SELECT * FROM {tableName}");
            if (queryParam != null)
            {
                sqlBuilder.Append(" WHERE ");
                if (!string.IsNullOrEmpty(queryCondition))
                {
                    sqlBuilder.Append(queryCondition);
                }
            }
            IEnumerable<T> result = await _conn.QueryAsync<T>(sqlBuilder.ToString(), queryParam, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 新增資料(全部欄位)
        /// </summary>
        /// <typeparam name="T">表單類型</typeparam>
        /// <param name="newData">新資料</param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual async Task<int> InsertAllAsync<T>(T newData, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (newData == null)
            {
                throw new ArgumentNullException(nameof(newData));
            }
            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            var allProps = typeof(T).GetProperties().Select(p => p.Name);
            StringBuilder sqlBuilder = new StringBuilder($"INSERT INTO {tableName}( ");
            sqlBuilder.AppendJoin(',', allProps);
            sqlBuilder.Append(") VALUES (");
            allProps = allProps.Select(p => $"{_adapter.DbParamTag}{p}");
            sqlBuilder.AppendJoin(',', allProps);
            sqlBuilder.Append(")");

            var result = await _conn.ExecuteAsync(sqlBuilder.ToString(), newData, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 新增資料
        /// </summary>
        /// <typeparam name="T">表單類型</typeparam>
        /// <param name="newData">新資料</param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual async Task<int> InsertAsync<T>(DynamicParameters newData, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (newData == null)
            {
                throw new ArgumentNullException(nameof(newData));
            }
            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            var allProps = typeof(T).GetProperties().Select(p => p.Name);
            StringBuilder sqlBuilder = new StringBuilder($"INSERT INTO {tableName}( ");
            sqlBuilder.AppendJoin(',', allProps);
            sqlBuilder.Append(") VALUES (");
            allProps = allProps.Select(p => $"{_adapter.DbParamTag}{p}");
            sqlBuilder.AppendJoin(',', allProps);
            sqlBuilder.Append(")");

            var result = await _conn.ExecuteAsync(sqlBuilder.ToString(), newData, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 新增資料
        /// </summary>
        /// <typeparam name="T">表單類型</typeparam>
        /// <param name="newData">新資料</param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual async Task<int> InsertAsync<T>(ColSet<T> newData, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (newData == null)
            {
                throw new ArgumentNullException(nameof(newData));
            }
            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            var newDataDict = newData.ToDictionary(n => GetMemberName(n.Key.Body), n => n.Value);

            StringBuilder sqlBuilder = new StringBuilder($"INSERT INTO {tableName}( ");
            sqlBuilder.AppendJoin(',', newDataDict.Keys);
            sqlBuilder.Append(") VALUES (");
            var newDataVal = newDataDict.Keys.Select(p => $"{_adapter.DbParamTag}{p}");
            sqlBuilder.AppendJoin(',', newDataVal);
            sqlBuilder.Append(")");

            DynamicParameters newDataParam = new DynamicParameters();
            foreach (var data in newDataDict)
            {
                newDataParam.Add(data.Key, data.Value);
            }
            var result = await _conn.ExecuteAsync(sqlBuilder.ToString(), newDataParam, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 更新單筆資料
        /// </summary>
        /// <typeparam name="T">更新表單類型</typeparam>
        /// <param name="updateData">更新資料</param>
        /// <param name="queryCondition">
        /// 查詢條件
        /// 使用範例: 
        /// new ColValue<T> 
        /// {
        ///    { c => c.Col1, value},
        ///    { c => c.Col2, value}
        /// }
        /// </param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual async Task<int> UpdateAsync<T>(ColSet<T> updateData, ColSet<T> queryCondition, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (updateData == null)
            {
                throw new ArgumentNullException(nameof(updateData));
            }

            if (queryCondition == null)
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }
            DynamicParameters paramData = new DynamicParameters();

            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;

            var needUpdateCols = updateData.Select(p => $"{GetMemberName(p.Key.Body)} = {_adapter.DbParamTag}{GetMemberName(p.Key.Body)}");
            // 開始組sql字串
            StringBuilder sqlBuilder = new StringBuilder($"UPDATE {tableName} SET ");
            sqlBuilder.AppendJoin(',', needUpdateCols);
            sqlBuilder.Append(" WHERE ");
            // 查詢條件的標記
            var queryTag = "query_tag_";
            var filters = queryCondition.Select(p => $"{GetMemberName(p.Key.Body)} = {_adapter.DbParamTag}{queryTag}{GetMemberName(p.Key.Body)}");
            sqlBuilder.AppendJoin(" AND ", filters);

            foreach (var col in updateData)
            {
                paramData.Add($"{GetMemberName(col.Key.Body)}", col.Value);
            }
            foreach (var col in queryCondition)
            {
                paramData.Add($"{queryTag}{GetMemberName(col.Key.Body)}", col.Value);
            }

            var result = await _conn.ExecuteAsync(sqlBuilder.ToString(), paramData, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 更新單筆資料
        /// </summary>
        /// <typeparam name="T">更新表單類型</typeparam>
        /// <param name="updateData">更新資料</param>
        /// <param name="queryCondition">更新查詢條件</param>
        /// <param name="paramData">查詢條件資料</param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual async Task<int> UpdateAsync<T>(ColSet<T> updateData, string queryCondition, DynamicParameters paramData, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (string.IsNullOrEmpty(queryCondition))
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }
            if (updateData == null)
            {
                throw new ArgumentNullException(nameof(updateData));
            }

            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            var needUpdateCols = updateData.Select(p => $"{GetMemberName(p.Key.Body)} = {_adapter.DbParamTag}{GetMemberName(p.Key.Body)}");
            // 開始組sql字串
            StringBuilder sqlBuilder = new StringBuilder($"UPDATE {tableName} SET ");
            sqlBuilder.AppendJoin(',', needUpdateCols);
            sqlBuilder.Append($" WHERE {queryCondition}");

            foreach (var col in updateData)
            {
                paramData.Add(GetMemberName(col.Key.Body), col.Value);
            }

            var result = await _conn.ExecuteAsync(sqlBuilder.ToString(), paramData, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 更新單筆資料
        /// </summary>
        /// <typeparam name="T">更新表單類型</typeparam>
        /// <param name="updateData">更新資料</param>
        /// <param name="queryCondition">
        /// 查詢條件
        /// 使用範例: 
        /// new ColValue<T> 
        /// {
        ///    { c => c.Col1, value},
        ///    { c => c.Col2, value}
        /// }
        /// </param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout">逾時長度</param>
        /// <returns></returns>
        protected virtual async Task<int> UpdateAllAsync<T>(T updateData, ColSet<T> queryCondition, string tableName = "", int? commandTimeout = null) where T : class
        {

            if (updateData == null)
            {
                throw new ArgumentNullException(nameof(updateData));
            }

            if (queryCondition == null)
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }
            DynamicParameters paramData = new DynamicParameters();

            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;

            var updateDataDict = typeof(T).GetProperties().ToDictionary(t => t.Name, t => t.GetValue(updateData, null));
            var needUpdateCols = updateDataDict.Keys.Select(c => $"{c} = {_adapter.DbParamTag}{c}");
            // 開始組sql字串
            StringBuilder sqlBuilder = new StringBuilder($"UPDATE {tableName} SET ");
            sqlBuilder.AppendJoin(',', needUpdateCols);
            sqlBuilder.Append(" WHERE ");
            // 查詢條件的標記
            var queryTag = "query_tag_";
            var filters = queryCondition.Select(p => $"{GetMemberName(p.Key.Body)} = {_adapter.DbParamTag}{queryTag}{GetMemberName(p.Key.Body)}");
            sqlBuilder.AppendJoin(" AND ", filters);

            foreach (var item in updateDataDict)
            {
                paramData.Add(item.Key, item.Value);
            }
            foreach (var col in queryCondition)
            {
                paramData.Add($"{queryTag}{GetMemberName(col.Key.Body)}", col.Value);
            }

            var result = await _conn.ExecuteAsync(sqlBuilder.ToString(), paramData, _transaction, commandTimeout);
            return result;
        }


        /// <summary>
        /// 刪除多筆資料
        /// </summary>
        /// <typeparam name="T">更新表單類型</typeparam>
        /// <param name="queryCondition">
        /// 查詢條件
        /// 使用範例: 
        /// new ColValue<T> 
        /// {
        ///    { c => c.Col1, value},
        ///    { c => c.Col2, value}
        /// }
        /// </param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        protected virtual async Task<int> DeleteAsync<T>(ColSet<T> queryCondition, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (queryCondition == null)
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }
            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            DynamicParameters paramData = new DynamicParameters();
            var filters = queryCondition.Select(p => $"{GetMemberName(p.Key.Body)} = {_adapter.DbParamTag}{GetMemberName(p.Key.Body)}");
            StringBuilder sqlBuilder = new StringBuilder($"DELETE FROM {tableName} WHERE ");
            sqlBuilder.AppendJoin(" AND ", filters);
            foreach (var col in queryCondition)
            {
                paramData.Add($"{GetMemberName(col.Key.Body)}", col.Value);
            }
            var result = await _conn.ExecuteAsync(sqlBuilder.ToString(), paramData, _transaction, commandTimeout);
            return result;
        }

        /// <summary>
        /// 刪除多筆資料
        /// </summary>
        /// <typeparam name="T">更新表單類型</typeparam>
        /// <param name="queryCondition">查詢條件</param>
        /// <param name="queryParam">查詢條件資料</param>
        /// <param name="tableName">表單名稱</param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        protected virtual async Task<int> DeleteAsync<T>(string queryCondition, DynamicParameters queryParam, string tableName = "", int? commandTimeout = null) where T : class
        {
            if (queryCondition == null)
            {
                throw new ArgumentNullException(nameof(queryCondition));
            }
            tableName = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;
            StringBuilder sqlBuilder = new StringBuilder($"DELETE FROM {tableName} WHERE {queryCondition}");
            var result = await _conn.ExecuteAsync(sqlBuilder.ToString(), queryParam, _transaction, commandTimeout);
            return result;
        }


        private static string GetMemberName(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return ((MemberExpression)expression).Member.Name;
                case ExpressionType.Convert:
                    return GetMemberName(((UnaryExpression)expression).Operand);
                default:
                    throw new NotSupportedException(expression.NodeType.ToString());
            }
        }
    }
}
