using IdGen;
using RS.Commons.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly IIdGenerator<long> IdGenerator;
        public BaseEntity()
        {
            //获取分布式主键生成服务
            this.IdGenerator = ServiceProviderExtensions.GetService<IIdGenerator<long>>();
        }

        /// <summary>
        /// 主键 
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 新增
        /// </summary>
        public BaseEntity Create()
        {
            this.Id = this.IdGenerator.CreateId();
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
        public long? CreateId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 最后一次更新人
        /// </summary>
        public long? UpdateId { get; set; }

        /// <summary>
        /// 最后一次更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 删除人
        /// </summary>
        public long? DeleteId { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime? DeleteTime { get; set; }

    }

}
