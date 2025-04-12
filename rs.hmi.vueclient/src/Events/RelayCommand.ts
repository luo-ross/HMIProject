import type { ICommand } from '../Interfaces/ICommand';

export class RelayCommand<T = void> implements ICommand<T> {
    private readonly _execute: (parameter?: T) => void;
    private readonly _canExecute: ((parameter?: T) => boolean) | null;
    private _canExecuteChangedCallbacks: (() => void)[] = [];
    private _commandParameter: T | undefined;

    constructor(
        execute: (parameter?: T) => void,
        canExecute?: (parameter?: T) => boolean
    ) {
        if (!execute) {
            throw new Error('Execute callback cannot be null');
        }
        this._execute = execute;
        this._canExecute = canExecute || null;
    }

    public Execute(parameter?: T): void {
        const param = parameter ?? this._commandParameter;
        if (this.CanExecute(param)) {
            this._execute(param);
        }
    }

    public CanExecute(parameter?: T): boolean {
        const param = parameter ?? this._commandParameter;
        return this._canExecute === null || this._canExecute(param);
    }

    public OnCanExecuteChanged(callback: () => void): void {
        this._canExecuteChangedCallbacks.push(callback);
    }

    public OffCanExecuteChanged(callback: () => void): void {
        const index = this._canExecuteChangedCallbacks.indexOf(callback);
        if (index !== -1) {
            this._canExecuteChangedCallbacks.splice(index, 1);
        }
    }

    public RaiseCanExecuteChanged(): void {
        this._canExecuteChangedCallbacks.forEach(callback => callback());
    }

    public SetCommandParameter(parameter: T): void {
        this._commandParameter = parameter;
        this.RaiseCanExecuteChanged();
    }

    public GetCommandParameter(): T | undefined {
        return this._commandParameter;
    }
}

// 非泛型版本的RelayCommand
export class SimpleRelayCommand implements ICommand<unknown> {
    private readonly _execute: (parameter?: unknown) => void;
    private readonly _canExecute: ((parameter?: unknown) => boolean) | null;
    private _canExecuteChangedCallbacks: (() => void)[] = [];
    private _commandParameter: unknown | undefined;

    constructor(
        execute: (parameter?: unknown) => void,
        canExecute?: (parameter?: unknown) => boolean
    ) {
        if (!execute) {
            throw new Error('Execute callback cannot be null');
        }
        this._execute = execute;
        this._canExecute = canExecute || null;
    }

    public Execute(parameter?: unknown): void {
        const param = parameter ?? this._commandParameter;
        if (this.CanExecute(param)) {
            this._execute(param);
        }
    }

    public CanExecute(parameter?: unknown): boolean {
        const param = parameter ?? this._commandParameter;
        return this._canExecute === null || this._canExecute(param);
    }

    public OnCanExecuteChanged(callback: () => void): void {
        this._canExecuteChangedCallbacks.push(callback);
    }

    public OffCanExecuteChanged(callback: () => void): void {
        const index = this._canExecuteChangedCallbacks.indexOf(callback);
        if (index !== -1) {
            this._canExecuteChangedCallbacks.splice(index, 1);
        }
    }

    public RaiseCanExecuteChanged(): void {
        this._canExecuteChangedCallbacks.forEach(callback => callback());
    }

    public SetCommandParameter(parameter: unknown): void {
        this._commandParameter = parameter;
        this.RaiseCanExecuteChanged();
    }

    public GetCommandParameter(): unknown | undefined {
        return this._commandParameter;
    }
} 