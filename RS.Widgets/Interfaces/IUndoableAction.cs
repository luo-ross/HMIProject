using System;

namespace RS.Widgets.Interfaces
{
    /// <summary>
    /// 可撤销操作接口
    /// </summary>
    public interface IUndoableAction
    {
        string Name { get; }
        bool IsValid { get; }
        void Undo();
        void Redo();
    }
}
