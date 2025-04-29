import { ViewModelBase } from './ViewModelBase';
import { SessionModel } from './WebAPI/SessionModel';


export abstract class HomeViewModelBase extends ViewModelBase {
  constructor() {
    super();

    //默认验证是否登录
    const sessionModelJson = sessionStorage.getItem("SessionModel");
    if (sessionModelJson == null) {
      this.RouterUtil.Push("/Login");
    }
    if (sessionModelJson != null) {
     const sessionModel = this.Utils.ToObject<SessionModel>(sessionModelJson, SessionModel);
      if (sessionModel == null||!sessionModel.IsLogin) {
        this.RouterUtil.Push("/Login");
      }
    }
  }
}
