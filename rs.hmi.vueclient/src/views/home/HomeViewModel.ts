import { ref } from 'vue'
import { HomeModel } from '../../Models/HomeModel'
import { HomeViewModelBase } from '../../Models/HomeViewModelBase';


export class HomeViewModel extends HomeViewModelBase {
  private homeModel = ref<HomeModel>(new HomeModel());
  constructor() {
    super();
  }

  public get HomeModel(): HomeModel {
    return this.homeModel.value;
  }

  public set HomeModel(viewModel: HomeModel) {
    this.homeModel.value = viewModel;
  }

  public handleLogout(): void {
   
  }
} 
