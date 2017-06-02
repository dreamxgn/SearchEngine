using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using 搜索引擎自动营销助手V1._0.Classs;

namespace 搜索引擎自动营销助手V1._0.Model
{
    public class UserModel
    {
        public string Id { get; set; }
        public string LoginName { get; set; }
        public string LoginPwd { get; set; }
        public int Category { get; set; }
        public SogouService service { get; set; }

        public bool isLogin { get; set; }
    }
}
