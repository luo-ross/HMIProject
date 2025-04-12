import { ref } from 'vue';
import RSEmail from '../Controls/RSEmail.vue';

export function useLoginViewModel() {
  const EmailRef = ref<InstanceType<typeof RSEmail> | null>(null);

  // 设置邮箱输入框的引用
  const SetEmailRef = (ref: InstanceType<typeof RSEmail> | null) => {
    EmailRef.value = ref;
  };

  // 获取邮箱输入框焦点
  const FocusEmail = () => {
    EmailRef.value?.Focus();
  };

  return {
    SetEmailRef,
    FocusEmail
  };
} 