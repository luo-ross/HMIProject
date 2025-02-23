using RS.Models;
using RS.RESTfulApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Extensions
{
    /// <summary>
    /// HttpRequest扩展
    /// </summary>
    public static class HttpRequestExtensions
    {

        /// <summary>
        /// AES Http异步Get 带返回Data
        /// </summary>
        /// <typeparam name="TResult">返回数据类型</typeparam>
        /// <param name="apiUrl">接口Url</param>
        /// <returns></returns>
        public static async Task<OperateResult<TResult>> AESHttpGetAsync<TResult>(this string apiUrl, string clientName)
        {
            //获取加解密服务
            var cryptographyService = ServiceProviderExtensions.GetService<ICryptographyService>();

            var getHttpGetJsonResult = await apiUrl.HttpGetJsonResultAsync(clientName);
            if (!getHttpGetJsonResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<TResult>(getHttpGetJsonResult);
            }

            var objectResult = getHttpGetJsonResult.Data.ToObject<OperateResult<AESEncryptModel>>();
            if (!objectResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<TResult>(objectResult);
            }

            //AES对称解密数据
            var aesDecryptResult = cryptographyService.AESDecrypt<TResult>(objectResult.Data);
            if (!aesDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<TResult>(aesDecryptResult);
            }

            return aesDecryptResult;
        }

        /// <summary>
        /// Http异步Get
        /// </summary>
        /// <typeparam name="TResult">返回数据类型</typeparam>
        /// <param name="apiUrl">接口Url</param>
        /// <returns></returns>
        public static async Task<OperateResult<TResult>> HttpGetAsync<TResult>(this string apiUrl, string clientName)
        {
            var getHttpGetJsonResult = await apiUrl.HttpGetJsonResultAsync(clientName);
            if (!getHttpGetJsonResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<TResult>(getHttpGetJsonResult);
            }
            var objectResult = getHttpGetJsonResult.Data.ToObject<OperateResult<TResult>>();
            if (!objectResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<TResult>(objectResult);
            }
            return objectResult;
        }

        /// <summary>
        /// Http异步获取
        /// </summary>
        /// <param name="apiUrl">接口Url</param>
        /// <returns></returns>
        public static async Task<OperateResult> HttpGetAsync(this string apiUrl, string clientName)
        {
            var getHttpGetJsonResult = await apiUrl.HttpGetJsonResultAsync(clientName);
            if (!getHttpGetJsonResult.IsSuccess)
            {
                return getHttpGetJsonResult;
            }
            var objectResult = getHttpGetJsonResult.Data.ToObject<OperateResult>();
            if (!objectResult.IsSuccess)
            {
                return objectResult;
            }
            return objectResult;
        }


        /// <summary>
        /// AES Http异步Post 有返回Data
        /// </summary>
        /// <typeparam name="TPost">Post数据类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="apiUrl">接口Url</param>
        /// <param name="postModel">Post数据</param>
        /// <returns></returns>
        public static async Task<OperateResult<TResult>> AESHttpPostAsync<TPost, TResult>(this string apiUrl, string clientName, TPost postModel) where TPost : class
        {
           
            var cryptographyService = ServiceProviderExtensions.GetService<ICryptographyService>();
            //AES对称加密数据
            var aesEncryptResult = cryptographyService.AESEncrypt(postModel);
            if (!aesEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<TResult>(aesEncryptResult);
            }

            //往服务端发送数据 并获取回传数据
            var aesEncryptModelResult = await apiUrl.HttpPostAsync<AESEncryptModel, AESEncryptModel>(clientName, aesEncryptResult.Data);
            if (!aesEncryptModelResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<TResult>(aesEncryptModelResult);
            }

            //AES对称解密数据
            var aesDecryptResult = cryptographyService.AESDecrypt<TResult>(aesEncryptModelResult.Data);
            if (!aesDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<TResult>(aesDecryptResult);
            }
            return aesDecryptResult;
        }


        /// <summary>
        /// AES Http异步Post 无返回Data
        /// </summary>
        /// <typeparam name="TPost">Post数据类型</typeparam>
        /// <param name="apiUrl">接口Url</param>
        /// <param name="postModel">Post数据</param>
        /// <returns></returns>
        public static async Task<OperateResult> AESHttpPostAsync<TPost>(this string apiUrl, string clientName, TPost postModel) where TPost : class
        {
            var cryptographyService = ServiceProviderExtensions.GetService<ICryptographyService>();
            //AES对称加密数据
            var aesEncryptResult = cryptographyService.AESEncrypt(postModel);
            if (!aesEncryptResult.IsSuccess)
            {
                return aesEncryptResult;
            }
            return await apiUrl.HttpPostAsync(clientName, aesEncryptResult.Data);
        }

        /// <summary>
        /// Http异步Post
        /// </summary>
        /// <typeparam name="TPost">Post数据类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="apiUrl">接口Url</param>
        /// <param name="postModel">Post数据</param>
        /// <returns></returns>
        public static async Task<OperateResult<TResult>> HttpPostAsync<TPost, TResult>(this string apiUrl, string clientName, TPost postModel) where TPost : class
        {
            var getHttpPostJsonResult = await apiUrl.HttpPostJsonResultAsync<TPost>(clientName, postModel);
            if (!getHttpPostJsonResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<TResult>(getHttpPostJsonResult);
            }

            var objectResult = getHttpPostJsonResult.Data.ToObject<OperateResult<TResult>>();
            if (!objectResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<TResult>(objectResult);
            }
            return objectResult;
        }

        /// <summary>
        /// Http异步Post
        /// </summary>
        /// <typeparam name="TPost">Post数据类型</typeparam>
        /// <param name="apiUrl">接口Url</param>
        /// <param name="postModel">Post数据</param>
        /// <returns></returns>
        public static async Task<OperateResult> HttpPostAsync<TPost>(this string apiUrl, string clientName, TPost postModel) where TPost : class
        {
            var getHttpPostJsonResult = await apiUrl.HttpPostJsonResultAsync<TPost>(clientName, postModel);
            if (!getHttpPostJsonResult.IsSuccess)
            {
                return getHttpPostJsonResult;
            }
            var objectResult = getHttpPostJsonResult.Data.ToObject<OperateResult>();
            if (!objectResult.IsSuccess)
            {
                return objectResult;
            }
            return objectResult;
        }

        /// <summary>
        /// Http异步过去Json
        /// </summary>
        /// <param name="apiUrl">接口Url</param>
        /// <returns></returns>
        public static async Task<OperateResult<string>> HttpGetJsonResultAsync(this string apiUrl, string clientName)
        {
            var handleHttpRequestExceptionResult = await HandleHttpRequestException(async () =>
            {
                return await ServiceProviderExtensions.GetHttpClient(clientName).GetAsync(apiUrl);
            });
            if (!handleHttpRequestExceptionResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<string>(handleHttpRequestExceptionResult);
            }
            HttpResponseMessage httpResponseMessage = handleHttpRequestExceptionResult.Data;

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return OperateResult.CreateFailResult<string>($"HttpGet请求失败：{httpResponseMessage.StatusCode}");
            }
            var jsonStr = await httpResponseMessage.Content.ReadAsStringAsync();
            return OperateResult.CreateSuccessResult<string>(jsonStr);
        }

        /// <summary>
        /// Http异步Post
        /// </summary>
        /// <typeparam name="TPost">Post数据类型</typeparam>
        /// <param name="apiUrl">接口Url</param>
        /// <param name="postModel">Post数据类型</param>
        /// <returns></returns>
        public static async Task<OperateResult<string>> HttpPostJsonResultAsync<TPost>(this string apiUrl, string clientName, TPost postModel) where TPost : class
        {
            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrWhiteSpace(apiUrl))
            {
                return OperateResult.CreateFailResult<string>("接口地址不能为空");
            }

            var handleHttpRequestExceptionResult = await HandleHttpRequestException(async () =>
            {
                return await ServiceProviderExtensions.GetHttpClient(clientName).PostAsync(apiUrl, postModel.ToStringContent());
            });

            if (!handleHttpRequestExceptionResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<string>(handleHttpRequestExceptionResult);
            }

            HttpResponseMessage httpResponseMessage = handleHttpRequestExceptionResult.Data;

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return OperateResult.CreateFailResult<string>($"HttpPost请求失败：{httpResponseMessage.StatusCode}");
            }
            var jsonStr = await httpResponseMessage.Content.ReadAsStringAsync();

            return OperateResult.CreateSuccessResult<string>(jsonStr);
        }

        /// <summary>
        /// 处理HttpRequest异常
        /// </summary>
        /// <typeparam name="TResult">结果数据类型</typeparam>
        /// <param name="func">带返回值的委托</param>
        /// <returns></returns>
        public static async Task<OperateResult<TResult>> HandleHttpRequestException<TResult>(Func<Task<TResult>> func)
        {
            try
            {
                var result = await func.Invoke();
                return OperateResult.CreateSuccessResult(result);
            }
            catch (HttpRequestException ex)
            {
                return OperateResult.CreateFailResult<TResult>("由于网络连接、DNS等潜在问题，请求失败、服务器证书验证或超时。");
            }
            catch (TaskCanceledException ex)
            {
                return OperateResult.CreateFailResult<TResult>("由于超时，请求失败");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
