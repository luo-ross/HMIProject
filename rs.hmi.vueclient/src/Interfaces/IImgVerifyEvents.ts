import { GenericOperateResult } from "../Commons/OperateResult/OperateResult"
import type { ImgVerifyResultModel } from "../Models/ImgVerifyResultModel";
export interface IImgVerifyEvents {
  GetImgVerifyResultAsync(): Promise<GenericOperateResult<ImgVerifyResultModel>>;
  ResetToInitialState(): void;
}    
