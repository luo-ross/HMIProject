import { ref} from 'vue'
import { UserModel, type UserInfo } from '../models/UserModel'

export class UserViewModel {
  private userModel: UserModel
  public loading = ref(false)
  public error = ref('')
  public searchQuery = ref('')
  public currentUser = ref<UserInfo | null>(null)
  constructor() {
    this.userModel = UserModel.getInstance()
  }
} 
