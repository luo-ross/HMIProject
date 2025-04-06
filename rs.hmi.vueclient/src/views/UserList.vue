<template>
  <div class="user-list">
    <div class="header">
      <h2>用户管理</h2>
      <div class="search-box">
        <input 
          type="text" 
          v-model="viewModel.searchQuery"
          placeholder="搜索用户..."
          class="form-control"
        >
      </div>
      <button 
        class="btn btn-primary"
        @click="showAddUserModal"
      >
        添加用户
      </button>
    </div>

    <!-- 错误提示 -->
    <div v-if="viewModel.error" class="alert alert-danger">
      {{ viewModel.error }}
    </div>

    <!-- 加载状态 -->
    <div v-if="viewModel.loading" class="text-center">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">加载中...</span>
      </div>
    </div>

    <!-- 用户列表 -->
    <div v-else class="table-responsive">
      <table class="table table-striped">
        <thead>
          <tr>
            <th>ID</th>
            <th>用户名</th>
            <th>邮箱</th>
            <th>角色</th>
            <th>创建时间</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in viewModel.filteredUsers" :key="user.id">
            <td>{{ user.id }}</td>
            <td>{{ user.username }}</td>
            <td>{{ user.email }}</td>
            <td>{{ user.role }}</td>
            <td>{{ user.createTime }}</td>
            <td>
              <button 
                class="btn btn-sm btn-info me-2"
                @click="editUser(user)"
              >
                编辑
              </button>
              <button 
                class="btn btn-sm btn-danger"
                @click="deleteUser(user.id)"
              >
                删除
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- 添加/编辑用户模态框 -->
    <div class="modal" :class="{ 'show d-block': showModal }" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ isEditing ? '编辑用户' : '添加用户' }}</h5>
            <button type="button" class="btn-close" @click="closeModal"></button>
          </div>
          <div class="modal-body">
            <form @submit.prevent="handleSubmit">
              <div class="mb-3">
                <label class="form-label">用户名</label>
                <input 
                  type="text" 
                  class="form-control"
                  v-model="formData.username"
                  required
                >
              </div>
              <div class="mb-3">
                <label class="form-label">邮箱</label>
                <input 
                  type="email" 
                  class="form-control"
                  v-model="formData.email"
                  required
                >
              </div>
              <div class="mb-3">
                <label class="form-label">角色</label>
                <select class="form-select" v-model="formData.role" required>
                  <option value="管理员">管理员</option>
                  <option value="普通用户">普通用户</option>
                </select>
              </div>
              <div class="text-end">
                <button type="button" class="btn btn-secondary me-2" @click="closeModal">
                  取消
                </button>
                <button type="submit" class="btn btn-primary">
                  {{ isEditing ? '保存' : '添加' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { UserViewModel } from '../viewmodels/UserViewModel'
import type { UserInfo } from '../models/UserModel'

const viewModel = new UserViewModel()
const showModal = ref(false)
const isEditing = ref(false)
const formData = reactive({
  username: '',
  email: '',
  role: '普通用户'
})

// 显示添加用户模态框
const showAddUserModal = () => {
  isEditing.value = false
  formData.username = ''
  formData.email = ''
  formData.role = '普通用户'
  showModal.value = true
}

// 编辑用户
const editUser = (user: UserInfo) => {
  isEditing.value = true
  formData.username = user.username
  formData.email = user.email
  formData.role = user.role
  showModal.value = true
}

// 删除用户
const deleteUser = async (id: number) => {
  if (confirm('确定要删除这个用户吗？')) {
    await viewModel.deleteUser(id)
  }
}

// 关闭模态框
const closeModal = () => {
  showModal.value = false
}

// 提交表单
const handleSubmit = async () => {
  if (isEditing.value) {
    // 编辑用户
    await viewModel.updateUser(viewModel.currentUser.value!.id, formData)
  } else {
    // 添加用户
    await viewModel.addUser(formData)
  }
  closeModal()
}
</script>

<style scoped>
.user-list {
  padding: 20px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.search-box {
  width: 300px;
}

.modal {
  background-color: rgba(0, 0, 0, 0.5);
}

.modal.show {
  display: block;
}
</style> 