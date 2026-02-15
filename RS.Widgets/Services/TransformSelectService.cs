using RS.Widgets.Adorners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Services
{
    public class TransformSelectService
    {
        public HashSet<TransformAdorner> TransformAdornerSelectList { get; set; }
        public TransformSelectService()
        {
            TransformAdornerSelectList = new HashSet<TransformAdorner>();
        }

        public void SingleSelect(TransformAdorner adorner)
        {
            ClearSelect();
            TransformAdornerSelectList.Add(adorner);
            adorner.IsSelect = true;
        }

        private void ClearSelect()
        {
            foreach (var item in TransformAdornerSelectList)
            {
                item.IsSelect = false;
            }
            TransformAdornerSelectList.Clear();
        }
    }
}
