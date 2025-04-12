// 定义ICommand接口
export interface ICommand<T = void> {
  Execute(parameter?: T): void;
  CanExecute(parameter?: T): boolean;
  OnCanExecuteChanged(callback: () => void): void;
  OffCanExecuteChanged(callback: () => void): void;
  SetCommandParameter(parameter: T): void;
  GetCommandParameter(): T | undefined;
}
