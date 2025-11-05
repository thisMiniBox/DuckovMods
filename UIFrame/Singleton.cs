using System;

namespace UIFrame
{
    public abstract class Singleton<T> where T : class
    {
        private static T? _instance;
        private static readonly object _lock = new object(); // 用于多线程安全
        public static T? Instance
        {
            get
            {
                // 在多线程环境下，确保只有一个线程能够创建实例
                lock (_lock)
                {
                    if (_instance == null)
                    {

                        _instance = Activator.CreateInstance(typeof(T), true) as T;
                        if (_instance == null)
                        {
                            throw new InvalidOperationException($"无法为类型 {typeof(T).Name} 创建单例实例。" +
                                                                "请确保它有一个私有的无参构造函数。");
                        }
                    }
                    return _instance;
                }
            }
        }
        protected Singleton()
        {
            if (_instance != null)
            {
                throw new InvalidOperationException($"试图创建第二个单例实例 {typeof(T).Name}。请通过 Singleton<{typeof(T).Name}>.Instance 访问。");
            }
        }
    }
}