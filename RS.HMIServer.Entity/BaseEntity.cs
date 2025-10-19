using System.ComponentModel.DataAnnotations;

namespace RS.HMIServer.Entity
{

    //SQL Server类型       C#类型
    //bit                 bool
    //tinyint             byte
    //smallint            short
    //int                 int
    //bigint              long
    //real                float
    //float               double
    //money               decimal
    //datetime            DateTime
    //char                string
    //varchar             string
    //nchar               string
    //nvarchar            string
    //text                string
    //ntext               string
    //image               byte[]
    //binary              byte[]
    //uniqueidentifier    Guid

    /// <summary>
    /// 基类
    /// </summary>
    public class BaseEntity
    {
        
        public BaseEntity()
        {
          
        }

        /// <summary>
        /// 主键 
        /// </summary>
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// 新增
        /// </summary>
        public BaseEntity Create()
        {
            this.Id = Guid.NewGuid().ToString();
            this.CreateTime = DateTime.Now;
            return this;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        public BaseEntity Update()
        {
            this.UpdateTime = DateTime.Now;
            return this;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        public BaseEntity Delete()
        {
            this.DeleteTime = DateTime.Now;
            return this;
        }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool? IsDelete { get; set; }


        /// <summary>
        /// 创建人
        /// </summary>
        public string? CreateId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 最后一次更新人
        /// </summary>
        public string? UpdateId { get; set; }

        /// <summary>
        /// 最后一次更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 删除人
        /// </summary>
        public string? DeleteId { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime? DeleteTime { get; set; }

    }

}
