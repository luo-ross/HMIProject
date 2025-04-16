import axios, { type AxiosInstance } from 'axios';
import { Cryptography } from '../Cryptography/Cryptography'
import { CommonUtils } from '../Utils';
import type { AESEncryptModel } from '../../Models/AESEncryptModel';
import { GenericOperateResult } from '../OperateResult/OperateResult';

export class AxiosUtil {
  private static Instance: AxiosUtil;
  private AxiosInstance: AxiosInstance;
  private GetSessionModelPathName: string = "/api/v1/General/GetSessionModel";
  public Token: string | null = null;
  public Cryptography: Cryptography;
  public CommonUtils: CommonUtils;
  constructor() {
    // 创建axios 实例
    this.AxiosInstance = axios.create({
      baseURL: 'http://localhost:54293', // 替换为你的实际 API 基础 URL
      timeout: 10000,// 设置请求超时时间
      headers: {
        'Content-Type': 'application/json'
      }
    });

    // 请求拦截器
    this.AxiosInstance.interceptors.request.use(config => {
      if (config.url == undefined) {
        return Promise.reject(new Error('url不能为空'));
      }
      const pathname = new URL(config.url, config.baseURL).pathname;
      //如果是请求对话则不拦截
      if (pathname == this.GetSessionModelPathName) {
        return config;
      }

      // 获取 aesKey, appId, token
      const getSessionModelResult = this.Cryptography.GetSessionModelFromStorage();
      if (!getSessionModelResult.IsSuccess) {
        return Promise.reject(new Error('未获取到会话权限'));
      }

      const sessionModel = getSessionModelResult.Data;
      if (!sessionModel?.Token) {
        return Promise.reject(new Error('必须提供token'));
      }

      //优先使用用户设置的Token 否则调用会话里的
      let token = this.Token;
      if (token == null) {
        token = sessionModel?.Token;
      }

      // 设置请求头
      if (token != null) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    }, error => {
      return Promise.reject(error);
    });

    // 响应拦截器
    this.AxiosInstance.interceptors.response.use(
      response => {
        return response.data;
      },
      error => {
        if (error.response) {
          switch (error.response.status) {
            case 401:
              // token过期或无效，清除认证信息
              break;
            case 403:
              break;
            case 500:
              break;
            default:
          }
        }
        return Promise.reject(error);
      }
    );


    this.CommonUtils = new CommonUtils();
    this.Cryptography = Cryptography.GetInstance();
  }

  public static GetInstance(): AxiosUtil {
    if (!AxiosUtil.Instance) {
      AxiosUtil.Instance = new AxiosUtil();
    }
    return AxiosUtil.Instance;
  }


  /**
 * 普通发起请求
 * @typeparam url webAPI接口
 * @typeparam data Post数据
 */
  public async Post<D, R>(url: string, data?: D | null): Promise<R> {
    return await this.AxiosInstance.post<D, R>(url, data);
  }

  /**
 * 加密发起请求并且解密结果
 * @typeparam url webAPI接口
 * @typeparam data Post数据
 */
  public async AESEncryptPost<D, R>(url: string, data?: D | null): Promise<GenericOperateResult<R>> {

    //使用AES密钥对数据进行加密
    const aesEncryptResult = await this.Cryptography.AESEncryptSimple(data);
    if (!aesEncryptResult.IsSuccess) {
      return GenericOperateResult.CreateFailResult(aesEncryptResult);
    }

    //向服务端发起获取邮箱验证码请求
    const result = await this.Post<AESEncryptModel, GenericOperateResult<AESEncryptModel>>('/api/v1/Register/GetEmailVerify', aesEncryptResult.Data);
    if (!result.IsSuccess || result.Data == null) {
      return GenericOperateResult.CreateFailResult(result.Message);
    }

    //AES对称解密数据
    const aesDecryptSimpleResult = await this.Cryptography.AESDecryptSimple<R>(result.Data);
    if (!aesDecryptSimpleResult.IsSuccess) {
      return aesDecryptSimpleResult;
    }

    return aesDecryptSimpleResult;
  }



}



