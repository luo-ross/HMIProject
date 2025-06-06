﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using StackExchange.Redis;

namespace RS.HMIServer.DAL.Redis
{
    [ServiceInjectConfig(ServiceLifetime.Singleton)]
    internal class RedisDbContext
    {
        private readonly ConnectionMultiplexer ConnectionMultiplexer;

        /// <summary>
        /// 鉴权Redis数据库
        /// </summary>
        private readonly IDatabase AuthRedis;

        /// <summary>
        /// 会话Redis数据库
        /// </summary>
        private readonly IDatabase SessionRedis;

        /// <summary>
        /// 注册Redis数据库
        /// </summary>
        private readonly IDatabase RegisterRedis;

        /// <summary>
        /// 注册Redis数据库
        /// </summary>
        private readonly IDatabase PasswordResetRedis;

        /// <summary>
        /// 客户端Id Redis数据库
        /// </summary>
        private readonly IDatabase ClientIdRedis;

        /// <summary>
        /// 客户端IP Redis数据库
        /// </summary>
        private readonly IDatabase ClientIPRedis;

        /// <summary>
        /// 图像验证码Redis数据库
        /// </summary>
        private readonly IDatabase ImgVerifyRedis;

        public RedisDbContext(IConfiguration configuration)
        {
            string host = configuration["RSAppRedis:Host"];
            string port = configuration["RSAppRedis:Port"];
            string password = configuration["RSAppRedis:Password"];
            var options = new ConfigurationOptions();
            options.EndPoints.Add(host, int.Parse(port));
            options.Password = password;
            this.ConnectionMultiplexer = ConnectionMultiplexer.Connect(options);
            this.AuthRedis = ConnectionMultiplexer.GetDatabase(0);
            this.SessionRedis = ConnectionMultiplexer.GetDatabase(1);
            this.RegisterRedis = ConnectionMultiplexer.GetDatabase(2);
            this.PasswordResetRedis = ConnectionMultiplexer.GetDatabase(3);
            this.ClientIdRedis = ConnectionMultiplexer.GetDatabase(4);
            this.ClientIPRedis = ConnectionMultiplexer.GetDatabase(5);
            this.ImgVerifyRedis = ConnectionMultiplexer.GetDatabase(6);
        }

        /// <summary>
        /// 获取鉴权连接
        /// </summary>
        /// <returns></returns>
        public IDatabase GetAuthRedis()
        {
            return this.AuthRedis;
        }

        /// <summary>
        /// 获取注册注册链接
        /// </summary>
        /// <returns></returns>
        public IDatabase GetRegisterRedis()
        {
            return this.RegisterRedis;
        }



        /// <summary>
        /// 获取会话连接
        /// </summary>
        /// <returns></returns>
        public IDatabase GetSessionRedis()
        {
            return this.SessionRedis;
        }

        /// <summary>
        /// 获取注册注册链接
        /// </summary>
        /// <returns></returns>
        public IDatabase GetPasswordResetRedis()
        {
            return this.PasswordResetRedis;
        }

        /// <summary>
        /// 获取客户端连接
        /// </summary>
        /// <returns></returns>
        public IDatabase GetClientIdRedis()
        {
            return this.ClientIdRedis;
        }

        /// <summary>
        /// 获取客户端IP连接
        /// </summary>
        /// <returns></returns>
        public IDatabase GetClientIPRedis()
        {
            return this.ClientIPRedis;
        }


        /// <summary>
        /// 获取图像验证码Redis
        /// </summary>
        /// <returns></returns>
        public IDatabase GetImgVerifyRedis()
        {
            return this.ImgVerifyRedis;
        }
    }
}
