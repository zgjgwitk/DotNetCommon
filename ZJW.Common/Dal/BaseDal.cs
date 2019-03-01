using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using ZJW.Common.DB;

namespace ZJW.Common.Dal
{
    public class BaseDal
    {
        public T Get<T>(long id) where T : class
        {
            using (SqlConnection connection = new SqlConnection(DbConnection.SqlConnStrStore))
            {
                connection.Open();
                var info = connection.Get<T>(id);
                return info;
            }
        }
        public T Get<T>(string id) where T : class
        {
            using (SqlConnection connection = new SqlConnection(DbConnection.SqlConnStrStore))
            {
                connection.Open();
                var info = connection.Get<T>(id);
                return info;
            }
        }

        public List<T> GetAll<T>() where T : class
        {
            using (SqlConnection connection = new SqlConnection(DbConnection.SqlConnStrStore))
            {
                connection.Open();
                var list = connection.GetAll<T>().ToList();
                return list;
            }
        }

        public long Insert<T>(T entity) where T : class
        {
            using (SqlConnection connection = new SqlConnection(DbConnection.SqlConnStrStore))
            {
                connection.Open();
                var id = connection.Insert(entity);
                return id;
            }
        }

        public long InsertIdentity<T>(T entity) where T : class
        {
            using (SqlConnection connection = new SqlConnection(DbConnection.SqlConnStrStore))
            {
                connection.Open();
                var id = connection.InsertIdentity(entity);
                return id;
            }
        }

        public bool Update<T>(T entity) where T : class
        {
            using (SqlConnection connection = new SqlConnection(DbConnection.SqlConnStrStore))
            {
                connection.Open();
                var res = connection.Update(entity);
                return res;
            }
        }

        public bool IsExist(string sql, object param)
        {
            using (SqlConnection connection = new SqlConnection(DbConnection.SqlConnStrStore))
            {
                connection.Open();
                var info = connection.Query(sql, param);
                if (info != null && info.Count() > 0)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
