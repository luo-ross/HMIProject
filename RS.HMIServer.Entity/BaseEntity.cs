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
        /// <summary>
        /// 主键 
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 动态创建分布式Id
        /// </summary>
        public BaseEntity Create()
        {
            //获取分布式主键生成服务
            var idGenerator = ServiceProviderExtensions.GetService<IIdGenerator<long>>();
            this.Id = idGenerator.CreateId();
            return this;
        }
    }

}
