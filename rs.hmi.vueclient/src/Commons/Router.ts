import { createRouter, createWebHistory } from 'vue-router'
import type { RouteLocationNormalized, NavigationGuardNext } from 'vue-router'
import { Cryptography } from './Cryptography/Cryptography'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'Root',
      component: () => import('../Views/Login/LoginView.vue')
    },
    {
      path: '/Login/Index',
      name: 'Login',
      component: () => import('../Views/Login/LoginView.vue')
    },
    {
      path: '/Register/Index',
      name: 'Register',
      component: () => import('../Views/Register/RegisterView.vue')
    },
    {
      path: '/Home/Index',
      name: 'Home',
      component: () => import('../Views/Home/HomeView.vue')
    }
  ]
})

// 路由守卫
router.beforeEach((to: RouteLocationNormalized, from: RouteLocationNormalized, next: NavigationGuardNext) => {
  // 白名单，不需要验证的路由
  const whiteList = ['/Login/Index']
  if (whiteList.includes(to.path)) {
    next();
    return;
  }

  // 获取会话信息
  const getSessionModelResult = Cryptography.GetSessionModelFromStorage()
  if (!getSessionModelResult.IsSuccess) {
    // 重定向到登录页，并添加时间戳
    next('/Login/Index')
    return
  }

  const sessionModel = getSessionModelResult.Data
  if (!sessionModel?.Token) {
    // 重定向到登录页，并添加时间戳
    next('/Login/Index')
    return
  }
  next()
})

export default router 
