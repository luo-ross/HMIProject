import axios from 'axios';

// 创建全局 axios 实例
const WebApi = axios.create({
  baseURL: 'http://localhost:54293', // 替换为你的实际 API 基础 URL
  timeout: 10000,// 设置请求超时时间
  headers: {
    'Content-Type': 'application/json'
  }
});

const filterPath = '/api/v1/General/GetSessionModel';

// 请求拦截器
WebApi.interceptors.request.use(config => {
  if (config.url == undefined) {
    return Promise.reject(new Error('url不能为空'));
  }
  const path = new URL(config.url, config.baseURL).pathname;

  if (path == filterPath) {
    return config;
  }
  
  const token = sessionStorage.getItem('token');
  if (!token) {
    return Promise.reject(new Error('必须提供token'));
  }
  // 如果不是获取会话的请求，添加认证信息
  if (path !== filterPath) {
    // 设置请求头
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
  }

  //// 检查 POST 请求是否包含 appid
  //if (config.method === 'post' && path !== filterPath) {
  //  if (!config.data || !config.data.appid) {
  //    return Promise.reject(new Error('POST 请求必须提供 appid'));
  //  }
  //}

  return config;
}, error => {
  return Promise.reject(error);
});

// 响应拦截器
WebApi.interceptors.response.use(
  response => {
    return response.data;
  },
  error => {
    if (error.response) {
      switch (error.response.status) {
        case 401:
          // token过期或无效，清除认证信息
          sessionStorage.removeItem('token');
          sessionStorage.removeItem('appid');
          sessionStorage.removeItem('aeskey');
          break;
        case 403:
          console.error('权限不足');
          break;
        case 500:
          console.error('服务器错误');
          break;
        default:
          console.error('请求失败');
      }
    }
    return Promise.reject(error);
  }
);

export default WebApi;  
