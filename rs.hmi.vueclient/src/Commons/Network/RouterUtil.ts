import { createRouter, createWebHistory } from 'vue-router'
import type { RouteLocationNormalized, NavigationGuardNext, Router } from 'vue-router'
import { Cryptography } from '../Cryptography/Cryptography'
import { Utils } from '../Utils';
import  { AxiosUtil } from './AxiosUtil';
import { SimpleOperateResult, type GenericOperateResult } from '../OperateResult/OperateResult';
import { SessionRequestModel } from '../../Models/SessionRequestModel';
import { MemoryCacheKey } from '../../Models/MemoryCacheKey';
import type { SessionResultModel } from '../../Models/SessionResultModel';

export class RouterUtil {
  private static Instance: RouterUtil;
  private AxiosUtil: AxiosUtil;
  private Router: Router;
  public Cryptography: Cryptography;
  public Utils: Utils;
  constructor() {
    // 路由守卫
    this.Router = createRouter({
      history: createWebHistory(),
      routes: [
        {
          path: '/',
          name: 'Root',
          component: () => import('../../Views/Login/LoginView.vue')
        },
        {
          path: '/Login',
          name: 'Login',
          component: () => import('../../Views/Login/LoginView.vue')
        },
        {
          path: '/Register',
          name: 'Register',
          component: () => import('../../Views/Register/RegisterView.vue')
        },
        {
          path: '/EmailVerify',
          name: 'EmailVerify',
          component: () => import('../../Views/EmailVerify/EmailVerifyView.vue')
        },
        {
          path: '/Home',
          name: 'Home',
          component: () => import('../../Views/Home/HomeView.vue')
        },
        {
          path: '/Security',
          name: 'Security',
          component: () => import('../../Views/Security/SecurityView.vue')
        }
      ]
    });
    this.Router.beforeEach((to: RouteLocationNormalized, from: RouteLocationNormalized, next: NavigationGuardNext) => {
      // 白名单，不需要验证的路由
      const whiteList = ['/Login']
      if (whiteList.includes(to.path)) {
        next();
        return;
      }

      // 获取会话信息
      const getSessionModelResult = this.Cryptography.GetSessionModelFromStorage()
      if (!getSessionModelResult.IsSuccess) {
        // 重定向到登录页，并添加时间戳
        next('/Login')
        return
      }

      const sessionModel = getSessionModelResult.Data
      if (!sessionModel?.Token) {
        // 重定向到登录页，并添加时间戳
        next('/Login')
        return
      }
      next()
    })


    this.Utils = new Utils();
    this.Cryptography = Cryptography.GetInstance();
    this.AxiosUtil = AxiosUtil.GetInstance();
    this.InitSessionAsync();
  }

  public static GetInstance(): RouterUtil {
    if (!RouterUtil.Instance) {
      RouterUtil.Instance = new RouterUtil();
    }
    return RouterUtil.Instance;
  }


  public GetRouter():Router {
    return this.Router;
  }

  Push(url: string) {
    this.Router.push(url);
  }


  private async InitSessionAsync(): Promise<SimpleOperateResult> {

    const initRSASecurityKeyDataResult = await this.Cryptography.InitRSASecurityKeyDataAsync();
    if (!initRSASecurityKeyDataResult.IsSuccess) {
      return initRSASecurityKeyDataResult;
    }

    // 将Date对象转换为Unix时间戳（毫秒）
    const timestamp = new Date().getTime();
    //创建会话请求
    const sessionRequestModel = new SessionRequestModel();
    sessionRequestModel.RSASignPublicKey = sessionStorage.getItem(MemoryCacheKey.GlobalRSASignPublicKey);
    sessionRequestModel.RSAEncryptPublicKey = sessionStorage.getItem(MemoryCacheKey.GlobalRSAEncryptPublicKey)
    sessionRequestModel.Nonce = this.Utils.CreateRandCode(10).toString();
    sessionRequestModel.TimeStamp = timestamp.toString();
    sessionRequestModel.AudiencesType = "Web";

    const arrayList = [
      sessionRequestModel.RSASignPublicKey,
      sessionRequestModel.RSAEncryptPublicKey,
      sessionRequestModel.Nonce,
      sessionRequestModel.TimeStamp,
    ];

    //获取会话的Hash数据
    const getRSAHashResult = await this.Cryptography.GetRSAHashAsync(arrayList);
    if (!getRSAHashResult.IsSuccess) {
      return getRSAHashResult;
    }

    const rsaSignDataResult = await this.Cryptography.RSASignDataAsync(getRSAHashResult.Data, sessionStorage.getItem(MemoryCacheKey.GlobalRSASignPrivateKey));
    if (!rsaSignDataResult.IsSuccess) {
      return rsaSignDataResult;
    }
    sessionRequestModel.MsgSignature = rsaSignDataResult.Data;

    const result = await this.AxiosUtil.Post<SessionRequestModel, GenericOperateResult<SessionResultModel>>('/api/v1/General/GetSessionModel', sessionRequestModel);

    const getSessionModelResult = await this.Cryptography.GetSessionModel(result);
    if (!getSessionModelResult.IsSuccess) {
      return getSessionModelResult;
    }
    return SimpleOperateResult.CreateSuccessResult();
  }


}


