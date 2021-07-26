using System;
using System.Collections.Generic;

namespace Freedom.ZHPHMachine.Common
{
    /// <summary>
    /// ServiceRegistry 的交互逻辑
    /// </summary>
    public class ServiceRegistry
    {
        #region 成员

        /// <summary>
        /// 注册表字典
        /// </summary>
        private Dictionary<string, object> services;

        #endregion

        #region 属性

        private static ServiceRegistry _Instance;

        /// <summary>
        /// 单例
        /// </summary>
        public static ServiceRegistry Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new ServiceRegistry();
                }
                return _Instance;
            }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        private ServiceRegistry()
        {
            this.services = new Dictionary<string, object>();
        }

        #endregion

        #region 方法

        /// <summary>
        /// 注册对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public void Register<T>(T service) where T : class
        {
            if (!this.services.ContainsKey(typeof(T).ToString()))
            {
                this.services.Add(typeof(T).ToString(), service);
            }
            else
            {
                throw new InvalidOperationException("the object have been registered");
            }
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() where T : class
        {
            T t = null;
            if (this.services.ContainsKey(typeof(T).ToString()))
            {
                t = this.services[typeof(T).ToString()] as T;
            }
            else
            {
                throw new InvalidOperationException("the string associated with the object does not exist:" + typeof(T).ToString());
            }
            return t;
        }
        #endregion


    }
}
