using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RS.Widgets.UndoActions
{
    /// <summary>
    /// 变换撤销/重做操作
    /// </summary>
    public class TransformUndoAction : IUndoableAction
    {
        public string Name { get; set; }

        public bool IsValid
        {
            get
            {
                if (Changes == null || Changes.Count == 0) return false;

                return Changes.Any(c =>
                    c.before != null && c.after != null && (
                    Math.Abs(c.before.X - c.after.X) > 0.001 ||
                    Math.Abs(c.before.Y - c.after.Y) > 0.001 ||
                    Math.Abs(c.before.Width - c.after.Width) > 0.001 ||
                    Math.Abs(c.before.Height - c.after.Height) > 0.001 ||
                    Math.Abs(c.before.Angle - c.after.Angle) > 0.001));
            }
        }

        public List<(TransformData target, TransformMemento before, TransformMemento after)> Changes { get; set; } = new List<(TransformData target, TransformMemento before, TransformMemento after)>();

        public void Undo()
        {
            foreach (var change in Changes)
            {
                change.before.Apply(change.target);
            }
        }

        public void Redo()
        {
            foreach (var change in Changes)
            {
                change.after.Apply(change.target);
            }
        }
    }
}
