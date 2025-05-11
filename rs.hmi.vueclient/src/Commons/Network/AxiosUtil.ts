import axios, { type AxiosInstance} from 'axios';
import { Cryptography } from '../Cryptography/Cryptography'
import { Utils } from '../Utils';
import type { AESEncryptModel } from '../../Models/WebAPI/AESEncryptModel';
import { BaseOperateResult, GenericOperateResult, SimpleOperateResult } from '../OperateResult/OperateResult';

export class AxiosUtil {
  private static Instance: AxiosUtil;
  public AxiosInstance: AxiosInstance;
  private GetSessionModelPathName: string = "/api/v1/General/GetSessionModel";
  public Token: string | null = null;
  public Cryptography: Cryptography;
  public Utils: Utils;
  constructor() {
    // 创建axios 实例
    this.AxiosInstance = axios.create({
      baseURL: import.meta.env.VITE_APP_BASE_API, // 使用环境变量配置baseURL
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
        const result = new BaseOperateResult();
        if (error.response) {
          // 服务器有响应（如 400、401、403、404、500 等）
          result.ErrorCode = error.response.status;
          result.Message = error.response.data?.message || error.response.statusText || '服务器错误';
        } else if (error.request) {
          // 请求已发出但无响应
          result.ErrorCode = -1;
          result.Message = '服务器无响应，请检查网络或稍后重试';
        } else {
          // 其他错误
          result.ErrorCode = -2;
          result.Message = error.message || '未知错误';
        }
        result.IsSuccess = false;
        return result;
      }
    );


    this.Utils = new Utils();
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
* 普通发起请求
* @typeparam url webAPI接口
*/
  public async Get<D, R>(url: string): Promise<R> {
    return await this.AxiosInstance.get<D, R>(url);
  }

  /**
 * 加密发起请求并且解密结果
 * @typeparam url webAPI接口
 * @typeparam data Post数据
 */
  public async AESEnAndDecryptPost<D, R>(url: string, data: D | null, type: new () => R): Promise<GenericOperateResult<R>> {

    //使用AES密钥对数据进行加密
    const aesEncryptResult = await this.Cryptography.AESEncryptSimple(data);
    if (!aesEncryptResult.IsSuccess) {
      return GenericOperateResult.CreateFailResult(aesEncryptResult);
    }

    //向服务端发起获取邮箱验证码请求
    const result = await this.Post<AESEncryptModel, GenericOperateResult<AESEncryptModel>>(url, aesEncryptResult.Data);
    if (!result.IsSuccess) {
      return GenericOperateResult.CreateFailResult(result.Message);
    }

    //AES对称解密数据
    const aesDecryptSimpleResult = await this.Cryptography.AESDecryptSimple<R>(result.Data, type);
    if (!aesDecryptSimpleResult.IsSuccess) {
      return aesDecryptSimpleResult;
    }

    return aesDecryptSimpleResult;
  }

  /**
* 加密发起请求但是不用解密
* @typeparam url webAPI接口
* @typeparam data Post数据
*/
  public async AESEncryptPost<D>(url: string, data?: D | null): Promise<SimpleOperateResult> {

    //使用AES密钥对数据进行加密
    const aesEncryptResult = await this.Cryptography.AESEncryptSimple(data);
    if (!aesEncryptResult.IsSuccess) {
      return aesEncryptResult;
    }
    //向服务端发起获取邮箱验证码请求
    return await this.Post<AESEncryptModel, SimpleOperateResult>(url, aesEncryptResult.Data);
  }


  /**
* 加密发起请求但是不用解密
* @typeparam url webAPI接口
*/
  public async DecryptGet<R>(url: string, type: new () => R): Promise<GenericOperateResult<R>> {

    //向服务端发起获取邮箱验证码请求
    const result = await this.Get<null, GenericOperateResult<AESEncryptModel>>(url);
    if (!result.IsSuccess) {
      return GenericOperateResult.CreateFailResult(result.Message);
    }

    //AES对称解密数据
    const aesDecryptSimpleResult = await this.Cryptography.AESDecryptSimple<R>(result.Data, type);
    if (!aesDecryptSimpleResult.IsSuccess) {
      return aesDecryptSimpleResult;
    }

    return aesDecryptSimpleResult;
  }



  public async GetBlobUrl<D>(url: string, data?: D | null): Promise<GenericOperateResult<string>> {

    try {

      //向服务端发起获取邮箱验证码请求
      const response = await this.AxiosInstance.post<D, Blob>(url, data, {
        responseType: 'blob',
      });
      // 创建 URL 对象
      const blobUrl = URL.createObjectURL(response);
      return GenericOperateResult.CreateSuccessResult(blobUrl);
    } catch {
      return GenericOperateResult.CreateSuccessResult("获取Blob错误");
    }

  }


  public async GetMultipartBlobUrl<D>(url: string, data?: D | null): Promise<GenericOperateResult<string>> {

    try {
      ////向服务端发起获取邮箱验证码请求
      //const response = await this.AxiosInstance.post<D, Blob[]>(url, data, {
      //  responseType: 'blob',
      //  headers: {
      //    'Accept': 'multipart/mixed'
      //  }
      //});

      //const response: AxiosResponse<ArrayBuffer> = await axios.post(url, data, {
      //  headers: {
      //    'Accept': 'multipart/mixed',
      //    'Content-Type': 'application/json'
      //  },
      //  responseType: 'arraybuffer'
      //});


      const buffer = await this.AxiosInstance.post<null, ArrayBuffer>(url, data, {
        responseType: 'arraybuffer',
        headers: {
          'Accept': 'application/octet-stream'
        }
      });

      //// 获取完整的数据
      //const buffer = response.data;

      // 读取第一个图片的长度（前8个字节）
      const dataView = new DataView(buffer);
      const image1Length = dataView.getInt32(0, true); // true表示小端字节序

      // 分割两个图片的数据
      const image1Data = buffer.slice(8, 8 + image1Length);
      const image2Data = buffer.slice(8 + image1Length);

      // 创建两个Blob对象
      const blob1 = new Blob([image1Data], { type: 'image/jpeg' });
      const blob2 = new Blob([image2Data], { type: 'image/jpeg' });

      console.log(blob1);
      console.log(blob2);
      //// 创建URL
      //this.LoginModel.VerifyImgUrl1 = URL.createObjectURL(blob1);
      //this.LoginModel.VerifyImgUrl2 = URL.createObjectURL(blob2);

      //const response = await this.AxiosInstance.post(url, data, {
      //  responseType: 'blob',
      //  headers: {
      //    'Accept': 'multipart/mixed'
      //  }
      //});

      //// 解析 multipart 响应
      //const multipartData = await this.parseMultipartResponse(response.data);

      //// 创建图片 URL
      //this.LoginModel.VerifyImgUrl1 = URL.createObjectURL(multipartData[0]);
      //this.LoginModel.VerifyImgUrl2 = URL.createObjectURL(multipartData[1]);



      //// 获取 boundary
      //const contentType = response.headers['content-type'];
      //const boundary = contentType?.match(/boundary=(.*)/)?.[1];

      //if (!boundary) {
      //  throw new Error('未找到 boundary 信息');
      //}

      //// 解析 multipart/mixed 数据
      //const parts = parseMultipart(response.data, boundary);

      //// 提取 Blob 数据
      //blobObjects = parts.map(part => {
      //  const contentType = part.headers['content-type'];
      //  return new Blob([part.body], { type: contentType });
      //});




      //// 获取 boundary
      //const contentType = response.headers['content-type'];
      //const boundary = contentType.match(/boundary=(.*)/)[1];

      //// 解析 multipart/mixed 数据
      //const parts = parseMultipart(response.data, boundary);

      //// 提取 Blob 数据
      //blobObjects = parts.map(part => {
      //  const contentType = part.headers['content-type'];
      //  return new Blob([part.body], { type: contentType });
      //});



      //console.log(response)
      //// 创建 URL 对象
      //const blobUrl = URL.createObjectURL(response);

      //// 解析 multipart 响应
      //const multipartData = await this.parseMultipartResponse(response.data);

      //// 创建图片 URL
      //this.LoginModel.VerifyImgUrl1 = URL.createObjectURL(multipartData[0]);
      //this.LoginModel.VerifyImgUrl2 = URL.createObjectURL(multipartData[1]);

      return GenericOperateResult.CreateSuccessResult("null");
    } catch {
      return GenericOperateResult.CreateSuccessResult("获取Blob错误");
    }



  }

  //private async parseMultipartResponse(blob: Blob): Promise<Blob[]> {
  //  const boundary = '--' + blob.type.split('boundary=')[1];
  //  const parts = await new Response(blob).arrayBuffer()
  //    .then(buffer => new Uint8Array(buffer));

  //  // 解析 multipart 数据的逻辑
  //  // 这里需要实现具体的解析逻辑
  //  // 返回解析后的 Blob 数组
  //  return [/* parsed blobs */];
  //}


}



