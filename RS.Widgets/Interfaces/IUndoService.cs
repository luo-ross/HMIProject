using System;

namespace RS.Widgets.Interfaces
{
    /// <summary>
    /// 撤销管理器接口
    /// </summary>
    public interface IUndoService
    {
        event EventHandler StateChanged;
        event EventHandler Undone;
        event EventHandler Redone;
        bool CanUndo { get; }
        bool CanRedo { get; }
        void AddAction(IUndoableAction action);
        void Undo();
        void Redo();
        void Clear();
    }
}
