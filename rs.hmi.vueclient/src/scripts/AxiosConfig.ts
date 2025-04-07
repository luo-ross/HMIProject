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
// 请求拦截器，全局检查 POST 请求是否包含 appid
WebApi.interceptors.request.use(config => {
  if (config.url == undefined) {
    return Promise.reject(new Error('url不能为空'));
  }
  if (config.method === 'post') {
    const path = new URL(config.url, config.baseURL).pathname;
    if (path !== filterPath && (!config.data || !config.data.appid)) {
      return Promise.reject(new Error('POST 请求必须提供 appid'));
    }
  }
  return config;
}, error => {
  return Promise.reject(error);
});

export default WebApi;  
