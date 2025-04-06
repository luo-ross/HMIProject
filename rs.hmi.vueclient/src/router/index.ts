import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/login/LoginView.vue'
import HomeView from '../views/home/HomeView.vue'


const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/login/index'
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
router.beforeEach((to, from, next) => {
  const isAuthenticated = localStorage.getItem('token') // 这里简单判断是否登录
  if (to.path !== '/login/index' && !isAuthenticated) {
    next({ path: '/login/index' })
  } else {
    next()
  }
})

export default router 
