using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SailingDemo.Cluster.Utils
{
    public class RedisCacheHelper
    {
        private static RedisCache _redisCache = null;
        private static RedisCacheOptions options = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="instanceName"></param>
        public RedisCacheHelper(string connectionString, string instanceName)
        {
            options = new RedisCacheOptions();
            options.Configuration = connectionString;
            options.InstanceName = instanceName;
            _redisCache = new RedisCache(options);
        }
        /// <summary>
        /// 初始化Redis
        /// </summary>
        public static void InitRedis(string connectionString, string instanceName)
        {
            options = new RedisCacheOptions();
            options.Configuration = connectionString;
            options.InstanceName = instanceName;
            _redisCache = new RedisCache(options);
        }
        /// <summary>
        /// 添加string数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="ExprireTime">过期时间 单位小时</param>
        /// <returns></returns>
        public static bool SetStringValue(string key, string value, int ExprireTime = -1)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            try
            {
                if (ExprireTime < 0)
                {
                    //不设置过期时间
                    _redisCache.SetString(key, value);
                }
                else
                {
                    _redisCache.SetString(key, value, new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpiration = DateTime.Now.AddHours(ExprireTime)
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 获取string数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static string GetStringValue(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            try
            {
                return _redisCache.GetString(key);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 获取数据（对象）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            string value = GetStringValue(key);
            if (string.IsNullOrEmpty(value))
            {
                return default(T);
            }
            try
            {
                var obj = JsonConvert.DeserializeObject<T>(value);
                return obj;
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="key">键</param>
        public static bool Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            try
            {
                _redisCache.Remove(key);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 刷新数据
        /// </summary>
        /// <param name="key">键</param>
        public static bool Refresh(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            try
            {
                _redisCache.Refresh(key);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 重置数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expireTime">过期时间 单位小时</param>
        public static bool Replace(string key, string value, int expireTime = 24)
        {
            if (Remove(key))
            {
                return SetStringValue(key, value, expireTime);
            }
            else
            {
                return false;
            }
        }
    }
}
