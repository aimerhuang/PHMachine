using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Freedom.ZHPHMachine.Common
{
    /// <summary>
    /// ElementManager 的交互逻辑
    /// </summary>
    public class ElementManager
    {
        #region 成员

        /// <summary>
        /// 元素字典
        /// </summary>
        private Dictionary<string, object> dictElements;

        /// <summary>
        /// 锁对象
        /// </summary>
        private object LockObj = new object();

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public ElementManager()
        {
            this.dictElements = new Dictionary<string, object>();
        }

        #endregion

        #region 方法

        /// <summary>
        /// 添加对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        public void Add<T>(T action) where T : class
        {
            lock (this.LockObj)
            {
                if (!this.dictElements.ContainsKey(typeof(T).ToString()))
                {
                    this.dictElements.Add(typeof(T).ToString(), action);
                }

            }
        }

        /// <summary>
        /// 移除对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        public void Remove<T>() where T : class
        {
            lock (this.LockObj)
            {
                if (this.dictElements.ContainsKey(typeof(T).ToString()))
                {
                    this.dictElements.Remove(typeof(T).ToString());
                }
            }
        }

        /// <summary>
        /// 移除全部对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        public void RemoveALL()
        {
            lock (this.LockObj)
            {
                this.dictElements?.Clear();
            }
        }

        /// <summary>
        /// 设置对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        public void Set<T>(T action, string key = "") where T : class
        {
            lock (this.LockObj)
            {
                string _key = string.IsNullOrWhiteSpace(key) ? typeof(T).ToString() : key;
                if (this.dictElements.ContainsKey(_key))
                {
                    this.dictElements.Remove(_key);
                }
                this.dictElements.Add(_key, action);
            }
        }

        /// <summary>
        /// 清空对象
        /// </summary>
        public void Clear()
        {
            lock (this.LockObj)
            {
                this.dictElements.Clear();
            }
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(string key = "") where T : class
        {
            lock (this.LockObj)
            {
                T t = null;
                string _key = string.IsNullOrWhiteSpace(key) ? typeof(T).ToString() : key;
                if (this.dictElements.ContainsKey(_key))
                {
                    t = (T)this.dictElements[_key];
                }
                return t;
            }
        }

        #endregion

    }
}
