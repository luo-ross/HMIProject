import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/login/LoginView.vue'
import RegisterView from '../views/register/RegisterView.vue'
import HomeView from '../views/home/HomeView.vue'
import { Cryptography} from '../scripts/Cryptography'
const Router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/login/index'
    },
    {
      path: '/register/index',
      name: 'register',
      component: RegisterView,
      meta: {
        layout: 'blank'
      }
    },
    {
      path: '/home',
      children: [
        {
          path: '',
          name: 'home',
          component: HomeView
        },
        {
          path: 'dashboard',
          name: 'dashboard',
          component: () => import('../views/dashboard/DashboardView.vue')
        },
      ]
    },
    {
      path: '/login/index',
      name: 'login',
      component: LoginView,
      meta: {
        layout: 'blank'
      }
    }
  ]
})

// 路由守卫
Router.beforeEach((to, from, next) => {
  // 白名单路由直接放行
  const whiteList = ['/login/index']
  if (whiteList.includes(to.path)) {
    next()
    return
  }

  // 获取 aesKey, appId, token
  const getSessionModelResult = Cryptography.GetSessionModelFromStorage();
  if (!getSessionModelResult.IsSuccess) {
    return;
  }
  const sessionModel = getSessionModelResult.Data;
  if (!sessionModel?.AppId || !sessionModel.AesKey || !sessionModel.Token) {
    next({ path: '/login/index' })
    return
  }
  next()
})

export default Router 
