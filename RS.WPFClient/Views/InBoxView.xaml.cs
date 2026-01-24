using Microsoft.Extensions.DependencyInjection;
using NPOI.SS.Formula.Functions;
using RS.Commons.Attributs;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace RS.WPFClient.Views
{
    // 商品实体类
    public class Product
    {
        public string Category { get; set; }  // 分组字段：商品类别
        public string Name { get; set; }     // 商品名称
        public decimal Price { get; set; }   // 商品价格
        public int Stock { get; set; }       // 库存数量
    }
    public partial class InBoxView : UserControl
    {
        public InBoxView()
        {
            InitializeComponent();
        }
    }
}
