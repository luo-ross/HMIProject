import type { Func } from "../Events/Func";
import { GenericOperateResult, SimpleOperateResult } from "../Commons/OperateResult/OperateResult"
export interface ILoadingEvents {
  GenericLoadingActionAsync<T>(func: Func<Promise<GenericOperateResult<T>>>): Promise<GenericOperateResult<T>>
  SimpleLoadingActionAsync(func: Func<Promise<SimpleOperateResult>>): Promise<SimpleOperateResult>
}    
