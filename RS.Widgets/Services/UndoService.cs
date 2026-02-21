using RS.Widgets.Interfaces;
using RS.Widgets.UndoActions;
using System;
using System.Collections.Generic;
using System.Linq;
using RS.Commons.Attributs;
using Microsoft.Extensions.DependencyInjection;

namespace RS.Widgets.Services
{
    /// <summary>
    /// 撤销服务
    /// </summary>
    [ServiceInjectConfig(ServiceLifetime.Singleton)]
    public class UndoService : IUndoService
    {
        private readonly Stack<IUndoableAction> undoStack = new Stack<IUndoableAction>();
        private readonly Stack<IUndoableAction> redoStack = new Stack<IUndoableAction>();

        public event EventHandler StateChanged;

        public void AddAction(IUndoableAction action)
        {
            if (action == null || !action.IsValid) return;

            undoStack.Push(action);
            redoStack.Clear();
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;

        public void Undo()
        {
            if (!CanUndo) return;
            var action = undoStack.Pop();
            action.Undo();
            redoStack.Push(action);
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Redo()
        {
            if (!CanRedo) return;
            var action = redoStack.Pop();
            action.Redo();
            undoStack.Push(action);
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
