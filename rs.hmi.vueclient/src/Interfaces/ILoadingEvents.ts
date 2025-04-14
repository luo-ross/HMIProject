import type { Func } from "../Events/Func";
import { SimpleOperateResult } from "../Commons/OperateResult/OperateResult"
export interface ILoadingEvents {
  InvokeLoadingActionAsync(func: Func<Promise<SimpleOperateResult>>): Promise<SimpleOperateResult>
}    
