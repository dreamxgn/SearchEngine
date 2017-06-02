using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 搜索引擎自动营销助手V1._0.Model
{
    public class QuestionBatchModel
    {
        public string BatchID { get; set; }
        public string BatchName { get; set; }

        public override string ToString()
        {
            return this.BatchName;
        }
    }
}
