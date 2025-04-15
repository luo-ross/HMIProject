import type { Func } from "../Events/Func";
import { GenericOperateResult } from "../Commons/OperateResult/OperateResult"
export interface ILoadingEvents {
  InvokeLoadingActionAsync<T>(func: Func<Promise<GenericOperateResult<T>>>): Promise<GenericOperateResult<T>>
}    
